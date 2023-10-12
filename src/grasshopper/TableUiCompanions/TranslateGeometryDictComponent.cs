using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using GrasshopperAsyncComponent;
using Grasshopper.Kernel.Types;

namespace TableUiCompanions
{
    public class TranslateGeometryDictComponent : GH_AsyncComponent
    {
        /// <summary>
        /// Initializes a new instance of the TranslateGeometryDictComponent class.
        /// </summary>
        public TranslateGeometryDictComponent()
          : base("Translate Geometry Dict", "Translate",
              "Intakes the dictionary from an assigned geometry and moves it",
              "Strategist", "TableUI")
        {
            BaseWorker = new TranslationWorker(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry Dictionary", "G", "The dictionary of geometries with IDs assigned", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Marker IDs", "ID", "The IDs of the markers to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Marker Planes", "P", "The planes to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Detected Geometries", "G", "The detected geometries", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Transformed Geometries", "G", "The transformed geometries", GH_ParamAccess.list);
        }

        private class TranslationWorker : WorkerInstance
        { 
            // Inputs
            Dictionary<int, object> GeoDict = new Dictionary<int, object>();
            List<int> ids = new List<int>();
            List<Plane> planes = new List<Plane>();

            // Internal

            // Outputs
            List<object> detectedGeometries = new List<object>();
            List<object> transformedGeometries = new List<object>();

            public TranslationWorker(GH_Component _parent) : base(_parent) { }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Clear outputs
                detectedGeometries.Clear();
                transformedGeometries.Clear();

                // Check if the dictionary is empty
                if (GeoDict.Count == 0)
                {
                    return;
                }

                // Compare the IDs of the dictionary and the IDs from the receiver
                foreach (KeyValuePair<int, object> pair in GeoDict)
                {
                    int id = pair.Key;
                    object geometry = pair.Value;

                    if (ids.Contains(id))
                    {
                        detectedGeometries.Add(geometry);
                        int index = ids.IndexOf(id);
                        Plane plane = planes[index];
                        if (geometry is GH_Brep)
                        {
                            GH_Brep brep = (GH_Brep)geometry;
                            brep.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                            transformedGeometries.Add(brep);
                        }
                        else if (geometry is GH_Mesh)
                        {
                            GH_Mesh mesh = (GH_Mesh)geometry;
                            mesh.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                            transformedGeometries.Add(mesh);
                        }
                    }
                }

                ReportProgress("Done", 1);
                Done();
            }

            public override WorkerInstance Duplicate() => new TranslationWorker(Parent);

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref GeoDict);
                DA.GetDataList(1, ids);
                DA.GetDataList(2, planes);
            }

            public override void SetData(IGH_DataAccess DA)
            {
                DA.SetDataList(0, detectedGeometries);
                DA.SetDataList(1, transformedGeometries);
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
            get { return new Guid("AC6CDAAC-24EA-49AE-BAF7-11D96CF8D22A"); }
        }
    }
}