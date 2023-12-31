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
            pManager.AddGeometryParameter("Geometries", "G", "The geometries to be translated", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IDs", "IDs", "The IDs to assign to geometries", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Detected IDs", "ID", "The IDs of the markers to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Detected Marker Planes", "P", "The planes to translate the geometry to (from TableUI Receiver Component)", GH_ParamAccess.list);
            pManager.AddNumberParameter("X Scale", "X", "The scale of the X axis", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Y Scale", "Y", "The scale of the Y axis", GH_ParamAccess.item, 1);
            pManager.AddPlaneParameter("Origin", "O", "The new origin of the translated geometry", GH_ParamAccess.item, Plane.WorldXY);

            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
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
            //List<GeometryWithId> assignedGeometries = new List<GeometryWithId>();
            List<IGH_GeometricGoo> geometries = new List<IGH_GeometricGoo>();
            List<int> ids = new List<int>();
            List<int> detectedIds = new List<int>();
            List<Plane> planes = new List<Plane>();
            double xScale;
            double yScale;
            Plane origin;

            // Internal
            Dictionary<int, IGH_GeometricGoo> assignedGeometries = new Dictionary<int, IGH_GeometricGoo>();
            Dictionary<int, Plane> idPlanePairs = new Dictionary<int, Plane>();

            // Outputs
            List<IGH_GeometricGoo> translatedGeometries = new List<IGH_GeometricGoo>();

            public TranslationWorker(GH_Component parent) : base(parent) { }

            public override void DoWork(Action<string, double> ReportProgress, Action Done)
            {
                if (geometries.Count == 0 || ids.Count == 0 || detectedIds.Count == 0 || planes.Count == 0)
                {
                    ReportProgress("No geometries or IDs detected", 0);
                    return;
                }
                else if (geometries.Count != ids.Count)
                {
                    ReportProgress("Number of geometries and IDs do not match", 0);
                    return;
                }
                else if (detectedIds.Count != planes.Count)
                {
                    ReportProgress("Number of detected IDs and planes do not match", 0);
                    return;
                }
                for (int i = 0; i < ids.Count; i++)
                {
                    assignedGeometries.Add(ids[i], geometries[i]);
                }
                // Build a dictionary of planes and their corresponding IDs
                for (int i = 0; i < detectedIds.Count; i++)
                {
                    Plane plane = planes[i];
                    // Translate the plane to be relative to the new origin
                    plane.Transform(Transform.PlaneToPlane(Plane.WorldXY, origin));
                    Vector3d distanceVector = plane.Origin - origin.Origin;
                    // Multiply the scale of the vector by the distance scaling
                    distanceVector *= xScale;
                    // Translate the plane by the distance vector
                    plane.Translate(distanceVector);
                    // Add the plane to the dictionary
                    idPlanePairs.Add(detectedIds[i], plane);
                }
                
                for (int i = 0 ; i < detectedIds.Count ; i++) // Go through each of the detected ids
                {
                    int id = detectedIds[i];
                    IGH_GeometricGoo geometry;

                    if (idPlanePairs.ContainsKey(id))           // If id matches an id in the dictionary
                    {
                        Plane plane = idPlanePairs[id];         // Get the plane corresponding to the id

                        if (assignedGeometries.ContainsKey(id)) // If the id matches an id in the dictionary
                        {
                            if (assignedGeometries[id] == null) // If the geometry is null
                            {
                                continue;
                            }
                            geometry = assignedGeometries[id].DuplicateGeometry();
                            geometry = geometry.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane)); // Translate the geometry to the plane
                        }
                        else
                        {
                            continue;
                        }

                        translatedGeometries.Add(geometry); // Add the translated geometry to the list of translated geometries
                    }
                }

                ReportProgress("Done", 1);
                Done();
            }

            public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
            {
                DA.GetDataList("Geometries", geometries);
                DA.GetDataList("IDs", ids);
                DA.GetDataList("Detected IDs", detectedIds);
                DA.GetDataList("Detected Marker Planes", planes);
                DA.GetData("X Scale", ref xScale);
                DA.GetData("Y Scale", ref yScale);
                DA.GetData("Origin", ref origin);
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