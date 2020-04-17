using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SG.CodeCoverage.Collection
{
    public sealed class RecorderControllerClient : IDisposable
    {
        public int PortNumber { get; }
        private TcpClient _tcpClient;
        private const string OkResponse = "OK";
        private const string ErrorResponse = "ERROR";

        public RecorderControllerClient(int portNumber)
        {
            PortNumber = portNumber;
        }

        public void SaveHitsAndReset(string outputHitsFilePath)
        {
            if (_tcpClient == null)
                _tcpClient = new TcpClient();
            if (!_tcpClient.Connected)
                _tcpClient.Connect("localhost", PortNumber);

            string result;
            using (var nstream = _tcpClient.GetStream())
            {
                BinaryWriter writer = new BinaryWriter(nstream);
                writer.Write("save " + outputHitsFilePath);
                writer.Flush();
                result = new BinaryReader(nstream).ReadString().Trim();
            }
            if(!result.Equals(OkResponse, StringComparison.OrdinalIgnoreCase))
            {
                if (result.StartsWith(ErrorResponse, StringComparison.OrdinalIgnoreCase))
                    throw new Exception(
                        "An error occurred in the recorder while saving hits file. The error was:\r\n" +
                        result.Substring(ErrorResponse.Length).Trim());
                else
                    throw new Exception("Unknown response:\r\n" + result);
            }
        }

        public void Dispose()
        {
            if (_tcpClient != null)
                _tcpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
