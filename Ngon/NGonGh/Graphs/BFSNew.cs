using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonsCore;
using NGonsCore.Clipper;
using Rhino.Geometry;

namespace SubD.Graphs {
    public class BFSNew : GH_Component {

        public BFSNew()
          : base("BFSNew", "BFSNew",
              "BreathFirstSearch from mesh",
              "NGon", "Graph") {
        }

          protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Start", "V", "Start Vertex", GH_ParamAccess.item, "0");
            pManager.AddIntegerParameter("Sequence", "Sequence", "Sequence", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddIntegerParameter("Vertices", "V", "Mesh Face ids", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edges", "E", "Mesh Face pair ids", GH_ParamAccess.tree);
            pManager.AddVectorParameter("EdgeVec", "EV", "EdgeVec", GH_ParamAccess.tree);



        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = DA.Fetch<Mesh>("Mesh");
            string v = DA.Fetch<string>("Start");
            List<int> o = DA.FetchList<int>("Sequence");

            v = Math.Min(Convert.ToInt32(v), mesh.Ngons.Count-1).ToString();


            var ve = NGonsCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(mesh, v);
            List<int> order = ve.Item1[0];

            DA.SetDataTree(0, NGonsCore.GrasshopperUtil.IE2(ve.Item1));
            DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IE3(ve.Item2));

            

            if(mesh.Ngons.Count == o.Count) {
                order = o;
            }



            int S = mesh.Ngons.Count;//stop
            //Insertion vectors
            DataTree<int> N = SequenceNeighbours(mesh, order, S);
            //FN = N;
            //O_ = O;
            //Edge Vectors
            int[][] tv = mesh.GetNGonsTopoBoundaries();
            int[][] fe = mesh.GetNGonFacesEdges(tv);
            HashSet<int> e = mesh.GetAllNGonEdges(tv);
            Dictionary<int, int[]> efDict = mesh.GetFE(e, false);
            Polyline[] outlines = mesh.GetPolylines();



            //Dictionary<int, Vector3d> edgeVectors = new Dictionary<int, Vector3d> ();
            DataTree<Vector3d> edgeVectors = new DataTree<Vector3d>();


            for (int i = 0; (i < S && i < mesh.Ngons.Count); i++) {

                /////////////
                // Properties
                /////////////
                //Current path and face
                GH_Path p = new GH_Path(i);
                int f = order[i];//O[i];
                if (!N.PathExists(p)) continue; //If no connectio nskip
                HashSet<int> fadj = new HashSet<int>(N.Branch(p));//adjacency list


                /////////////
                // Solution
                /////////////
                //Iterate mesh edges
                //Get connected faces
                //Check if they are in adjacency list
                //The order thoses ids

                List<int> NotOrderedEdges = new List<int>();
                for (int j = 0; j < fe[f].Length; j++) {
                    int[] facePair = efDict[fe[f][j]];//get face pair
                    if (facePair.Length == 1) continue;//if naked skip
                    if (fadj.Contains(facePair[0]) || fadj.Contains(facePair[1])) NotOrderedEdges.Add(j);//if edge face are in fadj
                }

                List<int> orderedEdges = SortIntegers(NotOrderedEdges, fe[f].Length);


                //Collect lines for Insertion Vector
                List<Line> el = new List<Line>();
                //Line[] lines = outlines[f].GetSegments();
                foreach (int j in orderedEdges) {
                    el.Add(outlines[f].SegmentAt(j));//el.Add(M.TopologyEdges.EdgeLine(fe[f][j]));

                }

                //Create Insertion Vector
                Vector3d vec = NGonsCore.VectorUtil.BisectorVector(el, outlines[f].AverageNormal(), false);

                foreach (int j in orderedEdges) {
                    //edgeVectors.Add(fe[f][j],vec);
                    edgeVectors.Add(vec, new GH_Path(fe[f][j]));
                }


                //A = el;
                //B = vec;
                //C = outlines[f].AverageNormal();

            }

            //EV = edgeVectors;
            DA.SetDataTree(2,edgeVectors);
            //Get current face edges
            //Take edge only connected in current set



        }
        public DataTree<int> SequenceNeighbours(Mesh M, List<int> O, int S) {
            List<int>[] ff = M.GetNgonFaceAdjacencyOrdered();

            DataTree<int> dtsequence = new DataTree<int>();
            DataTree<int> dtsequenceConnected = new DataTree<int>();
            HashSet<int> sequence = new HashSet<int>();

            for (int i = 0; i < O.Count; i++) {


                //Get current outlines
                List<int> cf = ff[O[i]];


                //Get current outlines already connected and skip not connected
                List<int> cfPlaced = new List<int>(cf.Count);

                foreach (int j in cf) {
                    if (sequence.Contains(j)) {
                        cfPlaced.Add(j);
                        dtsequenceConnected.Add(j, new GH_Path(i));
                    }
                }

                sequence.Add(O[i]);
                dtsequence.Add(O[i], new GH_Path(i));


                if (i == S)
                    break;
            }

            //O_ = dtsequence;
            //N = dtsequenceConnected;

            return dtsequenceConnected;
        }

        public List<int> SortIntegers(List<int> NotOrderedEdges_, int n) {

            List<int> orderedEdges = new List<int>();
            List<int> NotOrderedEdges = NotOrderedEdges_;
            NotOrderedEdges.Sort();


            int k = -1;
            for (int j = 0; j < NotOrderedEdges.Count - 1; j++) {
                if (NotOrderedEdges[j] == 0 && NotOrderedEdges[j + 1] == n - 1)
                    continue;
                if (Math.Abs(NotOrderedEdges[j] - NotOrderedEdges[j + 1]) > 1) {
                    k = j + 1;
                    break;
                }
            }


            if (k != -1) {
                for (int j = k; j < NotOrderedEdges.Count + k; j++) {
                    int cur = j % NotOrderedEdges.Count;
                    orderedEdges.Add(NotOrderedEdges[cur]);
                }
            } else {
                return NotOrderedEdges_;
            }

            return orderedEdges;

        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.BFS;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("7a1e9805-f8cb-41f9-979f-633cf121ecdc"); }
        }

    }
}