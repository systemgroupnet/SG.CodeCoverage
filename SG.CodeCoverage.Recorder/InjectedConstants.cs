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
        /// The port number that `RecordingController.Server` will listen on. It will be modified by the instrumenter.
        /// If set to 0, a random port is selected every time the server starts. The chosen port will be written to
        /// RuntimeConfig file.
        /// </summary>
        public static readonly int ControllerServerPort = -1;
        public static readonly string RecorderLogFileName = "CodeCoverageRecorder.log";
        /// <summary>
        /// The name of the file that stores listening port(s) for Recording Controller Server instance(s).
        /// This file will be created/filled by Server when it starts listening.
        /// </summary>
        public static readonly string RuntimeConfigFileName = "CodeCoverageRecorderRuntimeConfig.cfg";
        /// <summary>
        /// The path to store the RuntimeConfig file. If the path is null or empty, the file
        /// will be stored in the path that the Recorder assembly (this assembly) is loaded from.
        /// </summary>
        public static readonly string RuntimeConfigOutputPath = "";
    }
}
