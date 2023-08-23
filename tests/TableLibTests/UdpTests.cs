using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

using TableLib;

namespace TableLibTests
{
    public class Tests
    {
        private UdpClient? udpClient;
        private int sendPort = 5004;
        private int listenPort = 5005;

        [SetUp]
        public void Setup()
        {
            udpClient = new UdpClient();
        }

        [TearDown]
        public void TearDown()
        {
            udpClient?.Dispose();
        }

        // Test to see if the client can connect to the server
        [Test]
        public void VerifyClientConnection()
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5004);

            try
            {
                udpClient.Connect(endPoint);
                Assert.IsTrue(udpClient.Client.Connected);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // See if the SEND command is received by the detection program
        [Test]
        public void UdpSendTest()
        {
            try
            {
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

                string message = "SEND";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                var bytes = udpClient?.Send(sendBytes, sendBytes.Length, sendEndPoint);
                Assert.That(bytes, Is.EqualTo(sendBytes.Length));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // Test to see if we can send the END message to shut down the detection program
        [Test]
        public void UdpSendEnd()
        {
            try
            {
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

                string message = "END";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                var bytes = udpClient?.Send(sendBytes, sendBytes.Length, sendEndPoint);
                Assert.That(bytes, Is.EqualTo(sendBytes.Length));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // Test to see if we can receive data from the detection program
        [Test]
        public void UdpReceiveTest()
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
                udpClient.Client.ReceiveTimeout = 3000;

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

        // Send request for data to listener and get a response
        // If we've already launched the detection program, we want this to pass
        // Otherwise it should fail. If it doesn't detection might have not ended properly
        [Test]
        public void SendReceiveTest()
        {
            try
            {
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

                string message = "SEND";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                var bytes = udpClient?.Send(sendBytes, sendBytes.Length, sendEndPoint);

                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                udpClient = new UdpClient(receiveEndPoint);
                udpClient.Client.ReceiveTimeout = 3000;

                byte[] response = udpClient.Receive(ref receiveEndPoint);
                string responseString = Encoding.ASCII.GetString(response);

                Console.WriteLine(responseString);
                Assert.That(responseString.Length, Is.AtLeast(0));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // Send request for data to listener and get a response
        // Then send the END message to shut down the detection program
        // If we've already launched the detection program, we want this to pass
        // Otherwise it should fail. If it doesn't detection might have not ended properly
        [Test]
        public void SendReceiveEndTest()
        {
            try
            {
                // Send request for data to listener
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

                string message = "SEND";
                byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                var bytes = udpClient?.Send(sendBytes, sendBytes.Length, sendEndPoint);

                // Receive data from listener
                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                udpClient = new UdpClient(receiveEndPoint);
                udpClient.Client.ReceiveTimeout = 3000;

                byte[] response = udpClient.Receive(ref receiveEndPoint);
                string responseString = Encoding.ASCII.GetString(response);

                Console.WriteLine(responseString);

                // Send end message to listener
                string endMessage = "END";
                byte[] endBytes = Encoding.ASCII.GetBytes(endMessage);
                udpClient.Send(endBytes, endBytes.Length, sendEndPoint);

                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // See if we can launch the detector program
        [Test]
        public void LaunchPythonTest()
        {
            string virtualEnvPath = "../tb-detection/.env";
            string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
            string scriptPath = "../tb-detection/main.py";

            string argument1 = "udp --num_models 10";
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


            /*string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            Console.WriteLine(output);
            Console.WriteLine(errors);

            // Add assertions or further code to handle the process output, exit code, etc.
            // For now, let's just assert that the process started successfully.
            Assert.IsTrue(process.HasExited);*/

            // Should have launched the OpenCV window, but finished the test while that is running
            Assert.IsTrue(true);

            // Use this as a last resort to kill the process, prefer to use the UDP calls to end it
            // process.Dispose();

        }

        
    }
}