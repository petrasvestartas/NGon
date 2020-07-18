using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.SubD {
    public class Platonic : GH_Component_NGon {

        public Platonic()
          : base("Platonic", "Platonic",
              "Get Platonic Object i.e. Icosahedron, torus, cube, tetrahedron",
               "Subdivide") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddIntegerParameter("I", "I", "Id of object, 0 - icosahedron, 1 - torus , 2 - cube, 3 - tetrahedron", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("R", "R", "Radius", GH_ParamAccess.list, new List<double> { 1, 0.3});
            pManager.AddPlaneParameter("P", "P", "Plane", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("S", "S", "Subdvision level", GH_ParamAccess.list,new List<int> { 1,1 });
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            int id = 0;
            DA.GetData(0, ref id);

            List<double> r =new List<double> ();
            DA.GetDataList(1,  r);

            Plane plane = Plane.WorldXY;
            DA.GetData(2, ref plane);

            List<int> s = new List<int> ();
            DA.GetDataList(3,  s);

            Mesh mesh = new Mesh();
            switch (id) {

                case (3):
                mesh = NGonsCore.Tetrahedron.Create(r[0]);
                break;

                case (2):
                mesh = NGonsCore.Cube.Create(r[0]);
                break;

                case (1):
                mesh = NGonsCore.Torus.Create(s[0], s[Math.Min(1, s.Count - 1)], r[0], r[Math.Min(1, r.Count - 1)]);
                break;

                default :
                mesh = NGonsCore.IcoSphere.Create(Math.Min(3,s[0]), (float)r[0]);
                break;

            }

            //mesh.Transform(Transform.PlaneToPlane(Plane.WorldXY,plane));


            DA.SetData(0, mesh);
            this.PreparePreview(mesh, DA.Iteration,null,true,null,mesh.GetEdges());

        }



        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Plato;

            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b558c9-c00b-4bf1-b2b4-1425a5ee1f08"); }
        }
    }
}