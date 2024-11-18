using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Modifications
{
    public class AdjustMeshVertices : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the AdjustMeshVertices class.
        /// </summary>
        public AdjustMeshVertices()
          : base("AdjustMeshVertices", "MatchMesh",
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
            pManager.AddMeshParameter("MeshA", "MA", "Mesh to adjust", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh meshA = new Mesh();
            Mesh meshB = new Mesh();

            DA.GetData(0, ref meshA);
            DA.GetData(1, ref meshB);

            if (meshA.Vertices.Count == meshB.Vertices.Count) {
                Mesh tempMesh = meshA.AdjustMesh(meshB);
                DA.SetData(0, tempMesh);
                this.PreparePreview(tempMesh, DA.Iteration);


            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MATCH;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a8606789-1e62-44ac-803c-8cd799d9d60b"); }
        }
    }
}