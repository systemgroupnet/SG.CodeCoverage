using System.Collections.Generic;

namespace SG.CodeCoverage.Metadata
{
    public class InstrumentedAssemblyMap
    {
        public InstrumentedAssemblyMap(string name, IReadOnlyCollection<InstrumentedTypeMap> types)
        {
            Name = name;
            Types = types;
        }

        public string Name { get; }
        public IReadOnlyCollection<InstrumentedTypeMap> Types { get; }
    }
}
