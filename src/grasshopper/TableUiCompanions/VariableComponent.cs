using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class VariableComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VariableComponent class.
        /// </summary>
        public VariableComponent()
          : base("VariableComponent", "Variable",
              "Intakes incoming planes, incoming IDs, minimum value, maximum value, and desired IDs (optional). Buttons at the bottom allows users to switch between rotation and location as the input",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Planes", "P", "The planes to be assigned IDs", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IDs", "ID", "The IDs of the markers to assign the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Minimum Value", "Min", "The minimum value of the variable", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Value", "Max", "The maximum value of the variable", GH_ParamAccess.item);
            pManager.
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("E8A82803-BF56-4C2C-AA57-6BB835541E17"); }
        }
    }
}