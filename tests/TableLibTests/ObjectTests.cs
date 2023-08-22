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

        [SetUp]
        public void Setup()
        {
            _invoker = new Invoker();
        }
        [TearDown]
        public void TearDown()
        {
            _invoker.EndDetection();
        }

        [Test]
        public void LaunchThroughInvokerTest()
        {
            _invoker.LaunchDetection();
        }
        [Test]
        public void ReadThroughInvokerTest()
        {
            object response = _invoker.Run();
            Console.WriteLine(response);
        }
        [Test]
        public void EndThroughInvokerTest()
        {
            _invoker.EndDetection();
        }

        // See if the repository object can send a message
        [Test]
        public void RepositorySendTest()
        {
            Repository testRepository = Repository.Instance;
            testRepository.UdpSend("END");

            Assert.IsTrue(true);
        }

        // See if the repository object can receive a message
        [Test]
        public void RepositoryReceiveTest()
        {
            Repository testRepository = new Repository();
            testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive();

            Console.WriteLine(response);

            Assert.That(response.Length, Is.AtLeast(0));
        }

        // See if the repository object can send a message, receive a response, and then parse it
        [Test]
        public void RepoToParseTest()
        {
            Repository testRepository = new Repository();

            testRepository.UdpSend("SEND");

            string response = testRepository.UdpReceive();

            Console.WriteLine(response);

            IParser parser = ParserFactory.GetParser("Marker");
            List<Marker> parsedResponse = (List<Marker>)parser.Parse(response);

            Console.WriteLine(parsedResponse);
        }

        // See if the invoker object can get a response from the detection program via the repository
        [Test]
        public void InvokerParseTest()
        {
            Invoker invoker = new Invoker();
            IParser strategy = ParserFactory.GetParser("Marker");
            invoker.SetParseStrategy(strategy);

            object testReturn = invoker.Run();
            Console.WriteLine(testReturn);
            Assert.That(testReturn, Is.Not.Null);
        }

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
