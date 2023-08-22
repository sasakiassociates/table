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
            pManager.AddTextParameter("Link", "L", "Link to the database, could be url for http or port for udp", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Ids", "I", "Ids", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Rotation", "R", "Rotations", GH_ParamAccess.list);

        }

        private class ForLoopWorker : WorkerInstance
        {
            Invoker _invoker = new Invoker();

            private bool run;
            private string strategy = "Marker";

            List<Marker> markers = new List<Marker>();

            List<int> ids;
            List<Point2d> points;
            List<int> rotations;

            List<int> previousIds;
            List<Point2d> previousPoints;
            List<int> previousRotations;

            public ForLoopWorker() : base(null) { }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(1, ref run);

                IParser _parseStrategy = ParserFactory.GetParser(strategy);
                _invoker.SetParseStrategy(_parseStrategy);
            }

            //TODO either rework this to use repository directly or make a main to just run
            // What inputs do we need? Strat
            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                if (run)
                {

                    Repository _repository = new Repository();

                    // Connect to the UDP client
                    _repository.Connect();

                    // Tell the other program to send data
                    _repository.UdpSend("SEND");

                    // Receive the data
                    string response = _repository.UdpReceive(1000);
                    Console.WriteLine(response);

                    // Parse data
                    IParser _parseStrategy = ParserFactory.GetParser(strategy);
                    List<Marker> markers = (List<Marker>)_parseStrategy.Parse(response);

                    // Disconnect from the UDP client
                    _repository.EndUdpReceive();
                    
                    ids = new List<int>();
                    points = new List<Point2d>();
                    rotations = new List<int>();

                    

                    foreach (var marker in markers)
                    {
                        Point2d point = new Point2d(marker.location[0], marker.location[1]);
                        ids.Add(marker.id);
                        points.Add(point);
                        rotations.Add(marker.rotation);
                    }
                    
                    /*_invoker.Disconnect();*/
                    _repository.EndUdpReceive();
                }
                else if (!run)
                {
                    //_invoker.EndDetection();
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