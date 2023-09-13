using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
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

        // Auto Update tests
        public bool isListening = false;

        private string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

        public Dictionary<int, string> refDict;
        List<Marker> incomingMarkerData = new List<Marker>();
        public List<Marker> markerMemory = new List<Marker>();

        // Desired data
        public List<int> markerIds = new List<int>();
        public List<float> markerRotations = new List<float>();
        public List<int[]> markerLocations = new List<int[]>();
        public int _counter = 0;

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

        public async Task ListenerThread(CancellationToken cancelToken)
        {
            // This is the async method that listens for the udp messages
            string response = await _repository.Receive(cancelToken, 0);
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

                markerIds.Clear();
                markerRotations.Clear();
                markerLocations.Clear();
                foreach (Marker marker in markerMemory)
                {
                    markerIds.Add(marker.id);
                    markerRotations.Add(marker.rotation);
                    markerLocations.Add(marker.location);
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
                isListening = false;
            } catch (Exception ex)
            {
                LogError(ex);
            }
        }
    }
}