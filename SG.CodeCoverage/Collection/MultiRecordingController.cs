using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;
using SG.CodeCoverage.Recorder.RecordingController;
using System;
using System.Collections.Generic;
using System.IO;

namespace SG.CodeCoverage.Collection
{
    internal class MultiRecordingController : IRecordingController
    {
        private readonly string _recorderRuntimeConfigFilePath;
        private readonly InstrumentationMap _map;
        private DateTime _currentRuntimeConfigFileLastUpdate;
        private RuntimeConfig _currentRuntimeConfig;
        private const string _host = "localhost";

        public MultiRecordingController(string recorderRuntimeConfigFilePath, InstrumentationMap map)
        {
            _recorderRuntimeConfigFilePath = recorderRuntimeConfigFilePath;
            _map = map;
        }

        public void ResetHits()
        {
            if (!LoadRuntimeConfig())
                return;
            foreach(var process in _currentRuntimeConfig.Processes)
            {
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
                return false;
            }

            var newUpdateDate = File.GetLastWriteTime(_recorderRuntimeConfigFilePath);
            if (_currentRuntimeConfig == null ||
                newUpdateDate > _currentRuntimeConfigFileLastUpdate)
            {
                _currentRuntimeConfig = RuntimeConfig.Load(_recorderRuntimeConfigFilePath);
                _currentRuntimeConfigFileLastUpdate = newUpdateDate;
            }
            return true;
        }
    }
}
