using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Polygons {
    public class MeshCrv : GH_Component {
  
        public MeshCrv()
          : base("MeshCurve", "MeshCrv",
              "Are curves inside mesh",
              "NGon", "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh","M","Mesh to search fro a closest point",GH_ParamAccess.item);
            pManager.AddBooleanParameter("Flag", "F", "If true center, if false check corner point inclusiosn", GH_ParamAccess.item,true);
            pManager.AddNumberParameter("Tol","T","Distance",GH_ParamAccess.item,0.01);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID","I","Indices of Closest points",GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Curve> curves = new List<Curve>();
            Mesh c = null;
            double t = 0.01;
            bool flag = true;

            DA.GetDataList(0, curves);
            DA.GetData(1, ref c);
            DA.GetData(2, ref flag);
            DA.GetData(3, ref t);

            List<int> id = new List<int>();
            List<Curve> cp = c.IsCurvesInsideMesh(curves, ref id, t,flag);

            DA.SetDataList(0, cp);
            DA.SetDataList(1, id);
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.CurvesInMesh;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("537599f5-680a-44dd-8ad2-d5bd01235fa6"); }
        }
    }
}