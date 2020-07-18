using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SubD.SubD {
    public class BrepBrep : GH_Component {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public BrepBrep()
          : base("BrepBrep", "BrepBrep",
              "Cut an open Curve with Closed Curve",
              "NGon", "Util") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep0", "Brep0", "Brep0", GH_ParamAccess.tree);
            pManager.AddBrepParameter("Brep1", "Brep1", "Brep1", GH_ParamAccess.tree);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("A", "A", "A", GH_ParamAccess.tree);
            pManager.AddGenericParameter("A", "A", "A", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {




            //var a = NGonsCore.Clipper.DataAccessHelper.FetchTree<GH_Brep>(DA, "Brep0");
            //var b = NGonsCore.Clipper.DataAccessHelper.FetchTree<Brep>(DA, "Brep1");


          



            //DA.SetDataList(0, cutCurves);
            //DA.SetDataList(1, result);



        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.CutCurve;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-4786a5ee1f08"); }
        }
    }
}