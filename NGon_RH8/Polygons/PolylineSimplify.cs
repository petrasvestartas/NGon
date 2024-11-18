using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineSimplify : GH_Component {
        public PolylineSimplify()
          : base("Simplify", "Simplify",
              "Simplify Polyline", "NGon",
               "Polygon")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddPointParameter("Centers", "C", "NGon Centers", GH_ParamAccess.list);
           // pManager.AddVectorParameter("Normals", "N", "Average normal at ngon center", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA,"Curve");

            DA.SetData(0, NGonCore.Geometry.RhinClip.SimplifyCurve(curve));
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.simplifyPolyline;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-4161-8b61-1b46be64a157"); }
        }
    }
}