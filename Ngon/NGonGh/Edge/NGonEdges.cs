using System;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;

namespace NGonGh.Edge
{
    public class NGonEdges : GH_Component_NGon
    {

        public NGonEdges()
          : base("NGon Edges", "Edges",
              "Get All Ngon edges id and lines",
              "Edge")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Edges", "E", "Get Mesh Edges in All Ngons", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ID", "I", "Get Mesh Edges (Lines) in All Ngons", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("NakedID", "NID", "Naked Edges ", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Normal", "NN", "Edge normal by average of adjacent vertices ", GH_ParamAccess.tree);
            pManager.AddLineParameter("Naked", "N", "Naked edges as lines", GH_ParamAccess.tree);
            pManager.AddLineParameter("AllE", "AE", "All mesh edges", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            int[][] tv = mesh.GetNGonsTopoBoundaries();
            HashSet<int> e = mesh.GetAllNGonEdges(tv);
            Line[] lines = mesh.GetAllNGonEdgesLines(e);
            bool[] n = mesh.NakedNGonEdges(e);
            List<Line> linesNaked = new List<Line>();

            List<Vector3d> vecs = new List<Vector3d>();
            int j = 0;
            foreach (int i in e) { 
                Line l = mesh.TopologyEdges.EdgeLine(i);
                //l.Transform(Transform.Rotation())
                Rhino.IndexPair ip = mesh.TopologyEdges.GetTopologyVertices(i);
                int v0 = mesh.TopologyVertices.MeshVertexIndices(ip.I)[0];
                int v1 = mesh.TopologyVertices.MeshVertexIndices(ip.J)[0];
                Vector3d vec = new Vector3d(
                  (mesh.Normals[v0].X + mesh.Normals[v1].X) * 0.5,
                  (mesh.Normals[v0].Y + mesh.Normals[v1].Y) * 0.5,
                  (mesh.Normals[v0].Z + mesh.Normals[v1].Z) * 0.5
                  );

                vecs.Add(vec);

                if (n[j])
                    linesNaked.Add(l);
                j++;
            }

            this.PreparePreview(mesh, DA.Iteration, null, false, null,lines.ToList());

            DA.SetDataTree(0, new DataTree<Line>(lines,new GH_Path(iteration)));
            DA.SetDataTree(1, new DataTree<int>(e, new GH_Path(iteration)));
            DA.SetDataTree(2, new DataTree<bool>(n, new GH_Path(iteration)));
            DA.SetDataTree(3, new DataTree<Vector3d>(vecs, new GH_Path(iteration)));

            Line[] ae = new Line[mesh.TopologyEdges.Count];
            for (int i = 0; i < mesh.TopologyEdges.Count; i++) {
                ae[i] = mesh.TopologyEdges.EdgeLine(i);
            }

            DA.SetDataTree(4, new DataTree<Line>(linesNaked, new GH_Path(iteration)));
            DA.SetDataTree(5, new DataTree<Line>(ae, new GH_Path(iteration)));
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_EdgesAll;

        public override Guid ComponentGuid => new Guid("{356848b4-a6c0-4b02-9aa1-2967b950d0d0}");
    }
}