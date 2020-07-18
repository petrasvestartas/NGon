using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Collections;
using QuickGraph.Algorithms.MinimumSpanningTree;
using QuickGraph.Algorithms.Observers;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore.QuickGraph
{
    public static class QuickGraphRhino
    {

        /// <summary>
        /// Kruskal Minimum spanning tree from vertices, edges and optionally points to compute weights
        /// </summary>
        /// <param name="N"></param>
        /// <param name="U"></param>
        /// <param name="V"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public static int[][] RhinoKruskal(List<int> N, List<int> U, List<int> V, List<double> W = null, List<Point3d> P = null)
        {
           GetUndirectedFullGraph(N, U, V, P);
            int[][] resultEdges = new int[0][];//output
            UndirectedGraph<string, TaggedEdge<string, double>> graph = GetUndirectedFullGraph(N, U, V, P, W);
            Kruskal(graph, x => x.Tag, ref resultEdges);
            return resultEdges;
        }

        /// <summary>
        /// Prim Minimum spanning tree from vertices, edges and optionally points to compute weights
        /// </summary>
        /// <param name="N"></param>
        /// <param name="U"></param>
        /// <param name="V"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public static int[][] RhinoPrim(List<int> N, List<int> U, List<int> V, List<double> W = null, List<Point3d> P = null)
        {
            GetUndirectedFullGraph(N, U, V, P);
            int[][] resultEdges = new int[0][];//output
            UndirectedGraph<string, TaggedEdge<string, double>> graph = GetUndirectedFullGraph(N, U, V, P, W);
            Prim(graph, x => x.Tag, ref resultEdges);
            return resultEdges;
        }

        /// <summary>
        /// Create Undirected Graph
        /// </summary>
        /// <param name="N"></param>
        /// <param name="U"></param>
        /// <param name="V"></param>
        /// <param name="P"></param>
        /// <param name="W"></param>
        /// <returns></returns>
        private static UndirectedGraph<string, TaggedEdge<string, double>> GetUndirectedFullGraph(List<int> N, List<int> U, List<int> V, List<Point3d> P = null, List<double> W = null)
        {
            var graph = new UndirectedGraph<string, TaggedEdge<string, double>>();

            for (int i = 0; i < N.Count; i++)
            {
                graph.AddVertex(N[i].ToString());
            }

            bool pointDistanceFlag = false;
            if (P != null)
                if (P.Count == N.Count)
                    pointDistanceFlag = true;

            bool wFlag = false;
            if (W != null)
                if (W.Count == N.Count)
                    wFlag = true;

            for (int i = 0; i < U.Count; i++)
            {
                if (pointDistanceFlag)
                    graph.AddEdge(new TaggedEdge<string, double>(U[i].ToString(), V[i].ToString(), P[U[i]].DistanceToSquared(P[V[i]])));
                else if(wFlag)
                    graph.AddEdge(new TaggedEdge<string, double>(U[i].ToString(), V[i].ToString(), W[i]));
                else
                    graph.AddEdge(new TaggedEdge<string, double>(U[i].ToString(), V[i].ToString(), 1));
            }


            return graph;
        }


        /// <summary>
        /// Prim Minimum Spanning Tree
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g"></param>
        /// <param name="edgeWeights"></param>
        /// <param name="resultEdges"></param>
        private static void Prim<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, Func<TEdge, double> edgeWeights, ref int[][] resultEdges) where TEdge : IEdge<TVertex>
        {

            List<TEdge> ed = g.Edges.ToList();
            Dictionary<TEdge, double> distances = new Dictionary<TEdge, double>();

            foreach (TEdge e in g.Edges)
                distances[e] = edgeWeights(e);

            PrimMinimumSpanningTreeAlgorithm<TVertex, TEdge> prim = new PrimMinimumSpanningTreeAlgorithm<TVertex, TEdge>(g, e => distances[e]);

            //call this method
            AssertMinimumSpanningTree<TVertex, TEdge>(g, prim, ref resultEdges);
        }

        /// <summary>
        /// Kruskal Minimum Spanning Tree
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g"></param>
        /// <param name="edgeWeights"></param>
        /// <param name="resultEdges"></param>
        private static void Kruskal<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, Func<TEdge, double> edgeWeights, ref int[][] resultEdges) where TEdge : IEdge<TVertex>  {
            var ed = g.Edges.ToList();
            var distances = new Dictionary<TEdge, double>();
            foreach (var e in g.Edges)
                distances[e] = edgeWeights(e);

            var prim = new KruskalMinimumSpanningTreeAlgorithm<TVertex, TEdge>(g, e => distances[e]);
            AssertMinimumSpanningTree<TVertex, TEdge>(g, prim, ref resultEdges);
        }


        /// <summary>
        /// Collect edges
        /// ToDo Change To Prim.Compute() or Kruskal.Compute() look at AssertMinimumSpanningTree<TVertex, TEdge>(g, prim, ref resultEdges);
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="g"></param>
        /// <param name="algorithm"></param>
        /// <param name="resultEdges"></param>
        private static void AssertMinimumSpanningTree<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, IMinimumSpanningTreeAlgorithm<TVertex, TEdge> algorithm, ref int[][] resultEdges) where TEdge : IEdge<TVertex>
        {

            EdgeRecorderObserver<TVertex, TEdge> edgeRecorder = new EdgeRecorderObserver<TVertex, TEdge>();

            using (edgeRecorder.Attach(algorithm))
                algorithm.Compute();

            resultEdges = new int[edgeRecorder.Edges.Count][];
            for (int i = 0; i < edgeRecorder.Edges.Count; i++)
               resultEdges[i] = new int[] { Convert.ToInt32(edgeRecorder.Edges[i].Source), Convert.ToInt32(edgeRecorder.Edges[i].Target) };
        }



    }
}
