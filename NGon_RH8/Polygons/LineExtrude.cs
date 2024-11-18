using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Polylines
{
    public class LineExtrude : GH_Component {
        public LineExtrude()
          : base("LineExtrude", "LineExtrude",
              "LineExtrude", "NGon",
               "Polygon")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "Line", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "V", "Vector", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddNumberParameter("Dist", "D", "Distance", GH_ParamAccess.item,1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddPointParameter("Centers", "C", "NGon Centers", GH_ParamAccess.list);
           // pManager.AddVectorParameter("Normals", "N", "Average normal at ngon center", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Line line = NGonCore.Clipper.DataAccessHelper.Fetch<Line>(DA, "Line");
            Vector3d vector3D = NGonCore.Clipper.DataAccessHelper.Fetch<Vector3d>(DA, "Vector");
            double dist = NGonCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Dist");

            DA.SetData(0, NGonCore.LineUtil.ExtrudeLine(line,vector3D,dist));
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ExtrudeLine;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-570a-4168-8b61-1b46be64a458"); }
        }
    }
}