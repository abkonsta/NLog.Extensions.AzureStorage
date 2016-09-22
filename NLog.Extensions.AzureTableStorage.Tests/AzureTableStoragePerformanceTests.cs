using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NLog.Extensions.AzureTableStorage.Tests
{
    public class AzureTableStoragePerformanceTests : IDisposable
    {
        #region Constants
        //must match table name in AzureTableStorage target in NLog.config
        private const string TargetTableName = "AzureTableStoragePerformanceTests";
        #endregion Constants

        #region Fields
        private readonly Logger _logger;
        private readonly CloudTable _cloudTable;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        public AzureTableStoragePerformanceTests()
        {
            try
            {
                _logger = LogManager.GetLogger(TargetTableName);
                var storageAccount = GetStorageAccount();
                // Create the table client.
                var tableClient = storageAccount.CreateCloudTableClient();
                //create charts table if not exists.
                _cloudTable = tableClient.GetTableReference(TargetTableName);
                _cloudTable.DeleteIfExists();
                _cloudTable.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize tests, make sure Azure Storage Emulator is running.", ex);
            }
        }
        #endregion Constructors

        #region Methods
        [Fact]
        public void TestPerformance()
        {
            // Designed for use with a profiler

            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${date}_${guid}_${time}_${ticks}_${longdate}_${micros}_${guid}_${logger}_${level}_${machine}_${descticks}";
            LogManager.ReconfigExistingLoggers();

            const int totalEntities = 5000;
            for (var count = 0; count < totalEntities; count++)
                _logger.Log(LogLevel.Info, "information");

            var entities = GetLogEntities();
            Assert.Equal(totalEntities, entities.Count);
        }

        private string GetConnectionString()
        {
            return CloudConfigurationManager.GetSetting("ConnectionString");
        }

        private CloudStorageAccount GetStorageAccount()
        {
            var connectionString = GetConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return storageAccount;
        }

        private List<LogEntity> GetLogEntities()
        {
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            var query = new TableQuery<LogEntity>();
            var entities = _cloudTable.ExecuteQuery(query);
            return entities.ToList();
        }

        public void Dispose()
        {
            _cloudTable.DeleteIfExists();
        }
        #endregion Methods
    }
}
