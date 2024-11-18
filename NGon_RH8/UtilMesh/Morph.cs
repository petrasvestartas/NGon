using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Utils
{
    public class Morph : GH_Component_NGon
    {
  
        public Morph()
          : base("Morph", "Morph",
              "Morph",
             "Utilities Mesh")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pt", "Points", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polylines", "Plines", "Plines", GH_ParamAccess.list);
            pManager.AddCurveParameter("SquarePair0", "SP0", "SP0", GH_ParamAccess.list);
            pManager.AddCurveParameter("SquarePair1", "SP1", "SP1", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pt", "Points", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polylines", "Plines", "Plines", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Points
            List<Point3d> pts = new List<Point3d>();
           bool flagPts =  DA.GetDataList(0, pts);

            //Plines
            var curves = new List<Curve>();
            bool flagPlines =  DA.GetDataList(1, curves);
            var plines = NGonCore.PolylineUtil.ToPolylines(curves);

            //SquarePair0
            var c0 = new List<Curve>();
            DA.GetDataList(2, c0);
            var squarePair0 = NGonCore.PolylineUtil.ToPolylines(c0);

            //SquarePair1
            var c1 = new List<Curve>();
            DA.GetDataList(3, c1);
            var squarePair1 = NGonCore.PolylineUtil.ToPolylines(c1);

            if (flagPts)
            {
                var ptsMorphed = NGonCore.Morph.MorphPoints(pts, squarePair0.ToList(), squarePair1.ToList());
                DA.SetDataList(0, ptsMorphed);
            }

            if (flagPlines)
            {
                var plinesMorphed = NGonCore.Morph.MorphPolylines(plines.ToList(), squarePair0.ToList(), squarePair1.ToList());
                DA.SetDataList(1, plinesMorphed);
            }

        }

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Morph;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-85ad2b9e1122"); }
        }
    }
}