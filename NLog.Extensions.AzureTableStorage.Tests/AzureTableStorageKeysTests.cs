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
        private const string TargetTableName = "AzureTableStorageKeysTests";

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
        public void IncludeGdcInPatitionKey()
        {
            var gdcValue = Guid.NewGuid().ToString().Replace("-", string.Empty);
            GlobalDiagnosticsContext.Set("item", gdcValue);

            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${gdc:item}__${guid}";
            LogManager.ReconfigExistingLoggers();


            // log something
            _logger.Log(LogLevel.Info, "information");

            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(gdcValue, key[0]);
        }

        [Fact]
        public void IncludeDateInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${date}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var expectedDate = DateTime.UtcNow.ToString("yyyyMMdd");
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(expectedDate, key[0]);
        }

        [Fact]
        public void IncludeTimeAndGuidInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${time}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(key.Length, 2);
            Assert.Equal(6, key[0].Length);
            Assert.Equal(32, key[1].Length);
        }

        [Fact]
        public void IncludeTicksAndLongDateInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${ticks}__${longdate}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(key.Length, 2);
            Assert.Equal(key[0].Length, 19);
            Assert.Equal(key[1].Length, 20);
        }

        [Fact]
        public void IncludeMicrosInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${micros}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(6, key[0].Length);
        }

        [Fact]
        public void IncludeMachineInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${machine}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(Environment.MachineName, key[0]);
        }

        [Fact]
        public void IncludeDescendingTicksInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${descticks}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(19, key[0].Length);
        }

        [Fact]
        public void IncludeLevelInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${level}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(LogLevel.Info.ToString(), key[0]);
        }

        [Fact]
        public void IncludeLevelUppercaseInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget) LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${level:uppercase=true}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = GetLogEntities().First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
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
