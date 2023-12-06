using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableUiReceiver;

namespace TableLibTests
{
    public class objectTests
    {
        [SetUp]
        public void Setup()
        {
            
        }
        [TearDown] 
        public void TearDown() 
        { 
        }
        [Test]
        public void ParseTest()
        {
            string json = "{2: {'9fe8d177-5abd-4d7e-bdc9-a16ae43ec81b': {'location': [-1083, -503, 0], 'rotation': -1.877084173758166}, 'cae1e2af-4249-4cb0-b60e-3d793525e8d5': {'location': [-1087, -236, 0], 'rotation': -0.24175284107460282}}}";
            List<Marker> markers = Parser.Parse(json);
            Assert.That(markers.Count, Is.EqualTo(2));
        }

    }
}
