using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Polygons {
    public class CurveCP : GH_Component {
  
        public CurveCP()
          : base("CurveCP", "CurveCP",
              "Is point close to a given curve",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve","C","Curve to search fro a closest point",GH_ParamAccess.item);
            pManager.AddNumberParameter("Tol","T","Distance",GH_ParamAccess.item,0.01);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID","I","Indices of Closest points",GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Point3d> points = new List<Point3d>();
            Curve c = null;
            double t = 0.01;

            DA.GetDataList(0, points);
            DA.GetData(1, ref c);
            DA.GetData(2, ref t);

            List<int> id = new List<int>();
            List<Point3d> cp = c.IsPointsCloseToCurve(points, ref id, t);

            DA.SetDataList(0, cp);
            DA.SetDataList(1, id);
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.CurveCP;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("537599f5-680a-44dd-8ad2-d5bd06894fa2"); }
        }
    }
}