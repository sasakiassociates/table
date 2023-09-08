using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Types.Transforms;
using GrasshopperAsyncComponent;
using Rhino;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TableLib;

namespace TableUiAdapter
{
    public class TableUiAdapterComponent : GH_AsyncComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TableUiAdapterComponent()
          : base("TableUiAdapterComponent", "TableUi",
            "Runs necessary calls to receive data into Grasshopper for a tangible table interface",
            "Strategist", "TableUI")
        {
            BaseWorker = new ForLoopWorker();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
            pManager.AddTextParameter("Views", "V", "Views to switch through", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Camera Origin", "O", "Origin of the camera", GH_ParamAccess.item);
            pManager.AddPointParameter("Camera Target", "T", "Target of the camera", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pitch", "P", "Pitch of the camera", GH_ParamAccess.item);
            pManager.AddNumberParameter("Depth", "D", "Depth of the camera", GH_ParamAccess.item);
        }

        private class ForLoopWorker : WorkerInstance
        {
            // Makes singleton "Invoker", whos object "Repository" connects to a UDP Client to listen on port 5005
            Invoker _invoker = Invoker.Instance;
            DataLibrary _reference = DataLibrary.Instance;
            
            // INPUTS
            private bool run;
            private List<string> views = new List<string>();

            // PROCESS VARIABLES
            float cameraRotation;
            int[] cameraLocation;

            // OUTPUTS
            Point3d cameraOrigin;
            Point3d cameraTarget;
            float pitch;
            float depth;

            public ForLoopWorker() : base(null) { }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref run);
                DA.GetDataList(1, views);
            }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                if (run)
                {
                    // TODO if we launch the app from grasshopper, it crashes after a few updates
                    // Will deal with this after the demo, fo now LaunchDetectionProgram is commented out
                    if (!_invoker.isRunning)
                    {
                        // _invoker.LaunchDetectionProgram(detectionPath);
                        bool success = _invoker.ExecuteWithTimeLimit(TimeSpan.FromMilliseconds(5000), () => _invoker.SetupDetection(10, 10));
                        if (!success)
                        {
                            Console.WriteLine("Failed to start detection program");
                            Done();
                            return;
                        }
                    }

                    // TODO: Maybe this guy shouldn't even know about the markers!
                    // Instead return:
                    // List<Brep> models
                    // List<int[]> locations
                    // List<int> rotations
                    List<Marker> markers = (List<Marker>)_invoker.Run();
                    // FAILURE: No markers are detected
                    if (markers == null || markers.Count == 0)
                    {
                        Done();
                        return;
                    }

                    // This is really bad, but is the quickest way to test this
                    // TODO rework this so the markers know their type and will report their values
                    foreach (Marker marker in markers)
                    {
                        // Eventually we'll do this instead
                        //if (marker.type == "camera")
                        //{
                        //    cameraRotation = marker.rotation;
                        //    cameraLocation = marker.location;
                        //    // Make this into a point
                        //    cameraOrigin = new Point3d(cameraLocation[0], cameraLocation[1], 5.8);
                        //    cameraTarget = new Point3d(cameraLocation[0], cameraLocation[1] + 5, 6);

                        if (marker.id == 99)                // If the marker id is 99, it's the camera marker so get the location and rotation
                        {
                            // Check if perspective is the current view since that's the only one we want to change
                            Rhino.Display.RhinoView view = RhinoDoc.ActiveDoc.Views.ActiveView;
                            if (view.ActiveViewport.Name == "Perspective")
                            {
                                cameraRotation = marker.rotation;
                                cameraLocation = marker.location;
                                // Make this into a point
                                cameraOrigin = new Point3d(cameraLocation[0], cameraLocation[1], 5.8);
                                cameraTarget = new Point3d(cameraLocation[0], cameraLocation[1] + 5, 6);

                                Transform targetRotation = Transform.Rotation(cameraRotation, Vector3d.ZAxis, cameraOrigin);
                                cameraTarget.Transform(targetRotation);
                            }
                        }
                        else if (marker.id == 98)           // This is the pitch marker, so get the rotation
                        {
                            float pitchValue = marker.rotation;
                            pitch = pitchValue;
                            //pitch = Invoker.MapFloatToInt(Math.Abs(pitchValue), minRads, maxRads, minVariable, maxVariable);
                        }
                        else if (marker.id == 97)
                        {
                            float depthValue = marker.rotation;
                            depth = depthValue;
                            //depth = Invoker.MapFloatToInt(Math.Abs(depthValue), minRads, maxRads, minVariable, maxVariable);
                        }
                        else if (marker.id <= 96 && marker.id >= 96 - views.Count)
                        {
                            RhinoDoc doc = RhinoDoc.ActiveDoc;
                            int index = 96 - marker.id;
                            List<Guid> viewGuids = new List<Guid>();
                            List<Rhino.Display.RhinoView> viewList = new List<Rhino.Display.RhinoView>();

                            foreach (string viewName in views)
                            {
                                Rhino.Display.RhinoView view = doc.Views.Find(viewName, false);
                                if (view != null)
                                {
                                    viewList.Add(view);
                                }
                            }

                            viewList[index].MainViewport.SetCameraLocations(cameraOrigin, cameraTarget);
                        }
                    }
                    // use Marker.id to find the corresponding model
                    // move the model to the point of the marker
                    // add the model to the output list
                }
                else if (!run && _invoker.isRunning)
                {
                    _invoker.StopDetectionProgram();
                }

                Done();
            }
            
            public override void SetData(IGH_DataAccess DA)
            {
                // The outputs get nulled out here for a brief moment - leading to a flicker in the UI
                // Found using breakpoints
                DA.SetData(0, cameraOrigin);
                DA.SetData(1, cameraTarget);
                DA.SetData(2, depth);
                DA.SetData(3, pitch);
            }
            
            public override WorkerInstance Duplicate() => new ForLoopWorker();
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6fa5436d-eb7c-4070-825a-77682579a89c");
    }
}