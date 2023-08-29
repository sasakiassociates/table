using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TableLib
{
    public sealed class Repository
    {
        private UdpClient _udpClient;
        private int listenPort = 5005;
        private int sendPort = 5004;

        private IPAddress destination = IPAddress.Parse("127.0.0.1");
        private IPEndPoint receiveIpEndPoint;
        private IPEndPoint sendEndPoint;
        
        public string response;
        public bool isRunning = false;

        public Repository()
        {
            // Define the targets of the UDP clients
            // Receive from "Any" IP address, listens to all incoming traffic
            receiveIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            // Send to localhost, the same machine (Can be changed to send to another machine)
            sendEndPoint = new IPEndPoint(destination, sendPort);
            _udpClient = new UdpClient(receiveIpEndPoint);
        }

        // Launches the detection program (non-blocking)
        public void LaunchDetectionProgram(string detectionPath)
        {
            try
            {
                string virtualEnvPath = Path.Combine(detectionPath, ".env");
                // string virtualEnvPath = "..\\..\\..\\..\\..\\src\\tb-detection\\.env";
                string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
                // string scriptPath = "..\\..\\..\\..\\..\\src\\tb-detection\\main.py";
                string scriptPath = Path.Combine(detectionPath, "main.py");

                string argument1 = $"udp";
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
                isRunning = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UdpSend(string message)
        {
            byte[] sendData = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(sendData, sendData.Length, sendEndPoint);
        }

        public string UdpReceive(int expire = 0)
        {
            try
            {
                if (expire != 0)
                {
                    _udpClient.Client.ReceiveTimeout = expire;
                }
                byte[] receiveBytes = _udpClient.Receive(ref receiveIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                response = returnData;

                return returnData;
            }
            catch (Exception ex)
            {
                // return ex.Message;
                // Return null so that we can have a simple catch later so we don't try to parse an error message
                return null;
            }
        }

        public bool IsConnected()
        { 
            if (_udpClient != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Connect()
        {
            _udpClient = new UdpClient(receiveIpEndPoint);
        }
        public void Disconnect()
        {
            _udpClient.Close();
            //_udpClient.Dispose();
        }
    }
}
