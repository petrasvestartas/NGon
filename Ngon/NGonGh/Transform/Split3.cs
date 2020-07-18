using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Utils {
    public class Split3 : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Split3()
          : base("Split3", "Split3",
              "Split ngons into 3 polylines if their number of points is bigger than user defined, first biggest length polygons are split",
             "Transform") {
        }

      
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Size", "S", "Max Size", GH_ParamAccess.item, 7);
            pManager.AddIntegerParameter("Iteration", "I", "Max Iterations", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Smooth", "S", "Smooth Iterations", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Plines", "P", "Plines", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            int maxSize = 7;
            int maxIterations = 7;
            int smooth = 1;
            DA.GetData(1, ref maxSize);
            DA.GetData(2, ref maxIterations);
            DA.GetData(3, ref smooth);

            if (DA.GetData(0, ref m)) {


                if (m != null) {
                    var bfs = NGonsCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(m, "0");
                    m = NGonsCore.MeshCreate.MeshFromPolylines(m.UnifyWinding(bfs.Item2[0].ToArray()), 0.01);
                }

                var plines = m.SplitNGonsIn3(maxSize,maxIterations, smooth);
                base.PreparePreview(m, DA.Iteration, plines, false);
                DA.SetDataList(0, plines);
            }


        }
          
            

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Split;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-58ad2b9e1634"); }
        }
    }
}