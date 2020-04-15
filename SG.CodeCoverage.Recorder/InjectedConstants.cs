using System;
using System.Collections.Generic;
using System.Text;

namespace SG.CodeCoverage.Recorder
{
    public static class InjectedConstants
    {
        /// <summary>
        /// Total number of instrumented types.
        /// The value of this field will be modified by instrumenter.
        /// </summary>
        public static int TypeCount = 0;
        /// <summary>
        /// The port number that `RecordingControllerServer` will listen on. It will be modified by instrumenter.
        /// </summary>
        public static int ControllerServerPort = 54321;
        public static string WorkingDirectory = string.Empty;
        public static string RecorderLogFileName = "CodeCoverageRecorderLog.txt";
    }
}
