using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.SubD {
    public class CurveCut : GH_Component {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public CurveCut()
          : base("CurveCut", "CurveCut",
              "Cut an open Curve with Closed Curve",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("ClosedCurve", "Closed", "Curves", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "T", "-1 - curve is outside, 0 - coincident, 1 - inside", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {



           
            Curve closedCurve = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "ClosedCurve");
            List<Curve> C = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curves");

            //NGonCore.Geometry.RhinClip.CurveToPolyline(closedCurve);

            List<Curve> cutCurves = new List<Curve>();
            int[] result = new int[C.Count];
                
            for (int i = 0; i < C.Count; i++) {
                
              //  NGonCore.Geometry.Polyline3D.Boolean(NGonCore.Clipper642.ClipType.ctDifference, x, CurvesToPolyline(cutters, dist);  closedCurve.ToPolyline(, Plane.WorldXY, 0.001, true);

                Curve cutCurve = NGonCore.CurveUtil.BooleanOpenCurve(closedCurve, C[i], out result[i]);
                if (result[i] > 0) {
                    cutCurves.Add(cutCurve);
                }
            }

     




            DA.SetDataList(0, cutCurves);
            DA.SetDataList(1, result);



        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.CutCurve;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-4786a5ee1f08"); }
        }
    }
}