using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TableLib
{
    // A class that controls the flow of the program.
    // TODO: Redesign as a State Manager
    // It'll be much more intelliigable and easier to maintain
    public class Invoker
    {
        // Singleton elements
        // This class runs everything and keeps track of the state of the program, so we need to make sure there is only one instance of it throughout the program
        private static Invoker instance;
        private static readonly object _lock = new object();

        private IParser _parseStrategy;
        private Repository _repository;
        public int expire = 1000;
        public bool isRunning = false;

        private string logFilePath = "C:\\Users\\nshikada\\Documents\\GitHub\\table\\src\\tb-gh\\TableUiAdapter\\obj\\Debug\\net48\\error.log";

        public Dictionary<int, object> modelRefDict = new Dictionary<int, object>();
        public Dictionary<int, string> refDict;

        private void LogError(Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"[Error Timestamp: {DateTime.Now}]");
                    writer.WriteLine($"[Error Message]: {ex.Message}");
                    writer.WriteLine($"[Stack Trace]: {ex.StackTrace}");
                    writer.WriteLine(new string('-', 50));
                }
            }
            catch (Exception)
            {
            }
        }

        private Invoker()
        {
            _repository = new Repository();
            // The default strategy is to parse the json string into a list of Markers
            _parseStrategy = ParserFactory.GetParser("Marker");
        }

        // Singleton implementation
        public static Invoker Instance
        {
            get
            {
                // lock makes sure the Singleton works in a multi-threaded environment
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new Invoker();
                    }
                    return instance;
                }
            }
        }

        // Launches the detection program (non-blocking)
        public void LaunchDetectionProgram(string detectionPath)
        {
            try
            {
                string virtualEnvPath = Path.Combine(detectionPath, ".env");
                string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
                string scriptPath = Path.Combine(detectionPath, "main.py");

                string argument1 = $"udp";
                string arguments = $"{scriptPath} {argument1}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonPathInEnv,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                using (StreamWriter errorStream = new StreamWriter("error.log"))
                {
                    process.ErrorDataReceived += (sender, e) => { errorStream.WriteLine(e.Data); };
                    process.Start();
                    process.BeginErrorReadLine();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        // TODO this gets stuck in a loop and can't hear the response
        // (Only if the program was launched and then closed and then launched again)
        // UPDATE: This happens when the udp client was formerly closed and then reopened
        // SOLUTION: for now just don't close the udp client (not best practice)
        public void SetupDetection(int modelNum, int variableNum)
        {
            // Connect to the UDP client
            // _repository.Connect();
            while (!isRunning)
            {
                _repository.UdpSend($"SETUP {modelNum} {variableNum}");
                string response = _repository.UdpReceive(expire);
                _repository.IsConnected();
                if (response == "READY")
                {
                    isRunning = true;
                    break;
                }
            }
        }

        // TODO the program crashes if openned in Grasshopper
        // It'll open, then do a handful of updates, and then crashes
        // Might be several things, but has to be specific to the way "Process"
        // works to launch apps since it works fine when launched from command line
        // Might be a threading issue, or a memory issue
        public object Run()
        {
            object data = null;
            
            try
            {
                // Connect to the UDP client
                //_repository.Connect();

                // Tell the other program to send data
                _repository.UdpSend("SEND");

                // Receive the data
                string response = _repository.UdpReceive(expire);

                
                // If there is a response, parse the data
                if (response != null)
                {
                    // Parse data
                    data = _parseStrategy.Parse(response);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                LogError(ex);
            }
            
            return data;
        }

        public void SetParseStrategy(IParser parseStrategy)
        {
            _parseStrategy = parseStrategy;
        }

        public void StopDetectionProgram()
        {
            try
            {
                // Tell the other program to stop sending data
                _repository.UdpSend("END");
                // TODO: Figure out when to disconnect the udp client
                // After the udp client is closed and/or disposed, it can't be used again
                // Even when a new instance is created, it can't be used

                // _repository.Disconnect();
                isRunning = false;
            } catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

        public void BuildDict(int modelCount, int variableCount)
        {
            refDict = new Dictionary<int, string>();

            for (int i = 0; i < modelCount; i++)
            {
                refDict.Add(i, "model");
            }
            for (int i = 0; i < variableCount; i++)
            {
                refDict.Add(i + modelCount, "variable");
            }
        }

        public object GetModel(int id)
        {
            return modelRefDict[id];
        }
    }
}
