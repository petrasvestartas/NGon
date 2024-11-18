using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8 {
    public class ClosestCurveComponent : GH_Component {

        BoundingBox bbox = new BoundingBox();
        List<Line> crvs = new List<Line>();
        List<Point3d> pts = new List<Point3d>();

        public override BoundingBox ClippingBox => bbox;
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (!base.Hidden && !base.Locked) {

             
                    args.Display.DrawLines(crvs, System.Drawing.Color.Red, 5);

                    args.Display.DrawPoints(pts,Rhino.Display.PointStyle.RoundSimple,3, System.Drawing.Color.Red);


            }
        }

        public ClosestCurveComponent()
          : base("ClosestCurveComponent", "CPCurve",
              "Search closest curves by tolerance",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Search closest curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "T", "Distance to search", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Eccent", "E", "Deviation from given value", GH_ParamAccess.item, -1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            //pManager.AddCurveParameter("CP", "CP", "Closest curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("C0", "C0", "Closest curve A", GH_ParamAccess.list);
            pManager.AddCurveParameter("C1", "C1", "Closest curve B", GH_ParamAccess.list);
            pManager.AddIntegerParameter("I0", "I0", "Closest curve ID A minus means curve is touching by end", GH_ParamAccess.list);
            pManager.AddIntegerParameter("I1", "I1", "Closest curve ID B minus means curve is touching by end", GH_ParamAccess.list);
            pManager.AddNumberParameter("T0", "T0", "Closest curve Parameter A", GH_ParamAccess.list);
            pManager.AddNumberParameter("T1", "T1", "Closest curve Parameter B", GH_ParamAccess.list);
            pManager.AddGenericParameter("D", "D", "Data of closest parameters", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            bbox = new BoundingBox();
            crvs.Clear();
            pts.Clear();

            List<Curve> curves = new List<Curve>();
            DA.GetDataList(0, curves);
            double tolerance = 1;
            DA.GetData(1, ref tolerance);
            double ecc = -1;
            DA.GetData(2, ref ecc);
            ecc = ecc< 0 ? Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance : ecc;

            CP.ClosestCurveT cp = new CP.ClosestCurveT();
            CP.ClosestCurves(curves, tolerance, ref cp);

            foreach (Curve c in curves)
                bbox.Union(c.GetBoundingBox(false));

            for(int i = 0; i < cp.ID0.Count; i++) {
                Point3d p0 = cp.L0[i].PointAt(cp.T0[i]);
                Point3d p1 = cp.L1[i].PointAt(cp.T1[i]);
                if(p0.DistanceToSquared(p1)<= ecc) {
                    pts.Add(p0);
                } else {
                    crvs.Add(new Line(p0,p1));
                }
            }




            DA.SetDataList(0, cp.L0);
            DA.SetDataList(1, cp.L1);
            DA.SetDataList(2, cp.ID0);
            DA.SetDataList(3, cp.ID1);
            DA.SetDataList(4, cp.T0);
            DA.SetDataList(5, cp.T1);
            DA.SetData(6, cp);




        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.ClosestCurve;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("968c3782-d3c9-4f5f-8dfa-0e78568f5dbc"); }
        }

    }
}