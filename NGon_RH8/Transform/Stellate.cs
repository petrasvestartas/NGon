using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Transform {
    public class Stellate : GH_Component {
       
        public Stellate()
          : base("Stellate", "Stellate",
              "Create pyramids and cut them with a plane",
              "NGon", "Transform") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("M", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("D", "D", "Pyramid Height", GH_ParamAccess.list);
            pManager.AddNumberParameter("T", "T", "Plane at pyramid vertical line domain", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("S", "S", "Sides", GH_ParamAccess.tree);
            pManager.AddCurveParameter("C", "C", "Centers", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            List<double> d = new List<double>();
            DA.GetDataList(1, d);
            List<double> t = new List<double>();
            DA.GetDataList(2, t);

            d = mesh.Ngons.Count != d.Count ? Enumerable.Repeat(d[0], mesh.Ngons.Count).ToList() : d;
            t = mesh.Ngons.Count != t.Count ? Enumerable.Repeat(t[0], mesh.Ngons.Count).ToList() : t;


            if (mesh.Ngons.Count > 0) {
              var plines=  NGonCore.MeshCreate.Stellate(mesh, d, t);
                DA.SetDataTree(0, plines.Item1);
                DA.SetDataTree(1, plines.Item2);
            }

        }



        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.Stellate;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("4709f4d5-852c-4323-ba1e-9c1e257ba67f"); }
        }
    }
}