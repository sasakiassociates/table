using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TableUiReceiver
{
    public class Parser
    {
        public static List<Marker> Parse(string json)
        {
            List<Marker> result = new List<Marker>();

            if (json == null)
            {
                return null;
            }

            try
            {
                Dictionary<string, Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Marker>>>(json);

                foreach (var deserialJson in deserialJsonList)
                {
                    if (deserialJson.Key == "marker")
                    {
                        foreach (var marker in deserialJson.Value)
                        {
                            Marker incomingMarker = marker.Value;
                            incomingMarker.id = int.Parse(marker.Key);
                            incomingMarker.type = deserialJson.Key;
                            result.Add(incomingMarker);
                        }
                    }
                    else if (deserialJson.Key == "zone")
                    {
                        // Ignore it
                        continue;
                    }
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error parsing JSON: " + e.Message);
                return null;
            }

            return result;
        }
    }
}
