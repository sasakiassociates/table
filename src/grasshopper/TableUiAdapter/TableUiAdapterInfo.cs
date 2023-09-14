using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace TableUiAdapter
{
    public class TableUiAdapterInfo : GH_AssemblyInfo
    {
        public override string Name => "TableUiAdapter";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("5c5c3d60-f6f6-4d80-a611-780ff4d7828b");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}