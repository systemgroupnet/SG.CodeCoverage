using SG.CodeCoverage.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var tester = new InstrumenterTester();
            tester.InstrumentSampleProject();
            tester.RunSomeCode();
            var visitedFiles = tester.GetVisitedFiles();
            Console.WriteLine(visitedFiles.Count);
        }
    }
}
