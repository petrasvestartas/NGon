using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{
	public class PolyLine3d : IEnumerable<Vector3D>
	{
		protected List<Vector3D> vertices;
		public int Timestamp;

		public PolyLine3d() {
			vertices = new List<Vector3D>();
			Timestamp = 0;
		}

		public PolyLine3d(PolyLine3d copy)
		{
			vertices = new List<Vector3D>(copy.vertices);
			Timestamp = 0;
		}

		public PolyLine3d(Vector3D[] v)
		{
			vertices = new List<Vector3D>(v);
			Timestamp = 0;
		}
		public PolyLine3d(VectorArray3d v)
		{
			vertices = new List<Vector3D>(v.AsVector3d());
			Timestamp = 0;
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
			get { return vertices[vertices.Count-1]; }
		}


		public ReadOnlyCollection<Vector3D> Vertices {
			get { return vertices.AsReadOnly(); }
		}

		public int VertexCount
		{
			get { return vertices.Count; }
		}

		public void AppendVertex(Vector3D v)
		{
			vertices.Add(v);
			Timestamp++; 
		}


		public Vector3D GetTangent(int i)
		{
			if (i == 0)
				return (vertices[1] - vertices[0]).Normalized;
			else if (i == vertices.Count - 1)
				return (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized;
			else
				return (vertices[i + 1] - vertices[i - 1]).Normalized;
		}


		public AxisAlignedBox3d GetBounds() {
			if ( vertices.Count == 0 )
				return AxisAlignedBox3d.Empty;
			AxisAlignedBox3d box = new AxisAlignedBox3d(vertices[0]);
			for ( int i = 1; i < vertices.Count; ++i )
				box.Contain(vertices[i]);
			return box;
		}


		public IEnumerable<Segment3d> SegmentItr() {
			for ( int i = 0; i < vertices.Count-1; ++i )
				yield return new Segment3d( vertices[i], vertices[i+1] );
		}

		public IEnumerator<Vector3D> GetEnumerator() {
			return vertices.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return vertices.GetEnumerator();
		}
	}
}
