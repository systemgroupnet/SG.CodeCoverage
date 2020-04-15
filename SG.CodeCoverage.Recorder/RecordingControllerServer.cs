using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Recorder
{
    public class RecordingControllerServer
    {
        private readonly Dictionary<string, Func<string, string>> _commands;
        private readonly ILogger _logger;
        private static RecordingControllerServer _instance;
        private Task _listeningTask;

        public RecordingControllerServer(ILogger logger)
        {
            _logger = logger;
            _commands = new Dictionary<string, Func<string, string>>()
            {
                ["save"] = SaveAndReset
            };
            _listeningTask = StartAsync(InjectedConstants.ControllerServerPort);
        }

        public static void Initialize()
        {
            string logFileName = GetLogFileName();
            _instance = new RecordingControllerServer(new SimpleFileLogger(logFileName));
        }

        public async Task StartAsync(int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                try
                {
                    while (true)
                        _ = AcceptAsync(await listener.AcceptTcpClientAsync());
                }
                finally { listener.Stop(); }
            }
            catch(Exception ex)
            {
                _logger.LogError("RecordingController: Listening failed. Error:\r\n" + ex.ToString());
            }
        }

        private async Task AcceptAsync(TcpClient client)
        {
            await Task.Yield();
            try
            {
                using (client)
                using (NetworkStream nStream = client.GetStream())
                {
                    var bReader = new BinaryReader(nStream);
                    var command = bReader.ReadString();
                    var response = ProcessCommand(command.Trim());
                    var bWriter = new BinaryWriter(nStream);
                    bWriter.Write(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("There was an error while processing network request:\r\n" + ex.ToString());
            }
        }

        private string ProcessCommand(string command)
        {
            var (commandName, parameter) = ExtractCommandNameAndParameter(command);
            bool failed = false;
            string result;

            if (_commands.TryGetValue(command.ToLower(), out var operation))
            {
                try
                {
                    result = operation(parameter);
                }
                catch(Exception ex)
                {
                    failed = true;
                    result = ex.ToString();
                }
            }
            else
            {
                failed = true;
                result = "Command Not Found.";
            }
            if (failed)
                return "ERROR " + result;
            else
                return "OK " + result;
        }

        private static (string commandName, string parameter) ExtractCommandNameAndParameter(string command)
        {
            var end = command.IndexOfAny(new[] { ' ', '\t' });
            string commandName, param;
            if (end > 0)
            {
                commandName = command.Substring(0, end);
                param = command.Substring(end + 1).Trim();
            }
            else
            {
                commandName = command;
                param = null;
            }
            return (commandName, param);
        }

        private string SaveAndReset(string fileName)
        {
            HitsRepository.SaveAndResetHits(fileName);
            return null;
        }

        private static string GetLogFileName()
        {
            var logFileName = InjectedConstants.RecorderLogFileName;
            if (!Path.IsPathRooted(logFileName))
                logFileName = Path.Combine(InjectedConstants.WorkingDirectory, logFileName);
            return logFileName;
        }
    }
}

