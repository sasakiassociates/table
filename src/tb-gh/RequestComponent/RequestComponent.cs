using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using TableUiLogic;

namespace RequestComponent
{
    public class RequestComponent : GH_Component
    {
        Repository _repository = Repository.Instance;
        Parser _parser = new Parser();

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RequestComponent()
          : base("RequestComponent", "Request",
            "Make request to the server to receive data from the table",
            "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Run the component", GH_ParamAccess.item);
            pManager.AddTextParameter("Port", "P", "Port number to pull the data from from", GH_ParamAccess.item, "5005");
            pManager.AddTextParameter("RepoStrategy", "S", "The strategy to use for the database", GH_ParamAccess.item, "udp");
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

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            string port = "";
            string strategy = "";

            DA.GetData("Run", ref run);

            if (!run)
            {
                return;
            }

            List<Marker> markers = new List<Marker>();

            List<int> ids = new List<int>();
            List<Point2d> points = new List<Point2d>();
            List<int> rotations = new List<int>();

            if (!DA.GetData("Port", ref port)) return;
            if (!DA.GetData("RepoStrategy", ref strategy)) return;

            _repository.Setup(strategy);
            _repository.MakeCall(port);
            markers = _repository.Get();

            foreach (Marker marker in markers)
            {
                int x = marker.location[0];
                int y = marker.location[1];
                Point2d point = new Point2d(x, y);

                ids.Add(marker.id);
                points.Add(point);
                rotations.Add(marker.rotation);
            }

            DA.SetDataList("Ids", ids);
            DA.SetDataList("Points", points);
            DA.SetDataList("Rotation", rotations);
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
        public override Guid ComponentGuid => new Guid("dde06365-376b-4ae3-a66c-359d8f998252");
    }
}