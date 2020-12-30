using Microsoft.VisualStudio.TestTools.UnitTesting;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Metadata;
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
        InstrumenterTester _tester;

        [TestInitialize]
        public void Instrument()
        {
            _tester = new InstrumenterTester();
            _tester.InstrumentSampleProject();
        }

        [TestMethod]
        public void Coverage_Startup()
        {
            _tester.RunApp();
            AssertVisitedFilesAndMethods(Files.Startup, Methods.Startup);
        }

        [TestMethod]
        public void Coverage_IsPrime1()
        {
            _tester.RunApp("IsPrime 1");
            AssertVisitedFilesAndMethods(
                Files.Startup
                    .Concat(Files.PrimeCalculator),
                Methods.Startup
                    .Concat(Methods.RunCommand)
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2));
        }

        [TestMethod]
        public void Coverage_IsPrime4()
        {
            _tester.RunApp("IsPrime 4");
            AssertVisitedFilesAndMethods(
                Files.Startup
                    .Concat(Files.PrimeCalculator),
                Methods.Startup
                    .Concat(Methods.RunCommand)
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2)
                    .Concat(Methods.PrimeCalculator.GetUpperBound));
        }


        [TestCleanup]
        public void ExitApp()
        {
            if (_tester.AppIsRunning)
                _tester.ExitApp();
        }

        private void AssertVisitedFilesAndMethods(
            IEnumerable<string> expectedVisitedFiles,
            IEnumerable<string> expectedVisitedMethods)
        {
            var result = _tester.GetCoverageResult();
            var files = result.GetVisitedSources().ToList();
            var methods = result.GetVisitedMethodNames().ToList();
            ShouldVisit(expectedVisitedFiles.ToList(), files, "file");
            ShouldVisit(expectedVisitedMethods.ToList(), methods, "method");
        }

        private static void ShouldVisit(IReadOnlyList<string> expectedNames, IReadOnlyList<string> actualNames, string what)
        {
            Assert.AreEqual(expectedNames.Count, actualNames.Count, $"visited {what}s");
            foreach(var expected in expectedNames)
            {
                bool found = false;
                foreach(var actual in actualNames)
                {
                    if(actual.EndsWith(expected))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Assert.Fail($"{what} {expected} is not visited.");
            }
        }

        private static class Files
        {
            public static readonly IEnumerable<string> Startup = new string[]
            {
                "App.cs",
                "Program.cs"
            };

            public static readonly IEnumerable<string> PrimeCalculator = new string[]
            {
                "PrimeCalculator.cs"
            };

            public static readonly IEnumerable<string> SampleStruct = new string[]
            {
                "SampleStruct.cs"
            };
        }

        private static class Methods
        {
            public static readonly IEnumerable<string> Startup = new string[]
            {
                "Program::Main(System.String[])",
                "App::.ctor()",
                "App::get_Commands()",
                "App::Run(System.String[])",
                "Command::.ctor(System.String,System.String,System.Action`1<System.String>)",
                "Command::get_Help()",
            };

            public static readonly IEnumerable<string> RunCommand = new string[]
            {
                "App::RunCommand(System.String[])",
                "Command::get_CommandText()",
                "Command::get_Operation()",
            };

            public static class PrimeCalculator
            {
                public static readonly IEnumerable<string> IsPrime = new string[]
                {
                    "App::IsPrime(System.String)",
                    "PrimeCalculator::IsPrime(System.Int64)",
                };
                public static readonly IEnumerable<string> IsLessThan2 = new string[]
                {
                    "PrimeCalculator::IsLessThan2(System.Int64)"
                };
                public static readonly IEnumerable<string> GetUpperBound = new string[]
                {
                    "PrimeCalculator::GetUpperBound(System.Int64)"
                };
                public static IEnumerable<string> IsPrimeAndIsLessThan2
                    => IsPrime.Concat(IsLessThan2);
            }
        }
    }
}
