using SG.CodeCoverage.Instrumentation;
using SG.CodeCoverage.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.TestConsole
{
    class Program
    {
        static void Main()
        {
            var tester = new InstrumenterTester();
            tester.InstrumentSampleProject();
            tester.RunSomeCode();
            var result = tester.GetCoverageResult();
            Console.WriteLine($"Visited {result.GetVisitedSources().Count()} source files" +
                $" and {result.GetVisitedMethods().Count()} methods.");
        }
    }
}
