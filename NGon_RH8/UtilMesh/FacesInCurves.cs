using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.SimpleMesh {
    public class FacesInCurves : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the DiagonalMesh class.
        /// </summary>
        public FacesInCurves()
          : base("FacesInCurves", "FacesInCurves",
              "FacesInCurves",
              "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves to influence the triangulation", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = new Mesh();
            List<Curve> curves = new List<Curve>();
            bool flag = DA.GetDataList(1, curves);
            //if (!flag)
              //  curves.Add((new Line(new Point3d(-9999, 0, 0), new Point3d(9999, 0, 0))).ToNurbsCurve());

            if (DA.GetData(0, ref mesh)) {
                mesh.Ngons.Clear();
                mesh = mesh.RemoveByCurves(curves);
                this.PreparePreview(mesh, DA.Iteration);
                DA.SetData(0, mesh);
            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.CullFaces;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("1a518a74-41cc-43b4-a2bc-7eac58b3769e"); }
        }
    }
}