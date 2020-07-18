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

namespace NGonGh.Graphs {
    public class BFS : GH_Component {

        public BFS()
          : base("BFS", "BFS",
              "BreathFirstSearch from mesh",
              "NGon", "Graph") {
        }

          protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Start", "V", "Start Vertex", GH_ParamAccess.item, "0");
            pManager.AddIntegerParameter("Sequence", "Sequence", "Sequence", GH_ParamAccess.list);
            pManager.AddCurveParameter("SequenceCrv", "SequenceCrv", "SequenceCrv", GH_ParamAccess.item);
            pManager.AddLineParameter("CustomVectors", "CustomVectors", "CustomVectors",GH_ParamAccess.list);
            pManager.AddNumberParameter("Angle", "Angle", "Angle", GH_ParamAccess.item,0.14);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddIntegerParameter("Vertices", "V", "Mesh Face ids", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edges", "E", "Mesh Face pair ids", GH_ParamAccess.tree);
            pManager.AddVectorParameter("EdgeVec", "EV", "EdgeVec", GH_ParamAccess.tree);
            pManager.AddLineParameter("Bad", "Bad", "Bad Edges", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("FaceVec", "FV", "FaceVec", GH_ParamAccess.list);
        }



        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = DA.Fetch<Mesh>("Mesh");
            string v = DA.Fetch<string>("Start");
            List<int> o = DA.FetchList<int>("Sequence");
            Curve sequenceCrv = DA.Fetch<Curve>("SequenceCrv");
            List<Line> customVectors = DA.FetchList<Line>("CustomVectors");
            double angleTol = DA.Fetch<double>("Angle");
           



            v = Math.Min(Convert.ToInt32(v), mesh.Ngons.Count-1).ToString();

     



            if (mesh.Ngons.Count == o.Count) {
                mesh = mesh.ReoderMeshNgons(o);
                base.Message = "Sequence";

            } else if (sequenceCrv !=  null) {
                if (sequenceCrv.IsValid) {
                    mesh = mesh.ReoderMeshNgons(sequenceCrv);
                    List<int>  order = Enumerable.Range(0, mesh.Ngons.Count).ToList();
                    base.Message = "Sequence Curve";
                }

            }else{

                var ve = NGonsCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(mesh, v);
                List<int>  order = ve.Item1[0];
                mesh = mesh.ReoderMeshNgons(ve.Item1[0]);
                base.Message = "BFS";
                DA.SetDataTree(0, GrasshopperUtil.IE2(ve.Item1));
                DA.SetDataTree(1, GrasshopperUtil.IE3(ve.Item2));
            }

            DA.SetData(4,mesh);


            List<int> order_ = Enumerable.Range(0, mesh.Ngons.Count).ToList();

            DataTree<int> vertices_ = new DataTree<int>(order_);
            DataTree<int> edges_ = new DataTree<int>();
            for (int i = 0; i < order_.Count - 1; i++) {
                edges_.Add(order_[i], new GH_Path(i));
                edges_.Add(order_[i] + 1, new GH_Path(i));
            }


            int S = mesh.Ngons.Count;//stop
            //Insertion vectors
            DataTree<int> N = SequenceNeighbours(mesh, order_, S);
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
            List<Vector3d> faceVectors = new List<Vector3d>();
            Dictionary<int, List<GH_Path>> faceEdgeID = new Dictionary<int, List<GH_Path>>();


            for (int i = 0; (i < S && i < mesh.Ngons.Count); i++) {

                /////////////
                // Properties
                /////////////
                //Current path and face
                GH_Path p = new GH_Path(i);
                int f = order_[i];//O[i];
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
                faceVectors.Add(vec);

              
                List<GH_Path> paths = new List<GH_Path>();
                foreach (int j in orderedEdges) {
                    //edgeVectors.Add(fe[f][j],vec);
                    edgeVectors.Add(vec, new GH_Path(fe[f][j]));
                    paths.Add(new GH_Path(fe[f][j]));
                }

                faceEdgeID.Add(i,paths);
                //Rhino.RhinoApp.WriteLine(i.ToString() + " " + paths.Count.ToString());

                //A = el;
                //B = vec;
                //C = outlines[f].AverageNormal();

            }

            DataTree<Vector3d> EV = mesh.insertionVectors(true);
            DataTree<Line> edges = new DataTree<Line>();
            PointCloud cloud = new PointCloud();

            //Check angles if vectors are not parallel to mesh edge
            foreach(GH_Path p in EV.Paths) {

                Line line = mesh.TopologyEdges.EdgeLine(p.Indices[0]);
                Plane edgePlane = new Plane(line.PointAt(0.5), mesh.GetMeshEdgePerpDir(p.Indices[0]));
         
                cloud.Add(line.PointAt(0.5),new Vector3d(p.Indices[0],0,0));

                double angledifference = Math.Abs(Vector3d.VectorAngle(line.Direction, edgeVectors.Branch(p)[0], edgePlane)%Math.PI);

                //Rhino.RhinoApp.WriteLine(angledifference.ToString());
                if ( angledifference < angleTol || angledifference > (Math.PI-angleTol) ) {
                    edges.Add(new Line(line.PointAt(0.5), line.PointAt(0.5) + line.Direction.UnitVector() * line.Length * 0.25), p);
                    edges.Add(new Line(line.PointAt(0.5), line.PointAt(0.5)+ edgeVectors.Branch(p)[0].UnitVector()* line.Length*0.25), p);
                    edgeVectors.Branch(p)[0] = -EV.Branch(p)[0];
                } else {

                }
            }


            //Change insertion vectors
            if (customVectors.Count > 0) {
                for (int i = 0; i < customVectors.Count; i++) {
                    int edgeID = cloud.ClosestPoint(customVectors[i].From);
                    edgeVectors.Branch(new GH_Path((int)cloud[edgeID].Normal.X))[0] = customVectors[i].Direction;
                }

            }

            //Rhino.RhinoApp.WriteLine (faceEdgeID.Count.ToString());
            
            List<Vector3d> NGonsVectors = new List<Vector3d>() { Vector3d.ZAxis };

            for (int i = 1;i < faceEdgeID.Count; i++) {

                Vector3d vec = Vector3d.Zero;
                for (int j = 0; j < faceEdgeID[i].Count; j++) {
                    vec += edgeVectors.Branch(faceEdgeID[i][j])[0];
                }
                vec.Unitize();
                NGonsVectors.Add(vec);
            }

            DA.SetDataList(5, NGonsVectors);
          
            //EV = edgeVectors;
            DA.SetDataTree(2,edgeVectors);
            DA.SetDataTree(3,edges);
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
            get { return new Guid("7a1e9805-f8cb-41f9-979f-633cf393ecdc"); }
        }

    }
}