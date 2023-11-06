using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    public class Zone
    {
        public int id { get; set; }
        public int[] bounds { get; set; }
        public int[] size { get; set; }
        public string name { get; set; }
    }
}
