using Grasshopper.Kernel.Types.Transforms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUI
{
    internal class Parser
    {
        public List<Marker> markers = new();
        public ParsedData data = new();

        public void Parse(string jsonString)
        {
            if (jsonString == null || jsonString.Length < 1)
            {
                return;
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
