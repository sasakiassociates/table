using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using TableUiReceiver;

namespace TableUiAdapter
{
    public class GeometryTranslatorComponent : GH_Component
    {
        List<Brep> breps = new List<Brep>();
        List<Marker> incomingMarkers = new List<Marker>();
        List<Brep> transformedBreps = new List<Brep>();

        /// <summary>
        /// Initializes a new instance of the GeometryTranslatorComponent class.
        /// </summary>
        public GeometryTranslatorComponent()
          : base("Translate Geometry", "Translate Geometry",
              "Intakes the TableUI receiver geometry markers and a dictionary of geometries and the marker you want to assign them to and translates those geometries according to the corresponding marker",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "The geometries to be translated (named using TableUI's GeometryAssigner Component", GH_ParamAccess.list);
            pManager.AddGenericParameter("Markers", "M", "The incoming markers from the TableUI Receiver", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Translated Geometry", "G", "The translated geometries", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            breps.Clear();
            incomingMarkers.Clear();
            transformedBreps.Clear();

            DA.GetDataList("Geometry", breps);
            DA.GetDataList("Markers", incomingMarkers);

            foreach (Brep brep in breps)
            {
                string name = brep.UserDictionary.GetString("Name");

                foreach (Marker marker in incomingMarkers)
                {
                    if (marker.name == name)
                    {
                        Point3d newOrigin = new Point3d(marker.location[0], marker.location[1], 0);
                        Vector3d translationVector = newOrigin - brep.GetBoundingBox(false).Center;
                        brep.Translate(translationVector);

                        Transform rotation = Transform.Rotation(marker.rotation, Vector3d.ZAxis, newOrigin);
                        brep.Transform(rotation);
                        
                        transformedBreps.Add(brep);

                        break;
                    }
                }
            }

            DA.SetDataList("Translated Geometry", transformedBreps);
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
            get { return new Guid("8D8A8339-2054-4147-8B0B-25E4B8166924"); }
        }
    }
}