using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class VariableComponent : GH_Component
    {
        // Inputs
        List<Plane> planes = new List<Plane>();
        List<int> ids = new List<int>();
        double min = 0;
        double max = 1;
        List<int> desiredIds = new List<int>();
        Dictionary<int, Plane> idPlanePairs = new Dictionary<int, Plane>();

        // Outputs
        List<double> variableValues = new List<double>();

        /// <summary>
        /// Initializes a new instance of the VariableComponent class.
        /// </summary>
        public VariableComponent()
          : base("VariableComponent", "Variable",
              "Intakes incoming planes, incoming IDs, minimum value (optional), maximum value (optional), and desired IDs (optional). Buttons at the bottom allows users to switch between rotation and location as the input",
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
            pManager.AddIntegerParameter("Desired IDs", "Desired", "The desired IDs of the markers to assign the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Variable", "Var", "The desired value", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            planes.Clear();
            ids.Clear();
            desiredIds.Clear();

            if (!DA.GetDataList(0, planes)) return;
            if (!DA.GetDataList(1, ids)) return;
            DA.GetData(2, ref min);
            DA.GetData(3, ref max);
            DA.GetDataList(4, desiredIds);

            // Check if the number of planes and IDs are the same
            if (planes.Count != ids.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of planes and IDs must be the same");
                return;
            }
            
            idPlanePairs.Clear();
            // Build a dictionary of planes and their corresponding IDs
            for (int i = 0; i < planes.Count; i++)
            {
                Plane plane = planes[i];
                int id = ids[i];
                idPlanePairs.Add(id, plane);
            }

            variableValues.Clear();
            foreach (int id in desiredIds)
            {
                // if the incoming IDs contain the desired ID, set the variable value to the corresponding value from the plane
                if (ids.Contains(id))
                {
                    Plane plane = idPlanePairs[id];
                    // Get the rotation relative to the world XY plane
                    double angle = Vector3d.VectorAngle(plane.XAxis, Vector3d.XAxis, Plane.WorldXY);
                    // Map the angle to the range of the variable
                    double mappedValue = Map(0, 2*Math.PI, min, max, angle);
                    variableValues.Add(mappedValue);
                }
            }

            DA.SetDataList(0, variableValues);
        }

        private static double Map(double sourceMin, double sourceMax, double targetMin, double targetMax, double value)
        {
            // Ensure the input value is within the source range
            value = Math.Max(sourceMin, Math.Min(sourceMax, value));

            // Calculate the percentage of the input value within the source range
            double percentage = (value - sourceMin) / (sourceMax - sourceMin);

            // Map the percentage to the target range
            double mappedValue = targetMin + percentage * (targetMax - targetMin);

            return mappedValue;
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