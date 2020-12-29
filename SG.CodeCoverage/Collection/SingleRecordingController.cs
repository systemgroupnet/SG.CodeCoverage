using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Collection
{
    internal class SingleRecordingController : IRecordingController
    {
        private readonly string _host;
        private readonly int _port;
        private readonly InstrumentationMap _map;

        public SingleRecordingController(string host, int port, InstrumentationMap map)
        {
            _host = host;
            _port = port;
            _map = map;
        }

        public CoverageResult CollectResultAndReset()
        {
            return RecordingControllerClient.CollectResultAndReset(_host, _port, _map);
        }

        public void ResetHits()
        {
            RecordingControllerClient.ResetHits(_host, _port);
        }
    }
}
