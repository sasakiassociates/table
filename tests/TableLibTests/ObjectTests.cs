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
            //_invoker.Disconnect();
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
                Console.WriteLine(marker.rotation);
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

        [Test]
        public void GetMarkerTypes()
        {
            List<Marker> response = (List<Marker>)_invoker.Run();
            foreach (Marker marker in response)
            {
                if (marker.id == 99)
                {
                    Assert.That(marker.type, Is.EqualTo("camera"));
                }
            }
        }

        [Test]
        public void MarkerUpdate()
        {
            Marker marker1 = new Marker();
            marker1.id = 1;
            int[] location1 = { 1, 2 };
            marker1.location = location1;
            marker1.rotation = 3;

            Marker marker2 = new Marker();
            marker2.id = 2;
            int[] location2 = { 4, 5 };
            marker2.location = location2;
            marker2.rotation = 6;

            marker1.Update(marker2);
            Assert.That(marker2.rotation, Is.EqualTo(marker1.rotation));
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

        // See if the repository object can receive a message
        [Test]
        public void RepositoryReceiveTest()
        {
            Repository testRepository = new Repository();
            //testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive(1000);

            Console.WriteLine(response);

            Assert.That(response.Length, Is.AtLeast(0));

            testRepository.Disconnect();
            testRepository = null;
        }

        // See if the repository object can send a message, receive a response, and then parse it
        [Test]
        public void RepoToParseTest()
        {

            testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive(1000);

            Console.WriteLine(response);

            JsonToMarkerParser parser = new JsonToMarkerParser();
            List<Marker> parsedResponse = (List<Marker>)parser.Parse(response);

            Console.WriteLine(parsedResponse);
        }
        [Test]
        public void RepoSetupTest()
        {

            testRepository.UdpSend("SETUP 10 10");

            Assert.IsTrue(true);
        }

        [Test]
        public void BuildDictTest()
        {
            _invoker.BuildDict(10, 10);
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine(_invoker.refDict[i]);
            }
            Assert.That(_invoker.refDict[2].Equals("model"));
            Assert.That(_invoker.refDict[11].Equals("variable"));
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

        /*// See if the parser works
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
        }*/

        [Test]
        public void PairingTest()
        {
            List<string> markers = new List<string> { "Marker1", "Marker2", "Marker3", "Marker4" };
            List<string> breps = new List<string> { "Brep1", "Brep2", "Brep3" };

            Dictionary<string, string> markerBrepPairs = markers
                .Zip(breps, (marker, brep) => new { Marker = marker, Brep = brep })
                .ToDictionary(pair => pair.Marker, pair => pair.Brep);

            // Now, markerBrepPairs contains the pairings of markers and breps up to the length of breps
            foreach (var pair in markerBrepPairs)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
                
            }
            Assert.That(markerBrepPairs.Count, Is.EqualTo(3));
        }
    }
}
