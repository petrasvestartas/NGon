using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NGonGh.Utils {
    public class CurvePreview : GH_Component {
 
        public CurvePreview()
          : base("CurvePreview", "Display",
              "CurvePreview",
             "NGon","Polygon") {
        }

        List<Curve> crvs = new List<Curve>();
        List<System.Drawing.Color> col = new List<System.Drawing.Color>();
        List<int> wid = new List<int>();
        BoundingBox bbox = new BoundingBox();
        //System.Drawing.Color color = System.Drawing.Color.Black;
        //int W = 2;

        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (this.Hidden || this.Locked ) return;
            {
                if (crvs.Count == col.Count) {
                    for (int i = 0; i < crvs.Count; i++) {
                        if (crvs[i] != null) {
                            if (crvs[i].IsValid)
                                args.Display.DrawCurve(crvs[i], col[i], wid[i]);
                        }
                    }
                }
            }
        }

        public override BoundingBox ClippingBox => bbox;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Width", "W", "Width", GH_ParamAccess.tree);
            pManager.AddColourParameter("Colour", "S", "Colour", GH_ParamAccess.tree);

            // pManager.AddBooleanParameter("Ending", "F", "", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }

        protected override void BeforeSolveInstance() {

           List<Curve> crvs = new List<Curve>();
             col = new List<System.Drawing.Color>();
             wid = new List<int>();
             bbox = new BoundingBox();
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            //Input
            crvs.Clear();
            wid.Clear();
            col.Clear();
            bbox = new BoundingBox();

            // private void RunScript(Curve C, double R, int S, int t, int E, bool F, List<Interval> I, ref object A) {

            GH_Structure<GH_Curve> curve = NGonsCore.Clipper.DataAccessHelper.FetchTree<GH_Curve>(DA, "Curve");


            bool flag = false;

           
            foreach (GH_Curve c in curve.AllData(true)) {
                crvs.Add(c.Value);
                bbox.Union(c.Boundingbox);
                flag = true;
            }
            if (flag) {
                GH_Structure<GH_Integer> width = NGonsCore.Clipper.DataAccessHelper.FetchTree<GH_Integer>(DA, "Width");
                GH_Structure<GH_Colour> colour = NGonsCore.Clipper.DataAccessHelper.FetchTree<GH_Colour>(DA, "Colour");



                //Width
                List<int> widthFlat = new List<int>(width.DataCount);
                foreach (var w in width.AllData(true)) {
                    int val;
                    w.CastTo(out val);
                    widthFlat.Add(val);
                }

                if (widthFlat.Count == 0)
                    widthFlat.Add(2);

                for (int i = 0; i < this.crvs.Count; i++)
                    this.wid.Add(widthFlat[i % widthFlat.Count]);


                //Color
                List<System.Drawing.Color> colorFlat = new List<System.Drawing.Color>(colour.DataCount);
                foreach (var w in colour.AllData(true)) {
                    System.Drawing.Color val;
                    w.CastTo(out val);
                    colorFlat.Add(val);
                }

                if (colorFlat.Count == 0)
                    colorFlat.Add(System.Drawing.Color.Black);

                for (int i = 0; i < this.crvs.Count; i++)
                    this.col.Add(colorFlat[i % colorFlat.Count]);










            } else {
                crvs.Clear();
                wid.Clear();
                col.Clear();
                bbox = new BoundingBox();
            }


        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.display;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-7c0f-89ad2b9e9148"); }
        }
    }
}