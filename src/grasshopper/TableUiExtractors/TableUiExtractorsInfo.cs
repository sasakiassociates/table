using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace TableUiExtractors
{
    public class TableUiExtractorsInfo : GH_AssemblyInfo
    {
        public override string Name => "TableUiExtractors";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("b0e16c9b-40c7-4b03-b678-a943a74909cf");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}