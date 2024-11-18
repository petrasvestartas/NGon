using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Utils {
    public class MeshEdgeAnalysis : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshEdgeAnalysis class.
        /// </summary>
        public MeshEdgeAnalysis()
          : base("PreviewEdges", "PreviewEdges",
              "Find edges that are within the certain angle of adjacent mesh faces normals",
               "Utilities Mesh") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
            pManager.AddNumberParameter("tolerance","t","tolerance",GH_ParamAccess.item,0.49);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddLineParameter("Lines","L","Lines",GH_ParamAccess.list);
           
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();
            double angle = 0.49;

            DA.GetData<Mesh>(0,ref mesh);
            DA.GetData<double>(1,ref angle);
            if (mesh.IsValid && mesh != null) {
                List<Line> lines = NGonCore.Analysis.MeshEdgesByAngle(mesh, angle);
                this.PreparePreview(mesh, DA.Iteration, null, true, null, lines);
                DA.SetDataList(0, lines);

            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.PreviewEdges;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("638d1829-f92b-4ca7-afdf-7631754e94a6"); }
        }
    }
}