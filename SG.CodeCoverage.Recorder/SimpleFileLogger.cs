using System;
using System.IO;

namespace SG.CodeCoverage.Recorder
{
    public class SimpleFileLogger : ILogger
    {
        private readonly string _logFileName;

        public SimpleFileLogger(string logFileName)
        {
            _logFileName = logFileName;
        }

        private static string GetTimeStamp()
            => DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss.fff");

        bool ILogger.IsEnabled => true;
        public void LogError(string message) => Log(message);
        public void LogInformation(string message) => Log(message);
        public void LogVerbose(string message) => Log(message);
        public void LogWarning(string message) => Log(message);

        private void Log(string message)
        {
            File.WriteAllText(_logFileName, GetTimeStamp() + " " + message);
        }
    }
}
