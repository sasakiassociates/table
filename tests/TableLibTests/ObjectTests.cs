using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableLib;

namespace TableLibTests
{
    public class ObjectTests
    {
        Invoker _invoker;
        Repository testRepository;

        [SetUp]
        public void Setup()
        {
            _invoker = Invoker.Instance;
            // testRepository = Repository.Instance;
        }
        [TearDown]
        public void TearDown()
        {
            _invoker.Disconnect();
            //testRepository.Disconnect();
            //_invoker.DisconnectAndEndDetection();
        }

        [Test]
        public void RepoWaitSetupTest()
        {
            bool hasSetup = false;

            while (!hasSetup)
            {
                testRepository.UdpSend("SETUP 10, 10");
                string response = testRepository.UdpReceive();
                if (response == "READY")
                {
                    hasSetup = true;
                }
            }
            Assert.IsTrue(hasSetup);
        }

        [Test]
        public void LaunchThroughInvokerTest()
        {
            string path = "..\\..\\..\\..\\..\\src\\tb-detection\\";
            _invoker.LaunchDetectionProgram(path);
            // We need to run setup for the detection to start
            // _invoker.SetupDetection(10, 10);
            _invoker.SetupDetection(10, 10);
        }
        [Test]
        public void SetupThroughInvokerTest()
        {
            _invoker.SetupDetection(10, 10);
        }
        [Test]
        public void ReadThroughInvokerTest()
        {
            List<Marker> response = (List<Marker>)_invoker.Run();
            foreach (Marker marker in response)
            {
                Console.WriteLine(marker.id);
            }
            Assert.That(response, Is.Not.Null);
        }
        [Test]
        public void EndThroughInvokerTest()
        {
            _invoker.StopDetectionProgram();
            if (_invoker.isRunning)
            {
                Console.WriteLine("Detection program is still running");
                Assert.Fail();
            }
            Assert.IsTrue(true);
        }

        // We're having problems with the second time the video closes in Grasshopper
        // It stops being able to hear the detection program saying it's READY
        [Test]
        public void StartEndStartInvoker()
        {
            _invoker.LaunchDetectionProgram("..\\..\\..\\..\\..\\src\\tb-detection\\");
            _invoker.ExecuteWithTimeLimit(TimeSpan.FromMilliseconds(5000), () => _invoker.SetupDetection(10, 10));
            _invoker.StopDetectionProgram();

            _invoker.LaunchDetectionProgram("..\\..\\..\\..\\..\\src\\tb-detection\\");
            _invoker.ExecuteWithTimeLimit(TimeSpan.FromMilliseconds(5000), () => _invoker.SetupDetection(10, 10));
            _invoker.StopDetectionProgram();

            Assert.IsFalse(_invoker.isRunning);
        }
        //[Test]
        /*public void EndThroughInvokerTest()
        {
            _invoker = new Invoker();
            _invoker.DisconnectAndEndDetection();
        }
        [Test]
        public void SetupThroughInvoker()
        {
            _invoker = new Invoker();
            _invoker.SetupDetection(10, 20);
        }*/

        // See if the repository object can send a message
        /*[Test]
        public void RepositoryEndTest()
        {
            testRepository.Connect();
            testRepository.UdpSend("END");

            Assert.IsTrue(true);
        }
        [Test]
        public void RepositoryBoolTest()
        {
            testRepository.Connect();
            Assert.IsTrue(testRepository.IsConnected());
        }
*/
        // See if the repository object can receive a message
        [Test]
        public void RepositoryReceiveTest()
        {

            testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive(1000);

            Console.WriteLine(response);

            Assert.That(response.Length, Is.AtLeast(0));
        }

        // See if the repository object can send a message, receive a response, and then parse it
        [Test]
        public void RepoToParseTest()
        {

            testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive(1000);

            Console.WriteLine(response);

            IParser parser = ParserFactory.GetParser("Marker");
            List<Marker> parsedResponse = (List<Marker>)parser.Parse(response);

            Console.WriteLine(parsedResponse);
        }
        [Test]
        public void RepoSetupTest()
        {

            testRepository.UdpSend("SETUP 10 10");

            Assert.IsTrue(true);
        }

        // See if the invoker object can get a response from the detection program via the repository
        /*[Test]
        public void InvokerParseTest()
        {
            Invoker invoker = new Invoker();
            IParser strategy = ParserFactory.GetParser("Marker");
            invoker.SetParseStrategy(strategy);

            object testReturn = invoker.Run();
            Console.WriteLine(testReturn);
            Assert.That(testReturn, Is.Not.Null);
        }*/

        // See if the parser works
        [Test]
        public void ParseTest()
        {
            string testString = "{'48': {'id': 48, 'location': [316, 550], 'rotation': 140}, " +
                "'44': {'id': 44, 'location': [281, 496], 'rotation': 144}, " +
                "'39': {'id': 39, 'location': [239, 491], 'rotation': 143}, " +
                "'1': {'id': 1, 'location': [334, 454], 'rotation': 143}, " +
                "'45': {'id': 45, 'location': [291, 450], 'rotation': 142}, " +
                "'40': {'id': 40, 'location': [249, 444], 'rotation': 145}, " +
                "'47': {'id': 47, 'location': [310, 602], 'rotation': 139}}";
            IParser parser = ParserFactory.GetParser("Marker");
            List<Marker> testList = (List<Marker>)parser.Parse(testString);

            foreach (Marker marker in testList)
            {
                Console.WriteLine(marker.id);
            }

            Assert.That(testList.Count, Is.EqualTo(7));
        }
    }
}
