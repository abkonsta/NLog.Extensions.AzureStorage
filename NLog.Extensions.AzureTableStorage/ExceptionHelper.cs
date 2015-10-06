using System;
using System.Threading;

namespace NLog.Extensions.AzureTableStorage
{
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the exception must be rethrown RIGHT AWAY.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>True if the exception must be rethrown, false otherwise.</returns>
        public static bool MustBeRethrown(this Exception exception)
        {
            if (exception is StackOverflowException)
            {
                return true;
            }

            if (exception is ThreadAbortException)
            {
                return true;
            }

            if (exception is OutOfMemoryException)
            {
                return true;
            }

            if (exception is NLogConfigurationException)
            {
                return true;
            }

            if (exception.GetType().IsSubclassOf(typeof(NLogConfigurationException)))
            {
                return true;
            }

            return false;
        }
    }
}
