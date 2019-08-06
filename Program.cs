using System;
using System.Collections.Generic;
using System.Diagnostics;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using Newtonsoft.Json;

namespace _433_influx
{
    class Program
    {
        private static LineProtocolClient lineProtocolClient = null;

        static void Main(string[] args)
        {
            lineProtocolClient = new LineProtocolClient(new Uri("http://localhost:8086"), "rtl433");

            var process = new Process();
            process.StartInfo.FileName = "/usr/local/bin/rtl_433";
            process.StartInfo.Arguments = "-F json -M protocol -M newmodel";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += cmd_DataReceived;
            process.EnableRaisingEvents = true;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        static void cmd_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            var fields = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data);
            var measurement = fields["model"] as string;
            fields.Remove("model");

            var payload = new LineProtocolPayload();
            payload.Add(new LineProtocolPoint(measurement, fields));
            lineProtocolClient.WriteAsync(payload).Wait();
        }
    }
}
