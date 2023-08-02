using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Diagnostics.Eventing.Reader;
using Rhino;

namespace TableUI
{
    public class JsonParseComponent : GH_Component
    {
        private bool _shouldExpire = false;
        private RunState _currentState = RunState.Off;

        List<int> ids = new List<int>();
        List<Point2d> locations = new List<Point2d>();
        List<int> rotations = new List<int>();

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public JsonParseComponent()
          : base("ParseJson", "Parse",
              "Takes an incoming JSON of marker data and parses it into its component parts",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON", "J", "JSON to parse", 
                               GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("IDs", "ID", "Visible marker IDs", 
                                                        GH_ParamAccess.list);
            pManager.AddPointParameter("Position", "P", "Positions of the markers",
                                                        GH_ParamAccess.list);
            pManager.AddIntegerParameter("Rotation", "R", "Rotations of the markers",
                                                        GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (_shouldExpire)
            {
                switch (_currentState)
                {
                    case RunState.Off:
                        this.Message = "inactive";
                        DA.SetDataList(0, null);
                        DA.SetDataList(1, null);
                        DA.SetDataList(2, null);
                        _currentState = RunState.Idle;
                        break;
                    case RunState.Error:
                        this.Message = "ERROR";
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in JSON parsing");
                        _currentState = RunState.Idle;
                        break;
                    case RunState.Done:
                        this.Message = "done";
                        
                        // Send the output data
                        DA.SetDataList(0, ids);
                        DA.SetDataList(1, locations);
                        DA.SetDataList(2, rotations);
                        
                        _currentState = RunState.Idle;
                        break;
                }
                _shouldExpire = false;
                return;
            }
            
            ids = new List<int>();
            locations = new List<Point2d>();
            rotations = new List<int>();

            string jsonString = null;
            if (!DA.GetData(0, ref jsonString)) return;

            _currentState = RunState.Running;
            this.Message = "Running...";

            // Parse the JSON
            ParseJsonAsync(jsonString);
        }

        protected override void ExpireDownStreamObjects()
        {
            if (_shouldExpire)
            {
                base.ExpireDownStreamObjects();
            }
        }

        private void ParseJsonAsync(string jsonString)
        {
            Task.Run(() =>
            {
                try
                {
                    List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

                    foreach (var deserialJson in deserialJsonList)
                    {
                        foreach (var kvp in deserialJson)
                        {
                            string key = kvp.Key;
                            Marker marker = kvp.Value;

                            int id = marker.id;
                            ids.Add(id);
                            rotations.Add(marker.rotation);
                            if (marker.location != null)
                            {
                                locations.Add(new Point2d(marker.location[0], marker.location[1]));
                            }
                            else
                            {
                                locations.Add(new Point2d(0, 0));
                            }

                        }
                    }
                    _currentState = RunState.Done;
                    _shouldExpire = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                    
                    _currentState = RunState.Error;
                    _shouldExpire = true;
                    RhinoApp.InvokeOnUiThread((Action)delegate { ExpireSolution(true); });
                }
            });
        }

        private class Marker
        {
            public int id { get; set; }
            public int[] location { get; set; }
            public int rotation { get; set; }
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
            get { return new Guid("DBC68D05-E147-4AD0-BCA2-29199EF651CA"); }
        }
    }
}
