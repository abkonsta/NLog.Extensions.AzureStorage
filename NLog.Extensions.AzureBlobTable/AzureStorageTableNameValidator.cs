using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLog.Extensions.AzureTableStorage
{
    /// <summary>
    /// Storage table name validator.
    /// Validation rules described in: http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
    /// </summary>
    public class AzureStorageTableNameValidator
    {
        #region Constants
        private const string RegularExpression = @"^[A-Za-z][A-Za-z0-9]{2,62}$";
        #endregion Constants

        #region Fields
        private readonly string _tableName;
        private readonly List<string> _reservedWords;
        #endregion Fields

        #region Properties
        #endregion Properties

        #region Constructors
        public AzureStorageTableNameValidator(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new NullReferenceException(tableName);
            }
            _tableName = tableName;
            _reservedWords = new List<string> { "tables" };
        }
        #endregion Constructors

        #region Methods
        public bool IsValid()
        {
            return !_reservedWords.Contains(_tableName) 
                && Regex.IsMatch(_tableName, RegularExpression);
        }
        #endregion Methods
    }
}
