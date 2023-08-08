using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal class ParsedData
    {
        public List<int> ids { get; set; }
        public List<int[]> locations { get; set; }
        public List<int[]> rotations { get; set; }
    }
}
