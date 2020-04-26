namespace SG.CodeCoverage.Recorder
{
    public static class InjectedConstants
    {
        public static readonly string InstrumentationUniqueId = string.Empty;
        /// <summary>
        /// Total number of instrumented types.
        /// The value of this field will be modified by instrumenter.
        /// </summary>
        public static readonly int TypesCount = 0;
        /// <summary>
        /// The port number that `RecordingControllerServer` will listen on. It will be modified by instrumenter.
        /// </summary>
        public static readonly int ControllerServerPort = 54321;
        public static readonly string WorkingDirectory = string.Empty;
        public static readonly string RecorderLogFileName = "CodeCoverageRecorderLog.txt";
    }
}
