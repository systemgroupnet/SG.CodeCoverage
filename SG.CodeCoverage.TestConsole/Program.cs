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
            new TestInstrumenter().TestSampleProjectInstrumented();
        }
    }
}
