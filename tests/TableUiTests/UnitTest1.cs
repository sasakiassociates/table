using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TableUiLogic;

namespace TableUiTests
{
    // Issue with tests was that the NUnit project didn't support .Net Framework 4.5.2
    [TestClass]
    public class UnitTest1
    {
        private Repository _repository = Repository.Instance;
        private UdpClient udpClient;

        private int listenPort = 7000;
        private int sendPort = 7001;
        IPEndPoint ep;
        private string sendMessage = "Hello, UDP Server!";

        [TestInitialize]
        public void TestInitialize()
        {
            udpClient = new UdpClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            udpClient.Dispose();
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

        // Need to handle when there's no active port open for OpenCV

        [TestMethod]
        public void VerifyClientConnection()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, 5005);

            try
            {
                udpClient.Connect(iPEndPoint);
                Assert.IsTrue(true);
                Console.WriteLine("UDP client: " + udpClient.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void UdpPortSendTest()
        {
            udpClient = new UdpClient();

            // Define the remote endpoint you want to send data to
            IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Loopback, sendPort);

            byte[] sendData = Encoding.ASCII.GetBytes(sendMessage);

            // Act
            var bytes = udpClient.Send(sendData, sendData.Length, sendEndPoint);
            Assert.IsTrue(bytes > 0);
            Assert.IsTrue(true);
            Console.WriteLine(bytes);
        }

        [TestMethod]
        public void UdpPortCreateReceiver()
        {
            var bytes = Array.Empty<byte>();
            try
            {
                // Define the remote endpoint you want to listen to
                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Loopback, listenPort);

                // TODO test different constructors
                udpClient = new UdpClient(receiveEndPoint);

                udpClient.Client.ReceiveTimeout = 5000;

                // Receive blocks the thread and waits til it receives something
                bytes = udpClient.Receive(ref receiveEndPoint);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(bytes.Length == 0);
            }
        }

        [TestMethod]
        public void UdpReceiveTimeout()
        {
            var bytes = Array.Empty<byte>();
            try
            {
                // Define the remote endpoint you want to listen to
                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Loopback, 0);

                // TODO test different constructors
                udpClient = new UdpClient(receiveEndPoint);
                // This is necessary to prevent the test from hanging
                udpClient.Client.ReceiveTimeout = 5000;

                // Receive blocks the thread and waits til it receives something
                bytes = udpClient.Receive(ref receiveEndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.SocketErrorCode);
                Console.WriteLine(ex.Message);
            }
        }

        //TODO test connection again with the UDP client
        [TestMethod]
        public void UdpReceiveVerification()
        {
            var bytes = Array.Empty<byte>();
            try
            {
                // Define the remote endpoint you want to listen to
                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

                // TODO test different constructors
                udpClient = new UdpClient(receiveEndPoint); 
                
                udpClient.Connect(receiveEndPoint);
                // This is necessary to prevent the test from hanging
                udpClient.Client.ReceiveTimeout = 5000;

                // Receive blocks the thread and waits til it receives something
                bytes = udpClient.Receive(ref receiveEndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.SocketErrorCode);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // We expect no bytes to be received
                Assert.IsFalse(bytes.Length > 0, $"Bytes were received : {bytes.Length}");
            }
        }

        [TestMethod]
        public void UdpSendReceive()
        {
            udpClient = new UdpClient(5005);
            try
            {
                udpClient.Connect(IPAddress.Loopback, 5005);
                
                string message = "Hello, UDP Server!";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);

                udpClient.Send(sendBytes, sendBytes.Length);
                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                // This is necessary to prevent the test from hanging
                udpClient.Client.ReceiveTimeout = 5000;

                // Receive blocks the thread and waits til it receives something
                var bytes = udpClient.Receive(ref endPoint);
                string receivedString = Encoding.ASCII.GetString(bytes);
                Console.WriteLine("Received: " + receivedString);

                Assert.IsTrue(bytes.Length > 0);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }

        // Listens to UDP info sent from another program
        // Will time out after 5 seconds if no message is received
        // Finding: don't do uspClient.Client.Connect() if you want to receive from any IP
        [TestMethod]
        public void UdpReceiveFromExternalProgram()
        {
            udpClient = new UdpClient(5005);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                udpClient.Client.ReceiveTimeout = 5000;

                // Receive blocks the thread and waits til it receives something
                var bytes = udpClient.Receive(ref endPoint);
                string receivedString = Encoding.ASCII.GetString(bytes);
                Console.WriteLine("Received: " + receivedString);

                Assert.IsTrue(bytes.Length > 0);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }

        // Test begin and end receive functions
        // Reference: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient.beginreceive?view=net-7.0
        struct UdpState
        {
            public UdpClient udpClient;
            public IPEndPoint endPoint;
        }
        bool messageReceived = false;
        public void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient udpClient = ((UdpState)(ar.AsyncState)).udpClient;
            IPEndPoint endPoint = ((UdpState)(ar.AsyncState)).endPoint;

            Byte[] receiveBytes = udpClient.EndReceive(ar, ref endPoint);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            Console.WriteLine("Received: " + receiveString);
            messageReceived = true;
        }

        [TestMethod]
        public void UdpBeginReceive()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            UdpClient udpClient = new UdpClient(endPoint);

            UdpState udpState = new UdpState();
            udpState.udpClient = udpClient;
            udpState.endPoint = endPoint;

            try
            {
                // Begin receive runs a non-blocking instance of the receive function so we can do other things while is waits for a message
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);
                
                while (!messageReceived)
                {
                    Thread.Sleep(100);
                }

                Assert.IsTrue(messageReceived);
                
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }   
        // Get python script to send a generic message for testing
        // Get the connection between the TableLib and Grasshopper script to work (eventual)
    }
}
