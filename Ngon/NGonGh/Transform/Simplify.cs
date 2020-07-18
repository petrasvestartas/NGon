using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Utils {
    public class Simplify : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Simplify()
          : base("Simplify", "Simplify",
              "Remove points that are less than valence 3 or fixed",
             "Transform") {
        }

      
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Fixed","P","Fixed Points",GH_ParamAccess.list);

            pManager[1].Optional = true;
            

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            // private void RunScript(Curve C, double R, int S, int t, int E, bool F, List<Interval> I, ref object A) {
            Mesh m = new Mesh();
            List<Point3d> points = new List<Point3d>();

            if (DA.GetData(0, ref m)) {



                DA.GetDataList(1, points);

                m = m.SimplifyMesh(points);

                if (m.IsValid) { }




                base.PreparePreview(m, DA.Iteration);

                DA.SetData(0, m);
            }


        }
          
            

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.simplify;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-48ad2b9e1637"); }
        }
    }
}