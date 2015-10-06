using System;
using System.Diagnostics;
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
            Initialize(connectionString, tableName);
        }
        #endregion Constructors

        #region Methods
        public void EnsureConfigurationIsCurrent(string connectionString, string tableName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            if (string.IsNullOrWhiteSpace(tableName))
                return;

            // if connection string or table name have changed...
            if (_connectionString != connectionString || _tableName != tableName)
            {
                // re-initialize the storage manager. the target will now log to the newly specified
                // storage account and table
                Initialize(connectionString, tableName);
            }
        }

        private void Initialize(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            // get the storage account
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            Trace.TraceWarning($"NLog.Extensions.AzureTableStorage will log to {storageAccount.TableEndpoint.AbsoluteUri}");

            // it is possible that at this stage we do not yet have a storage account; for example
            // if we defer the NLog target initialization till Global.asax.cs

            // in this case, we will just let this silently fail. it will get re-initialized
            // once a valid storage account connection string is available
            try
            {
                // Create the table client.
                var tableClient = storageAccount.CreateCloudTableClient();

                //create charts table if not exists.
                _cloudTable = tableClient.GetTableReference(tableName);
                _cloudTable.CreateIfNotExists();
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
        public void Add(LogEntity entity)
        {
            // check if this connection has been initialized. if it hasn't, it is likely because
            // we were unable to connect, or the config is invalid, or the "development storage" is used 
            // on the production server
            if (_cloudTable == null)
            {
                Trace.TraceWarning("NLog.Extensions.AzureTableStorage logging was attempted but the target is not initialized.");
                Trace.TraceWarning("This could be due to a missing connection string or an error during the target initialization.");
                Trace.TraceWarning("Please check the log for previous error messages!");

                return;
            }

            var insertOperation = TableOperation.Insert(entity);
            _cloudTable.Execute(insertOperation);
        }
    }
    #endregion Methods
}
