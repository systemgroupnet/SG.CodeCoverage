using System.Collections.Generic;

namespace SG.CodeCoverage.Metadata
{
    public class InstrumentedTypeMap
    {
        public InstrumentedTypeMap(string fullName, int index, IReadOnlyCollection<InstrumentedMethodMap> methods)
        {
            FullName = fullName;
            Index = index;
            Methods = methods;
        }

        public string FullName { get; }
        public int Index { get; }
        public IReadOnlyCollection<InstrumentedMethodMap> Methods { get; }
    }
}
