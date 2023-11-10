using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class DIstanceBetweenComponent : GH_Component
    {
        List<int> ids = new List<int>();
        List<int> detectedIds = new List<int>();
        List<Plane> planes = new List<Plane>();
        double min = 0.0;
        double max = 100.0;
        double scale = 1.0;
        double distance;

        /// <summary>
        /// Initializes a new instance of the DIstanceBetweenComponent class.
        /// </summary>
        public DIstanceBetweenComponent()
          : base("DIstanceBetweenComponent", "Nickname",
              "Description",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("IDs", "IDs", "The two markers we are looking out for", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Detected IDs", "ID", "The IDs currently seen", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Marker Planes", "P", "The planes of the markers", GH_ParamAccess.list);
            pManager.AddNumberParameter("Min", "Min", "The minimum value we are mapping to", GH_ParamAccess.item, 0.00);
            pManager.AddNumberParameter("Max", "Max", "The maximum value we are mapping to", GH_ParamAccess.item, 100.00);
            pManager.AddNumberParameter("Scale", "S", "The scale of the changes", GH_ParamAccess.item, 1.00);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance", "D", "The distance between the two markers mapped to the custom values", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            detectedIds.Clear();
            ids.Clear();
            planes.Clear();

            DA.GetData("Min", ref min);
            DA.GetData("Max", ref max);
            DA.GetData("Scale", ref scale);

            DA.GetDataList("Detected IDs", detectedIds);

            if (!DA.GetDataList("IDs", ids) || !DA.GetDataList("Marker Planes", planes))
            {
                // Use the last calculated distance value if any of the inputs are null
                DA.SetData("Distance", distance);
                return;
            }

            // check if both ids are detected
            if (detectedIds.Contains(ids[0]) && detectedIds.Contains(ids[1]))
            {
                int index = detectedIds.IndexOf(ids[0]);
                Plane plane1 = planes[index];
                index = detectedIds.IndexOf(ids[1]);
                Plane plane2 = planes[index];

                double distance = plane1.Origin.DistanceTo(plane2.Origin);
                double mappedDistance = Remap(distance, scale, min, max);
                DA.SetData("Distance", mappedDistance);
                
                distance = mappedDistance;
            }
            else
            {
                DA.SetData("Distance", distance);
            }
        }

        private double Remap(double value, double scale, double min, double max)
        {
            double minDistance = 0.0;
            double maxDistance = 500.0;
            maxDistance = maxDistance * scale;

            // Map the value to the new range
            value = min + (value - minDistance) * (max - min) / (maxDistance - minDistance);

            return value;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6666C6A7-559C-4C44-8DCD-662E6552EB48"); }
        }
    }
}