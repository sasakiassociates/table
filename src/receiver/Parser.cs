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
                Dictionary<string, Marker> deserialJsonList = JsonConvert.DeserializeObject<Dictionary<string, Marker>>(json);

                foreach (var deserialJson in deserialJsonList)
                {
                    Marker marker = deserialJson.Value;
                    result.Add(marker);
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
