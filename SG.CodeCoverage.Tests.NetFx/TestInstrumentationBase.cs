﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Tests.NetFx
{
    public abstract class TestInstrumentationBase
    {
        InstrumenterTester _tester;

        public TestInstrumentationBase(int port)
        {
            _tester = new InstrumenterTester()
            {
                PortNumber = port
            };
            _tester.InstrumentSampleProject();
        }

        [TestMethod]
        public void Coverage_Startup()
        {
            _tester.RunApp();
            AssertVisitedFilesAndMethods(Files.Startup, Methods.Startup);
            _tester.KillApp();
        }

        [TestMethod]
        public void Coverage_IsPrime1()
        {
            _tester.RunApp("IsPrime 1");
            AssertVisitedFilesAndMethods(
                Files.Startup
                    .Append(Files.PrimeCalculator),
                Methods.Startup
                    .Concat(Methods.RunCommand)
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2));
            _tester.KillApp();
        }

        [TestMethod]
        public void Coverage_IsPrime4()
        {
            _tester.RunApp("IsPrime 4");
            AssertVisitedFilesAndMethods(
                Files.Startup
                    .Append(Files.PrimeCalculator),
                Methods.Startup
                    .Concat(Methods.RunCommand)
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2)
                    .Concat(Methods.PrimeCalculator.GetUpperBound));
            _tester.KillApp();
        }

        [TestMethod]
        public void Coverage_IsPrime7()
        {
            _tester.RunApp("IsPrime 7");
            AssertVisitedFilesAndMethods(
                Files.Startup
                    .Append(Files.PrimeCalculator)
                    .Append(Files.SampleStruct),
                Methods.Startup
                    .Concat(Methods.RunCommand)
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2)
                    .Concat(Methods.PrimeCalculator.GetUpperBound)
                    .Concat(Methods.SampleStruct));
            _tester.KillApp();
        }

        [TestMethod]
        public void Coverage_ResetHits_IsPrime2()
        {
            _tester.RunApp();
            _tester.ResetHits();
            _tester.RunIsPrimeInApp(2);
            AssertVisitedFilesAndMethods(
                new string[] { Files.App }
                    .Append(Files.PrimeCalculator),
                Methods.RunCommand
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2)
                    .Append(Methods.get_Commands));
            _tester.KillApp();
        }

        [TestMethod]
        public void Coverage_ResetHits_IsPrime7()
        {
            _tester.RunApp();
            _tester.ResetHits();
            _tester.RunIsPrimeInApp(7);
            AssertVisitedFilesAndMethods(
                new string[] { Files.App }
                    .Append(Files.PrimeCalculator)
                    .Append(Files.SampleStruct),
                Methods.RunCommand
                    .Concat(Methods.PrimeCalculator.IsPrimeAndIsLessThan2)
                    .Concat(Methods.PrimeCalculator.GetUpperBound)
                    .Concat(Methods.SampleStruct)
                    .Append(Methods.get_Commands));
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
            foreach (var expected in expectedNames)
            {
                bool found = false;
                foreach (var actual in actualNames)
                {
                    if (actual.EndsWith(expected))
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
            public static readonly string App = "App.cs";
            public static readonly string Program = "Program.cs";

            public static readonly IEnumerable<string> Startup = new string[]
            {
                App,
                Program
            };

            public static readonly string PrimeCalculator = "PrimeCalculator.cs";
            public static readonly string SampleStruct = "SampleStruct.cs";
        }

        private static class Methods
        {
            public static string get_Commands = "App::get_Commands()";

            public static readonly IEnumerable<string> Startup = new string[]
            {
                "Program::Main(System.String[])",
                "App::.ctor()",
                get_Commands,
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

            public static readonly IEnumerable<string> SampleStruct = new string[]
            {
                "SampleStruct::.ctor(System.Int32)",
                "SampleStruct::get_Value()",
                "SampleStruct::Multiply(System.Int32)"
            };
        }
    }
}
