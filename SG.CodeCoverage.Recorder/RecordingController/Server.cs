﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Recorder.RecordingController
{
    public class Server
    {
        private readonly Dictionary<string, Func<string, string>> _commands;
        private readonly ILogger _logger;
        private static Server _instance;
        private readonly Task _listeningTask;
        public int Port { get; private set; }

        public Server(ILogger logger)
        {
            _logger = logger;
            _commands = new Dictionary<string, Func<string, string>>()
            {
                [Constants.SaveCommand] = SaveAndReset,
                [Constants.ResetCommand] = Reset
            };
            _listeningTask = StartAsync(InjectedConstants.ControllerServerPort);
        }

        public static void Initialize()
        {
            if (InjectedConstants.ControllerServerPort == -1)
                return;
            string logFileName = InjectedConstants.RecorderLogFileName;
            _instance = new Server(new SimpleFileLogger(logFileName));
        }

        public async Task StartAsync(int port)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                this.Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                _logger.LogInformation("Started listening on port " + this.Port + ".");
                RuntimeConfig.Update(this.Port);
                try
                {
                    while (true)
                        _ = AcceptAsync(await listener.AcceptTcpClientAsync());
                }
                finally { listener.Stop(); }
            }
            catch (Exception ex)
            {
                _logger.LogError("Listening failed. Error:\r\n" + ex.ToString());
            }
        }

        private async Task AcceptAsync(TcpClient client)
        {
            await Task.Yield();
            try
            {
                _logger.LogInformation("A connection is established.");
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
            _logger.LogVerbose("Processing command '" + command + "'");
            var (commandName, parameter) = ExtractCommandNameAndParameter(command);
            bool failed = false;
            string result;

            if (_commands.TryGetValue(commandName.ToLower(), out var operation))
            {
                try
                {
                    result = operation(parameter);
                    _logger.LogVerbose("Command successfully processed.");
                }
                catch (Exception ex)
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
            {
                _logger.LogError($"Error while processing command '{commandName}': {result}");
                return Constants.CommandErrorResponse + " " + result;
            }
            else
            {
                var ok = Constants.CommandOkResponse;
                if (result != null)
                    return ok + " " + result;
                return ok;
            }
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

        private string Reset(string notUsedParam)
        {
            HitsRepository.ResetHits();
            return null;
        }
    }
}

