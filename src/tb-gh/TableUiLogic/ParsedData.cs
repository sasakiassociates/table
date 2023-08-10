using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiLogic
{
    public class ParsedData
    {
        public List<int> ids = new List<int>();
        public List<int[]> locations = new List<int[]>();
        public List<int> rotations = new List<int>();

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
