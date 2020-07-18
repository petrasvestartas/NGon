using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore.Clipper;

namespace NGonGh.Polygons {
    public class CurveChamfer : GH_Component {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public CurveChamfer()
          : base("Chamfer", "Chamfer",
              "Cut an open Curve with Closed Curve",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Poly", "P", "Polyline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Chamfer", "C", "Chamfer values, positive number is scalar value, negative is distance", GH_ParamAccess.item,-0.1);
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Curve curve = DA.Fetch<Curve>("Poly");
            double t = DA.Fetch<double>("Chamfer");

            Polyline polyline;
            if (curve.TryGetPolyline(out polyline)) {

                DA.SetData(0, PolylineUtil.Chamfer(polyline, t));
            }




        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.Chamfer;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-1bf1-b2b4-4786a5ee1f89"); }
        }
    }
}
