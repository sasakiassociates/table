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
        public IGH_GeometricGoo Geometry { get; set; }

        public BoundingBox Boundingbox => Geometry.Boundingbox;

        public Guid ReferenceID { get => Geometry.ReferenceID; set => Geometry.ReferenceID = value; }

        public bool IsReferencedGeometry => Geometry.IsReferencedGeometry;

        public bool IsGeometryLoaded => Geometry.IsGeometryLoaded;

        public bool IsValid => Geometry.IsValid;

        public string IsValidWhyNot => Geometry.IsValidWhyNot;

        public string TypeName => Geometry.TypeName;

        public string TypeDescription => Geometry.TypeDescription;

        public GeometryWithId(IGH_GeometricGoo geometry, int id)
        {
            Id = id;
            Geometry = geometry;
        }

        public IGH_GeometricGoo DuplicateGeometry()
        {
            return Geometry.DuplicateGeometry();
        }

        public BoundingBox GetBoundingBox(Transform xform)
        {
            return Geometry.GetBoundingBox(xform);
        }

        public IGH_GeometricGoo Transform(Transform xform)
        {
            return Geometry.Transform(xform);
        }

        public IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return Geometry.Morph(xmorph);
        }

        public bool LoadGeometry()
        {
            return Geometry.LoadGeometry();
        }

        public bool LoadGeometry(RhinoDoc doc)
        {
            return Geometry.LoadGeometry(doc);
        }

        public void ClearCaches()
        {
            Geometry.ClearCaches();
        }

        public IGH_Goo Duplicate()
        {
            return Geometry.Duplicate();
        }

        public IGH_GooProxy EmitProxy()
        {
            return Geometry.EmitProxy();
        }

        public bool CastFrom(object source)
        {
            return Geometry.CastFrom(source);
        }

        public bool CastTo<T>(out T target)
        {
            return Geometry.CastTo(out target);
        }

        public object ScriptVariable()
        {
            return Geometry.ScriptVariable();
        }

        public bool Write(GH_IWriter writer)
        {
            return Geometry.Write(writer);
        }

        public bool Read(GH_IReader reader)
        {
            return Geometry.Read(reader);
        }
    }
}
