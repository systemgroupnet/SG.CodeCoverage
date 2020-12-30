using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Collection
{
    internal class FixedPortRecordingController : IRecordingController
    {
        private readonly string _host;
        private readonly int _port;
        private readonly InstrumentationMap _map;
        private readonly ILogger _logger;

        public FixedPortRecordingController(string host, int port, InstrumentationMap map, ILogger logger = null)
        {
            _host = host;
            _port = port;
            _map = map;
            _logger = logger ?? new ConsoleLogger();
        }

        public InstrumentationMap InstrumentationMap => _map;

        public CoverageResult CollectResultAndReset()
        {
            _logger.LogInformation($"Collecting coverage result for {_host}:{_port}");
            return RecordingControllerClient.CollectResultAndReset(_host, _port, _map);
        }

        public void ResetHits()
        {
            _logger.LogInformation($"Resetting hits for {_host}:{_port}");
            RecordingControllerClient.ResetHits(_host, _port);
        }
    }
}
