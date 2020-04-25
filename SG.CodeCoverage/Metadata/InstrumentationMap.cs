using System;
using System.Collections.Generic;

namespace SG.CodeCoverage.Metadata
{
    public partial class InstrumentationMap
    {
        public InstrumentationMap(IReadOnlyCollection<InstrumentedAssemblyMap> assemblies)
        {
            Assemblies = assemblies;
        }

        public IReadOnlyCollection<InstrumentedAssemblyMap> Assemblies { get; }
    }
}
