using Grasshopper.Kernel.Types.Transforms;
using Newtonsoft.Json;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal class Parser
    {

        public ParsedData Parse(string jsonString)
        {
            // TODO change the format of the JSON to be a list of dictionaries
            List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

            List<int> ids = new();
            List<int[]> locations = new();
            List<int[]> rotations = new();

            foreach (var deserialJson in deserialJsonList)
            {
                foreach (var kvp in deserialJson)
                {
                    string key = kvp.Key;
                    Marker marker = kvp.Value;

                    int id = marker.id;
                    ids.Add(id);
                    rotations.Add(marker.rotation);
                    if (marker.location != null)
                    {
                        int[] point = { marker.location[0], marker.location[1], 0 };
                        locations.Add(point);
                    }
                    else
                    {
                        // TODO figure out how to form a Rotation object the Rhino geometry kernel (but move that to the Adapter)
                        int[] point = { 0, 0, 0 };
                        locations.Add(point);
                    }

                }
            }

            return new ParsedData
            {
                ids = ids,
                locations = locations,
                rotations = rotations
            };
        }

        private class Marker
        {
            public int id { get; set; }
            public int[] location { get; set; }
            public int[] rotation { get; set; }
        }

/*        public class ParsedData
        {
            public List<int> ids { get; set; }
            public List<Point2d> locations { get; set; }
            public List<int> rotations { get; set; }
        }*/
    }
}
