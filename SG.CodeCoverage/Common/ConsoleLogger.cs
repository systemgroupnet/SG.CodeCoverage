using System;
using System.Collections.Generic;
using System.Text;

namespace SG.CodeCoverage.Common
{
    public class ConsoleLogger : ILogger
    {
        private static string GetTimeStamp()
            => DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss.fff");

        bool ILogger.IsEnabled => true;
        public void LogError(string message) => Log(message);
        public void LogInformation(string message) => Log(message);
        public void LogVerbose(string message) => Log(message);
        public void LogWarning(string message) => Log(message);

        private void Log(string message)
        {
            Console.WriteLine(GetTimeStamp() + " " + message);
        }
    }
}
