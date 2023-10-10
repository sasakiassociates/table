using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class GetVisibleGeometriesComponent : GH_Component
    {
        // Inputs
        List<GeometryBase> geometries = new List<GeometryBase>();
        List<int> ids = new List<int>();

        // Outputs
        List<GeometryBase> visibleGeometries = new List<GeometryBase>();
        bool changed = false;

        /// <summary>
        /// Initializes a new instance of the GetVisibleGeometriesComponent class.
        /// </summary>
        public GetVisibleGeometriesComponent()
          : base("Get Visible Geometries", "Nickname",
              "Description",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "The geometries to be translated (named using TableUI's GeometryAssigner Component)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Detected Marker IDs", "ID", "The IDs of the markers to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "The visible geometries", GH_ParamAccess.list);
            pManager.AddBooleanParameter("New Geometries", "N", "True if there are new geometries", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            geometries.Clear();
            ids.Clear();

            DA.GetDataList("Geometries", geometries);
            DA.GetDataList("Detected Marker IDs", ids);

            visibleGeometries.Clear();

            for (int i = 0; i < geometries.Count; i++)
            {
                GeometryBase geometry = geometries[i];
                int id = geometry.UserDictionary.ContainsKey("TableUI ID") ? (int)geometry.UserDictionary["TableUI ID"] : -1;

                if (ids.Contains(id))
                {
                    visibleGeometries.Add(geometry);
                    changed = true;
                }
            }

            DA.SetDataList("Geometries", visibleGeometries);
            DA.SetData("New Geometries", changed);
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
            get { return new Guid("545A586D-0F68-4C47-8E95-6ABD6C6D9976"); }
        }
    }
}