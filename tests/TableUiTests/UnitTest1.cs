using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TableUiLogic;

namespace TableUiTests
{
    // Issue with tests was that the NUnit project didn't support .Net Framework 4.5.2
    [TestClass]
    public class UnitTest1
    {
        private Repository _repository = Repository.Instance;
        private UdpClient receiveClient;

        private int listenPort = 7000;
        private int sendPort = 7001;
        IPEndPoint ep;
        private string sendMessage = "Hello, UDP Server!";

        [TestInitialize]
        public void TestInitialize()
        {
            receiveClient = new UdpClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            receiveClient.Dispose();
        }

        [TestMethod]
        public void TestMarkerConnection()
        {
            Marker _testMarker = new Marker();
            _testMarker.id = 1;

            Assert.AreEqual(1, _testMarker.id);
        }

        [TestMethod]
        public void TestMarkerLocation()
        {
            Marker _testMarker = new Marker();
            _testMarker.location = new int[] { 1, 2, 3 };

            Assert.AreEqual(1, _testMarker.location[0]);
            Assert.AreEqual(2, _testMarker.location[1]);
            Assert.AreEqual(3, _testMarker.location[2]);
        }

        [TestMethod]
        static async Task UdpReceiveTest()
        {
            while (true)
            {
                await Task.Run(() =>
                {
                    IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Loopback, 5005);
                    receiveClient.Client.Bind(receiveEndPoint);

                    receiveClient.Client.ReceiveTimeout = 5000;

                    Byte[] receiveData = receiveClient.Receive(ref receiveEndPoint);
                    string receiveString = Encoding.ASCII.GetString(receiveData);
                });
            }
        }
        
        /*[TestMethod]
        public async Task UdpPortReceiveTest()
        {
            await Task.Run(() =>
            {
                UdpClient receiveClient = new UdpClient();

                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Loopback, listenPort);
                receiveClient.Client.Bind(receiveEndPoint);

                udpClient.Client.ReceiveTimeout = 5000;

                Byte[] receiveData = receiveClient.Receive(ref receiveEndPoint);
                string receiveString = Encoding.ASCII.GetString(receiveData);
                
            });
            
            udpClient.Close();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void UdpPortSendTest()
        {
            udpClient = new UdpClient();
            UdpClient udpClient2 = new UdpClient();
            
            // Define the remote endpoint you want to send data to
            IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Loopback, sendPort);
            IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Loopback, listenPort);
            udpClient2.Client.Bind(receiveEndPoint);
            byte[] sendData = Encoding.ASCII.GetBytes(sendMessage);

            // Act
            udpClient.Send(sendData, sendData.Length, sendEndPoint);

            udpClient.Client.ReceiveTimeout = 5000;

            // I guess UDP receive isn't getting any messages cause it happens in real time, so something else needs to send data at the same time as receiving
            
            Byte[] receiveData = udpClient2.Receive(ref receiveEndPoint);
            string receiveString = Encoding.ASCII.GetString(receiveData);

            udpClient.Close();

            Assert.IsTrue(true);
        }*/

        
    }
}
