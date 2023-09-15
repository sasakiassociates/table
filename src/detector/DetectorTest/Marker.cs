using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectorTest
{
    internal class Marker
    {
        private int id;
        private List<Observer> observers = new List<Observer>();

        private double rotation = 0;
        private double prevRotation = 0;
        private double rotationThreshold = Math.PI / 64;

        private Tuple<double, double> center = Tuple.Create(0.0, 0.0);
        private Tuple<double, double> prevCenter = Tuple.Create(0.0, 0.0);
        private double centerThreshold = 2;

        private string type = "";
        private bool significantChange = false;

        public Marker(int markerId)
        {
            id = markerId;
        }

        public void AttachObserver(Observer observer)
        {
            observers.Add(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update(BuildJson(), id);
            }
        }

        public void Update(double[][] corners)
        {
            // Check if it's a more significant change than the threshold
            var (newRotation, newCenter) = CheckForThresholdChange(corners);
            rotation = newRotation;
            center = newCenter;
            NotifyObservers();
        }

        public int GetId()
        {
            return id;
        }

        private Dictionary<string, object> BuildJson()
        {
            var markerData = new Dictionary<string, object>
        {
            { "id", id },
            { "location", new double[] { center.Item1, center.Item2, 0 } },
            { "rotation", rotation },
            { "type", type }
        };
            return markerData;
        }

        private Tuple<double, double> CalculateCenter(double[][] corners)
        {
            prevCenter = center;
            double npCenterX = 0.0;
            double npCenterY = 0.0;

            foreach (var corner in corners)
            {
                npCenterX += corner[0];
                npCenterY += corner[1];
            }

            double centerX = npCenterX / corners.Length;
            double centerY = npCenterY / corners.Length;

            return Tuple.Create(centerX, centerY);
        }

        private double CalculateRotation(double[][] corners)
        {
            prevRotation = rotation;

            double[] centroid = new double[2];
            foreach (var corner in corners)
            {
                centroid[0] += corner[0];
                centroid[1] += corner[1];
            }

            centroid[0] /= corners.Length;
            centroid[1] /= corners.Length;

            double[] referenceVector = new double[] { corners[0][0] - centroid[0], corners[0][1] - centroid[1] };

            double angleRads = Math.Atan2(referenceVector[1], referenceVector[0]);
            double angleDegree = angleRads * 180 / Math.PI;

            // Subtract 45 degrees and wrap it to the -180 to 180 degree range
            double adjustedAngleDegree = ((angleDegree - 45 + 180) % 360 - 180);
            double adjustedAngleRadians = ((angleRads - Math.PI / 4 + Math.PI) % (2 * Math.PI) - Math.PI);

            return adjustedAngleRadians;
        }

        public void SetType(string type_)
        {
            type = type_;
        }

        private Tuple<double, Tuple<double, double>> CheckForThresholdChange(double[][] corners)
        {
            // Calculate the rotation and center
            double newRotation = CalculateRotation(corners);
            var newCenter = CalculateCenter(corners);

            // Check if it's a more significant change than the threshold
            // If it is, return the new values
            // If not, return the old values
            if (Math.Abs(newRotation - prevRotation) >= rotationThreshold ||
                Math.Abs(newCenter.Item1 - prevCenter.Item1) >= centerThreshold ||
                Math.Abs(newCenter.Item2 - prevCenter.Item2) >= centerThreshold)
            {
                significantChange = true;
                return Tuple.Create(newRotation, newCenter);
            }
            else
            {
                significantChange = false;
                return Tuple.Create(prevRotation, prevCenter);
            }
        }
    }
}
