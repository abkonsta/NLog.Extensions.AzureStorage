using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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

        private readonly CloudTable _cloudTable;
        private readonly Logger _logger;

        #endregion Fields



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

        public void Dispose()
        {
            _cloudTable.DeleteIfExists();
        }

        [Fact]
        public async Task TestPerformance()
        {
            // Designed for use with a profiler

            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${date}_${guid}_${time}_${ticks}_${guid}_${logger}_${level}";
            LogManager.ReconfigExistingLoggers();

            const int totalEntities = 1500;
            for (var count = 0; count < totalEntities; count++)
                _logger.Log(LogLevel.Info, "information");

            var entities = (await GetLogEntities(totalEntities));
            Assert.Equal(totalEntities, entities.Count);
        }

        private string GetConnectionString()
        {
            return CloudConfigurationManager.GetSetting("ConnectionString");
        }

        private async Task<List<LogEntity>> GetLogEntities(int expectedCount)
        {
            var entities = new List<LogEntity>();

            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(2500);
                var query = new TableQuery<LogEntity>();
                entities = new List<LogEntity>(_cloudTable.ExecuteQuery(query));
                if (expectedCount == entities.Count)
                {
                    break;
                }
            }

            return entities;
        }

        private CloudStorageAccount GetStorageAccount()
        {
            var connectionString = GetConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return storageAccount;
        }

        #endregion Methods
    }
}