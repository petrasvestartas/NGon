using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.SimpleMesh {
    public class Triangulate : GH_Component_NGon {

        public Triangulate()
          : base("Triangulate", "Triangulate",
              "Triangulate by Curves",
              "Utilities Mesh") {
        }



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves to influence the triangulation", GH_ParamAccess.list);
            pManager[1].Optional=true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mj = new Mesh();
            List<Curve> curves = new List<Curve>();
            bool flag = DA.GetDataList(1, curves);
            if (!flag)
                curves.Add( (new Line(new Point3d(-9999, 0, 0), new Point3d(9999, 0, 0))).ToNurbsCurve() );

            if (DA.GetData(0, ref mj)) {
                mj = mj.TriangulateMeshByCurves(curves);
                //mesh.Clean();

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

                return  Properties.Resources.TriangulateByCurve;
                
            }
        }

    
        public override Guid ComponentGuid {
            get { return new Guid("1a518a74-41cc-43b4-a2bc-4eac61b3764e"); }
        }
    }
}