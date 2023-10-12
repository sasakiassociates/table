using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using GrasshopperAsyncComponent;
using System.Windows.Forms;

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
            pManager.AddGenericParameter("Dictionary Geometries", "G", "The geometries+IDs to be translated (named using TableUI's GeometryAssigner Component)", GH_ParamAccess.item);
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
            Dictionary<int, object> idGeometryPairs = new Dictionary<int, object>();
            List<int> detectedIds = new List<int>();
            List<Plane> detectedPlanes = new List<Plane>();

            // Internal
            Dictionary<int, Plane> idPlanePairs = new Dictionary<int, Plane>();
            Dictionary<object, Plane> detectedGeometryPlanePairs = new Dictionary<object, Plane>();
            List<object> detectedGeometries = new List<object>();

            // Outputs
            List<GeometryBase> translatedGeometries = new List<GeometryBase>();

            public TranslationWorker(GH_Component parent) : base(parent) { }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                // Clear the lists
                idPlanePairs.Clear();
                detectedGeometryPlanePairs.Clear();
                detectedGeometries.Clear();
                translatedGeometries.Clear();

                // Check if there are any geometries to translate
                if (idGeometryPairs.Count == 0)
                {
                    ReportProgress("No geometries to translate", 0);
                    Done();
                    return;
                }

                if (detectedIds.Count == 0)
                {
                    ReportProgress("No markers detected", 0);
                    Done();
                    return;
                }

                // Build a dictionary of planes and their corresponding IDs
                for (int i = 0; i < detectedIds.Count; i++)
                {
                    idPlanePairs.Add(detectedIds[i], detectedPlanes[i]);
                }

                // Compare the IDs of the geometries to the IDs of the incoming markers
                foreach (int id in idGeometryPairs.Keys)
                {
                    if (detectedIds.Contains(id))
                    {
                        detectedGeometries.Add(idGeometryPairs[id]);
                        // Build a dictionary of the geometries and their corresponding planes
                        detectedGeometryPlanePairs.Add(idGeometryPairs[id], idPlanePairs[id]);
                    }
                }

                // Run through the detected dictionary
                foreach (var kvp in detectedGeometryPlanePairs)
                {
                    var geometry = kvp.Key;
                    var plane = kvp.Value;

                    if (geometry is GeometryBase)
                    {
                           GeometryBase g = (GeometryBase)geometry;

                        g.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                        translatedGeometries.Add(g);
                    }
                    else
                    {
                        // It might be a Magpie Building (which is a List of GeometryBase objects)
                        try
                        {
                            List<GeometryBase> geometries = (List<GeometryBase>)geometry;

                            foreach (GeometryBase g in geometries)
                            {
                                g.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                                translatedGeometries.Add(g);
                            }
                        }
                        catch
                        {
                            // It's not a Magpie Building, so it's not a list of GeometryBase objects
                            ReportProgress("Geometry is not a GeometryBase object", 0);
                            Done();
                        }
                    }
                }


                // If any of the geometry IDs match the IDs of the incoming markers, translate the geometry to the corresponding plane
                /*foreach (int id in idGeometryPairs.Keys)
                {
                    if (idPlanePairs.ContainsKey(id))
                    {
                        GeometryBase geometry = idGeometryPairs[id];
                        Plane plane = idPlanePairs[id];

                        geometry.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
                        translatedGeometries.Add(geometry);
                    }
                }*/

                ReportProgress("Done", 1);
                Done();
            }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetData(0, ref idGeometryPairs);
                DA.GetDataList(1, detectedIds);
                DA.GetDataList(2, detectedPlanes);
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