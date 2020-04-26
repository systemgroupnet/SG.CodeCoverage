using SG.CodeCoverage.Common;
using System;
using System.Collections.Generic;

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
    }
}
