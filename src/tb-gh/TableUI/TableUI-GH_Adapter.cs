using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using Grasshopper.Kernel;
using GrasshopperAsyncComponent;
using Rhino.Geometry;

namespace TableUI
{
    public class TableUI_GH_Adapter : GH_AsyncComponent
    {

        /// <summary>
        /// Initializes a new instance of the TableUI_GH_Adapter class.
        /// </summary>
        public TableUI_GH_Adapter()
          : base("TableUI-GH_Adapter", "TableUI",
              "Asynchronously runs the main TableUI functions to get registered points from an external database",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("URL", "URL", "Link to the database", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Expire", "Ex", "Time in ms to timeout the process", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
            pManager.AddTextParameter("Authorization", "A", "The authentication ID for the database", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Points", "P", "Points from the database", GH_ParamAccess.list);
        }

        private class ForLoopWorker : WorkerInstance
        {
            Main _main = new();
            private string _url;
            private int _expire;
            private bool _run;
            private string _auth;

            public ForLoopWorker() : base(null) { }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref _url);
                DA.GetData(1, ref _expire);
                DA.GetData(2, ref _run);
                DA.GetData(3, ref _auth);
            }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                _main.setup(_url, _expire, _auth);
                _main.run();
                Done();
            }

            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetData(0, _main.get_results());
            }
            
            public override WorkerInstance Duplicate()
            {
                throw new NotImplementedException();
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
            get { return new Guid("943FC0DE-C8D4-4101-ADBA-50965343DFA9"); }
        }
    }
}