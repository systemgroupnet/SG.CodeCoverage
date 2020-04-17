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
            var sqrt = Math.Sqrt(number);
            if (number < 2)
                return false;
            if (number == 2)
                return true;
            for (int i = 3; i <= sqrt; i++)
                if (number % i == 0)
                    return false;
            return true;
        }
    }
}
