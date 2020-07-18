using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.NGons {
    public class AddNgonToMesh : GH_Component_NGon {
        public AddNgonToMesh()
            : base("From Mesh", "From Mesh",
                "Takes a mesh and add ngons that has outline of naked mesh edge and collection of all mesh faces",
                "NGons") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ID", "I", "Choose which outline", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("It", "I", "Iterations", GH_ParamAccess.item, 1000);
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("VertexID", "V", "Naked edge vertex id", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Polylines", "P", "Naked edge polylines", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            DA.GetData(0, ref m);
            int i = 0;
            DA.GetData(1,ref i);
            int iterations = 0;
            DA.GetData(2,ref iterations);

            if (m.IsValid) {
                List<List<int>> nakedVertices = m.GetNakedVerticesID(iterations);
               List<Polyline> nakedPolylines = m.GetNakedPolylines(nakedVertices);
                Mesh mesh = m.DuplicateMesh();

     
                if (nakedVertices.Count != 0) {
                    int check = mesh.Ngons.AddNgon(MeshNgon.Create(nakedVertices[ i.Wrap(nakedVertices.Count) ], Enumerable.Range(0,m.Faces.Count).ToList()));
                    PreparePreview(mesh, DA.Iteration, nakedPolylines);
                    DA.SetData(0, mesh);
                    DA.SetDataTree(1, GrasshopperUtil.IE2(nakedVertices, DA.Iteration));
                    DA.SetDataTree(2, GrasshopperUtil.IE(nakedPolylines, DA.Iteration));

                } else {
                    DA.SetData(0, m);
                    DA.SetDataTree(1, new Grasshopper.DataTree<int>());
                    DA.SetDataTree(2, new Grasshopper.DataTree<Polyline>());
                }

            } else {
                DA.SetData(0, m);
                DA.SetDataTree(1, new Grasshopper.DataTree<int>());
                DA.SetDataTree(2, new Grasshopper.DataTree<Polyline>());
            }

           
            

        }


        
        protected override System.Drawing.Bitmap Icon => Properties.Resources.MeshToNgon;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b111aa048d14}");
    }
}