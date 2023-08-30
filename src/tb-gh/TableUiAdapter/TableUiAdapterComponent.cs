using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Grasshopper.Kernel.Types.Transforms;
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
            pManager.AddBrepParameter("Models", "M", "Models to test for", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Variables", "V", "Number of variable values you want to keep track of", GH_ParamAccess.item);
       }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Models", "M", "Transformed models", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Variables", "V", "Variable values to change (currently associated with the location in the y dimension)", GH_ParamAccess.list);
        }

        private class ForLoopWorker : WorkerInstance
        {
            // Makes singleton "Invoker", whos object "Repository" connects to a UDP Client to listen on port 5005
            Invoker _invoker = Invoker.Instance;
            
            // INPUTS
            private bool run;
            private string detectionPath;
            List<Brep> incomingModels = new List<Brep>();
            int numVariables;

            // OUTPUTS
            List<int> variableValues = new List<int>();
            List<Brep> transformedModels;

            public ForLoopWorker() : base(null) { }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref run);
                DA.GetData(1, ref detectionPath);
                DA.GetDataList(2, incomingModels);
                DA.GetData(3, ref numVariables);

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
                        if (!success)
                        {
                            Console.WriteLine("Failed to start detection program");
                            Done();
                            return;
                        }

                        _invoker.modelRefDict = new Dictionary<int, object>();

                        // Build the reference dictionary
                        _invoker.BuildDict(incomingModels.Count, numVariables);

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

                    for (int i = 0; i < numVariables; i++)
                    {
                        variableValues.Add(0);
                    }

                    transformedModels = new List<Brep>();
                    // Here, we'll add logic to move the models to the point of their assigned markers
                    // check marker.id against the list of models
                    foreach (Marker marker in markers)
                    {
                        // if the marker is in the dictionary
                        if (_invoker.refDict.ContainsKey(marker.id))
                        {
                            // check if it's a model or a variable
                            switch (_invoker.refDict[marker.id])
                            {
                                // if it's a model, move the model to the point of the marker and rotate it
                                case "model":
                                    Brep model = incomingModels[marker.id];
                                    Point3d newOrigin = new Point3d(1080 - marker.location[0], 720 - marker.location[1], 0);

                                    Transform translation = Transform.Translation(newOrigin - model.GetBoundingBox(false).Center);
                                    //Transform transform = Transform.Multiply(rotation, translation);

                                    model.Transform(translation);

                                    // ISSUE: rotation is adding to it's previous rotation
                                    Transform rotation = Transform.Rotation(Math.Sin(marker.rotation), Math.Cos(marker.rotation), Vector3d.ZAxis, newOrigin);
                                    model.Transform(rotation);
                                    transformedModels.Add(model);
                                    break;
                                // if it's a variable, update the variable value
                                case "variable":
                                    //variableValues[marker.id] = marker.location[0];
                                    int angle = (int)(marker.rotation * 180 / Math.PI);
                                    variableValues[marker.id - incomingModels.Count] = angle;
                                    break;
                            }

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
                DA.SetDataList(0, transformedModels);
                DA.SetDataList(1, variableValues);
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