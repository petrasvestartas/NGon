using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace NGon_RH8.SubD {
    public class TriangleSubdivision : GH_Component {
      
        public TriangleSubdivision()
          : base("TriangleSubdivision", "Tesselate",
              "Tesselate",
             "NGon", "Subdivide") {
        }

   
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
            pManager.AddIntegerParameter("N","N","Number of Suvdivisions", GH_ParamAccess.item,1);
        }

      
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();
            int n = 1;
            DA.GetData(0,ref mesh);
            DA.GetData(1,ref n);


            DA.SetData(0, NGonCore.Tesselation.TriangleSubdivision.SubdivideTriangleMesh(mesh, n, 0));
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.SubdivideTriangles;
            }
        }

    
        public override Guid ComponentGuid {
            get { return new Guid("80183cec-01de-4814-83ba-a1762875c9d8"); }
        }
    }
}