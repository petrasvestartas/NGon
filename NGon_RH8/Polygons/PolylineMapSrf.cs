using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineMapSrf : GH_Component {
        public PolylineMapSrf()
          : base("MapSrf", "MapSrf",
              "Polyline Map Surface", "NGon",
               "Polygon")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves","C","Curve to map",GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Srf0", "S0", "Source Mesh", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Srf1", "S1", "Target Mesh", GH_ParamAccess.item);


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "A list of curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Surface m0 = DA.Fetch<Surface>("Srf0");
            Surface m1 = DA.Fetch<Surface>("Srf1");
            List<Curve> curves = DA.FetchList<Curve>("Curves");

            List<Polyline> polylines = new List<Polyline>();
            foreach (Curve c in curves) {
                Polyline polyline;
                if(c.TryGetPolyline(out polyline)) {
                    polylines.Add(polyline);
                }
            }

  
            DA.SetDataList(0, polylines.MappedFromSurfaceToSurface(m0, m1));


       }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.mapsrf;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-1589-8b14-4b46be14a185"); }
        }
    }
}