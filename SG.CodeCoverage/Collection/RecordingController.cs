using SG.CodeCoverage.Common;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Collection
{
    public static class RecordingController
    {
        public static IRecordingController ForRuntimeConfigFile(string runtimeConfigFilePath, InstrumentationMap map, ILogger logger)
        {
            return new MultiRecordingController(runtimeConfigFilePath, map, logger);
        }

        public static IRecordingController ForEndPoint(string host, int port, InstrumentationMap map, ILogger logger)
        {
            return new SingleRecordingController(host, port, map, logger);
        }
    }
}
