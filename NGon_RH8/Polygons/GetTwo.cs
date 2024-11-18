using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper;

namespace NGon_RH8.SubD {
    public class GetTwo : GH_Component {

        public GetTwo()
          : base("GetTwo", "GetTwo",
              "Get Two Largest Outlines",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Brep", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "T", "0 - two curves,  other all", GH_ParamAccess.item,0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            Brep brep = NGonCore.Clipper.DataAccessHelper.Fetch<Brep>(DA, "Brep");
            int type = NGonCore.Clipper.DataAccessHelper.Fetch<int>(DA, "Type");

            var c = new List<Curve>();
            var l = new List<double>();
   

            foreach (BrepFace face in brep.Faces) {
                //Brep b = face.DuplicateFace(false);
                //Curve[] edges = b.DuplicateNakedEdgeCurves(true, false);
                //c.AddRange(Curve.JoinCurves(edges));
                c.Add(face.OuterLoop.To3dCurve());
                //var curve = face.OuterLoop.To3dCurve();
            }

            foreach (Curve curve in c)
                l.Add(curve.GetLength());


            var cArray = c.ToArray();
            var lArray = l.ToArray();
            Array.Sort(lArray, cArray);
            cArray=cArray.Reverse().ToArray();



            switch (type) {

                case (0):
                    if (cArray.Length==1)
                        DA.SetDataList(0, new Curve[] { cArray[0]});
                    else if(cArray.Length > 1)
                        DA.SetDataList(0, new Curve[] { cArray[0], cArray[1] });
                    break;
                default:
                    DA.SetDataList(0, cArray);
                    break;

            }









        }

   

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.GetTwo;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf8-b2b4-8796a5ee1f82"); }
        }
    }
}