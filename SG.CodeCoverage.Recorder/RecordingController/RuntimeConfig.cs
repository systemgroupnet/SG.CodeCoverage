using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SG.CodeCoverage.Recorder.RecordingController
{
    public class RuntimeConfig
    {
        public List<RunningProcess> Processes { get; set; } = new List<RunningProcess>();

        public static string GetDefaultFileName()
        {
            var path = InjectedConstants.RuntimeConfigOutputPath;
            if (string.IsNullOrEmpty(path))
                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(path, InjectedConstants.RuntimeConfigFileName);
        }

        public void Save()
        {
            Save(GetDefaultFileName());
        }

        public static void Update(int port)
        {
            var path = GetDefaultFileName();
            RuntimeConfig runtimeConfig;
            if (File.Exists(path))
                runtimeConfig = Load(path);
            else
                runtimeConfig = new RuntimeConfig();
            var id = Process.GetCurrentProcess().Id;
            runtimeConfig.Processes.RemoveAll(p => p.ID == id);
            runtimeConfig.Processes.Add(new RunningProcess(id, port));
            runtimeConfig.Save(path);
        }

        private void Save(string fileName)
        {
            Cleanup();
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var p in Processes)
                    writer.WriteLine($"PID: {p.ID}, Port: {p.ListeningPort}");
            }
        }

        public static RuntimeConfig Load(string fileName)
        {
            List<RunningProcess> processes = new List<RunningProcess>();
            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var parts = line.Split(new char[] { ' ', ':', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 4)
                        ThrowInvalidFormat();
                    if (!parts[0].Equals("PID", StringComparison.OrdinalIgnoreCase))
                        ThrowInvalidFormat();
                    if (!parts[2].Equals("Port", StringComparison.OrdinalIgnoreCase))
                        ThrowInvalidFormat();
                    processes.Add(new RunningProcess(int.Parse(parts[1]), int.Parse(parts[3])));
                }
            }

            var r = new RuntimeConfig() { Processes = processes };
            r.Cleanup();
            return r;
        }

        private void Cleanup()
        {
            var toDelete = new List<RunningProcess>();
            var runningProcessIDs = new HashSet<int>(Process.GetProcesses().Select(p => p.Id));

            Processes.RemoveAll(p => !runningProcessIDs.Contains(p.ID));
        }

        private static void ThrowInvalidFormat()
        {
            throw new Exception("Invalid file format.");
        }

        public class RunningProcess
        {
            public RunningProcess(int iD, int listeningPort)
            {
                ID = iD;
                ListeningPort = listeningPort;
            }

            public int ID { get; }
            public int ListeningPort { get; }
        }
    }
}
