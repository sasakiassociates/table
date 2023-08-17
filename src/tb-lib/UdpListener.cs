using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace TableLib
{
    public class UdpListener
    {
        private const int Port = 12345;
        private UdpClient udpClient;
        private Thread listenerThread;

        public event Action ReceivedSendCommand;

        public UdpListener()
        {
            udpClient = new UdpClient(Port);
            listenerThread = new Thread(Listen);
            listenerThread.Start();
        }

        private void Listen()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

            while (true)
            {
                byte[] data = udpClient.Receive(ref endPoint);
                string receivedText = Encoding.UTF8.GetString(data);

                if (receivedText.Trim().ToLower() == "send")
                {
                    ReceivedSendCommand?.Invoke();
                }
            }
        }

        public void Stop()
        {
            udpClient.Close();
            listenerThread.Join();
        }
    }
}
