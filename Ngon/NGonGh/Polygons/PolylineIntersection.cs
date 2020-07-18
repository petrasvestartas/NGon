using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Polylines
{
    public class PolylineIntersection : GH_Component {
        public PolylineIntersection()
          : base("Intersection", "Intersection",
              "Polyline intersection", "NGon",
               "Polygon")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve0", "C0", "A list of curves to cut", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve1", "C1", "A list of curves cutters", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("InnerCurves", "I", "A list of  Inner Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("CutCurves", "C", "A list of Cut Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("EdgeCurves", "E", "A list of Edge Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curve0");
            List<Curve> cutters = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curve1");

            var result = NGonsCore.Geometry.RhinClip.BooleanIntersection(curves, cutters, 0);

            DA.SetDataList(0, result.Item1);
            DA.SetDataList(1, result.Item2);
            DA.SetDataList(2, result.Item3);


        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.intersection;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-890a-1589-8b61-4b87be64a876"); }
        }
    }
}