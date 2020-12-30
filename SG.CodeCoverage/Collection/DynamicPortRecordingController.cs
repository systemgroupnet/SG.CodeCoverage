using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;
using SG.CodeCoverage.Recorder.RecordingController;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SG.CodeCoverage.Collection
{
    internal class DynamicPortRecordingController : IRecordingController
    {
        private readonly string _recorderRuntimeConfigFilePath;
        private readonly InstrumentationMap _map;
        private readonly ILogger _logger;
        private DateTime _currentRuntimeConfigFileLastUpdate;
        private RuntimeConfig _currentRuntimeConfig;
        private const string _host = "localhost";

        public DynamicPortRecordingController(string recorderRuntimeConfigFilePath, InstrumentationMap map, ILogger logger = null)
        {
            _recorderRuntimeConfigFilePath = recorderRuntimeConfigFilePath;
            _map = map;
            _logger = logger ?? new ConsoleLogger();
        }

        public InstrumentationMap InstrumentationMap => _map;

        public void ResetHits()
        {
            if (!LoadRuntimeConfig())
                return;
            foreach(var process in _currentRuntimeConfig.Processes)
            {
                _logger.LogInformation($"Resetting hits for process {process.ID} on port {process.ListeningPort}");
                RecordingControllerClient.ResetHits(_host, process.ListeningPort);
            }
        }

        public CoverageResult CollectResultAndReset()
        {
            if (!LoadRuntimeConfig())
                return new CoverageResult(_map, Array.Empty<int[]>());
            CoverageResult result = null;
            foreach(var process in _currentRuntimeConfig.Processes)
            {
                _logger.LogInformation($"Collecting coverage result from process {process.ID} on port {process.ListeningPort}");
                var procResult = RecordingControllerClient.CollectResultAndReset(_host, process.ListeningPort, _map);
                if (result == null)
                    result = procResult;
                else
                    result = result.MergeWith(procResult);
            }
            return result;
        }

        private bool LoadRuntimeConfig()
        {
            if(!File.Exists(_recorderRuntimeConfigFilePath))
            {
                _currentRuntimeConfigFileLastUpdate = default;
                _currentRuntimeConfig = null;
                _logger.LogWarning($"Runtime Config file '{_recorderRuntimeConfigFilePath}' not found.");
                return false;
            }

            var newUpdateDate = File.GetLastWriteTime(_recorderRuntimeConfigFilePath);
            if (_currentRuntimeConfig == null ||
                newUpdateDate > _currentRuntimeConfigFileLastUpdate)
            {
                _logger.LogInformation($"Loading Runtime Config file '{_recorderRuntimeConfigFilePath}'");
                _currentRuntimeConfig = RuntimeConfig.Load(_recorderRuntimeConfigFilePath);
                _currentRuntimeConfigFileLastUpdate = newUpdateDate;
                _logger.LogVerbose($"Runtime Config file loaded. " +
                                   $"Processes: {string.Join(", ", _currentRuntimeConfig.Processes.Select(p => p.ID))}" +
                                   $" - Last update: {newUpdateDate:O}");
            }
            return true;
        }
    }
}
