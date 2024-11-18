using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.SimpleMesh {
    public class DiagonalMesh : GH_Component_NGon {

        public DiagonalMesh()
          : base("DiagonalMesh", "Diagonal",
              "DiagonalMesh",
              "Utilities Mesh") {
        }

 
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

  
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mj = new Mesh();
            if(DA.GetData(0,ref mj)){
                mj = mj.Diagonalize();

               mj = mj.WeldUsingRTree(0.01, true);


                mj.Compact();
                mj.Vertices.CombineIdentical(true, true);
                mj.Vertices.CullUnused();

                if (mj.Ngons.Count > 0)
                    mj.UnifyNormalsNGons();
                else
                    mj.UnifyNormals();


                mj.Weld(3.14159265358979);
                mj.FaceNormals.ComputeFaceNormals();
                mj.Normals.ComputeNormals();



                if (mj.SolidOrientation() == -1)
                    mj.Flip(true, true, true);


                this.PreparePreview(mj, DA.Iteration);
                DA.SetData(0, mj);
            }
        }

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Diagonal;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("1a518a74-41cc-43b4-a2bc-7eac61b3769e"); }
        }
    }
}