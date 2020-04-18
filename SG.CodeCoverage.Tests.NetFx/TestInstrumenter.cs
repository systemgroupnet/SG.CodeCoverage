using Microsoft.VisualStudio.TestTools.UnitTesting;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Instrumentation;
using System;
using System.IO;

namespace SG.CodeCoverage.Tests
{
    [TestClass]
    public class TestInstrumenter
    {
        public const int PortNumber = 61238;

        private string InstrumentSampleProject()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "SG.CodeCoverage");
            if (Directory.Exists(tempPath))
                CleanDirectory(tempPath);
            else
                Directory.CreateDirectory(tempPath);

            var oridgAssemblyFileName = typeof(SampleProjectForTest.PrimeCalculator).Assembly.Location;
            var origPdbFileName = Path.ChangeExtension(oridgAssemblyFileName, "pdb");

            var assemblyFileName = Path.Combine(tempPath, Path.GetFileName(oridgAssemblyFileName));
            var mapFileName = Path.Combine(tempPath, "map.json");

            File.Copy(oridgAssemblyFileName, assemblyFileName);
            File.Copy(origPdbFileName, Path.Combine(tempPath, Path.GetFileName(origPdbFileName)));

            Instrumenter instrumenter = new Instrumenter(new[] { assemblyFileName },
                new[] { tempPath }, tempPath, mapFileName, PortNumber, new ConsoleLogger());

            instrumenter.Instrument();
            return assemblyFileName;
        }

        private void CleanDirectory(string dirPath)
        {
            foreach (var file in Directory.EnumerateFiles(dirPath))
                File.Delete(file);
            foreach (var dir in Directory.EnumerateDirectories(dirPath))
                Directory.Delete(dir, true);
        }

        [TestMethod]
        public void TestSampleProjectInstrumented()
        {
            var asmFileName = InstrumentSampleProject();
            CheckIsInstrumented(asmFileName);
        }

        private void CheckIsInstrumented(string asmFilePath)
        {
            // to do
        }
    }
}
