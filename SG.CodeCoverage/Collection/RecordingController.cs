using SG.CodeCoverage.Recorder;
using System;
using System.IO;
using System.Net.Sockets;

namespace SG.CodeCoverage.Collection
{
    public sealed class RecordingController : IDisposable
    {
        public string Host { get; }
        public int PortNumber { get; }
        private TcpClient _tcpClient;

        public RecordingController(int portNumber)
            : this("localhost", portNumber)
        {
        }

        public RecordingController(string host, int portNumber)
        {
            Host = host;
            PortNumber = portNumber;
        }

        public void SaveHitsAndReset(string outputHitsFilePath)
        {
            var response = SendCommand(Constants.SaveCommand, outputHitsFilePath);
            ValidateResponse(response, "saving hits file");
        }

        public void ResetHits()
        {
            var response = SendCommand(Constants.ResetCommand, null);
            ValidateResponse(response, "resettings hits");
        }

        private void ValidateResponse((bool successful, string result) response, string operationString)
        {
            if (!response.successful)
                throw new Exception(
                    $"An error occurred in the recorder while {operationString}. The error was:\r\n" +
                    response.result);
        }

        private (bool successful, string result) SendCommand(string command, string param)
        {
            if (_tcpClient == null)
                _tcpClient = new TcpClient();
            if (!_tcpClient.Connected)
                _tcpClient.Connect(Host, PortNumber);

            string result;
            using (var nstream = _tcpClient.GetStream())
            {
                BinaryWriter writer = new BinaryWriter(nstream);
                writer.Write(command + (string.IsNullOrEmpty(param) ? string.Empty : " " + param));
                writer.Flush();
                result = new BinaryReader(nstream).ReadString().Trim();
            }
            if (result.Equals(Constants.CommandOkResponse, StringComparison.OrdinalIgnoreCase))
                return (successful: true, result: result.Substring(Constants.CommandOkResponse.Length).Trim());
            else if (result.StartsWith(Constants.CommandErrorResponse, StringComparison.OrdinalIgnoreCase))
                return (successful: false, result: result.Substring(Constants.CommandErrorResponse.Length).Trim());
            else
                throw new Exception("Unknown response:\r\n" + result);

        }

        public void Dispose()
        {
            if (_tcpClient != null)
                _tcpClient.Dispose();
        }
    }
}
