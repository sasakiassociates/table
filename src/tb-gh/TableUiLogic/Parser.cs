using Newtonsoft.Json;
using System.Collections.Generic;

namespace TableUiLogic
{
    internal class JsonToMarkerParser
    {
        public List<Marker> markers = new List<Marker>();

        public List<Marker> Parse(string jsonString)
        {
            if (jsonString == null)
            {
                return null;
            }

            List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

            foreach (var deserialJson in deserialJsonList)
            {
                foreach (var kvp in deserialJson)
                {

                    Marker marker = kvp.Value;
                    markers.Add(marker);

                }
            }
            return markers;
        }
    }
}
