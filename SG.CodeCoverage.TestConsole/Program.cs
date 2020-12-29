using SG.CodeCoverage.Instrumentation;
using SG.CodeCoverage.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.TestConsole
{
    class Program
    {
        static void Main()
        {
            var tester = new InstrumenterTester();
            tester.InstrumentSampleProject();
            tester.RunSomeCode();
            var visitedFiles = tester.GetVisitedFiles();
            Console.WriteLine(visitedFiles.Count);
        }

        static void InstrumentPath(string dir, string backupFolder, string filePattern)
        {
            var mapFilePath = Path.Combine(dir, "map.json");
            var filesToInstrument = Directory
                    .GetFiles(dir, filePattern)
                    .ToList();
            var options = new InstrumentationOptions(filesToInstrument, new[] { dir }, dir, 12398);
            var instrumenter = new Instrumenter(options, mapFilePath, null)
            {
                BackupFolder = backupFolder,
                RecorderLogFilePath = Path.Combine(dir, "CodeCoverageRecorderLog.txt")
            };
            Directory.CreateDirectory(instrumenter.BackupFolder);
            var map = instrumenter.Instrument();
        }
    }
}
