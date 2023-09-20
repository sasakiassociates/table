using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    internal class MarkerFactory
    {
        public static Marker CreateMarker(int id)
        {
            return new Marker(id);
        }

        public static List<Marker> CreateMarkers(List<int> ids)
        {
            List<Marker> markers = new List<Marker>();
            for (int i = 0; i < ids.Count; i++)
            {
                markers.Add(CreateMarker(ids[i]);
            }
            return markers;
        }
    }
}
