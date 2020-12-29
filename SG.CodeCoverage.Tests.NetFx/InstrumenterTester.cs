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
        public const int PortNumber = 61238;
        public static string DefaultOutputPath { get; }
        public string OutputPath { get; }
        public string MapFilePath { get; }
        public InstrumentationMap Map { get; private set; }
        public string InstrumentedAssemblyPath { get; private set; }

        static InstrumenterTester()
        {
            DefaultOutputPath = Path.Combine(Path.GetTempPath(), "SG.CodeCoverage");
        }

        public InstrumenterTester()
        {
            OutputPath = DefaultOutputPath;
        }

        public InstrumenterTester(string existingInstrumentedSampleFolder)
        {
            OutputPath = existingInstrumentedSampleFolder;
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

            var options = new InstrumentationOptions(
                new[] { assemblyFileName },
                Array.Empty<string>(), OutputPath, PortNumber);
            Instrumenter instrumenter = new Instrumenter(options, new ConsoleLogger());
            instrumenter.BackupFolder = Path.Combine(OutputPath, "backup");
            Directory.CreateDirectory(instrumenter.BackupFolder);
            Map = instrumenter.Instrument();
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
            var client = RecordingController.ForEndPoint("localhost", PortNumber, Map);
            Assembly.LoadFrom(Path.Combine(OutputPath, "SG.CodeCoverage.Recorder.dll"));
            var assembly = Assembly.LoadFrom(InstrumentedAssemblyPath);
            var calc = assembly.DefinedTypes.Where(x => x.Name == nameof(PrimeCalculator)).FirstOrDefault();
            var res = calc.GetMethod("IsPrime").Invoke(calc.DeclaredConstructors.First().Invoke(null), new object[] { 7 });
        }

        public List<string> GetVisitedFiles()
        {
            if (InstrumentedAssemblyPath == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            var client = RecordingController.ForEndPoint("localhost", PortNumber, Map);
            return client.CollectResultAndReset().GetVisitedSources().ToList(); ;
        }
    }
}
