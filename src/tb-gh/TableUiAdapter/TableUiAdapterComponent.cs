using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using GrasshopperAsyncComponent;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
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
            pManager.AddTextParameter("Detection Path", "P", "Path to the detection program", GH_ParamAccess.item);
            // TODO: Assign markers to models in output
            pManager.AddBrepParameter("Models", "M", "Models to be placed on the table", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "List of points where the markers are", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Ids", "I", "List of detected ids", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rotation", "R", "List of rotations of markers in radians", GH_ParamAccess.list);
            pManager.AddTextParameter("Types", "T", "List of types of markers", GH_ParamAccess.list);

            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
            // pManager.AddBrepParameter("Models", "M", "Models, now assigned to the location of the corresponding marker", GH_ParamAccess.list);
        }

        private class ForLoopWorker : WorkerInstance
        {
            // Makes singleton "Invoker", whos object "Repository" connects to a UDP Client to listen on port 5005
            Invoker _invoker = Invoker.Instance;
            
            private bool run;
            private string detectionPath;

            // INPUTS
            List<Brep> models;
            
            // OUTPUTS
            List<int> ids;
            List<Point2d> points;
            List<float> rotations;
            List<string> types;

            public ForLoopWorker() : base(null) { }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref run);
                DA.GetData(1, ref detectionPath);
                DA.GetDataList(2, models);

                _invoker.SetParseStrategy(ParserFactory.GetParser("Marker"));
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
                        // FAILURE: the detetion program doesn't launch properly
                        if (!success)
                        {
                            Console.WriteLine("Failed to start detection program");
                            // _invoker.Disconnect();
                            Done();
                            return;
                        }
                        // _invoker.SetupDetection(10, 10);
                    }
                    // If there are no markers, finish the component
                    List<Marker> markers = (List<Marker>)_invoker.Run();

                    // FAILURE: No markers are detected
                    if (markers == null || markers.Count == 0)
                    {
                        Done();
                        return;
                    }

                    // Otherwise, we need to parse the data into the lists we're outputting
                    ids = new List<int>();
                    points = new List<Point2d>();
                    rotations = new List<float>();
                    types = new List<string>();

                    // Here, we'll add logic to move the models to the point of their assigned markers

                    foreach (var marker in markers)
                    {
                        Point2d point = new Point2d(marker.location[0], marker.location[1]);
                        ids.Add(marker.id);
                        points.Add(point);
                        rotations.Add(marker.rotation);
                        types.Add(marker.type);
                    }
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
                DA.SetDataList(0, points);
                DA.SetDataList(1, ids);
                DA.SetDataList(2, rotations);
                DA.SetDataList(3, types);

                DA.SetData(4, _invoker.isRunning);
                // Output a list of the models in their new locations
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