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
        public void LaunchTest()
        {
            Launcher.LaunchDetectionProgram();
        }

    }
}
