using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    public struct SampleStruct
    {
        public int Value { get; }

        public SampleStruct(int value)
        {
            Value = value;
        }

        public int Multiply(int n)
            => Value * n;

        public double Divide(int n)
            => (double)Value / n;
    }
}
