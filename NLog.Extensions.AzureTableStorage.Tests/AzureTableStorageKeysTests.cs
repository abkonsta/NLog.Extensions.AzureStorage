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
    public class AzureTableStorageKeysTests : IDisposable
    {
        #region Constants

        //must match table name in AzureTableStorage target in NLog.config
        private const string TargetTableName = "AzureTableStorageKeysTests";

        #endregion Constants

        #region Fields

        private readonly CloudTable _cloudTable;
        private readonly Logger _logger;

        #endregion Fields



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

        public void Dispose()
        {
            _cloudTable.DeleteIfExists();
        }

        [Fact]
        public async Task IncludeDateInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${date:format=yyyyMMdd}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var expectedDate = DateTime.UtcNow.ToString("yyyyMMdd");
            var entity = (await GetLogEntities()).First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(expectedDate, key[0]);
        }

        //[Fact]
        //public async Task IncludeDescendingTicksInRowKey()
        //{
        //    // configure keys
        //    var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
        //    target.RowKey = "${descticks}__${guid}";
        //    LogManager.ReconfigExistingLoggers();

        //    // log something
        //    _logger.Log(LogLevel.Info, "information");

        //    // extract the key and assert
        //    var entity = (await GetLogEntities()).First();
        //    var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    Assert.Equal(2, key.Length);
        //    Assert.Equal(19, key[0].Length);
        //}

        [Fact]
        public async Task IncludeLevelInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${level}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = (await GetLogEntities()).First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(LogLevel.Info.ToString(), key[0]);
        }

        [Fact]
        public async Task IncludeLevelUppercaseInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${level:uppercase=true}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = (await GetLogEntities()).First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(LogLevel.Info.ToString().ToUpper(), key[0]);
        }

        [Fact]
        public async Task IncludeMachineInRowKey()
        {
            // configure keys
            var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            target.RowKey = "${machinename}__${guid}";
            LogManager.ReconfigExistingLoggers();

            // log something
            _logger.Log(LogLevel.Info, "information");

            // extract the key and assert
            var entity = (await GetLogEntities()).First();
            var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, key.Length);
            Assert.Equal(Environment.MachineName, key[0]);
        }

        //[Fact]
        //public async Task IncludeMicrosInRowKey()
        //{
        //    // configure keys
        //    var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
        //    target.RowKey = "${micros}__${guid}";
        //    LogManager.ReconfigExistingLoggers();

        //    // log something
        //    _logger.Log(LogLevel.Info, "information");

        //    // extract the key and assert
        //    var entity = (await GetLogEntities()).First();
        //    var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    Assert.Equal(2, key.Length);
        //    Assert.Equal(6, key[0].Length);
        //}

        //[Fact]
        //public async Task IncludeTicksAndLongDateInRowKey()
        //{
        //    // configure keys
        //    var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
        //    target.RowKey = "${ticks}__${longdate}";
        //    LogManager.ReconfigExistingLoggers();

        //    // log something
        //    _logger.Log(LogLevel.Info, "information");

        //    // extract the key and assert
        //    var entity = (await GetLogEntities()).First();
        //    var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    Assert.Equal(key.Length, 2);
        //    Assert.Equal(key[0].Length, 19);
        //    Assert.Equal(key[1].Length, 20);
        //}

        //[Fact]
        //public async Task IncludeTimeAndGuidInRowKey()
        //{
        //    // configure keys
        //    var target = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
        //    target.RowKey = "${time}__${guid}";
        //    LogManager.ReconfigExistingLoggers();

        //    // log something
        //    _logger.Log(LogLevel.Info, "information");            

        //    // extract the key and assert
        //    var entity = (await GetLogEntities()).First();
        //    var key = entity.RowKey.Split("__".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //    Assert.Equal(key.Length, 2);
        //    Assert.Equal(6, key[0].Length);
        //    Assert.Equal(32, key[1].Length);
        //}

        private string GetConnectionString()
        {
            return CloudConfigurationManager.GetSetting("ConnectionString");
        }

        private async Task<List<LogEntity>> GetLogEntities()
        {
            await Task.Delay(250);
            var query = new TableQuery<LogEntity>();
            var entities = _cloudTable.ExecuteQuery(query);
            return entities.ToList();
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