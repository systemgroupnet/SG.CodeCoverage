using System;
using System.Collections.Generic;
using System.Text;

namespace SG.CodeCoverage.Recorder
{
    public static class InjectedConstants
    {
        /// <summary>
        /// Total number of instrumented classes.
        /// The value of this field will be modified by instrumenter.
        /// </summary>
        public static int ClassCount = 0;
        /// <summary>
        /// The port number that `RecordingControllerServer` will listen on. It will be modified by instrumenter.
        /// </summary>
        public static int ControllerServerPort = 54321;
        public static string WorkingDirectory = "";
        public static string RecorderLogFileName = "CodeCoverageRecorderLog.txt";
    }
}
