using SG.CodeCoverage.Common;
using System;
using System.Collections.Generic;

namespace SG.CodeCoverage.Metadata
{
    public partial class InstrumentationMap
    {
        public InstrumentationMap(VersionInfo version, IReadOnlyCollection<InstrumentedAssemblyMap> assemblies)
        {
            Version = version;
            Assemblies = assemblies;
        }

        public VersionInfo Version { get; }
        public IReadOnlyCollection<InstrumentedAssemblyMap> Assemblies { get; }
    }
}
