using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using TableLib;

namespace NonAsyncTestComponent
{
    public class NonAsyncTestComponent : GH_Component
    {
        Invoker _invoker = new Invoker();
        private bool testBool;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NonAsyncTestComponent()
          : base("NonAsyncTestComponent", "Nickname",
            "Description",
            "Strategies", "TableUI")
        {
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

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Test test = new Test();

            DA.GetData(0, ref testBool);

            bool newBool = test.TestMethod(testBool);

            DA.SetData(0, newBool);
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
        public override Guid ComponentGuid => new Guid("ffb4de41-f9ee-4134-bbe9-40e03ff44622");
    }
}