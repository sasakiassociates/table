using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiAdapter
{
    public class GeometryAssigner : GH_Component
    {
        List<string> geometryNames = new List<string>();
        List<Brep> geometries = new List<Brep>();

        /// <summary>
        /// Initializes a new instance of the GeometryAssigner class.
        /// </summary>
        public GeometryAssigner()
          : base("GeometryAssigner", "Assign ID",
              "Assigns each geometry a name (required for the TableUI model tracking). The incoming geometries and names will be assigned in order. If names list is shorter than geometries, the excess will be named 'Geometry [i]'",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "The geometries to be assigned", GH_ParamAccess.list);
            pManager.AddTextParameter("Geometry Names", "N", "Custom names to assign to the geometries", GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_BRepParam("Geometry", "G", "The geometries with assigned marker IDs", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataList("Geometry", geometries);
            DA.GetDataList("Geometry Names", geometryNames);

            for (int i = 0; i < geometries.Count; i++)
            {
                if (i < geometryNames.Count)
                {
                    geometries[i].UserDictionary.Set("Name", geometryNames[i]);
                }
                else
                {
                    geometries[i].UserDictionary.Set("Name", "Geometry " + i);
                }
            }

            DA.SetDataList("Geometry", geometries);
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
            get { return new Guid("1256DBC5-0540-4B6C-8D7C-328200AF4A15"); }
        }
    }
}