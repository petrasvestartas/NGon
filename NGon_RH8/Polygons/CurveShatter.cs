using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.SubD {
    public class CurveShatter : GH_Component {

        public CurveShatter()
          : base("CurveShatter", "CurveShatter",
              "Shatter Curves by Points",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Points to fix points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "D", "Distance where points will affect the curve", GH_ParamAccess.item,0.01);

            pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            Curve C_ = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "Curve");
            Curve C = C_.DuplicateCurve();
            double T = NGonCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Tolerance");
            List<Point3d> P = NGonCore.Clipper.DataAccessHelper.FetchList<Point3d>(DA, "Points");


            var c = NGonCore.PolylineUtil.ShatterCurve(C, P, T);




            
            DA.SetDataList(0, c);


         
        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.shatter;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-9873a5ee1f08"); }
        }
    }
}