using System;
using System.Collections.Generic;
using System.Text;

namespace TableUiReceiver
{
    // TODO Markers need to be persistent elements so they can smooth themselves
    internal class Marker
    {
        // Main constructor for creatring markers to remember
        public Marker(int id, string type)
        {
            this.id = id;
            this.type = type;
        }
        // Overload method for use in JSON deserialization
        public Marker() { }

        public int id { get; set; }
        public int[] location
        {
            get { return _location; }
            set { _location = SmoothLocation(value);}
        }
        public float rotation
        { 
            get { return _rotation; }
            set { _rotation = SmoothRotation(value);}
        }
        public string type { get; set; }

        // Internal variables for smoothing so the setter doesn't call itself recursively
        private int[] _location;
        private float _rotation;

        private int[] SmoothLocation(int[] incomingLocation)
        {
            if (_location == null || _location.Length == 0)
            {
                _location = incomingLocation;
                return _location;
            }

            for (int i = 0; i < incomingLocation.Length; i++)
            {
                if (Math.Abs(location[i] - incomingLocation[i]) >= 1)
                {
                    _location[i] = incomingLocation[i];
                }
            }
            return _location;
        }

        private float SmoothRotation(float incomingRotation)
        {
            if (Math.Abs(_rotation - incomingRotation) >= Math.PI/64)
            {
                _rotation = incomingRotation;
            }

            return _rotation;
        }

        public void Update(Marker marker)
        {
            this.location = marker.location;
            this.rotation = marker.rotation;
        }
    }
}
