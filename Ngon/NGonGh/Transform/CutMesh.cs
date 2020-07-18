using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Modifications {
    public class CutMesh : GH_Component {
        /// <summary>
        /// Initializes a new instance of the CutMesh class.
        /// </summary>
        public CutMesh()
          : base("CutMesh", "CutMesh",
              "CutMesh",
              "NGon", "Transform") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("M", "M", "Mesh",GH_ParamAccess.item);
            pManager.AddPlaneParameter("P","P","Plane",GH_ParamAccess.list,Plane.WorldXY );
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("P", "P", "Polylines", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();

            var planes = new List<Plane>();
            DA.GetDataList(1,  planes);

            if(DA.GetData(0,ref mesh)) {
               DA.SetDataList(0, mesh.CutMesh(planes));
            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {
   
                return Properties.Resources.CutMesh;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("1433eb88-2402-4256-8f80-6e162ef7e13e"); }
        }
    }
}