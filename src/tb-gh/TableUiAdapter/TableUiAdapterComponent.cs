using Grasshopper;
using Grasshopper.Kernel;
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
            pManager.AddTextParameter("Link", "L", "Link to the database, could be url for http or port for udp", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
            pManager.AddTextParameter("RepoStrategy", "S", "The strategy to use for the database", GH_ParamAccess.item, "udp");

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Ids", "I", "Ids", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Rotation", "R", "Rotations", GH_ParamAccess.list);
        }

        private class ForLoopWorker : WorkerInstance
        {
            private int expire;
            private bool run;
            private bool wasRunning = false;

            List<Marker> markers = new List<Marker>();
            Invoker _invoker;

            List<int> ids = new List<int>();
            List<Point2d> points = new List<Point2d>();
            List<int> rotations = new List<int>();

            public ForLoopWorker() : base(null) { }

            

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(1, ref run);

                JsonToMarkerParser parseStrategy = new JsonToMarkerParser();
                _invoker = new Invoker(parseStrategy);
            }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                if (run)
                {
                    if (!wasRunning)
                    {
                        _invoker.LaunchDetection();
                        wasRunning = true;
                    }

                    markers = (List<Marker>)_invoker.Run();
                    foreach (var marker in markers)
                    {
                        Point2d point = new Point2d(marker.location[0], marker.location[1]);
                        ids.Add(marker.id);
                        points.Add(point);
                        rotations.Add(marker.rotation);
                    }
                }
                else
                {
                    if (wasRunning)
                    {
                        _invoker.EndDetection();
                        wasRunning = false;
                    }
                }
            }
            
            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetDataList(0, ids);
                DA.SetDataList(1, points);
                DA.SetDataList(2, rotations);
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