using Sasaki;
using System.Net;
using System.Net.Sockets;

namespace TableLibTests
{
    public class Tests
    {
        UdpClient udpClient;
        IPEndPoint ep;

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void UdpBeginReceive()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(ep);

            UdpState state = new UdpState();
        }
    }
}