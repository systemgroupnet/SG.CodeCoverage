using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var e = new EmptyClass();
            if(!(e is IComparable))
                Console.WriteLine(new PrimeCalculator().IsPrime(44533));
            foreach (var s in StaticClassWithStaticConstructor.YeildValues())
                Console.WriteLine(s);
            var data = new ClassWithConstructorAndProperties(10, "hello");
            Console.WriteLine(data);
        }
    }
}
