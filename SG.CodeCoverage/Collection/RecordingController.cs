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
            if (_tcpClient == null)
                _tcpClient = new TcpClient();
            if (!_tcpClient.Connected)
                _tcpClient.Connect(Host, PortNumber);

            string result;
            using (var nstream = _tcpClient.GetStream())
            {
                BinaryWriter writer = new BinaryWriter(nstream);
                writer.Write("save " + outputHitsFilePath);
                writer.Flush();
                result = new BinaryReader(nstream).ReadString().Trim();
            }
            if (!result.Equals(Constants.CommandOkResponse, StringComparison.OrdinalIgnoreCase))
            {
                if (result.StartsWith(Constants.CommandErrorResponse, StringComparison.OrdinalIgnoreCase))
                    throw new Exception(
                        "An error occurred in the recorder while saving hits file. The error was:\r\n" +
                        result.Substring(Constants.CommandErrorResponse.Length).Trim());
                else
                    throw new Exception("Unknown response:\r\n" + result);
            }
        }

        public void Dispose()
        {
            if (_tcpClient != null)
                _tcpClient.Dispose();
        }
    }
}
