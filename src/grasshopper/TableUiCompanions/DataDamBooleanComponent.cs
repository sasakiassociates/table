using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class DataDamBooleanComponent : GH_Component
    {
        // Input
        List<object> input = new List<object>();
        bool dam = false;

        // Internal
        List<object> inputHistory = new List<object>();
        bool recompute = false;

        // Output
        List<object> output = new List<object>();

        /// <summary>
        /// Initializes a new instance of the DataDamBooleanComponent class.
        /// </summary>
        public DataDamBooleanComponent()
          : base("DataDamBooleanComponent", "Nickname",
              "Description",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input", "I", "The input to be dammed", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Dam", "D", "The dam to be opened or closed", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Output", "O", "The output to be dammed", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            input.Clear();
            output.Clear();

            DA.GetDataList("Input", input);
            DA.GetData("Dam", ref dam);

            if (input != inputHistory)
            {
                output = input;
                inputHistory = input;
                recompute = true;
            }
            
            DA.SetDataList("Output", output);

        }

        protected override void ExpireDownStreamObjects()
        {
            if (!dam)
            {
                base.ExpireDownStreamObjects();
                if (recompute)
                {
                    recompute = false;
                    ExpireSolution(true);
                }
            }
        }

        /*protected override void AfterSolveInstance()
        {
            base.AfterSolveInstance();
            if (!dam)
            {
                ExpireSolution(true);
            }
        }*/

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
            get { return new Guid("759B9114-7B10-4065-83EE-258E5C39105F"); }
        }
    }
}