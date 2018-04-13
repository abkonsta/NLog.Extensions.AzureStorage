using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NLog.Common;

namespace NLog.Extensions.AzureTableStorage
{
    /// <summary>
    /// Table storage manager
    /// </summary>
    public class TableStorageManager
    {
        #region Constants
        #endregion Constants

        #region Fields
        private string _connectionString;
        private string _tableName;
        private CloudTable _cloudTable;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        public TableStorageManager(string connectionString, string tableName)
        {
            // the constructor for the table storage manager only initializes the properties
            // the actual connection is deferred till we need it

            // one of the reasons for doing this is to avoid the startup errors because the connection
            // string, if it is environment-dependent, may not yet be valid. watch the trace logs ;)
            _connectionString = connectionString;
            _tableName = tableName;
        }
        #endregion Constructors

        #region Methods
        public async Task EnsureConfigurationIsCurrentAsync(string connectionString, string tableName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            if (string.IsNullOrWhiteSpace(tableName))
                return;

            // if connection string or table name have changed, invalidate
            // the existing connection
            if (_connectionString != connectionString || _tableName != tableName)
            {
                Trace.TraceInformation("NLog.Extensions.AzureTableStorage connection information has changed.");
                Trace.TraceInformation("NLog.Extensions.AzureTableStorage will (re)initialize.");
                _cloudTable = null;
            }

            // _cloudTable may be null for two reasons: 1) it hasn't been initialized yet and 2) it was
            // cleared because connection information changed
            if(_cloudTable == null)
            { 
                // re-initialize the storage manager. the target will now log to the newly specified
                // storage account and table
                await InitializeAsync(connectionString, tableName);
            }
        }

        private async Task InitializeAsync(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Trace.TraceInformation("NLog.Extensions.AzureTableStorage does not have a connection string.");
                Trace.TraceInformation("You will need to specify it in NLog.config, or assign one in code.");
                return;
            }

            // it is possible that at this stage we do not yet have a storage account; for example
            // if we defer the NLog target initialization till Global.asax.cs

            // in this case, we will just let this fail (and log the exception, of course). 
            // you will need to make sure the target gets reinitialized in code, once a valid storage 
            // account connection string is available
            try
            {
                // get the storage account
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                Trace.TraceInformation($"NLog.Extensions.AzureTableStorage will log to {storageAccount.TableEndpoint.AbsoluteUri}");

                // Create the table client.
                var tableClient = storageAccount.CreateCloudTableClient();

                //create charts table if not exists.
                _cloudTable = tableClient.GetTableReference(tableName);
                await _cloudTable.CreateIfNotExistsAsync();
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                    throw;

                // write to trace. since you are using azure, this may be helpful. enable the streaming logs and
                // see the trace statements!
                Trace.TraceError("Exception in AzureTableStorage initialization:");
                Trace.TraceError(exception.ToString());

                // write to nlog's internal logger (if configured)
                InternalLogger.Error(exception.ToString());
            }
        }

        /// <summary>
        /// Add an entry to the Azure Storage table.
        /// Check if the connection has been initialized properly (_cloudTable is not null).
        /// </summary>
        /// <param name="entity"></param>
        public async Task AddAsync(LogEntity entity)
        {
            // check if this connection has been initialized. if it hasn't, it is likely because
            // we were unable to connect, or the config is invalid, or the "development storage" is used 
            // on the production server
            if (_cloudTable == null)
            {
                Trace.TraceWarning("NLog.Extensions.AzureTableStorage logging was attempted but the target is not initialized.");
                Trace.TraceWarning("You will not capture any NLog messages until the target initializes successfully.");
                Trace.TraceWarning("This could be due to a missing connection string or an error during the target initialization.");
                Trace.TraceWarning("Please check the log for previous error messages!");

                return;
            }

            var insertOperation = TableOperation.Insert(entity);
            await _cloudTable.ExecuteAsync(insertOperation);
        }
    }
    #endregion Methods
}
