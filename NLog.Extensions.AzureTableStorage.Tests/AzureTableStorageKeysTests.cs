using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NLog.Extensions.AzureTableStorage.Tests
{
    public class AzureTableStorageKeysTests : IDisposable
    {
        #region Constants
        //must match table name in AzureTableStorage target in NLog.config
        private const string TargetTableName = "TempAzureTableStorageTargetTestsLogs"; 
        #endregion Constants

        #region Fields
        private readonly Logger _logger;
        private readonly CloudTable _cloudTable;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        public AzureTableStorageKeysTests()
        {
            try
            {
                _logger = LogManager.GetLogger(GetType().ToString());
                var storageAccount = GetStorageAccount();
                // Create the table client.
                var tableClient = storageAccount.CreateCloudTableClient();
                //create charts table if not exists.
                _cloudTable = tableClient.GetTableReference(TargetTableName);
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
        public void IncludeDateInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${date}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var expectedDate = DateTime.UtcNow.ToString("yyyyMMdd");
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0], expectedDate);
        }

        [Fact]
        public void IncludeTimeAndGuidInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${time}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0].Length, 6);
            Assert.Equal(key[1].Length, 32);
        }

        [Fact]
        public void IncludeTicksAndLongDateInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${ticks}__${longdate}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0].Length, 19);
            Assert.Equal(key[1].Length, 20);
        }

        [Fact]
        public void IncludeMicrosInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${micros}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0].Length, 6);
        }

        [Fact]
        public void IncludeMachineInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${machine}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0], Environment.MachineName);
        }

        [Fact]
        public void IncludeDescendingTicksInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${descticks}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0].Length, 19);
        }

        [Fact]
        public void IncludeLevelInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${level}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(2, key.Length);
            Assert.Equal(LogLevel.Info.ToString(), key[0]);
        }

        [Fact]
        public void IncludeLevelUppercaseInRowKey()
        {
            // configure keys and log something
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName("AzureTableStorage");
            target.RowKey = "${level:uppercase=true}__${guid}";
            LogManager.ReconfigExistingLoggers();
            _logger.Log(LogLevel.Info, "information");

            // extract the key
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // assert
            Assert.Equal(2, key.Length);
            Assert.Equal(LogLevel.Info.ToString().ToUpper(), key[0]);
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
            var query = new TableQuery<LogEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Test." + GetType()));
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
