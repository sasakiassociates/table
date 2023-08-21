using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TableLib
{
    public sealed class Repository
    {
        private static Repository _instance;
        private static readonly object _lock = new object();

        private UdpClient _udpClient;
        private int listenPort = 5005;
        private int sendPort = 5004;

        private bool connected = false;

        private IPAddress destination = IPAddress.Parse("127.0.0.1");
        private IPEndPoint receiveIpEndPoint;
        private IPEndPoint sendEndPoint;
        
        public string response;
        public static Repository Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Repository();
                    }
                    return _instance;
                }
            }
        }

        public Repository()
        {
            // Define the targets of the UDP clients
            // Receive from "Any" IP address, listens to all incoming traffic
            receiveIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            // Send to localhost, the same machine (Can be changed to send to another machine)
            sendEndPoint = new IPEndPoint(destination, sendPort);

            //_udpClient = new UdpClient(receiveIpEndPoint);
        }

        public void Connect()
        {
            if (connected)
            {
                return;
            }
            _udpClient = new UdpClient(receiveIpEndPoint);
            connected = true;
        }

        public string UdpReceive(int expire = 0)
        {
            // Make a new client - this might need to be optimized so it is continuously connected to the port
            // But this makes a new one and closes every time to work with Grasshopper
            try
            {
/*                if (_udpClient == null)
                {
                    return "UDP client is null";
                }*/
                if (expire != 0)
                {
                    _udpClient.Client.ReceiveTimeout = expire;
                }
                byte[] receiveBytes = _udpClient.Receive(ref receiveIpEndPoint);
                string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);

                response = returnData;

                return returnData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void EndUdpReceive()
        {
            if (!connected)
            {
                return;
            }
            _udpClient.Close();
            connected = false;
        }

        public void UdpSend(string message)
        {
            byte[] sendData = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(sendData, sendData.Length, sendEndPoint);
        }

        public void SetSendDestination(string ip, int port)
        {
            destination = IPAddress.Parse(ip);
            sendEndPoint = new IPEndPoint(destination, port);
        }

        public void SetListenDestination(string ip, int port)
        {
            destination = IPAddress.Parse(ip);
            receiveIpEndPoint = new IPEndPoint(destination, port);
            _udpClient = new UdpClient(receiveIpEndPoint);
        }
    }
}
