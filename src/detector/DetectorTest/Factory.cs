using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectorTest
{
    internal class Factory
    {
        public static List<Marker> MakeMarkers(int dictLength, Observer observer)
        {
            List<Marker> markerList = new List<Marker>();
            for (int i = 0; i < dictLength; i++)
            {
                Marker marker = new Marker(i);
                marker.AttachObserver(observer);

                if (i == dictLength - 1)
                {
                    marker.SetType("camera");
                }
                else if (i >= dictLength - 5)
                {
                    marker.SetType("rotator");
                }
                else if (i >= dictLength - 9)
                {
                    marker.SetType("slider");
                }
                else
                {
                    marker.SetType("model");
                }

                markerList.Add(marker);
            }

            return markerList;
        }
    }
}
