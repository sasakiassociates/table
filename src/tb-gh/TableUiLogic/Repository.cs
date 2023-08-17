using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TableUiLogic
{
    public class Repository
    {
        private static Repository _instance;
        private static readonly object padlock = new object();          // Makes the singelton class thread safe

        private UdpClient udpClient;
        private int port = 5005;

        public string response;
        public List<Marker> parsedResponse;

        private JsonToMarkerParser parser = new JsonToMarkerParser();   // David: Parser is too coupled to the incoming data format?

        public static Repository Instance
        {
            get
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new Repository();
                    }
                    return _instance;
                }
            }
        }

        // Needs to be a singleton and strategy+singleton was overkill
        public string UdpReceive(int port, int expire = 0)
        {
            try
            {
                if (udpClient == null)
                {
                    udpClient = new UdpClient(port);
                }
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                if (expire != 0)
                {
                    udpClient.Client.ReceiveTimeout = expire;
                }
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                return returnData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void UdpSend(int port, string message)
        {
            try
            {
                if (udpClient == null)
                {
                    udpClient = new UdpClient(port);
                }
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                udpClient.Send(sendBytes, sendBytes.Length, RemoteIpEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<Marker> GetMarkers()
        {
            parsedResponse = parser.Parse(response);
            return parsedResponse;
        }
    }
}
