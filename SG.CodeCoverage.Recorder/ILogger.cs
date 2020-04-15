using System;
using System.Collections.Generic;
using System.Text;

namespace SG.CodeCoverage.Recorder
{
    public interface ILogger
    {
        void LogVerbose(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        bool IsEnabled { get; }
    }
}
