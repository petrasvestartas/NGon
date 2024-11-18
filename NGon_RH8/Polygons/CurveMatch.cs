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
    public class CurveMatch : GH_Component {

        public CurveMatch()
          : base("CurveMatch", "CurveMatch",
              "CurveMatch",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("Type", "T", "Type", GH_ParamAccess.item,0);


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            List<Curve> Curves= NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curves");
            double type = NGonCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Type");

            if (Curves.Count != 2) return;

            var c = new List<Curve>();
            int t = (int)Math.Floor(type);
            switch (t) {


                case (1):
                    c = CurveUtil.AlignSeams(Curves, 0, false, false);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (2):
                    c = CurveUtil.AlignSeams(Curves, 0, true, false);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (3):
                    c = CurveUtil.AlignSeams(Curves, 0, false, true);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (4):
                    c = CurveUtil.AlignSeams(Curves, 0, true, true);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;
                    

           case (-1):
                    c = CurveUtil.AlignSeams(Curves, Math.Abs(type), false, false);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (-2):
                    c = CurveUtil.AlignSeams(Curves, Math.Abs(type), true, false);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (-3):
                    c = CurveUtil.AlignSeams(Curves, Math.Abs(type), false, true);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                case (-4):
                    c = CurveUtil.AlignSeams(Curves, Math.Abs(type), true, true);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

                default:
                    c = CurveUtil.AlignSeams(Curves);
                    DA.SetData(0, c[0]);
                    DA.SetData(1, c[1]);
                    break;

            }




        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.MatchCurves;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf8-b2b4-8796a5ee1f28"); }
        }
    }
}