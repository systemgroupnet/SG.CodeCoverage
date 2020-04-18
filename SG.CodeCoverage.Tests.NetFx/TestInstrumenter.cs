using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleProjectForTest;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Instrumentation;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SG.CodeCoverage.Tests
{
    [TestClass]
    public class TestInstrumenter
    {
        public const int PortNumber = 61238;

        private (string asm, string map) InstrumentSampleProject()
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
            return (assemblyFileName, mapFileName);
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
            var (asmFileName, mapPath) = InstrumentSampleProject();
            CheckIsInstrumented(asmFileName, mapPath);
        }

        private void CheckIsInstrumented(string asmFilePath, string mapPath)
        {
            var dirPath = Path.GetDirectoryName(asmFilePath);
            var client = new Collection.RecorderControllerClient(PortNumber);
            string hitsPath(string testId) => Path.Combine(dirPath, $"{testId}.bin");
            var assembly = Assembly.LoadFrom(asmFilePath);
            PrimeCalculator calc = (PrimeCalculator)assembly.CreateInstance(nameof(PrimeCalculator));
            calc.IsPrime(7);
            client.SaveHitsAndReset(hitsPath("Prime"));
            var visitedFile = new Collection.DataCollector(mapPath, asmFilePath).GetVisitedFiles();
            File.WriteAllText(Path.Combine(dirPath, "visitedFile"), string.Join(",", visitedFile));

        }
    }
}
