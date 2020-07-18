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
    public class MeshEdges : GH_Component_NGon
    {

        public MeshEdges()
          : base("Mesh Edges", "MeshEdges",
              "Get All Mesh",
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

            pManager.AddLineParameter("AllE", "AE", "All mesh edges", GH_ParamAccess.tree);
            pManager.AddLineParameter("NakedE", "NE", "Naked mesh edges", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);


            Line[] ae = new Line[mesh.TopologyEdges.Count];
            List<Line> ne = new List<Line>();
            for (int i = 0; i < mesh.TopologyEdges.Count; i++) {
                Line line = mesh.TopologyEdges.EdgeLine(i);
                if (mesh.TopologyEdges.GetConnectedFaces(i).Length == 1)
                    ne.Add(line);
                ae[i] = line;
            }


            base.PreparePreview(mesh, DA.Iteration, null, false, null, ae.ToList());
            DA.SetDataTree(0, new DataTree<Line>(ae, new GH_Path(DA.Iteration)));
            DA.SetDataTree(1, new DataTree<Line>(ne, new GH_Path(DA.Iteration)));
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.meshedges;

        public override Guid ComponentGuid => new Guid("{356848b4-a6c0-4b02-9aa1-2967b950d5d0}");
    }
}