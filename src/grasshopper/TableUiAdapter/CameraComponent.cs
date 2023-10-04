using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using TableUiReceiver;

namespace TableUiAdapter
{
    public class CameraComponent : GH_Component
    {
        Marker marker = new Marker();
        Mesh topo = new Mesh();
        bool cameraTracking = true;
        GH_Point newOrigin = new GH_Point();
        double newRotation = 0;

        /// <summary>
        /// Initializes a new instance of the CameraComponent class.
        /// </summary>
        public CameraComponent()
          : base("CameraComponent", "Camera",
              "Moves the camera according to the input",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Camera Marker", "C", "The incoming camera marker from the TableUI Receiver", GH_ParamAccess.item);
            pManager.AddMeshParameter("Base Topography", "T", "(Optional) The topography of the base model", GH_ParamAccess.item);
            pManager.AddPointParameter("Custom Origin", "O", "(Optional) The new origin the model's movement will be based on", GH_ParamAccess.item);
            pManager.AddNumberParameter("Custom Rotation", "R", "(Optional) The new rotation the model's rotation will be based on", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref marker);
            DA.GetData(1, ref topo);
            DA.GetData(2, ref newOrigin);
            DA.GetData(3, ref newRotation);
            
            if (cameraTracking && marker.location != null)
            {
                var doc = Rhino.RhinoDoc.ActiveDoc;
                var view = doc.Views.ActiveView;
                var camera = view.ActiveViewport.CameraLocation;

                double z = 5.5;
                Point3d markerPoint = new Point3d(marker.location[0], marker.location[1], 0);
                if (topo != null)
                {
                    Point3d closestPoint = topo.ClosestPoint(markerPoint);
                    z = closestPoint.Z + 5.5;
                }

                Point3d cameraLocation = new Point3d(marker.location[0], marker.location[1], z);
                view.ActiveViewport.SetCameraLocation(cameraLocation, false);

                Point3d cameraTarget = new Point3d(marker.location[0] - 1, marker.location[1], z);
                Transform rotation = Transform.Rotation(marker.rotation, cameraLocation);
                cameraTarget.Transform(rotation);

                Vector3d cameraDirection = cameraTarget - cameraLocation;

                view.ActiveViewport.SetCameraDirection(cameraDirection, false);

                //cameraTarget.Transform(Transform.Rotation(marker.rotation, cameraLocation));
                view.ActiveViewport.SetCameraTarget(cameraTarget, false);
                view.Redraw();
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
            get { return new Guid("F2CFCC12-6883-4125-AB5D-036457F40B3A"); }
        }
    }
}