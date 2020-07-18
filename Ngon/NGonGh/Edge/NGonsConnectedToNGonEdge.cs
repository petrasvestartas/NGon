using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;

namespace NGonGh.Edge
{
    public class NGonsConnectedToNGonEdge : GH_Component {
        /// <summary>
        /// Initializes a new instance of the NGonsConnectedToNGonEdge class.
        /// </summary>
        public NGonsConnectedToNGonEdge()
          : base("NGons Connected To NGon Edge", "EdgeNGon",
              "Gets ngons connected to ngons edges, -1 is added to beggining of the list edge is naked (left / right property for clean meshes)",
               "NGon","Edge")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Naked", "N", "Add -1 for naked edge", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("ID", "I", "Get Mesh Edges (Lines) in All Ngons", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ID", "I", "Get Mesh Edges (Lines) in All Ngons", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            bool flag = true;
            DA.GetData(1, ref flag);

            int iteration = DA.Iteration;

            int[][] tv = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(tv);
            int[] allEArray = allE.ToArray();
            int[][] NGons = mesh.GetNgonsConnectedToNGonsEdges(allE, flag);
            //int[][] ef = mesh.GetNgonsConnectedToNGonsEdges(allE, false);

            DataTree<int> dt = new DataTree<int>();
            DataTree<int> dt2 = new DataTree<int>();

            //Mesh edge
            for (int i = 0; i < NGons.Length; i++) {
                dt.AddRange(NGons[i], new GH_Path(i));//new GH_Path(allE.ElementAt(i)));

                //int nei = ()
                if (NGons[i].Count()>1) {
                    dt2.AddRange(NGons[i], new GH_Path(allE.ElementAt(i)));
                }
            }
            //Rhino.RhinoApp.WriteLine(NGons.Length.ToString());
            DA.SetDataTree(0, dt);
            DA.SetDataTree(1, dt2);
            // DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(NGons,iteration));
            //Instead of mesh edge -> adjacent mesh face

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_AdjEF;


        public override Guid ComponentGuid => new Guid("{33e21565-d38e-4571-85f0-0da265872d2b}");
    }
}