using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using TableUiReceiver;

namespace TableUiAdapter
{
    public class VariableComponent : GH_Component
    {
        List<Marker> incomingMarkers = new List<Marker>();
        int desiredMarkerId;
        int targetMin;
        int targetMax;
        
        /// <summary>
        /// Initializes a new instance of the SingleVariableComponent class.
        /// </summary>
        public VariableComponent()
          : base("Get Variable", "Variable",
              "Intakes the controller marker output from the TableUI Receiver and gets one rotation value from it",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Markers", "C", "The controller marker output from the TableUI Receiver", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Marker ID", "I", "The marker you want to get a value from", GH_ParamAccess.item, 1);      // Default is 1 since 0 is the camera marker
            pManager.AddIntegerParameter("Minimum", "M", "The minimum value you want to map the values to", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Maximum", "X", "The maximum value you want to map the values to", GH_ParamAccess.item, 180);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Variable", "V", "The variable you want to get a value from", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataList("Markers", incomingMarkers);
            DA.GetData("Marker ID", ref desiredMarkerId);
            DA.GetData("Minimum", ref targetMin);
            DA.GetData("Maximum", ref targetMax);

            foreach (Marker marker in incomingMarkers)
            {
                if (marker.type != "controller")
                {
                    Exception e = new Exception("The marker you are trying to get a variable from is not a controller marker");
                    throw e;
                }
                if (marker.id == desiredMarkerId)
                {
                    float rotation = marker.rotation;
                    int variableValue = (int)(rotation * 180/Math.PI);
                    // Map to the values given
                    int mappedVariableValue = Utilities.Map(0, 180, targetMin, targetMax, variableValue);
                    DA.SetData("Variable", mappedVariableValue);
                    break;
                }
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
            get { return new Guid("D5A3C1B9-F6B4-416B-8829-C2AF87F88A0B"); }
        }
    }
}