using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace NGon_RH8
{
    public class NGon_RH8Info : GH_AssemblyInfo
    {
        public override string Name => "NGon_RH8";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("f6d5dced-feca-40f7-90d8-ebcaa945a53a");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}