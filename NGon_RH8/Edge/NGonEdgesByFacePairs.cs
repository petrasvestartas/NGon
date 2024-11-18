using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace NGon_RH8.Edge
{
    public class NGonEdgesByFacePairs : GH_Component {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public NGonEdgesByFacePairs()
          : base("NGon Face Edges by NGon Pairs", "EdgeByFP",
              "Get NGon Face Edges by NGon Pairs", "NGon",
               "Adjacency")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NGon Face Pair", "P", "NGon face pairs - DataTree", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }



        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Edges", "E", "Get Ngon Pair Edges (Lines) in Ngons Faces", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID", "I", "Get Ngon Edges", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Flag", "F", "Belongs to pair", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            GH_Structure<GH_Integer> pairs = new GH_Structure<GH_Integer>();
            DA.GetDataTree(1, out pairs);

            //Solution

            int[][] p = new int[pairs.PathCount][];


            //Grasshopper datatree to int[] {a,b}
            for (int i = 0; i < pairs.PathCount; i++)
                 p[i] = (new int[] { pairs[i][0].Value, pairs[i][1].Value });



            var edges = mesh.FindMeshEdgeByNGonPair(p);

            //Output

            DA.SetDataList(0, edges.Item2);
            DA.SetDataList(1, edges.Item3);
            DA.SetDataList(2, edges.Item1);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.subd_13_13_14;
            }
        }

        public override Guid ComponentGuid => new Guid("{b0e10148-f1a2-430b-aa49-a28312982df0}");
    }
}