using System.Collections.Generic;
using System.Linq;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{
    public class DCurve3 : ISampledCurve3d
    {
        // [TODO] use dvector? or double-indirection indexing?
        //   question is how to insert efficiently...
        protected List<Vector3D> vertices;
        public bool Closed { get; set; }
        public int Timestamp;

        public DCurve3()
        {
            vertices = new List<Vector3D>();
            Closed = false;
            Timestamp = 1;
        }

        public DCurve3(List<Vector3D> verticesIn, bool bClosed, bool bTakeOwnership = false)
        {
            if (bTakeOwnership)
                this.vertices = verticesIn;
            else
                this.vertices = new List<Vector3D>(verticesIn);
            Closed = bClosed;
            Timestamp = 1;
        }
        public DCurve3(IList<Vector3D> verticesIn, bool bClosed)
        {
            this.vertices = new List<Vector3D>(verticesIn);
            Closed = bClosed;
            Timestamp = 1;
        }

        public DCurve3(DCurve3 copy)
        {
            vertices = new List<Vector3D>(copy.vertices);
            Closed = copy.Closed;
            Timestamp = 1;
        }

        public void AppendVertex(Vector3D v) {
            vertices.Add(v);
            Timestamp++;
        }

        public int VertexCount {
            get { return vertices.Count; }
        }

        public Vector3D GetVertex(int i) {
            return vertices[i];
        }
        public void SetVertex(int i, Vector3D v) {
            vertices[i] = v;
            Timestamp++;
        }

        public void SetVertices(VectorArray3d v)
        {
            vertices = new List<Vector3D>();
            for (int i = 0; i < v.Count; ++i)
                vertices.Add(v[i]);
            Timestamp++;
        }

        public void SetVertices(IEnumerable<Vector3D> v)
        {
            vertices = new List<Vector3D>(v);
            Timestamp++;
        }

        public void SetVertices(List<Vector3D> vertices, bool bTakeOwnership)
        {
            if (bTakeOwnership)
                this.vertices = vertices;
            else
                this.vertices = new List<Vector3D>(vertices);
            Timestamp++;
        }

        public void ClearVertices()
        {
            vertices = new List<Vector3D>();
            Closed = false;
            Timestamp++;
        }


        public Vector3D this[int key]
        {
            get { return vertices[key]; }
            set { vertices[key] = value; Timestamp++; }
        }

        public Vector3D Start {
            get { return vertices[0]; }
        }
        public Vector3D End {
            get { return vertices.Last(); }
        }

        public IEnumerable<Vector3D> Vertices {
            get { return vertices; }
        }


        public AxisAlignedBox3d GetBoundingBox()
        {
            // [RMS] problem w/ readonly because vector is a class...
            //AxisAlignedBox3d box = AxisAlignedBox3d.Empty;
            AxisAlignedBox3d box = new AxisAlignedBox3d(false);
            foreach (Vector3D v in vertices)
                box.Contain(v);
            return box;
        }

        public double ArcLength {
            get {
                double dLen = 0;
                for (int i = 1; i < vertices.Count; ++i)
                    dLen += (vertices[i] - vertices[i - 1]).Length;
                return dLen;
            }
        }

        public Vector3D Tangent(int i)
        {
            if (i == 0)
                return (vertices[1] - vertices[0]).Normalized;
            else if (i == vertices.Count - 1)
                return (vertices.Last() - vertices[vertices.Count - 2]).Normalized;
            else
                return (vertices[i + 1] - vertices[i - 1]).Normalized;
        }

        public Vector3D Centroid(int i)
        {
            if (i == 0 || i == vertices.Count - 1)
                return vertices[i];
            else
                return 0.5 * (vertices[i + 1] + vertices[i - 1]);
        }

    }
}
