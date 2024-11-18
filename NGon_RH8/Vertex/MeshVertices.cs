using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonCore;
using System.Linq;

namespace NGon_RH8.Vertex
{
    public class MeshVertices : GH_Component_NGon
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public MeshVertices()
          : base("Naked Vertices", "Naked",
              "Mesh Naked Vertices",
              "Adjacency")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
          
            pManager.AddPointParameter("Vertices", "V", "Naked Vertices", GH_ParamAccess.list);
            //pManager.AddPointParameter("Vertices", "V", "Vertices", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Status", "S", "Naked Statua", GH_ParamAccess.list);


        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            var v = mesh.Vertices.ToPoint3dArray();
            var n = mesh.GetNakedEdgePointStatus();

            //DA.SetDataList(1, v);
            List<Point3d> nakedVertices = new List<Point3d>();
            for (int i = 0; i < v.Length; i++)
                if (n[i])
                    nakedVertices.Add(v[i]);
            DA.SetDataList(0, nakedVertices);
            DA.SetDataList(1, n);
            base.PreparePreview(mesh, DA.Iteration, null, false, nakedVertices);
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.meshvertices;

        public override Guid ComponentGuid => new Guid("{f6fe728a-5e0d-4224-a12d-92d80150df7f}");
    }
}