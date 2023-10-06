using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace TableUiCompanions
{
    public class TableUiCompanionsInfo : GH_AssemblyInfo
    {
        public override string Name => "TableUiCompanions";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("cdc9d39b-6584-4a10-89ee-744c8196a15c");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}