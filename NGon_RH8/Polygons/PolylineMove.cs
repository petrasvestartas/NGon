using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineMove : GH_Component {
        public PolylineMove()
          : base("Polyline Move", "PolylineMove",
              "Move Closed Curve by Z", "NGon",
               "Polygon")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curves", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Distance", GH_ParamAccess.item,1);
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            // pManager.AddCurveParameter("InnerCurves", "I", "A list of  Inner Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("A", "A", "A list of Cut Curves", GH_ParamAccess.item);
            pManager.AddCurveParameter("B", "B", "A list of Cut Curves", GH_ParamAccess.item);
            // pManager.AddCurveParameter("EdgeCurves", "E", "A list of Edge Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Curve c = null;
            DA.GetData(0, ref c);
            double d = 0;
            DA.GetData(1, ref d);

            Polyline pline = new Polyline();
            c.TryGetPolyline(out pline);
            var plines = PolylineUtil.OffsetPolyline(pline,d);

            DA.SetData(0, plines[0]);
            DA.SetData(1, plines[1]);


        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.OffsetClosedPolyline;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-575a-1589-4b61-4b46be44a876"); }
        }
    }
}