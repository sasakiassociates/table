using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    internal class MarkerFactory
    {
        public static Marker CreateMarker(int id, string type)
        {
            return new Marker(id, type);
        }

        public static List<Marker> CreateMarkers(List<int> ids, List<string> types)
        {
            List<Marker> markers = new List<Marker>();
            for (int i = 0; i < ids.Count; i++)
            {
                markers.Add(CreateMarker(ids[i], types[i]));
            }
            return markers;
        }
    }
}
