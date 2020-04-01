using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NLog.Extensions.AzureTableStorage.Tests
{
    public class AzureTableStorageTargetTests : IDisposable
    {
        private readonly Logger _logger;
        private readonly CloudTable _cloudTable;
        private const string TargetTableName = "AzureTableStorageTargetTests"; //must match table name in AzureTableStorage target in NLog.config

        public AzureTableStorageTargetTests()
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

        [Fact]
        public void CanReconfigureOnTheFly()
        {
            var azureStorageTarget = (AzureTableStorageTarget)LogManager.Configuration.FindTargetByName(TargetTableName);
            azureStorageTarget.ConnectionString = "yo";
            LogManager.ReconfigExistingLoggers();
        }

        [Fact]
        public void CanLogInformation()
        {
            var message = "information";

            _logger.Log(LogLevel.Info, message);

            var entities = GetLogEntities();
            Assert.Equal(1, entities.Count);
            var entity = entities.Single();
            Assert.Equal(message, entity.Message);
            Assert.Equal("Info", entity.Level);
            Assert.Equal(entity.LoggerName, TargetTableName);
        }

        [Fact]
        public void CanLogExceptions()
        {
            var message = "exception message";

            _logger.Log(LogLevel.Error, new NullReferenceException(), message);

            var entities = GetLogEntities();
            Assert.Equal(1, entities.Count);
            var entity = entities.Single();
            Assert.Equal(message, entity.Message);
            Assert.Equal("Error", entity.Level);
            Assert.Equal(entity.LoggerName, TargetTableName);
            Assert.NotNull(entity.Exception);
        }

        [Fact]
        public void IncludeExceptionFormattedMessengerInLoggedRow()
        {
            _logger.Debug("exception message {0} and {1}.", 2010, 2014);

            var entity = GetLogEntities().Single();

            Assert.Equal("exception message 2010 and 2014.", entity.Message);
        }

        [Fact]
        public void IncludeExceptionDataInLoggedRow()
        {
            var exception = new NullReferenceException();
            var errorId = Guid.NewGuid();
            exception.Data["id"] = errorId;
            exception.Data["name"] = "ahmed";

            _logger.Log(LogLevel.Error, exception, "exception message");

            var entities = GetLogEntities();
            var entity = entities.Single();
            Assert.True(entity.ExceptionData.Contains(errorId.ToString()));
            Assert.True(entity.ExceptionData.Contains("name=ahmed"));
        }

        [Fact]
        public void IncludeExceptionDetailsInLoggedRow()
        {
            var exception = new NullReferenceException();

            _logger.Log(LogLevel.Error, exception, "exception message");

            var entity = GetLogEntities().Single();
            Assert.NotNull(entity.Exception);
            Assert.Equal(exception.ToString().ExceptBlanks(), entity.Exception.ExceptBlanks());
        }

        [Fact]
        public void IncludeInnerExceptionDetailsInLoggedRow()
        {
            var message = "exception message";
            var exception = new NullReferenceException(message, new DivideByZeroException());

            _logger.Log(LogLevel.Error, exception, message);

            var entity = GetLogEntities().Single();
            Assert.NotNull(entity.Exception);
            Assert.Equal(exception.ToString().ExceptBlanks(), entity.Exception.ExceptBlanks());
            Assert.NotNull(entity.InnerException);
            Assert.Equal(exception.InnerException.ToString().ExceptBlanks(), entity.InnerException.ExceptBlanks());
        }

        [Fact]
        public void IncludePrefixInPartitionKey()
        {
            var exception = new NullReferenceException();

            _logger.Log(LogLevel.Error, exception, "exception message");

            var entity = GetLogEntities().Single();
            Assert.True(entity.PartitionKey.Contains("Test"));
        }

        [Fact]
        public void IncludeGuidAndTimeInRowKey()
        {
            var exception = new NullReferenceException();

            _logger.Log(LogLevel.Error, exception, "exception message");

            var entity = GetLogEntities().Single();
            const string splitter = "__";
            Assert.True(entity.RowKey.Contains(splitter));
            var splitterArray = "__".ToCharArray();
            var segments = entity.RowKey.Split(splitterArray, StringSplitOptions.RemoveEmptyEntries);
            long timeComponent;
            Assert.True(segments[1].Length == 32);
            Assert.True(long.TryParse(segments[0], out timeComponent));
        }

        [Fact]
        public void IncludeMachineName()
        {
            var exception = new NullReferenceException();

            _logger.Log(LogLevel.Error, exception, "exception message");

            var entity = GetLogEntities().Single();
            Assert.Equal(entity.MachineName, Environment.MachineName);
        }

        private CloudStorageAccount GetStorageAccount()
        {
            var connectionString = CloudConfigurationManager.GetSetting("ConnectionString"); 
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
    }
}
