using SampleProjectForTest;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Instrumentation;
using SG.CodeCoverage.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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
        private Process _process;
        private List<string> _processOutput;

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

        public void RunApp(string args = null)
        {
            if (InstrumentedAssemblyPath == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            _process = Process.Start(new ProcessStartInfo()
            {
                FileName = InstrumentedAssemblyPath,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = OutputPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            });
            _processOutput = new List<string>();
            _process.OutputDataReceived += _process_OutputDataReceived;
            _process.BeginOutputReadLine();
            WaitForAppIdle();
        }

        private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            _processOutput.Add(e.Data);
        }

        private void WaitForAppIdle()
        {
            CheckAppStarted();
            while (_processOutput.Count == 0 || _processOutput.Last() != "Enter command:")
                Thread.Sleep(1);
        }

        public bool AppIsRunning
            => _process != null && !_process.HasExited;

        public void ExitApp()
        {
            CheckAppStarted();
            _process.StandardInput.WriteLine("exit");
            if (!_process.WaitForExit(10000))
                throw new Exception("App did not exit withing 10 seconds.");
            _process.WaitForExit();
            _process = null;
        }

        public void RunIsPrimeInApp(int number)
        {
            CheckAppStarted();
            _process.StandardInput.WriteLine("IsPrime " + number);
            WaitForAppIdle();
        }

        private void CheckAppStarted()
        {
            if (_process == null)
                throw new InvalidOperationException("Sample application is not started.");
        }

        public CoverageResult GetCoverageResult()
        {
            if (InstrumentedAssemblyPath == null || RecordingController == null)
                throw new InvalidOperationException("Sample assembly is not instrumented.");
            return RecordingController.CollectResultAndReset();
        }
    }
}
