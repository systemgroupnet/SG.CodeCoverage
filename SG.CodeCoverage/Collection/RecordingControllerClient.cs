using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;
using SG.CodeCoverage.Recorder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Collection
{
    public static class RecordingControllerClient
    {
        public const int ConnectionTimeoutSeconds = 10;

        public static void SaveHitsAndReset(string host, int port, string outputHitsFilePath)
        {
            var response = SendCommand(host, port, Constants.SaveCommand, outputHitsFilePath);
            ValidateResponse(response, "saving hits file");
        }

        public static CoverageResult CollectResultAndReset(string host, int port, InstrumentationMap map)
        {
            var fileName = GetTempFileName();
            try
            {
                SaveHitsAndReset(host, port, fileName);
                return new CoverageResult(map, fileName);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }


        public static void ResetHits(string host, int port)
        {
            var response = SendCommand(host, port, Constants.ResetCommand, null);
            ValidateResponse(response, "resettings hits");
        }

        private static (bool successful, string result) SendCommand(string host, int port, string command, string param)
        {
            var tcpClient = new TcpClient();
            if (!tcpClient.ConnectAsync(host, port).Wait(ConnectionTimeoutSeconds * 1000))
                throw new TimeoutException($"Cannot connect to '{host}:{port}'. Connection timed out.");

            string result;
            using (var nstream = tcpClient.GetStream())
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

        private static string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        private static void ValidateResponse((bool successful, string result) response, string operationString)
        {
            if (!response.successful)
                throw new Exception(
                    $"An error occurred in the recorder while {operationString}. The error was:\r\n" +
                    response.result);
        }

    }
}
