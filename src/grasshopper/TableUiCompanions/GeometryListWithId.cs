using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUiCompanions
{
    internal class GeometryListWithId : List<IGH_GeometricGoo>
    {
        public int Id { get; set; }
        public List<IGH_GeometricGoo> Building { get; set; }

        public GeometryListWithId(List<IGH_GeometricGoo> building, int id)
        {
            Id = id;
            Building = building;
        }
    }
}
