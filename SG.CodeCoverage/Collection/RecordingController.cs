using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Collection
{
    public static class RecordingController
    {
        public static IRecordingController ForRuntimeConfigFile(string runtimeConfigFilePath, InstrumentationMap map)
        {
            return new MultiRecordingController(runtimeConfigFilePath, map);
        }

        public static IRecordingController ForEndPoint(string host, int port, InstrumentationMap map)
        {
            return new SingleRecordingController(host, port, map);
        }
    }
}
