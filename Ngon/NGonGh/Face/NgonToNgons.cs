using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;
using System.Collections.Generic;
using Grasshopper;

namespace NGonGh.Face
{
    public class NgonToNgons : GH_Component {

        public NgonToNgons()
          : base("NGon To NGons", "NGonToNgons",
              "Get adjancent NGons to current NGon","NGon",
             "Face")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("AdjNgons", "A", "Get adjancent NGons to current NGon", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("V", "V", "V", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("E0", "E0", "E0", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("E1", "E1", "E1", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;




            //Vertex array as number of ngons
            int n = mesh.Ngons.Count;
            int[] nodes = Enumerable.Range(0, n).ToArray();

            //Get face adjacency
            List<int>[] adj = mesh.GetNgonFaceAdjacencyOrdered();

            //Edges start and end point
            List<int> edges0 = new List<int>();
            List<int> edges1 = new List<int>();

            for (int i = 0; i < adj.Length; i++) {
                edges0.AddRange(adj[i]);
                edges1.AddRange(Enumerable.Repeat(i, adj[i].Count));
            }

            //Output
            //V = nodes;
            //E0 = edges0;
            //E1 = edges1;

            DataTree<int> dt0 = new DataTree<int>();
            DataTree<int> dt1 = new DataTree<int>();
            DataTree<int> dt2 = new DataTree<int>();
            dt0.AddRange(nodes, new Grasshopper.Kernel.Data.GH_Path(iteration));
            dt1.AddRange(edges0, new Grasshopper.Kernel.Data.GH_Path(iteration));
            dt2.AddRange(edges1, new Grasshopper.Kernel.Data.GH_Path(iteration));

            DA.SetDataTree(0, GrasshopperUtil.ArrayOfListsToTree(adj, iteration));
            DA.SetDataTree(1, dt0);
            DA.SetDataTree(2, dt1);
            DA.SetDataTree(3, dt2);



        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_AdjFF;


        public override Guid ComponentGuid => new Guid("{171630da-22fe-4899-9eb2-3023a408715a}");
    }
}