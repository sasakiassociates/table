using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUI
{
    public class JsonParseComponent : GH_Component
    {
        /* Example JSON:
        [
          3 : {
            "id": "3",
            "location": {
              "x": 10,
              "y": 20
            },
          },
          24: {
            "id": "24",
            "location": {
                "x": -5,
              "y": 30
            },
          },
        ]
        */

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public JsonParseComponent()
          : base("ParseJson", "Parse",
              "Takes an incoming JSON and parses it into its component parts",
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
            pManager.AddVectorParameter("Rotation", "R", "Rotations of the markers",
                                                        GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jsonString = null;
            if (!DA.GetData(0, ref jsonString)) return;

            try
            {
                List<Dictionary<string, Marker>> deserialJsonList = JsonConvert.DeserializeObject<List<Dictionary<string, Marker>>>(jsonString);

                List<int> ids = new List<int>();
                List<Point2d> locations = new List<Point2d>();
                List<Vector2d> rotations = new List<Vector2d>();

                foreach (var deserialJson in deserialJsonList)
                {
                    foreach (var kvp in deserialJson)
                    {
                        string key = kvp.Key;
                        Marker marker = kvp.Value;

                        int id = marker.id;
                        ids.Add(id);
                        if (marker.location != null)
                        {
                            locations.Add(new Point2d(marker.location[0], marker.location[1]));
                        }
                        else
                        {
                            locations.Add(new Point2d(0, 0));
                        }
                        if (marker.rotation != null)
                        {
                            rotations.Add(new Vector2d(marker.rotation[0], marker.rotation[1]));
                        }
                        else
                        {
                            rotations.Add(new Vector2d(0, 0));
                        }
                    }
                }

                DA.SetDataList(0, ids);
                DA.SetDataList(1, locations);
                DA.SetDataList(2, rotations);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }
        }

        private class Marker
        {
            public int id { get; set; }
            public int[] location { get; set; }
            public int[] rotation { get; set; }
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
