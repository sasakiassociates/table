using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Newtonsoft.Json;
using TableUiReceiver;

namespace TableUiAdapter
{
    public class MarkerJsonToPlanes : GH_Component
    {
        string incomingJson;
        List<Plane> planes = new List<Plane>();

        /// <summary>
        /// Initializes a new instance of the MarkerJsonToPlanes class.
        /// </summary>
        public MarkerJsonToPlanes()
          : base("MarkerJsonToPlanes", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Incoming JSON", "JSON", "A JSON of markers and theis information", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "Planes", "Planes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref incomingJson);

            if (incomingJson != null)
            {
                var markers = JsonConvert.DeserializeObject<List<Marker>>(incomingJson);

                foreach (var marker in markers)
                {
                    Point3d origin = new Point3d(marker.location[0], marker.location[1], marker.location[2]);

                    Plane plane = new Plane(origin, Vector3d.ZAxis);
                    plane.Rotate(marker.rotation, Vector3d.ZAxis);

                    planes.Add(plane);
                }

                DA.SetDataList(0, planes);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No JSON data");
            }
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
            get { return new Guid("A66D1A9D-5FEB-47B5-86E4-F91D536784DE"); }
        }
    }
}