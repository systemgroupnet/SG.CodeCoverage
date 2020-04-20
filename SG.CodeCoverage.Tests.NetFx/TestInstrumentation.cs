using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Tests.NetFx
{
    [TestClass]
    public class TestInstrumentation
    {
        [TestMethod]
        public void TestSampleProjectInstrumented()
        {
            var tester = new InstrumenterTester();
            tester.InstrumentSampleProject();
        }
    }
}
