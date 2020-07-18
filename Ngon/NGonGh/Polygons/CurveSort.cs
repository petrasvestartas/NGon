using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper;

namespace NGonGh.SubD {
    public class CurveSort : GH_Component {

        public CurveSort()
          : base("CurveSort", "CurveSort",
              "Sort a list of curve by other curves",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("CurvesGuide", "CurvesGuide", "Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("CurvesToSort", "CurvesToSort", "Curves", GH_ParamAccess.list);


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            List<Curve> CurvesGuide= NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "CurvesGuide");
            List<Curve> CurvesToSort = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "CurvesToSort");

            Grasshopper.DataTree<Curve> dt = new DataTree<Curve>();

            int gCount = 0;

            foreach (Curve g in CurvesGuide) {

                List<double> t = new List<double>();
                List<Curve> cID = new List<Curve>();

                int i = 0;
                foreach (Curve c in CurvesToSort) {

                    Rhino.Geometry.Intersect.CurveIntersections intersection = Rhino.Geometry.Intersect.Intersection.CurveCurve(g, c, 0.01, 0.01);
                    if (intersection.Count > 0) {
                        t.Add(intersection[0].ParameterA);
                        cID.Add(c);
                    }

                    i++;
                }

                var tSorted = t.ToArray();
                var cIDSorted = cID.ToArray();
                Array.Sort(tSorted, cIDSorted);

                dt.AddRange(cIDSorted, new GH_Path(gCount++));
            }



            DA.SetDataTree(0,dt);
        


        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.CurveCurveSort;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-8796a5ee1f08"); }
        }
    }
}