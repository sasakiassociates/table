using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TableUiReceiver
{
    public class Parser
    {
        public static (List<int> ids, List<float> rotations, List<int[]> locations) Parse(string json)
        {
            List<int> ids = new List<int>();
            List<float> rotations = new List<float>();
            List<int[]> locations = new List<int[]>();

            if (json == null)
            {
                return (null, null, null);
            }

            try
            {
                Dictionary<string, Marker> deserialJsonList = JsonConvert.DeserializeObject<Dictionary<string, Marker>>(json);

                foreach (var deserialJson in deserialJsonList)
                {
                    Marker marker = deserialJson.Value;

                    ids.Add(marker.id);
                    rotations.Add(marker.rotation);
                    locations.Add(marker.location);
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error parsing JSON: " + e.Message);
                return (null, null, null);
            }

            return (ids, rotations, locations);
        }
    }
}
