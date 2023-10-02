using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using TableUiReceiver;

namespace TableUiAdapter
{
    public class GeometryTranslatorComponent : GH_Component
    {
        List<Brep> breps = new List<Brep>();
        List<Marker> incomingMarkers = new List<Marker>();
        List<Brep> transformedBreps = new List<Brep>();
        Point2d newOrigin = new Point2d(0, 0);
        double newRotation = 0;

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
            pManager.AddPointParameter("New Origin", "O", "The new origin the model's movement will be based on", GH_ParamAccess.item);
            pManager.AddNumberParameter("New Rotation", "R", "The new rotation the model's rotation will be based on", GH_ParamAccess.item);
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
            GH_Point tempOrigin = new GH_Point();

            breps.Clear();
            incomingMarkers.Clear();
            transformedBreps.Clear();

            DA.GetDataList("Geometry", breps);
            DA.GetDataList("Markers", incomingMarkers);
            DA.GetData("New Origin", ref tempOrigin);
            DA.GetData("New Rotation", ref newRotation);

            newOrigin = new Point2d(tempOrigin.Value.X, tempOrigin.Value.Y);

            foreach (Brep brep in breps)
            {
                int id = int.Parse(brep.UserDictionary.GetString("Name"));

                foreach (Marker marker in incomingMarkers)
                {
                    if (marker.id == id)
                    {
                        Point3d markerOrigin = new Point3d(marker.location[0] + newOrigin.X, marker.location[1] + newOrigin.Y, 0);
                        Point3d center = brep.GetBoundingBox(false).Center;
                        Point3d projectedCenter = new Point3d(center.X, center.Y, 0);
                        Vector3d translationVector = markerOrigin - projectedCenter;
                        brep.Translate(translationVector);

                        double newRotationRads = newRotation * Math.PI / 180;
                        Transform rotation = Transform.Rotation(marker.rotation + newRotationRads, Vector3d.ZAxis, markerOrigin);
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