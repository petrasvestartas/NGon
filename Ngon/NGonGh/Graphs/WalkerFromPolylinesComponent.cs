using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using NGonsCore.Graphs;
using NGonsCore;

namespace NGonGh.Graphs {
    public class WalkerFromPolylinesComponent : GH_Component
    {

        PointCloud _pointCloud;
        List<Polyline> _polylines;
        UndirectedWeightedGraph _g;
        private int oddWalks = 0;

        protected override Bitmap Icon
        {
            get { return Properties.Resources.walker; }
        }

        public WalkerFromPolylinesComponent() : base("Walker", "Walker",
              "Walker",
              "NGon", "Graph")
        {

        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("86a3af5f-400a-41da-a3dc-82ed5cfa1458");
            }
        }

        //public override GH_Exposure Exposure
        //{
        //    get { return GH_Exposure.primary; }
        //}


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "Polylines", "Polylines", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Flip", "Flip", "Flip", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Inflate", "Inflate", "Inflate", GH_ParamAccess.item, 100.0);
            pManager.AddNumberParameter("RotX", "RotX", "RotX", GH_ParamAccess.item, 45.0);
            pManager.AddNumberParameter("RotY", "RotY", "RotY", GH_ParamAccess.item, 160.0);
            pManager.AddIntegerParameter("MaxLength", "MaxLength", "MaxLength", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("Commbine", "Commbine", "CommbineOddWalks", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("MaxIterations", "Loops", "MaxIterations", GH_ParamAccess.item, 100);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("Dijkstra", "Dijkstra", "Dijkstra", GH_ParamAccess.tree);
            pManager.AddLineParameter("Line", "Line", "Line", GH_ParamAccess.item);
            pManager.AddIntegerParameter("OddWalks", "OddWalks", "OddWalks", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Curve> curves = new List<GH_Curve>();
            if (!DA.GetDataList<GH_Curve>(0, curves)) return;
            bool flip = true;
            DA.GetData(1, ref flip);
            double inflate = 100.0;
            DA.GetData(2, ref inflate);
            double RotateX = 45.0;
            DA.GetData(3, ref RotateX);
            double RotateY = 160.0;
            DA.GetData(4, ref RotateY);
            int maxLength = 0;
            DA.GetData(5, ref maxLength);
            bool flag = true;
            DA.GetData(6, ref flag);
            int loops = 0;
            DA.GetData(7, ref loops);



            //Get direction
            Line line = Direction(flip, inflate, curves, RotateX, RotateY);

            //Create Graph
            CreateGraph();


            //Perform walk
            List<List<string>> pathList = ShortestWalkStripper(_g, line, maxLength, 1, flag, loops);

            //Output
            DataTree<string> output = new DataTree<string>();

            for (int i = 0; i < pathList.Count; i++)
                output.AddRange(pathList[i], new GH_Path(i));


            DA.SetDataTree(0, output);
            DA.SetData(1, line);
            DA.SetData(2, oddWalks);


        }


        public List<List<string>> ShortestWalkStripper(UndirectedWeightedGraph g, Line line, int maxLength, int perimeterEdgeWeight, bool flag, int loops)
        {

            //For adjusting odd walks
            UndirectedWeightedGraph adjustGraph = new UndirectedWeightedGraph(g);

            //Store paths
            List<List<string>> allPaths = new List<List<string>>();

            //Find shortest path
            for (int i = 0; i < loops; i++)
            {
                if (g.N == 0)
                {
                    base.Message = "Iterations " + i.ToString();
                    break;
                }

                //1.0 Perimeter weight
                if (perimeterEdgeWeight != 1)
                {
                    //How to retrieve edges from a graph if it is represented as adjacency matrix?
                }

                //1.1 Split graph into connected subgraphs and pick first one

                List<UndirectedWeightedGraph> components = g.ConnectedComponents(g);

                for (int j = 0; j < components.Count; j++)
                {
                    //Check if this component has one or two elements add them to path and deleter from graph
                    if (components[j].N < 3)
                    {
                        allPaths.Add(components[j].GetVertexNames());
                        g.DeleteVertices(components[j].GetVertexNames());
                        components[j].DeleteVertices(components[j].GetVertexNames());
                    }

                    //If not procede with Walker
                    else
                    {
                        //1.0 Gather all points in that component; vertex name = point index
                        List<string> vnames = components[j].GetVertexNames();
                        PointCloud componentCloud = new PointCloud();
                        foreach (var name in vnames)
                            componentCloud.Add(_pointCloud[Convert.ToInt32(name)].Location,
                                new Vector3d(0, 0, Convert.ToInt32(name)));

                        //1.1 index of closest nodes to line endpoints; normals are indexes in global _pointCloud

                        int indexA = componentCloud.ClosestPoint(line.From);
                        string startId = ((int)componentCloud[indexA].Normal.Z).ToString();
                        componentCloud.RemoveAt(indexA);
                        int indexB = componentCloud.ClosestPoint(line.To);
                        string endId = ((int)componentCloud[indexB].Normal.Z).ToString();

                        //1.2 Shortest Path / Dijkstra
                        List<string> path = components[j].FindPaths(startId, new List<string> { endId })[0];

                        //1.3 Limit the length
                        if (path.Count > maxLength && maxLength > 1)
                            path.RemoveRange(0, path.Count - maxLength);

                        //1.4 Add path to path list
                        allPaths.Add(path);

                        //1.5 Remove edges from the main Graph
                        g.DeleteVertices(path);
                    }

                } //For
            }

            //Combine Odd Walks

            //2.0 Append very short segments to walks 
            List<List<string>> oddPaths = new List<List<string>>();
            List<List<string>> oddPaths2 = new List<List<string>>();
            List<List<string>> normalPaths = new List<List<string>>();

            for (int i = 0; i < allPaths.Count; i++)
                if (allPaths[i].Count == 1)
                    oddPaths.Add(allPaths[i]);
                else
                    normalPaths.Add(allPaths[i]);


            if (flag)
            {
                //2.1 Get neighbour walks by original adjacency map
                for (int i = 0; i < oddPaths.Count; i++)
                {
                    Dictionary<string, int> neighbours = adjustGraph.GetAdjacentVertices(oddPaths[i][0], adjustGraph);
                    List<string> _neighbours = new List<string>(neighbours.Keys);

                    for (int j = 0; j < normalPaths.Count; j++)
                        if (_neighbours.Contains(normalPaths[j].First()))
                        {
                            normalPaths[j].Insert(0, oddPaths[i][0]);
                            break;
                        }
                        else if (_neighbours.Contains(normalPaths[j].Last()))
                        {
                            normalPaths[j].Add(oddPaths[i][0]);
                            break;
                        }
                        else if (j == normalPaths.Count - 1 && true)
                        {
                            oddPaths2.Add(oddPaths[i]);
                        }

                }
                normalPaths.AddRange(oddPaths2);
                oddWalks = oddPaths2.Count;
                return normalPaths;
            }

            oddWalks = oddPaths.Count;
            return allPaths;
        }

        public Line Direction(bool flip, double inflate, List<GH_Curve> curves, double rotateX, double rotateY)
        {
            _pointCloud = new PointCloud();
            _polylines = new List<Polyline>();

            foreach (var c in curves)
            {
                Polyline polyline;
                c.Value.TryGetPolyline(out polyline);
                _polylines.Add(polyline);
                _pointCloud.Add(c.Value.PointAt(0.0));
            }

            //Get boudingbox and inflate it
            BoundingBox boundingBox = _pointCloud.GetBoundingBox(false);
            boundingBox.Inflate(inflate);

            //Get box center/top points and make line
            Line line = new Line(boundingBox.PointAt(0.5, 0.5, 0), boundingBox.PointAt(0.5, 0.5, 1));

            if (!flip)
                line.Flip();

            //Rotate line around center
            line.Transform(Rhino.Geometry.Transform.Rotation(Rhino.RhinoMath.ToRadians(rotateX), Vector3d.XAxis, line.PointAt(0.5)));
            line.Transform(Rhino.Geometry.Transform.Rotation(Rhino.RhinoMath.ToRadians(rotateY), Vector3d.ZAxis, line.PointAt(0.5)));

            return line;
        }

        public void CreateGraph()
        {
            Mesh mesh = NGonsCore.MeshCreate.MeshFromPolylines(_polylines, 0.01);
            
            //PolyMesh polyMesh = PolyMesh.CreateFromPolylines(_polylines);
            //polyMesh.Weld(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 123);

            //1.2 Construct graph
            _g = new UndirectedWeightedGraph(_polylines.Count);
            //List<int>[] adj = polyMesh.Faces.GetAdjacencyMap();
            List<int>[] adj = mesh.GetNGonFaceAdjacency();

            for (int i = 0; i < _polylines.Count; i++)
                _g.InsertVertex(i.ToString());

            for (int i = 0; i < adj.Length; i++)
                for (int j = 0; j < adj[i].Count; j++)
                    _g.InsertEdge(i.ToString(), adj[i][j].ToString(), 1);
        }

    }
}

