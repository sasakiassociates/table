using Newtonsoft.Json;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal class JsonParse
    {

        public ParsedData Parse(string jsonString)
        {
            List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

            List<int> ids = new();
            List<Point2d> locations = new();
            List<int> rotations = new();

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
                        locations.Add(new Point2d(marker.location[0], marker.location[1]));
                    }
                    else
                    {
                        locations.Add(new Point2d(0, 0));
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
            public int rotation { get; set; }
        }

        public class ParsedData
        {
            public List<int> ids { get; set; }
            public List<Point2d> locations { get; set; }
            public List<int> rotations { get; set; }
        }
    }
}
