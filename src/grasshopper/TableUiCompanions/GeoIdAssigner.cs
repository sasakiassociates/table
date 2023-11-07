using System;
using System.Collections.Generic;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TableUiCompanions
{
    public class GeoIdAssigner : GH_Component
    {
        List<IGH_GeometricGoo> geometries = new List<IGH_GeometricGoo>();           // IGH_GeometricGoo allows us to work with custom Magpie Geometry as well as all Rhino Geometries
        List<int> ids = new List<int>();
        Plane originPlane = new Plane();
        int rotation = 0;

        Dictionary<int, IGH_GeometricGoo> assignedGeometries = new Dictionary<int, IGH_GeometricGoo>();
        //List<GeometryWithId> assignedGeometries = new List<GeometryWithId>();

        /// <summary>
        /// Initializes a new instance of the GeoIdAssigner class.
        /// </summary>
        public GeoIdAssigner()
          : base("GeoIdAssigner", "Nickname",
              "Makes a new object that holds a geometry and an ID, also moves the geometry to the origin to be available to move.",
              "Strategist", "TableUI")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "The geometries to be assigned IDs", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IDs", "ID", "The IDs of the markers to assign the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Origin Plane", "Origin", "The plane that all geometries will be transformed in resepct to", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGeometryParameter("Assigned Geometries", "AG", "The geometries with IDs assigned", GH_ParamAccess.list);
            pManager.AddGenericParameter("Assigned Geometries", "AG", "The geometries with IDs assigned", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            geometries.Clear();
            ids.Clear();
            assignedGeometries.Clear();

            if (!DA.GetDataList(0, geometries)) return;
            DA.GetDataList(1, ids);
            DA.GetData(2, ref originPlane);

            if (ids.Count == 0)
            {
                for (int i = 0; i < geometries.Count; i++)
                {
                    int id = i + 1;
                    IGH_GeometricGoo geo = geometries[i].DuplicateGeometry();

                    assignedGeometries.Add(id, geo);

                    //GeometryWithId assignedGeo = new GeometryWithId(geo, id);
                    //assignedGeometries.Add(assignedGeo);
                }
            }
            else
            {
                if (ids.Count != geometries.Count)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of geometries and IDs must be the same");
                    return;
                }
                foreach (int id in ids)
                {
                    int index = ids.IndexOf(id);
                    IGH_GeometricGoo geo = geometries[index];

                    assignedGeometries.Add(id, geo);

                    //GeometryWithId assignedGeo = new GeometryWithId(geo, id);
                    //assignedGeometries.Add(assignedGeo);
                }
            }

            DA.SetDataList("Assigned Geometries", assignedGeometries);
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
            get { return new Guid("6A70CD5B-BD86-42C4-8E5E-4F6039D6156C"); }
        }
    }
}