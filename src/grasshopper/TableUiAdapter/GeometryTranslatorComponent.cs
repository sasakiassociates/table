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
        Mesh topo = new Mesh();
        Point2d newOriginVar = new Point2d(0, 0);
        double newRotation = 0;
        double newModelRotation = 0;

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
            pManager.AddMeshParameter("Base Topography", "T", "(Optional) The topography of the base model", GH_ParamAccess.item);
            pManager.AddPointParameter("Custom Origin", "O", "(Optional) The new origin the model's movement will be based on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Custom Rotation", "R", "(Optional) The new rotation the model's rotation will be based on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Custom Model Rotation", "R", "(Optional) The new rotation the model's rotation will be based on", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
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
            DA.GetData("Base Topography", ref topo);
            DA.GetData("Custom Origin", ref tempOrigin);
            DA.GetData("Custom Rotation", ref newRotation);
            DA.GetData("Custom Model Rotation", ref newModelRotation);

            newOriginVar = new Point2d(tempOrigin.Value.X, tempOrigin.Value.Y);

            foreach (Brep brep in breps)
            {
                int id = int.Parse(brep.UserDictionary.GetString("Name"));

                foreach (Marker marker in incomingMarkers)
                {
                    if (marker.id == id)
                    {
                        int z = 0;
                        if (topo != null)
                        {
                            Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                            Point3d topoPoint = topo.ClosestPoint(markerPoint);
                            z = (int)topoPoint.Z;
                        }

                        // Create a new origin point for all marker transforms
                        Point3d newOrigin = new Point3d(marker.location[0] + newOriginVar.X, marker.location[1] + newOriginVar.Y, z); // First find where the marker is
                        // then rotate that point by the predefined variable the user inputs
                        Transform originRotation = Transform.Rotation(newRotation * Math.PI / 180, Vector3d.ZAxis, Point3d.Origin);
                        newOrigin.Transform(originRotation); // this is our new origin

                        // Now we need to move our model here
                        Point3d modelCenter = brep.GetBoundingBox(false).Center;
                        Point3d projectedModelCenter = new Point3d(modelCenter.X, modelCenter.Y, z);
                        Vector3d translationVector = newOrigin - projectedModelCenter;
                        Transform modelTranslation = Transform.Translation(translationVector);
                        brep.Transform(modelTranslation);

                        // Now we rotate that model around the new origin
                        Transform modelRotation = Transform.Rotation(marker.rotation + (newModelRotation * Math.PI / 180), Vector3d.ZAxis, newOrigin);
                        brep.Transform(modelRotation);

                        transformedBreps.Add(brep);

                        break;
                    }
                }
            }

            DA.SetDataList("Translated Geometry", transformedBreps);
        }

        private Vector3d RotateVector(Vector3d vector, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double x = vector.X * cos - vector.Y * sin;
            double y = vector.X * sin + vector.Y * cos;
            return new Vector3d(x, y, vector.Z);
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