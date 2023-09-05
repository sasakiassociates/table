using System;
using System.Collections.Generic;
using System.Text;

namespace TableLib
{
    // TODO Markers need to be persistent elements so they can smooth themselves
    public class Marker
    {
        public int id { get; set; }
        public int[] location
        {
            get { return location; }
            set { location = SmoothLocation(value); }
        }
        public float rotation
        { 
            get { return rotation; } 
            set { rotation = SmoothRotation(value); }
        }

        private int[] SmoothLocation(int[] incomingLocation)
        {
            if (location == null || location.Length == 0)
            {
                location = incomingLocation;
                return location;
            }

            for (int i = 0; i < incomingLocation.Length; i++)
            {
                if (Math.Abs(location[i] - incomingLocation[i]) >= 5)
                {
                    location[i] = incomingLocation[i];
                }
            }
            return location;
        }

        private float SmoothRotation(float incomingRotation)
        {
            if (Math.Abs(rotation - incomingRotation) >= Math.PI/8)
            {
                rotation = incomingRotation;
            }

            return rotation;
        }
    }
}
