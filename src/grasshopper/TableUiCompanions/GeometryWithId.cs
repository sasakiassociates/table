using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiCompanions
{
    internal class GeometryWithId : IGH_GeometricGoo
    {
        public int Id { get; set; }
        public List<IGH_GeometricGoo> Geometries { get; set; }
        public bool IsList { get; set; }

        public BoundingBox Boundingbox
        {
            get
            {
                if (IsList)
                {
                    BoundingBox bbox = BoundingBox.Empty;
                    foreach (var geometry in Geometries)
                    {
                        bbox.Union(geometry.Boundingbox);
                    }
                    return bbox;
                }
                return Geometries[0].Boundingbox;
            }
        }

        public Guid ReferenceID { get; set; }

        public bool IsReferencedGeometry => IsList ? Geometries[0].IsReferencedGeometry : Geometries[0].IsReferencedGeometry;

        public bool IsGeometryLoaded => IsList ? Geometries[0].IsGeometryLoaded : Geometries[0].IsGeometryLoaded;

        public bool IsValid => IsList ? Geometries[0].IsValid : Geometries[0].IsValid;

        public string IsValidWhyNot => IsList ? Geometries[0].IsValidWhyNot : Geometries[0].IsValidWhyNot;

        public string TypeName => IsList ? Geometries[0].TypeName : Geometries[0].TypeName;

        public string TypeDescription => IsList ? Geometries[0].TypeDescription : Geometries[0].TypeDescription;

        public GeometryWithId(IGH_GeometricGoo geometry, int id)
        {
            Id = id;
            Geometries = new List<IGH_GeometricGoo>() { geometry };
            IsList = false;
        }

        public GeometryWithId(List<IGH_GeometricGoo> geometries, int id)
        {
            Id = id;
            Geometries = geometries;
            IsList = true;
        }

        public IGH_GeometricGoo DuplicateGeometry()
        {
            if (IsList)
            {
                var duplicateGeometries = Geometries.Select(g => g.DuplicateGeometry()).ToList();
                return new GeometryWithId(duplicateGeometries, Id);
            }
            return Geometries[0].DuplicateGeometry();
        }

        public BoundingBox GetBoundingBox(Transform xform)
        {
            if (IsList)
            {
                BoundingBox bbox = BoundingBox.Empty;
                foreach (var geometry in Geometries)
                {
                    bbox.Union(geometry.GetBoundingBox(xform));
                }
                return bbox;
            }
            return Geometries[0].GetBoundingBox(xform);
        }

        public IGH_GeometricGoo Transform(Transform xform)
        {
            if (IsList)
            {
                var transformedGeometries = Geometries.Select(g => g.Transform(xform)).ToList();
                return new GeometryWithId(transformedGeometries, Id);
            }
            return Geometries[0].Transform(xform);
        }

        public IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            if (IsList)
            {
                var morphedGeometries = Geometries.Select(g => g.Morph(xmorph)).ToList();
                return new GeometryWithId(morphedGeometries, Id);
            }
            return Geometries[0].Morph(xmorph);
        }

        public bool LoadGeometry()
        {
            if (IsList)
            {
                return false; // Loading is not supported for lists
            }
            return Geometries[0].LoadGeometry();
        }

        public bool LoadGeometry(RhinoDoc doc)
        {
            if (IsList)
            {
                return false; // Loading is not supported for lists
            }
            return Geometries[0].LoadGeometry(doc);
        }

        public void ClearCaches()
        {
            foreach (var geometry in Geometries)
            {
                geometry.ClearCaches();
            }
        }

        public IGH_Goo Duplicate()
        {
            return new GeometryWithId(Geometries, Id) { ReferenceID = ReferenceID };
        }

        // This one will probably not work with lists of geometries
        public IGH_GooProxy EmitProxy()
        {
            return IsList ? Geometries[0].EmitProxy() : Geometries[0].EmitProxy();
        }

        public bool CastFrom(object source)
        {
            return false; // Casting not supported in this example
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false; // Casting not supported in this example
        }

        public object ScriptVariable()
        {
            return null; // Scripting not supported in this example
        }

        public bool Write(GH_IWriter writer)
        {
            if (IsList)
            {
                writer.SetInt32("IsList", 1);
                writer.SetInt32("Count", Geometries.Count);
                for (int i = 0; i < Geometries.Count; i++)
                {
                    var subWriter = writer.CreateChunk($"Geometry{i}");
                    if (!Geometries[i].Write(subWriter))
                        return false;
                }
            }
            else
            {
                writer.SetInt32("IsList", 0);
                writer.SetInt32("Count", 1);
                var subWriter = writer.CreateChunk("Geometry0");
                if (!Geometries[0].Write(subWriter))
                    return false;
            }

            writer.SetInt32("Id", Id);
            return true;
        }

        public bool Read(GH_IReader reader)
        {
            int isList = reader.GetInt32("IsList");
            int count = reader.GetInt32("Count");

            List<IGH_GeometricGoo> geometries = new List<IGH_GeometricGoo>();

            for (int i = 0; i < count; i++)
            {
                var subReader = reader.FindChunk($"Geometry{i}");
                if (subReader == null)
                    return false;

                var geometry = Activator.CreateInstance(this.GetType()) as IGH_GeometricGoo;
                if (!geometry.Read(subReader))
                    return false;

                geometries.Add(geometry);
            }

            Id = reader.GetInt32("Id");

            if (isList == 1)
            {
                Geometries = geometries;
                IsList = true;
            }
            else
            {
                Geometries = new List<IGH_GeometricGoo> { geometries[0] };
                IsList = false;
            }

            return true;
        }
    }
}