using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace RequestComponent
{
    public class RequestComponentInfo : GH_AssemblyInfo
    {
        public override string Name => "RequestComponent";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("08752e9d-ad46-4cba-bdae-f4478a820d03");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}