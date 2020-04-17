using SG.CodeCoverage.Common;
using SG.CodeCoverage.Instrumentation;
using System;
using System.IO;
using Xunit;

namespace SG.CodeCoverage.Tests
{
    public class TestInstrumenter
    {
        public const int PortNumber = 61238;

        private void InstrumentSampleProject()
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
                tempPath, mapFileName, PortNumber, new ConsoleLogger());

            instrumenter.Instrument();
        }

        private void CleanDirectory(string dirPath)
        {
            foreach (var file in Directory.EnumerateFiles(dirPath))
                File.Delete(file);
            foreach (var dir in Directory.EnumerateDirectories(dirPath))
                Directory.Delete(dir, true);
        }

        [Fact]
        public void TestSampleProjectInstrumented()
        {
            InstrumentSampleProject();
        }
    }
}
