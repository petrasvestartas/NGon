using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonsCore;
using Advanced.Algorithms.Graph;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Algo

{
    public static class Graph

    {


        public static DataTree<int> Color (List<int> V, List<int> E0, List<int> E1, int numberOfColors = 2)
        {


            var graph = new Advanced.Algorithms.DataStructures.Graph.AdjacencyList.Graph<int>();

            for (int i = 0; i < V.Count; i++)
            {
                graph.AddVertex(V[i]);
            }

            for (int i = 0; i < E0.Count; i++)
            {

                if (!graph.HasEdge(E0[i], E1[i]))
                    graph.AddEdge(E0[i], E1[i]);

            }


            MColorer<int, string> algorithm = new MColorer<int, string>();
            MColorResult<int, string> result = algorithm.Color(graph, Letters(numberOfColors));
            DataTree<int> dtResult = new DataTree<int>();

            if (result.Partitions != null)
            {
               

                int counter = 0;
                foreach (KeyValuePair<string, List<int>> p in result.Partitions)
                {
                    dtResult.AddRange(p.Value, new GH_Path(counter++));
                }
            }



            return dtResult;

        }



        public static DataTree<int> _GraphColorHalfEdges(this Mesh M, int numberOfColors = 2) {

            var graph = new Advanced.Algorithms.DataStructures.Graph.AdjacencyList.Graph<int>();


            var V = M._FEFlattenV();

            foreach (var v in V)
                graph.AddVertex(v);

            var E = M._eehalf();

            foreach (var edges in E)
                foreach (var e in edges)
                    if (!graph.HasEdge(e[0], e[1]))
                        graph.AddEdge(e[0], e[1]);

            MColorer<int, string> algorithm = new MColorer<int, string>();
            MColorResult<int, string> result = algorithm.Color(graph, Letters(numberOfColors));
            DataTree<int> dtResult = new DataTree<int>();


            if (result.Partitions != null) {
                int counter = 0;
                foreach (KeyValuePair<string, List<int>> p in result.Partitions) {
                    dtResult.AddRange(p.Value, new GH_Path(counter++));
                }
            }

            return dtResult;


        }

        public static DataTree<int> _GraphColorFaces(this Mesh M, int numberOfColors = 2) {

            var graph = new Advanced.Algorithms.DataStructures.Graph.AdjacencyList.Graph<int>();


    

   

        
            List<int>[] adj = M.GetNgonFaceAdjacencyOrdered();

            for (int i = 0; i < adj.Length; i++)
                graph.AddVertex(i);

            for (int i = 0; i< adj.Length; i++)
                foreach (var e in adj[i])
                    if (!graph.HasEdge(i, e))
                        graph.AddEdge(i, e);

            MColorer<int, string> algorithm = new MColorer<int, string>();
            MColorResult<int, string> result = algorithm.Color(graph, Letters(numberOfColors));
            DataTree<int> dtResult = new DataTree<int>();


            if (result.Partitions != null) {
                int counter = 0;
                foreach (KeyValuePair<string, List<int>> p in result.Partitions) {
                    dtResult.AddRange(p.Value, new GH_Path(counter++));
                }
            }

            return dtResult;


        }

        public static Dictionary<int,int> _GraphColorHalfEdgesDict(this Mesh M, int numberOfColors = 2)
        {

            var graph = new Advanced.Algorithms.DataStructures.Graph.AdjacencyList.Graph<int>();


            var V = M._FEFlattenV();

            foreach (var v in V)
                graph.AddVertex(v);

            var E = M._eehalf();

            foreach (var edges in E)
                foreach (var e in edges)
                    if (!graph.HasEdge(e[0], e[1]))
                        graph.AddEdge(e[0], e[1]);

            MColorer<int, string> algorithm = new MColorer<int, string>();
            MColorResult<int, string> result = algorithm.Color(graph, Letters(numberOfColors));
            Dictionary<int,int> dtResult = new Dictionary<int,int>();


            if (result.Partitions != null)
            {
                int counter = 0;
                foreach (KeyValuePair<string, List<int>> p in result.Partitions)
                {
                    foreach(int id in p.Value)
                    {
                        dtResult.Add(id,counter);
                    }
                    counter++;
                }
            }

            return dtResult;


        }

        private static string[] Letters(int n)
        {
            string[] letters = new string[n];
            for(int i = 0; i < n; i++)
            {
                letters[i] = i.ToString();
            }
            return letters;
        }



    }
}