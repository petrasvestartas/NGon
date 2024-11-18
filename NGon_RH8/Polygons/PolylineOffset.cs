using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineOffset : GH_Component {
        public PolylineOffset()
          : base("Polyline Offset", "Offset",
              "Polyline Offset", "NGon",
               "Polygon")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("Distance", "D", "Distance", GH_ParamAccess.item,1);
            pManager.AddNumberParameter("Divisions", "T", "Divisions for curves", GH_ParamAccess.item,0);
            pManager[2].Optional = true;
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            // pManager.AddCurveParameter("InnerCurves", "I", "A list of  Inner Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("C", "C", "A list of Cut Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("H", "H", "A list of Cut Curves", GH_ParamAccess.list);
            // pManager.AddCurveParameter("EdgeCurves", "E", "A list of Edge Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Curve> curves = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curve");
            double distance = NGonCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Distance");
             double D = NGonCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Divisions");

            if (distance < 0.00001) {
                //distance = 0.01;
            } else {


                List<Polyline> poly = new List<Polyline>();
                foreach (Curve crv in curves) {

                    Polyline po;
                    bool flag = crv.TryGetPolyline(out po);


                    if (flag) {

                        poly.Add(po);
                    } else {
                        if (D == 0)
                            poly.Add(crv.ToPolylineFromCP());
                        else {
                            poly.Add(crv.ToPolyline(0, 0, 0, D).ToPolyline());
                        }
                    }
                }


                List<Polyline> c = new List<Polyline>();
                List<Polyline> h = new List<Polyline>();
                NGonCore.Geometry.Polyline3D.Offset(poly, distance, out c, out h);


                DA.SetDataList(0, c);
                DA.SetDataList(1, h);
            }

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.offsetBothSides;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-575a-1589-8b61-4b46be87a147"); }
        }
    }
}