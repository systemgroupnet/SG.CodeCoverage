using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using AssemblyMaps = System.Collections.Generic.IEnumerable<SG.CodeCoverage.Map.Assembly>;
using System.Linq;

namespace SG.CodeCoverage.Collection
{
    public class DataCollector
    {
        public string HitsFilePath { get; }
        private AssemblyMaps _assemblyMaps;

        public DataCollector(string mapFilePath, string hitsFilePath)
        {
            HitsFilePath = hitsFilePath;
            _assemblyMaps = LoadMapFile(mapFilePath);
            ValidateHitsFilePath();
        }

        public DataCollector(AssemblyMaps assemblyMaps, string hitsFilePath)
        {
            HitsFilePath = hitsFilePath;
            _assemblyMaps = assemblyMaps;
            ValidateHitsFilePath();
        }

        private AssemblyMaps LoadMapFile(string mapFilePath)
        {
            if (!File.Exists(mapFilePath))
                throw new FileNotFoundException("Could not find the map file.");

            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var reader = new JsonTextReader(new StreamReader(mapFilePath)))
                return serializer.Deserialize<AssemblyMaps>(reader);
        }

        public HashSet<string> GetVisitedFiles()
        {
            var result = new HashSet<string>();

            using (var fs = new FileStream(HitsFilePath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {

                var typeIdToSourceMapper = _assemblyMaps.Select(asm => asm.Types)
                    .SelectMany(x => x)
                    .ToDictionary(
                        t => t.Index,
                        t => t.Methods.ToDictionary(m => m.Index, m => m.Source));

                var typesCount = br.ReadInt32();
                for (int typeId = 0; typeId < typesCount; typeId++)
                {
                    var methodsCount = br.ReadInt32();
                    for (int j = 0; j < methodsCount; j++)
                    {
                        var methodId = br.ReadInt32();
                        var source = typeIdToSourceMapper[typeId][methodId];
                        result.Add(source);
                    }
                }
            }

            return result;
        }

        public void ValidateHitsFilePath()
        {
            if (!File.Exists(HitsFilePath))
                throw new FileNotFoundException("Could not find the hits file.");
        }

    }
}
