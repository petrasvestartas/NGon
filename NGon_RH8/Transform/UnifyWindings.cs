using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Modifications {
    public class UnifyWindings : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the Dual class.
        /// </summary>
        public UnifyWindings()
          : base("UnifyWindings", "Unify",
              "UnifyWindings",
              "Transform") {
        }

  
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                Mesh mesh = null;
                DA.GetData(0, ref mesh);

               
                if (mesh != null) {

                    var bfs = NGonCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(mesh, "0");

                    var plines = mesh.UnifyWinding(bfs.Item2[0].ToArray());


                    Mesh mesh_ = MeshCreate.MeshFromPolylines(plines, -1, 0);
                    //Mesh mesh_ = NGonCore.MeshCreate.MeshFromPolylines(plines, 0.01, -1);


                    PreparePreview(mesh_, DA.Iteration);
                    DA.SetData(0, mesh_);

                }
            }catch(Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.unifyWindings;// Properties.Resources.Dual;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("f2f8ba8b-822c-4e11-9ece-60b7aca4785f"); }
        }
    }
}