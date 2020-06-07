using SampleProjectForTest;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Instrumentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SG.CodeCoverage.Tests
{
    public class InstrumenterTester
    {
        public const int PortNumber = 61238;
        public const string MapFileName = "map.json";
        public static string DefaultOutputPath { get; }
        public string OutputPath { get; }
        public string MapFilePath { get; }
        public string InstrumentedAssemblyPath { get; private set; }

        static InstrumenterTester()
        {
            DefaultOutputPath = Path.Combine(Path.GetTempPath(), "SG.CodeCoverage");
        }

        public InstrumenterTester()
        {
            OutputPath = DefaultOutputPath;
            MapFilePath = Path.Combine(OutputPath, MapFileName);
        }

        public InstrumenterTester(string existingInstrumentedSampleFolder)
        {
            OutputPath = existingInstrumentedSampleFolder;
            MapFilePath = Path.Combine(OutputPath, MapFileName);
            InstrumentedAssemblyPath = Path.Combine(OutputPath, Path.GetFileName(typeof(PrimeCalculator).Assembly.Location));
        }

        public void InstrumentSampleProject()
        {
            if (Directory.Exists(OutputPath))
                CleanDirectory(OutputPath);
            else
                Directory.CreateDirectory(OutputPath);

            var oridgAssemblyFileName = typeof(PrimeCalculator).Assembly.Location;
            var origPdbFileName = Path.ChangeExtension(oridgAssemblyFileName, "pdb");

            var assemblyFileName = Path.Combine(OutputPath, Path.GetFileName(oridgAssemblyFileName));

            File.Copy(oridgAssemblyFileName, assemblyFileName);
            File.Copy(origPdbFileName, Path.Combine(OutputPath, Path.GetFileName(origPdbFileName)));

            Instrumenter instrumenter = new Instrumenter(new[] { assemblyFileName },
                Array.Empty<string>(), OutputPath, MapFilePath, PortNumber, new ConsoleLogger());
            instrumenter.BackupFolder = Path.Combine(OutputPath, "backup");
            Directory.CreateDirectory(instrumenter.BackupFolder);
            instrumenter.Instrument();
            InstrumentedAssemblyPath = assemblyFileName;
        }

        private void CleanDirectory(string dirPath)
        {
            foreach (var file in Directory.EnumerateFiles(dirPath))
                File.Delete(file);
            foreach (var dir in Directory.EnumerateDirectories(dirPath))
                Directory.Delete(dir, true);
        }

        public void RunSomeCode()
        {
            if (InstrumentedAssemblyPath == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            var client = new Collection.RecordingController(PortNumber);
            Assembly.LoadFrom(Path.Combine(OutputPath, "SG.CodeCoverage.Recorder.dll"));
            var assembly = Assembly.LoadFrom(InstrumentedAssemblyPath);
            var calc = assembly.DefinedTypes.Where(x => x.Name == nameof(PrimeCalculator)).FirstOrDefault();
            var res = calc.GetMethod("IsPrime").Invoke(calc.DeclaredConstructors.First().Invoke(null), new object[] { 7 });
        }

        public List<string> GetVisitedFiles()
        {
            if (InstrumentedAssemblyPath == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            var client = new RecordingController(PortNumber);
            var hitsFile = Path.Combine(OutputPath, "hits.bin");
            client.SaveHitsAndReset(hitsFile);
            return new CoverageResult(MapFilePath, hitsFile).GetVisitedSources().ToList();
        }
    }
}
