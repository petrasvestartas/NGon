using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGonGh.Utils {
    public class Smooth : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Smooth()
          : base("Smooth", "Smooth",
              "Smooth",
             "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Value","V","Value of smoothing",GH_ParamAccess.item,0.1);
            pManager.AddIntegerParameter("Iterations","I","Number of iterations for smoothing",GH_ParamAccess.item,10);
            pManager.AddBooleanParameter("Fixed", "F", "Fixed boundaries", GH_ParamAccess.item, true);
            pManager.AddPointParameter("FixedPt,","P","Fixed Points",GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            // private void RunScript(Curve C, double R, int S, int t, int E, bool F, List<Interval> I, ref object A) {
            Mesh m = new Mesh();
            List<Point3d> pts = new List<Point3d>();
            DA.GetDataList(4, pts);

            if (DA.GetData(0, ref m)) {

                double t = 0.1;
                int n = 10;
                bool f = true;

                DA.GetData(1, ref t);
                DA.GetData(2, ref n);
                DA.GetData(3,ref f);


                Mesh mesh = m.DuplicateMesh();

                if (pts.Count == 0) {
                    for (int i = 0; i < n; i++) {
                        mesh.Smooth(t, true, true, true, f, SmoothingCoordinateSystem.Object);
                    }

                } else {
                    bool[] flags = CP.RTreeSearchID(mesh.Vertices.ToPoint3dArray().ToList(), pts, 0.01);
                    List<int> ids = new List<int>();
                    for (int i = 0; i < flags.Length; i++) {
                        if (flags[i] == false)
                            ids.Add(i);
                    }
                    for (int i = 0; i < n; i++) {
                        mesh.Smooth(ids, t, true, true, true, f, SmoothingCoordinateSystem.Object, Plane.WorldXY);
                    }
                }



                    PreparePreview(mesh, DA.Iteration);

                    DA.SetData(0, mesh);

               
            }
            
          
            }

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.smooth;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-89ad2b9e1637"); }
        }
    }
}