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
        public void LogError(string message) => Log(ConsoleColor.Red, message);
        public void LogInformation(string message) => Log(ConsoleColor.Green, message);
        public void LogVerbose(string message) => Log(ConsoleColor.DarkGray, message);
        public void LogWarning(string message) => Log(ConsoleColor.Yellow, message);

        private void Log(ConsoleColor color, string message)
        {
            var prevColor = Console.ForegroundColor;
            try
            {
                Console.Write(GetTimeStamp() + " | ");
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = prevColor;
            }
        }
    }
}
