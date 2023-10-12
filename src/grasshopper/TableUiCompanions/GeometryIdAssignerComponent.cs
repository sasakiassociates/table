using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class GeometryIdAssignerComponent : GH_Component
    {
        List<object> geometries = new List<object>();
        List<int> ids = new List<int>();
        Dictionary<int, object> idToGeometry = new Dictionary<int, object>();

        /// <summary>
        /// Initializes a new instance of the GeometryIdAssignerComponent class.
        /// </summary>
        public GeometryIdAssignerComponent()
          : base("Geometry Id Assigner", "ID Assigner",
              "Intakes geometries (breps or meshes) and a list of IDs. Assigns each geometry a marker ID",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometries", "G", "The geometries to be assigned IDs", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IDs", "ID", "The IDs of the markers to assign the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Assigned Geometries", "G", "A dictionary of geometries with IDs assigned", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            geometries.Clear();
            ids.Clear();
            idToGeometry.Clear();

            if (!DA.GetDataList(0, geometries)) return;
            if (!DA.GetDataList(1, ids)) return;

            if (geometries.Count != ids.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of geometries and IDs must be the same");
                return;
            }

            if (ids.Count == 0)
            {
                for (int i = 0; i < geometries.Count; i++)
                {
                    idToGeometry.Add((i + 1), geometries[i]);
                }
            }
            else
            {
                // Otherwise, use the custom list of ids to assign
                // First, both lists need to be of the same length
                if (geometries.Count != ids.Count)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of geometries and IDs must be the same");
                    return;
                }
                else
                {
                    foreach (var geometry in geometries)
                    {
                        // Add the corresponding ID
                        idToGeometry.Add(ids[geometries.IndexOf(geometry)], geometry);
                    }
                }
            }
            

            DA.SetData(0, idToGeometry);
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
            get { return new Guid("00F5A114-07D6-49C5-AD98-B3BB41D8CF11"); }
        }
    }
}