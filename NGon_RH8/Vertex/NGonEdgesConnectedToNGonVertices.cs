using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Vertex
{
    public class NGonEdgesConnectedToNGonVertices : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public NGonEdgesConnectedToNGonVertices()
          : base("NGons Edges Around Vertices", "VEdge Ngons",
              "Get Ngons that are around mesh topology vertices", "NGon",
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
            pManager.AddCurveParameter("Line", "L", "Lines connected to ngon vertices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Index", "I", "Edge Index", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            HashSet<int> allNGonTvAll = mesh.GetAllNGonsTopoVertices();
            int[][] nGonTV = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allNGonEAll = mesh.GetAllNGonEdges(nGonTV);

            int[][] connectedE = mesh.GetConnectedNGonEdgesToNGonTopologyVertices(allNGonTvAll, allNGonEAll);
            Line[][] connectedLines = mesh.GetConnectedNGonLinesToNGonTopologyVertices(connectedE);

            DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(connectedLines, iteration));
            DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(connectedE, iteration));

          

        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_VE;

        public override Guid ComponentGuid => new Guid("{213a7bc2-71af-4399-83f4-078b7d6176b0}");
    }
}