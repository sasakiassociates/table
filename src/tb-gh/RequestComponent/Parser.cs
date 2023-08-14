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
