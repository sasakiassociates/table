using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TableLib
{
    public class Repository
    {
        private static Repository _instance;
        private static readonly object _lock = new object();

        private UdpClient _receiveUdpClient;
        private int _port = 5005;

        private IPAddress destination = IPAddress.Loopback;
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
            receiveIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            // Send to localhost, the same machine (Can be changed to send to another machine)
            sendEndPoint = new IPEndPoint(destination, _port);

            _receiveUdpClient = new UdpClient(receiveIpEndPoint);
        }

        public string UdpReceive(int expire = 0)
        {
            try
            {
                if (_receiveUdpClient == null)
                {
                    return "UDP client is null";
                }
                if (expire != 0)
                {
                    _receiveUdpClient.Client.ReceiveTimeout = expire;
                }
                byte[] receiveBytes = _receiveUdpClient.Receive(ref receiveIpEndPoint);
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
            _receiveUdpClient.Close();
        }

        public void UdpSend(string message)
        {
            UdpClient _sendUdpClient = new UdpClient(sendEndPoint);

            byte[] sendData = Encoding.ASCII.GetBytes(message);
            _sendUdpClient.Send(sendData, sendData.Length, sendEndPoint);
            _sendUdpClient.Close();
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
            _receiveUdpClient = new UdpClient(receiveIpEndPoint);
        }
    }
}
