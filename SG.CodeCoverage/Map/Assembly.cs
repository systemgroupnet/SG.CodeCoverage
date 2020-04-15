using System.Collections.Generic;

namespace SG.CodeCoverage.Map
{
    public class Assembly
    {
        public string Name { get; set; }
        public List<Type> Types { get; set; } = new List<Type>();
    }
}
