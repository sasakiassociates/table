using System.Net;
using NUnit.Framework;

namespace StrategyObjectTest
{

    [TestFixture]
    public class Tests
    {

        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void HttpGetRequestTest()
        {
            bool result = false;
            Console.WriteLine("Testing HttpGetRequest");
            Assert.IsFalse(result, "1 should not be prime");
        }
    }
}