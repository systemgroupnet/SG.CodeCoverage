using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    public static class StaticClassWithStaticConstructor
    {
        static readonly IReadOnlyCollection<string> _values;

        static StaticClassWithStaticConstructor()
        {
            _values = new List<string>() { "Val1", "Val2" }.AsReadOnly();
        }

        public static IEnumerable<string> GetValues()
        {
            return _values;
        }

        public static IEnumerable<string> YeildValues()
        {
            if (_values == null)
                throw new InvalidOperationException("Values not initialized.");

            foreach(var val in _values)
            {
                yield return val;
            }
        }
    }
}
