using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore.Graphs {
    public class SimpleGraph<T> {
        private List<T> _vertices;

        private HashSet<GraphEdge> _edges;

        public int Count {
            get {
                return this._vertices.Count;
            }
        }

        public HashSet<GraphEdge> Edges {
            get {
                return this._edges;
            }
            set {
                this._edges = value;
            }
        }

        public T this[int index] {
            get {
                return this._vertices[index];
            }
            set {
                this._vertices[index] = value;
            }
        }

        public SimpleGraph(IEnumerable<T> Vertices = null) {
            this._vertices = new List<T>();
            this._edges = new HashSet<GraphEdge>();
            if (Vertices != null) {
                int num = checked(Vertices.Count<T>() - 1);
                for (int i = 0; i <= num; i = checked(i + 1)) {
                    this._vertices.Add(Vertices.ElementAtOrDefault<T>(i));
                }
            }
        }

        public int AddVertex(T Vertex) {
            this._vertices.Add(Vertex);
            return checked(this._vertices.Count - 1);
        }

        public void AddVertices(IEnumerable<T> Vertices) {
            IEnumerator<T> enumerator = null;
            try {
                enumerator = Vertices.GetEnumerator();
                while (enumerator.MoveNext()) {
                    this.AddVertex(enumerator.Current);
                }
            } finally {
                if (enumerator != null) {
                    enumerator.Dispose();
                }
            }
        }

        public List<int> GetConnectedVertices(int VertexIndex) {
            HashSet<GraphEdge>.Enumerator enumerator = new HashSet<GraphEdge>.Enumerator();
            List<int> nums = new List<int>();
            try {
                enumerator = this.Edges.GetEnumerator();
                while (enumerator.MoveNext()) {
                    int otherEnd = enumerator.Current.GetOtherEnd(VertexIndex);
                    if (otherEnd == -1) {
                        continue;
                    }
                    nums.Add(otherEnd);
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
            return nums;
        }

        public List<int>[] GetConnectivityMap() {
            HashSet<GraphEdge>.Enumerator enumerator = new HashSet<GraphEdge>.Enumerator();
            List<int>[] nums = new List<int>[checked(checked(this.Count - 1) + 1)];
            int count = checked(this.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                nums[i] = new List<int>();
            }
            try {
                enumerator = this.Edges.GetEnumerator();
                while (enumerator.MoveNext()) {
                    GraphEdge current = enumerator.Current;
                    if (!current.IsValid()) {
                        continue;
                    }
                    nums[current.From].Add(current.To);
                    nums[current.To].Add(current.From);
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
            return nums;
        }

        public static SimpleGraph<Point3d> LinesToGraph(IEnumerable<Line> Lines, double Tolerance) {

            IEnumerator<Line> enumerator = null;

            List<Point3d> point3ds = new List<Point3d>();

                enumerator = Lines.GetEnumerator();
                while (enumerator.MoveNext()) {
                    Line current = enumerator.Current;
                    point3ds.Add(current.From);
                    point3ds.Add(current.To);
                }
         

            DuplicateRemover duplicateRemover = new DuplicateRemover(point3ds, Tolerance, 123);
            duplicateRemover.Solve();

            SimpleGraph<Point3d> simpleGraph = new SimpleGraph<Point3d>(duplicateRemover.UniquePoints);
            int num = checked(Lines.Count<Line>() - 1);
            for (int i = 0; i <= num; i = checked(i + 1)) {
                simpleGraph.Edges.Add(new GraphEdge(checked(i * 2), checked(checked(i * 2) + 1)));
            }
            return simpleGraph;
        }

        public void PurgeEdges() {
            HashSet<GraphEdge>.Enumerator enumerator = new HashSet<GraphEdge>.Enumerator();
            List<GraphEdge> graphEdges = new List<GraphEdge>();
            try {
                enumerator = this.Edges.GetEnumerator();
                while (enumerator.MoveNext()) {
                    GraphEdge current = enumerator.Current;
                    if (current.IsValid()) {
                        continue;
                    }
                    graphEdges.Add(current);
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
            int count = checked(graphEdges.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                this.Edges.Remove(graphEdges[i]);
            }
        }

        public bool RemoveVertex(int VertexIndex) {
            bool flag;
            HashSet<GraphEdge>.Enumerator enumerator = new HashSet<GraphEdge>.Enumerator();
            if (VertexIndex <= checked(this._vertices.Count - 1)) {
                try {
                    enumerator = this.Edges.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        enumerator.Current.OnVertexRemoved(VertexIndex);
                    }
                } finally {
                    ((IDisposable)enumerator).Dispose();
                }
                flag = true;
            } else {
                flag = false;
            }
            return flag;
        }
    }
}
