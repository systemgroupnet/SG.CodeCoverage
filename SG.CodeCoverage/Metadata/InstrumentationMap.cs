using Newtonsoft.Json;
using SG.CodeCoverage.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace SG.CodeCoverage.Metadata
{
    public partial class InstrumentationMap
    {
        public InstrumentationMap(VersionInfo version, Guid uniqueId, IReadOnlyCollection<InstrumentedAssemblyMap> assemblies)
        {
            Version = version;
            UniqueId = uniqueId;
            Assemblies = assemblies;
        }

        public VersionInfo Version { get; }
        public Guid UniqueId { get; }
        public IReadOnlyCollection<InstrumentedAssemblyMap> Assemblies { get; }

        public static InstrumentationMap Parse(string mapFilePath)
        {
            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var reader = new JsonTextReader(new StreamReader(mapFilePath)))
                return serializer.Deserialize<InstrumentationMap>(reader);
        }
    }
}
