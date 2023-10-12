using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TableUiReceiver
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

        public Repository()
        {
            // Define the targets of the UDP clients
            // Receive from "Any" IP address, listens to all incoming traffic
            receiveIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            // Send to localhost, the same machine (Can be changed to send to another machine)
            sendEndPoint = new IPEndPoint(destination, sendPort);
            try
            {
                _udpClient = new UdpClient(receiveIpEndPoint);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void UdpSend(string message)
        {
            byte[] sendData = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(sendData, sendData.Length, sendEndPoint);
        }

        public async Task<string> Receive(CancellationToken cancelToken, int expire = 0)
        {
            try
            {
                if (expire != 0)
                {
                    _udpClient.Client.ReceiveTimeout = expire;
                }
                UdpReceiveResult listenTask;
                listenTask = await _udpClient.ReceiveAsync();
                byte[] receiveBytes = listenTask.Buffer;
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                response = returnData;
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
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
