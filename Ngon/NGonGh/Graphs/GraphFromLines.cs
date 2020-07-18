using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NGonGh.Graphs {
    public class GraphFromLines : GH_Component {

        public GraphFromLines()
          : base("LineGraph", "LineGraph",
              "LineGraph",
              "NGon", "Graph") {
        }

          protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddLineParameter("Lines", "L", "Line List", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddGenericParameter("Topology Points", "TP", "Topology Points", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "N", "Nodes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Topology Edges U", "U", "Topology Edges U", GH_ParamAccess.list);
            pManager.AddGenericParameter("Topology Edges V", "V", "Topology Edges V", GH_ParamAccess.list);
            pManager.AddGenericParameter("Weights", "W", "Weights", GH_ParamAccess.list);
            pManager.AddGenericParameter("Adj", "Adj", "Adj", GH_ParamAccess.tree);
            

        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Grasshopper.Kernel.Types.GH_Line> lines = new List<Grasshopper.Kernel.Types.GH_Line>();
            if (!DA.GetDataList<Grasshopper.Kernel.Types.GH_Line>(0, lines)) return;


            List<Line> RhinoLines = new List<Line>();
            foreach (GH_Line l in lines) {
                RhinoLines.Add(l.Value);
            }

            var GraphData = NGonsCore.Graphs.LineGraph.GetGraphData(RhinoLines);
            
            DA.SetDataList(0, GraphData.Item1);
            DA.SetDataList(1, GraphData.Item2);
            DA.SetDataList(2, GraphData.Item3);
            DA.SetDataList(3, GraphData.Item4);
            DA.SetDataList(4, GraphData.Item5);
            DA.SetDataTree(5, GraphData.Item6);


        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.lineGraph;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("7a8e9802-f8cb-41f9-979f-633cf393ecdc"); }
        }


        private NGonsCore.Graphs.UndirectedGraph LinesToUndirectedGrap(List<Line> lines) {

            List<Point3d> pts = new List<Point3d>();

            foreach (Line l in lines) {
                pts.Add(l.From);
                pts.Add(l.To);
            }

            //Sorting
            var edges = new List<int>();

            var allPoints = new List<Point3d>(pts); //naked points

            int i = 0;

            while (allPoints.Count != 0) {
                Point3d pt = allPoints[0];
                allPoints.RemoveAt(0);


                for (int d = 0; d < pts.Count; d++) {
                    if (pt.Equals(pts[d])) {
                        edges.Add(d);
                        break;
                    }
                }

                i++;
            }

            var uniqueVertices = new HashSet<int>(edges).ToList();

            //Creating typological points
            var topologyPoints = new PointCloud();

            foreach (int k in uniqueVertices)
                topologyPoints.Add(pts[k]);

            //var vertices = Enumerable.Range(0, uniqueVertices.Count);

            for (int k = 0; k < uniqueVertices.Count; k++)
                if (uniqueVertices.ElementAt(k) != k)
                    for (int l = 0; l < edges.Count; l++)
                        if (edges[l] == uniqueVertices[k])
                            edges[l] = k;

            //Create graph
            NGonsCore.Graphs.UndirectedGraph g = new NGonsCore.Graphs.UndirectedGraph(uniqueVertices.Count);

            for (int k = 0; k < uniqueVertices.Count; k++)
                g.InsertVertex(k.ToString());


            for (int k = 0; k < edges.Count; k += 2)
                g.InsertEdge(edges[k].ToString(), edges[k + 1].ToString());

            g.SetAttribute((object)topologyPoints);


            return g;

        }

    }
}