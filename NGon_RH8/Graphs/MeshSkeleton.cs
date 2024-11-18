using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Graphs {
    public class MeshSkeleton : GH_Component {

        public MeshSkeleton()
          : base("MeshSkeleton", "MeshSkeleton",
              "MeshSkeleton",
              "NGon", "Graph") {
        }

          protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {


            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Must be the same number of points as mesh faces else they will be igonred", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Smooth", "I", "Iterations for smoothing", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Prune", "N", "Prune Naked Branches, iterations", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddCurveParameter("Skeleton", "S", "Skeleton lines", GH_ParamAccess.list);
            pManager.AddCurveParameter("3ValenceLines", "3L", "Lines that are drawn from face point to its vertices", GH_ParamAccess.tree);
            pManager.AddPointParameter("Naked Points", "P", "End Points of skeleton", GH_ParamAccess.tree);
            pManager.AddPointParameter("All End Points", "P", "All End Points", GH_ParamAccess.list);


        }

        protected override void SolveInstance(IGH_DataAccess DA) {


            Mesh m = DA.Fetch<Mesh>("Mesh");
            List<Point3d> p = DA.FetchList<Point3d>("Points");
            int n = DA.Fetch<int>("Smooth");
            int prune = DA.Fetch<int>("Prune");
            if (m != null) {
                DataTree<Line> dt;
            DataTree<Point3d> naked;
            var c = NGonCore.MeshSkeleton.GetCurves(m, p, out dt,out naked);

        

            //Smooth
            foreach(Polyline curve in c) {

                Point3d start = new Point3d(curve[0]);

                for (int i = 0; i < n; i++) {
                    curve.Smooth(0.1);

                    if (curve[0].DistanceToSquared(curve.Last()) < 0.001) {
                  
                        curve[0] = start;
                        curve[curve.Count - 1] = start;
                    }
                }
            }


                //Prune


                List<Polyline> c_ = new List<Polyline>();
                foreach (Polyline cc in c) {
                    c_.Add(new Polyline(cc));

                }



                for (int y = 0; y < prune; y++) {
                    List<Line> lines = new List<Line>();

                    foreach (Polyline cc in c_)
                        lines.Add(new Line(cc.First(), cc.Last()));

                    List<Polyline> curves = new List<Polyline>();

                    Tuple<Point3d[], List<string>, List<int>, List<int>, List<int>, DataTree<int>> data = NGonCore.Graphs.LineGraph.GetGraphData(lines);
                    for (int i = 0; i < data.Item3.Count; i++) {
                        if (data.Item6.Branch(data.Item3[i]).Count != 1 && data.Item6.Branch(data.Item4[i]).Count != 1) {
                            curves.Add(new Polyline(c_[i]));
                        }
                    }
                    c_ = curves;

                }
                //A = curves;

                DA.SetDataList(0, c_);
            DA.SetDataTree(1, dt);
                DA.SetDataTree(2, naked);

                List<Point3d> ptAll = new List<Point3d>();
                ptAll.AddRange(naked.AllData());

                foreach (var cc in c_) {
                    ptAll.Add(cc[0]);
                    ptAll.Add(cc[cc.Count - 1]);
                }

                foreach (var cc in dt.AllData()) {
                    ptAll.Add(cc.From);
                    ptAll.Add(cc.To);
                }
                DA.SetDataList(3, ptAll);
            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.skeleton;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("7a8e9987-f8cb-41f9-979f-589cf378ecdc"); }
        }



    }
}