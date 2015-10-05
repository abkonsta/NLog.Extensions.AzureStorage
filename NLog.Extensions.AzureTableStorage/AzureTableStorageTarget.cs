using System;
using System.ComponentModel.DataAnnotations;
using NLog.Targets;

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
        private ConfigManager _configManager;
        private TableStorageManager _tableStorageManager;
        #endregion Fields

        #region Properties
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string TableName { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }
        #endregion Properties

        #region Constructors
        #endregion Constructors

        #region Methods
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            ValidateParameters();

            _configManager = new ConfigManager(ConnectionString);
            _tableStorageManager = new TableStorageManager(_configManager, TableName);

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
            if (_tableStorageManager != null)
            {
                var layoutMessage = Layout.Render(logEvent);
                _tableStorageManager.Add(new LogEntity(PartitionKey, RowKey, logEvent, layoutMessage));
            }
        }
        #endregion Methods
    }
}
