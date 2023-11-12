using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TableUiReceiver
{
    public class Parser
    {
        public static List<ParsableObject> Parse(string json)
        {
            List<ParsableObject> result = new List<ParsableObject>();

            if (json == null)
            {
                return null;
            }

            try
            {
                Dictionary<string, Dictionary<string, ParsableObject>> deserialJsonList = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ParsableObject>>>(json);

                foreach (var deserialJson in deserialJsonList)
                {
                    foreach (var item in deserialJson.Value)
                    {
                        if (item.Value.GetType() == typeof(Marker))
                        {
                            Marker incomingMarker = (Marker)item.Value;
                            incomingMarker.uuid = int.Parse(item.Key);
                            incomingMarker.type = deserialJson.Key;
                            result.Add(incomingMarker);
                        }
                        else if (item.Value.GetType() == typeof(Collection))
                        {
                            Collection collection = (Collection)item.Value;
                            collection.uuid = int.Parse(item.Key);
                            collection.type = deserialJson.Key;
                            result.Add(collection);
                        }
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
