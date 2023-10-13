using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using GrasshopperAsyncComponent;
using Grasshopper.Kernel.Types;

namespace TableUiCompanions
{
    public class TranslateGeometryComponent : GH_AsyncComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TranslateGeometryComponent()
          : base("Translate Geometry", "Translate",
            "Description",
            "Strategist", "TableUI")
        {
            BaseWorker = new TranslationWorker(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometries", "G", "The geometries to be translated (named using TableUI's GeometryAssigner Component)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Marker IDs", "ID", "The IDs of the markers to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Marker Planes", "P", "The planes to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Translated Geometry", "G", "The translated geometries", GH_ParamAccess.list);
        }

        private class TranslationWorker : WorkerInstance
        {
            // Inputs
            List<GeometryWithId> assignedGeometries = new List<GeometryWithId>();
            List<int> ids = new List<int>();
            List<Plane> planes = new List<Plane>();

            // Internal
            Dictionary<int, Plane> idPlanePairs = new Dictionary<int, Plane>();

            // Outputs
            List<IGH_GeometricGoo> translatedGeometries = new List<IGH_GeometricGoo>();

            public TranslationWorker(GH_Component parent) : base(parent) { }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Build a dictionary of planes and their corresponding IDs
                for (int i = 0; i < ids.Count; i++)
                {
                    idPlanePairs.Add(ids[i], planes[i]);
                }

                // If any of the geometry IDs match the IDs of the incoming markers, translate the geometry to the corresponding plane
                foreach (GeometryWithId geoWithId in assignedGeometries)
                {
                    int id = geoWithId.Id;
                    
                    if (idPlanePairs.ContainsKey(id))
                    {
                        Plane plane = idPlanePairs[id];
                        Console.WriteLine(geoWithId.Geometry);

                        // Now, the hard part. We need to translate the geometry to the plane while not knowing what the geometry is
                        // We can't use the Translate method because it only works on Brep and Mesh

                        foreach (GeometryBase geoBase in geoWithId.Geometry)
                        {
                            plane.Origin = geoBase.GetBoundingBox(false).Center;
                            geoBase.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                            translatedGeometries.Add((IGH_GeometricGoo)geoBase);
                        }


                    }
                }

                ReportProgress("Done", 1);
                Done();
            }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetDataList("Geometries", assignedGeometries);
                DA.GetDataList("Marker IDs", ids);
                DA.GetDataList("Marker Planes", planes);
            }

            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetDataList("Translated Geometry", translatedGeometries);
            }

            public override WorkerInstance Duplicate() => new TranslationWorker(Parent);
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
        public override Guid ComponentGuid => new Guid("1c3cb685-08b4-4690-b72b-8697ed96a51d");
    }
}