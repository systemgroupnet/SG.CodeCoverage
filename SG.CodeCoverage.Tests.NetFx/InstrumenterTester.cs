using SampleProjectForTest;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Instrumentation;
using SG.CodeCoverage.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SG.CodeCoverage.Tests
{
    public class InstrumenterTester
    {
        public int PortNumber { get; set; } = 61238;
        public static string DefaultOutputPath { get; } = Path.Combine(Path.GetTempPath(), "SG.CodeCoverage");
        public string OutputPath { get; }
        public string MapFilePath { get; }
        public IRecordingController RecordingController { get; private set; }
        public string InstrumentedAssemblyPath { get; private set; }
        private readonly ILogger _logger;

        public InstrumenterTester(ILogger logger = null)
        {
            OutputPath = DefaultOutputPath;
            _logger = logger ?? new ConsoleLogger();
        }

        public InstrumenterTester(string existingInstrumentedSampleFolder, ILogger logger = null)
        {
            OutputPath = existingInstrumentedSampleFolder;
            InstrumentedAssemblyPath = Path.Combine(OutputPath, Path.GetFileName(typeof(PrimeCalculator).Assembly.Location));
            _logger = logger ?? new ConsoleLogger();
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

            var options = new InstrumentationOptions(
                new[] { assemblyFileName },
                Array.Empty<string>(), OutputPath, PortNumber);
            Instrumenter instrumenter = new Instrumenter(options, _logger);
            instrumenter.BackupFolder = Path.Combine(OutputPath, "backup");
            Directory.CreateDirectory(instrumenter.BackupFolder);
            RecordingController = instrumenter.Instrument();
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
            Assembly.LoadFrom(Path.Combine(OutputPath, "SG.CodeCoverage.Recorder.dll"));
            var assembly = Assembly.LoadFrom(InstrumentedAssemblyPath);
            var calc = assembly.DefinedTypes.Where(x => x.Name == nameof(PrimeCalculator)).FirstOrDefault();
            var res = calc.GetMethod("IsPrime").Invoke(calc.DeclaredConstructors.First().Invoke(null), new object[] { 7 });
        }

        public CoverageResult GetCoverageResult()
        {
            if (InstrumentedAssemblyPath == null || RecordingController == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            return RecordingController.CollectResultAndReset();
        }
    }
}
