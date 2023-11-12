using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    public class Collection : ParsableObject
    {
        public int uuid { get; set; }
        public string type { get; set; }
        public List<ParsableObject> objects { get; set; }
    }
}
