using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace NLog.Extensions.AzureTableStorage
{
    /// <summary>
    /// This is the entity class that represents the records being written out to Azure Storage
    /// </summary>
    public class LogEntity : TableEntity
    {
        #region Constants

        #endregion Constants

        #region Fields

        private readonly object _syncRoot = new object();
        private static readonly CompareInfo InvariantCompareInfo = CultureInfo.InvariantCulture.CompareInfo;

        #endregion Fields

        #region Properties

        public string LogTimeStamp { get; set; }

        public string Level { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

        public string InnerException { get; set; }

        public string StackTrace { get; set; }

        public string MessageWithLayout { get; set; }

        public string ExceptionData { get; set; }

        public string MachineName { get; set; }

        #endregion Properties

        #region Constructors

        public LogEntity()
        {
        }

        public LogEntity(string partitionKey, string rowKey, LogEventInfo logEvent, string layoutMessage)
        {
            lock (_syncRoot)
            {
                LoggerName = logEvent.LoggerName;
                LogTimeStamp = logEvent.TimeStamp.ToString("g");
                Level = logEvent.Level.Name;
                Message = logEvent.FormattedMessage;
                MessageWithLayout = layoutMessage;

                if (logEvent.Exception != null)
                {
                    Exception = logEvent.Exception.ToString();
                    if (logEvent.Exception.Data.Count > 0)
                    {
                        ExceptionData = GetExceptionDataAsString(logEvent.Exception);
                    }
                    if (logEvent.Exception.InnerException != null)
                    {
                        InnerException = logEvent.Exception.InnerException.ToString();
                    }
                }

                if (logEvent.StackTrace != null)
                {
                    StackTrace = logEvent.StackTrace.ToString();
                }

                MachineName = Environment.MachineName;

                PartitionKey = TransformKey(partitionKey, logEvent);
                RowKey = TransformKey(rowKey, logEvent);
            }
        }

        #endregion Constructors

        #region Methods

        private string TransformKey(string key, LogEventInfo logEvent)
        {
            var capacity = key.Length * 3; // Guesstimate of a reasonable maximum length after transform
            var builder = new StringBuilder(key, capacity);

            var date = logEvent.TimeStamp.ToUniversalTime();
            if (InvariantCompareInfo.IndexOf(key, "${date}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${date}", date.ToString("yyyyMMdd"));
            if (InvariantCompareInfo.IndexOf(key, "${time}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${time}", date.ToString("HHmmss"));
            if (InvariantCompareInfo.IndexOf(key, "${ticks}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${ticks}", date.Ticks.ToString("d19"));
            if (InvariantCompareInfo.IndexOf(key, "${longdate}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${longdate}", date.ToString("yyyyMMddHHmmssffffff"));
            if (InvariantCompareInfo.IndexOf(key, "${micros}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${micros}", date.ToString("ffffff"));
            if (InvariantCompareInfo.IndexOf(key, "${descticks}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${descticks}", (DateTime.MaxValue.Ticks - date.Ticks).ToString("d19"));

            if (InvariantCompareInfo.IndexOf(key, "${guid}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${guid}", Guid.NewGuid().ToString("N"));
            if (InvariantCompareInfo.IndexOf(key, "${logger}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${logger}", logEvent.LoggerName);

            var level = logEvent.Level.Name;
            if (InvariantCompareInfo.IndexOf(key, "${level}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${level}", level);
            if (InvariantCompareInfo.IndexOf(key, "${level:uppercase=true}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${level:uppercase=true}", level.ToUpper());

            if (InvariantCompareInfo.IndexOf(key, "${machine}", CompareOptions.Ordinal) >= 0)
                builder.Replace("${machine}", MachineName);

            var gdcRegEx = new Regex(@"\$\{(gdc:)(\w{1,})\}", RegexOptions.Compiled);
            foreach (Match match in gdcRegEx.Matches(key))
            {
                var gdcItem = GlobalDiagnosticsContext.Get(match.Groups[2].Value);
                if (!string.IsNullOrWhiteSpace(gdcItem))
                {
                    builder.Replace(match.Groups[0].Value, gdcItem);
                }
            }

            var result = builder.ToString();
            return result;
        }

        private static string GetExceptionDataAsString(Exception exception)
        {
            var data = new StringBuilder();
            foreach (DictionaryEntry entry in exception.Data)
            {
                data.AppendLine(entry.Key + "=" + entry.Value);
            }
            return data.ToString();
        }

        #endregion Methods
    }
}
