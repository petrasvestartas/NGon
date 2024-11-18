using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Vertex
{
    public class NGonsConnectedToTopoVertices : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public NGonsConnectedToTopoVertices()
          : base("NGons Around Vertices", "Vertex Ngons",
              "Get Ngons that are around mesh topology vertices", "NGon",
               "Adjacency")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Naked", "N", "Cull naked", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Index", "I", "NGon Index", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Naked", "N", "NGon topology vertex naked status (be aware that ngon topology vertex Count and order is different from mesh topo vertex list order)", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            bool flag = true;
            DA.GetData(1, ref flag);

            int iteration = DA.Iteration;

            HashSet<int> tvAll = mesh.GetAllNGonsTopoVertices();
            int[][] NGons = mesh.GetNGonsConnectedToNGonTopologyVertices(tvAll, flag);


            bool[] nakedNGonVertices = mesh.GetNakedNGonTopologyPointStatus(tvAll);





            
            DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(NGons, iteration));
            DA.SetDataList(1, nakedNGonVertices);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_AdjVF;


        public override Guid ComponentGuid => new Guid("{6d54d2b6-64a9-4291-bc29-77ce1dbd56af}");
    }
}