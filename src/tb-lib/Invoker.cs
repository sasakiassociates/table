using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TableLib
{
    // A class that controls the flow of the program.
    public class Invoker
    {
        private IParser _parseStrategy;
        private Repository _repository;

        public Invoker(IParser parseStrategy)
        {
            _repository = Repository.Instance;
            _parseStrategy = parseStrategy;
        }

        public object Run()
        {
            // Tell the other program to send data
            _repository.UdpSend("SEND");
            // Receive data
            string response = _repository.UdpReceive();
            // Parse data
            object data = _parseStrategy.Parse(response);
            return data;
        }
        
        public void SetParseStrategy(IParser parseStrategy)
        {
            _parseStrategy = parseStrategy;
        }

        public void EndDetection()
        {
            // Tell the other program to stop sending data
            _repository.UdpSend("END");
            _repository.EndUdpReceive();
        }

        // Launches the detection program (non-blocking)
        public void LaunchDetection()
        {
            string virtualEnvPath = "C:/Users/nshikada/Documents/GitHub/table/src/tb-detection/.env";
            string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
            string scriptPath = "../../../../../src/tb-detection/main.py";

            string argument1 = "udp";
            string arguments = $"{scriptPath} {argument1}";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonPathInEnv,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
        }
    }
}
