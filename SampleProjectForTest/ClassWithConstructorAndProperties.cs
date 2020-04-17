using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    class ClassWithConstructorAndProperties
    {
        public ClassWithConstructorAndProperties(int count, string text)
        {
            Count = count;
            Text = text;
        }

        public int Count { get; }
        public string Text { get; }

        public override string ToString()
        {
            return $"Count: {Count}, Text: {Text}";
        }
    }
}
