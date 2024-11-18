using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineMapMesh : GH_Component {
        public PolylineMapMesh()
          : base("MapMesh", "MapMesh",
              "Polyline Map Mesh", "NGon",
               "Polygon")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves","C","Curve to map",GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh0", "M0", "Source Mesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh1", "M1", "Target Mesh", GH_ParamAccess.item);


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "A list of curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh m0 = DA.Fetch<Mesh>("Mesh0");
            Mesh m1 = DA.Fetch<Mesh>("Mesh1");
            List<Curve> curves = DA.FetchList<Curve>("Curves");

            List<Polyline> polylines = new List<Polyline>();
            foreach (Curve c in curves) {
                Polyline polyline;
                if(c.TryGetPolyline(out polyline)) {
                    polylines.Add(polyline);
                }
            }

  
            DA.SetDataList(0, polylines.MappedFromMeshToMesh(m0, m1));


       }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.mapmesh1;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-1589-8b14-4b46be84a147"); }
        }
    }
}