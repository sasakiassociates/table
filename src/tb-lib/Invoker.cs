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

        private JsonToMarkerParser _parseStrategy = new JsonToMarkerParser();
        private Repository _repository;
        public int expire = 1000;
        public bool isRunning = false;
        public bool isListening = false;

        private string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

        public Dictionary<int, string> refDict;
        List<Marker> incomingMarkerData = new List<Marker>();
        List<Marker> markerMemory = new List<Marker>();

        // TODO change this class to cooperate with a State Pattern
        // We need a persistant memory for smoothing purposes
        // For now, we'll put it here, but the structure of this code needs an overhaul later


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

        public async Task<List<Marker>> ListenerThread()
        {
            if (!isListening)
            {
                isListening = true;
            }
            try
            {
                while (true)
                {
                    List<Marker> response = (List<Marker>)QueryResponse();
                    await Task.Delay(100);
                    if (response.Count > 0)
                    {
                        return response;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
            finally
            {
                isListening = false;
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
            string path = "C:\\Users\\nshikada\\Documents\\GitHub\\table\\src\\tb-detection";
            LaunchDetectionProgram(path);

            if (_repository == null)
            {
                _repository = new Repository();
            }

            if (!_repository.IsConnected())
            {
                _repository.Connect();
            }
            else
            {
                isRunning = true;
            }
        }

        // TODO the program crashes if openned in Grasshopper
        // It'll open, then do a handful of updates, and then crashes
        // Might be several things, but has to be specific to the way "Process"
        // works to launch apps since it works fine when launched from command line
        // Might be a threading issue, or a memory issue
        public object QueryResponse()
        {
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
                    incomingMarkerData = _parseStrategy.Parse(response);

                    foreach (Marker newMarker in incomingMarkerData)                         // For each marker in the incoming data
                    {
                        bool exists = false;                                // Assume the marker doesn't exist in the stable data
                        foreach (Marker oldMarker in markerMemory)         // Check each marker in the stable data to see if its id matches the incoming marker
                        {
                            if (newMarker.id == oldMarker.id)
                            {
                                exists = true;                              // If it does, then mark it as existing
                                oldMarker.Update(newMarker);                // And update the marker's position
                                break;                                      // Then break out of the loop
                            }
                        }
                        if (!exists)                                        // If the marker doesn't exist in the stable data
                        {
                            markerMemory.Add(newMarker);                         // Add it to the stable data
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                LogError(ex);
            }
            
            return markerMemory;
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

                _repository.Disconnect();
                _repository = null;
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

        public static int MapFloatToInt(float value, float fromMin, float fromMax, int toMin, int toMax)
        {
            // Ensure the value is within the specified range
            value = Math.Max(fromMin, Math.Min(fromMax, value));

            // Perform linear mapping
            float range1 = fromMax - fromMin;
            float range2 = toMax - toMin;
            return (int)(((value - fromMin) / range1) * range2 + toMin);
        }
    }
}