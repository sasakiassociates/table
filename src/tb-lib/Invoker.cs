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
        public int expire = 1000;

        // TODO this might be obsolete, see other ways to format this
        public Invoker()
        {
            _repository = Repository.Instance;
            // The default strategy is to parse the json string into a list of Markers
            _parseStrategy = ParserFactory.GetParser("Marker");
        }

        // Launches the detection program (non-blocking)
        public void LaunchDetection(int num_models)
        {
            try
            {
                string virtualEnvPath = "C:/Users/nshikada/Documents/GitHub/table/src/tb-detection/.env";
                string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
                string scriptPath = "../../../../../src/tb-detection/main.py";

                string argument1 = $"udp --num_models {num_models}";
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Setup(int modelNum, int variableNum)
        {
            _repository.UdpSend($"SETUP {modelNum} {variableNum}");
        }

        public object Run()
        {
            // Connect to the UDP client
            _repository.Connect();
            
            // Tell the other program to send data
            _repository.UdpSend("SEND");
            
            // Receive the data
            string response = _repository.UdpReceive(expire);
            if (response == null)
            {
                Console.WriteLine("No response");
                return null;
            }
            
            // Parse data
            object data = _parseStrategy.Parse(response);
            
            // Disconnect from the UDP client
            _repository.EndUdpReceive();
            
            return data;
        }
        public void Disconnect()
        {
            _repository.EndUdpReceive();
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
    }
}
