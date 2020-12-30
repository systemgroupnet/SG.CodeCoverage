using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleProjectForTest
{
    public class ClassWithStaticInitializer
    {
        public static readonly Lazy<string> AssemblyLocation =
            new Lazy<string>(() => typeof(ClassWithStaticInitializer).Assembly.Location);

        public ClassWithStaticInitializer()
        {
            Console.WriteLine("constructor");
        }

        public void PrintAssemblyLocation()
        {
            Console.WriteLine(AssemblyLocation.Value);
        }

        public void PrintAnotherValueAndWait(string value)
        {
            Console.WriteLine("This is another value: " + value);
            Console.ReadKey();
        }

        public void EmptyMethod()
        {
        }
    }
}
