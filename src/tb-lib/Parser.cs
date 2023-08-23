using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TableLib
{
    public interface IParser
    {
        object Parse(string json);
    }

    public class ParserFactory
    {
        public static IParser GetParser(string parserType)
        {
            switch (parserType)
            {
                case "Marker":
                    return new JsonToMarkerParser();
                default:
                    return null;
            }
        }
    }

    public class JsonToMarkerParser : IParser
    {
        public object Parse(string json)
        {
            List<Marker> markers = new List<Marker>();

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
                    markers.Add(marker);
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error parsing JSON: " + e.Message);
                return null;
            }

            return markers;
        }
    }
}
