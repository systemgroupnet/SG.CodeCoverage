using System.Collections.Generic;

namespace SG.CodeCoverage.Map
{
    public class Type
    {
        public string FullName { get; set; }
        public int Index { get; set; }
        public List<Method> Methods { get; set; } = new List<Method>();
    }
}
