using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace residence
{
    public class distanceInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "gh";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                 return new Guid("daeaed41-e0f6-4953-a150-b4f1709dc006");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
