using Microsoft.WindowsAzure.Storage.Table;

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
        private readonly CloudTable _cloudTable;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        public TableStorageManager(ConfigManager configManager, string tableName)
        {
            var storageAccount = configManager.GetStorageAccount();
            
            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();
            
            //create charts table if not exists.
            _cloudTable = tableClient.GetTableReference(tableName);
            _cloudTable.CreateIfNotExists();
        }
        #endregion Constructors

        #region Methods
        public void Add(LogEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            _cloudTable.Execute(insertOperation);
        }
    }
    #endregion Methods
}
