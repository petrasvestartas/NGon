using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Modifications
{
    public class LatticeFromTwoMeshes : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the AdjustMeshVertices class.
        /// </summary>
        public LatticeFromTwoMeshes()
          : base("LatticeFromTwoMeshes", "MatchMesh",
              "Takes one mesh vertices and adjusts other mesh vertices by former positions",
                "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("MeshA", "MA", "Mesh to adjust", GH_ParamAccess.item);
            pManager.AddMeshParameter("MeshB", "MB", "Mesh to follow", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Lattice", "La", "Lattice Mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("Joined", "JM", "JoinedMesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh MA = new Mesh();
            Mesh MB = new Mesh();
            DA.GetData(0,ref MA);
            DA.GetData(1, ref MB);

            Mesh x = new Mesh();
            x.Append(MA);
            x.Append(MB);


            Mesh lattice = new Mesh();
            lattice.Vertices.AddVertices(x.Vertices);
            var tv = x.GetNGonsTopoBoundaries();
            int[][] ev = x.GetAllNGonEdges_TopoVertices(tv, x.GetAllNGonEdges(tv));
            int n = (int)(ev.Length * 0.5);

            for (int i = 0; i < n; i++) {


                lattice.Faces.AddFace(ev[i][0], ev[i][1], ev[i + n][1], ev[i + n][0]);
                lattice.Ngons.AddNgon(MeshNgon.Create(
                  new int[] { ev[i][0], ev[i][1], ev[i + n][1], ev[i + n][0] },
                  new int[] { lattice.Faces.Count - 1 }
                  ));
            }
           // lattice.Weld(0.001);
            lattice.Clean();
            base.PreparePreview(lattice, DA.Iteration);
            DA.SetData(0,lattice);
            DA.SetData(1, x);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.lattice;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a8606789-1e62-44ac-803c-8cd486d9d60b"); }
        }
    }
}