using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using AssembliesMap = System.Collections.Generic.IEnumerable<SG.CodeCoverage.Map.Assembly>;
using System.Linq;

namespace SG.CodeCoverage.Collection
{
    public class DataCollector
    {
        public string MapFilePath { get; }

        public string HitsFilePath { get; }

        public DataCollector(string mapFilePath, string hitsFilePath)
        {
            MapFilePath = mapFilePath;
            HitsFilePath = hitsFilePath;
        }

        public IEnumerable<string> GetVisitedFiles()
        {
            ValidiatePaths();
            var rawJson = File.ReadAllText(MapFilePath);
            var assemblies = JsonConvert.DeserializeObject<AssembliesMap>(rawJson);

            var typeIdToSourceMapper = assemblies.Select(asm => asm.Types)
                .SelectMany(x => x)
                .ToDictionary(
                    t => t.Index,
                    t => t.Methods.ToDictionary(m => m.Index, m => m.Source));

            var result = new HashSet<string>();
            using (var fs = new FileStream(HitsFilePath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                var typesCount = br.ReadInt32();
                for (int typeId = 0;  typeId < typesCount; typeId++)
                {
                    var methodsCount = br.ReadInt32();
                    for (int j = 0; j < methodsCount; j++)
                    {
                        var methodId = br.ReadInt32();
                        var source = typeIdToSourceMapper[typeId][methodId];
                        if (result.Add(source)) yield return source;
                    }
                }
            }
        }

        public void ValidiatePaths()
        {
            if (!File.Exists(MapFilePath))
                throw new FileNotFoundException("Could not find the map file.");

            if (!File.Exists(HitsFilePath))
                throw new FileNotFoundException("Could not find the hits file.");
        }

    }
}
