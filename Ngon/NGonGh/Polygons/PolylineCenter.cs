using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Polylines
{
    public class PolylineCenter : GH_Component {
        public PolylineCenter()
          : base("PolylineCenter", "PCenter",
              "Gets center,normal and plane for polylines", "NGon",
               "Polygon")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centers", "C", "NGon Centers", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normals", "N", "Average normal at ngon center", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "P", "Average planes at ngon center", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            DA.GetData(0, ref curve);

            Polyline p = new Polyline();
            bool flag = curve.TryGetPolyline(out p);

            if (flag)
            {
                DA.SetData(0, p.CenterPoint());
                DA.SetData(1, p.Normal());
                DA.SetData(2, p.plane());

            }
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.PolylineCenter;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-4161-8b61-4b46be64a780"); }
        }
    }
}