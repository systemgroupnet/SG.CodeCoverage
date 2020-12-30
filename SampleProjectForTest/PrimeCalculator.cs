using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    public class PrimeCalculator
    {
        public bool IsPrime(long number)
        {
            if (IsLessThan2(number))
                return false;
            if (number == 2)
                return true;
            var up = GetUpperBound(number);
            for (int i = 2; i <= up; i++)
                if (number % i == 0)
                    return false;

            var value = new SampleStruct(4);
            value.Multiply(3);

            return true;
        }

        private static bool IsLessThan2(long number)
        {
            return number < 2;
        }

        private long GetUpperBound(long number)
        {
            return (long)Math.Ceiling(Math.Sqrt(number));
        }
    }
}
