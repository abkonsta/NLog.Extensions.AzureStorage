using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Diagnostics;

namespace NLog.Extensions.AzureTableStorage
{
    /// <summary>
    /// This class represents a target in the NLog.config file.
    /// </summary>
    [Target("AzureTableStorage")]
    public class AzureTableStorageTarget : TargetWithLayout
    {
        #region Constants
        #endregion Constants

        #region Fields
        private TableStorageManager _tableStorageManager;
        #endregion Fields

        #region Properties
        [RequiredParameter]
        public string ConnectionString { get; set; }

        [RequiredParameter]
        public string TableName { get; set; }

        [RequiredParameter]
        public string PartitionKey { get; set; }

        [RequiredParameter]
        public string RowKey { get; set; }
        #endregion Properties

        #region Constructors
        #endregion Constructors

        #region Methods
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            ValidateParameters();

            _tableStorageManager = new TableStorageManager(ConnectionString, TableName);

            if (string.IsNullOrWhiteSpace(PartitionKey))
                PartitionKey = "${date}";

            if (string.IsNullOrWhiteSpace(RowKey))
                PartitionKey = "${ticks}.${guid}";
        }

        private void ValidateParameters()
        {
            IsNameValidForTableStorage(TableName);
        }

        private void IsNameValidForTableStorage(string tableName)
        {
            var validator = new AzureStorageTableNameValidator(tableName);
            if (!validator.IsValid())
            {
                throw new NotSupportedException(tableName + " is not a valid name for Azure storage table name.")
                {
                    HelpLink = "http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx"
                };
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                WriteToDatabase(logEvent);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                    throw;

                Trace.TraceError($"NLog.Extensions.AzureTableStorage.Write() error: {exception}");
                InternalLogger.Error($"Error writing to azure storage table: {exception}");
            }
        }

        private void WriteToDatabase(LogEventInfo logEvent)
        {
            _tableStorageManager.EnsureConfigurationIsCurrent(ConnectionString, TableName);

            Layout rowKeyLayout = RowKey;
            var rowKey = rowKeyLayout.Render(logEvent);

            _tableStorageManager.Add(new LogEntity(PartitionKey, rowKey, logEvent, Layout.Render(logEvent)));
        }
        #endregion Methods
    }
}
