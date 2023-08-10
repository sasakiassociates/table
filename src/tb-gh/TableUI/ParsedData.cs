using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal class ParsedData
    {
        public List<int> ids = new();
        public List<int[]> locations = new();
        public List<int> rotations = new();

        public void add_id(int id)
        {
            this.ids.Add(id);
        }
        public void add_location(int[] location)
        {
            this.locations.Add(location);
        }
        public void add_rotation(int rotation)
        {
            this.rotations.Add(rotation);
        }
    }
}
