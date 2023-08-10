using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiLogic
{
    internal class Parser
    {
        public List<Marker> markers = new List<Marker>();
        public ParsedData data = new ParsedData();

        public void Parse(string jsonString)
        {

            List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

            foreach (var deserialJson in deserialJsonList)
            {
                foreach (var kvp in deserialJson)
                {

                    Marker marker = kvp.Value;
                    markers.Add(marker);

                }
            }
        }

        public ParsedData MakeDataList()
        {
            foreach (Marker marker in markers)
            {
                int id = marker.id;
                int[] location = marker.location;
                int rotation = marker.rotation;

                data.add_id(id);
                data.add_location(location);
                data.add_rotation(rotation);
            }

            return data;
        }
    }
}
