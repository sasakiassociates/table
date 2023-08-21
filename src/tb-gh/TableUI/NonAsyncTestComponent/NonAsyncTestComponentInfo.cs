using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace NonAsyncTestComponent
{
    public class NonAsyncTestComponentInfo : GH_AssemblyInfo
    {
        public override string Name => "NonAsyncTestComponent";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("e27bbe63-46d3-49d3-941a-54d99cdfbf10");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}