using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace NLog.Extensions.AzureTableStorage
{
    /// <summary>
    /// Config manager
    /// </summary>
    public class ConfigManager
    {
        #region Constants
        #endregion Constants

        #region Fields
        private readonly string _connectionString;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        #endregion Constructors

        #region Methods
        public ConfigManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(_connectionString);
        }
        #endregion Methods
    }
}
