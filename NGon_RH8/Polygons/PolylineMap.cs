using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineMap : GH_Component {
        public PolylineMap()
          : base("MapCrv", "MapCrv",
              "Maps a list of curve from one polyline to another", "NGon",
               "Polygon")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "A list of curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Source0", "S0", "Source Polyline 0", GH_ParamAccess.item);
            pManager.AddCurveParameter("Target0", "T0", "Target Polyline 0", GH_ParamAccess.item);
            pManager.AddCurveParameter("Source1", "S1", "Source Polyline 1", GH_ParamAccess.item);
            pManager.AddCurveParameter("Target1", "T1", "Target Polyline 1", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "A list of curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curve");
            Curve source0 = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "Source0");
            Curve target0 = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "Target0");
            Curve source1 = null;
            Curve target1 = null;
            source1 = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "Source1");
             target1 = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "Target1");



            DA.SetDataList(0,NGonCore.Mapping.MapCurve(curves, target0, source0, null, null));


       }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MapCurve;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-1589-8b14-4b46be64a785"); }
        }
    }
}