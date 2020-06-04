using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using SG.CodeCoverage.Recorder;
using SG.CodeCoverage.Metadata;
using System;

namespace SG.CodeCoverage.Collection
{
    public class DataCollector
    {
        private readonly InstrumentationMap _map;

        public DataCollector(string mapFilePath)
        {
            _map = LoadMapFile(mapFilePath);
        }

        public DataCollector(InstrumentationMap map)
        {
            _map = map;
        }

        public static InstrumentationMap LoadMapFile(string mapFilePath)
        {
            ValidateFilePath(mapFilePath);

            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var reader = new JsonTextReader(new StreamReader(mapFilePath)))
                return serializer.Deserialize<InstrumentationMap>(reader);
        }

        public (Guid uniqueId, int[][] hits) LoadHits(string hitsFile)
        {
            ValidateFilePath(hitsFile);
            return HitsRepository.LoadHits(hitsFile);
        }

        public ISet<string> GetVisitedFiles(string hitsFile)
        {
            var (uniqueId, hits) = LoadHits(hitsFile);
            return GetVisitedFiles(uniqueId, hits);
        }

        public ISet<string> GetVisitedMethods(string hitsFile)
        {
            var (uniqueId, hits) = LoadHits(hitsFile);
            return GetVisitedMethods(uniqueId, hits);
        }

        public ISet<string> GetVisitedFiles(Guid uniqueId, int[][] hits)
        {
            if (uniqueId != _map.UniqueId)
                throw new Exception($"Hits file's unique id ({uniqueId}) does not match the unique id in the map file ({_map.UniqueId}).");

            var typeIdToSourceMapper = _map.Assemblies.Select(asm => asm.Types)
                .SelectMany(x => x)
                .ToDictionary(
                    t => t.Index,
                    t => t.Methods.ToDictionary(m => m.Index, m => m.Source));

            var result = new HashSet<string>();
            for (int typeId = 0; typeId < hits.Length; typeId++)
            {
                var typeHits = hits[typeId];
                for (int methodId = 0; methodId < typeHits.Length; methodId++)
                {
                    var hitCount = typeHits[methodId];
                    if (hitCount > 0)
                    {
                        var source = typeIdToSourceMapper[typeId][methodId];
                        result.Add(source);
                    }
                }
            }
            return result;
        }

        private ISet<string> GetVisitedMethods(Guid uniqueId, int[][] hits)
        {
            if (uniqueId != _map.UniqueId)
                throw new Exception($"Hits file's unique id ({uniqueId}) does not match the unique id in the map file ({_map.UniqueId}).");

            var typeMethodsDict = _map.Assemblies.Select(asm => asm.Types)
                .SelectMany(x => x)
                .ToDictionary(
                    t => t.Index,
                    t => t.Methods.ToDictionary(m => m.Index));

            var result = new HashSet<string>();
            for (int typeId = 0; typeId < hits.Length; typeId++)
            {
                var typeHits = hits[typeId];
                for (int methodId = 0; methodId < typeHits.Length; methodId++)
                {
                    var hitCount = typeHits[methodId];
                    if (hitCount > 0)
                    {
                        var methodName = typeMethodsDict[typeId][methodId].FullName;
                        result.Add(methodName);
                    }
                }
            }
            return result;
        }

        public static void ValidateFilePath(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Could not find the file '{file}'.");
        }

    }
}
