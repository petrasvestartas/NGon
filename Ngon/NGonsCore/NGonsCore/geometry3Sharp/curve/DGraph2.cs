using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;

namespace NGonsCore.geometry3Sharp.curve
{
	public class DGraph2
	{
		public const int InvalidID = -1;
		public const int DuplicateEdgeID = -1;

		public static readonly Vector2D InvalidVertex = new Vector2D(Double.MaxValue, 0);
		public static readonly Index2i InvalidEdge = new Index2i(InvalidID, InvalidID);


		RefCountVector vertices_refcount;
		DVector<double> vertices;

		DVector<List<int>> vertex_edges;

		RefCountVector edges_refcount;
		DVector<int> edges;

		int timestamp = 0;
		int shape_timestamp = 0;

		int max_group_id = 0;

		public DGraph2()
		{
			vertices = new DVector<double>();
			vertex_edges = new DVector<List<int>>();
			vertices_refcount = new RefCountVector();

			edges = new DVector<int>();
			edges_refcount = new RefCountVector();
			max_group_id = 0;
		}



		void UpdateTimeStamp(bool bShapeChange) {

			timestamp++;
			if (bShapeChange)
				shape_timestamp++;
		}
		public int Timestamp {
			get { return timestamp; }
		}
		public int ShapeTimestamp {
			get { return shape_timestamp; }
		}



		public int VertexCount {
			get { return vertices_refcount.Count; }
		}
		public int EdgeCount {
			get { return edges_refcount.Count; }
		}


		// these values are (max_used+1), ie so an iteration should be < MaxVertexID, not <=
		public int MaxVertexID {
			get { return vertices_refcount.Max_index; }
		}
		public int MaxEdgeID {
			get { return edges_refcount.Max_index; }
		}
		public int MaxGroupID {
			get { return max_group_id; }
		}


		public bool IsVertex(int vID) {
			return vertices_refcount.IsValid(vID);
		}
		public bool IsEdge(int eID) {
			return edges_refcount.IsValid(eID);
		}


		public Vector2D GetVertex(int vID) {
			return vertices_refcount.IsValid(vID) ?
				new Vector2D(vertices[2 * vID], vertices[2 * vID + 1]) : InvalidVertex;
		}

		public void SetVertex(int vID, Vector2D vNewPos) {
			Debug.Assert(vNewPos.IsFinite);     // this will really catch a lot of bugs...
			if (vertices_refcount.IsValid(vID)) {
				int i = 2 * vID;
				vertices[i] = vNewPos.x; vertices[i + 1] = vNewPos.y;
				UpdateTimeStamp(true);
			}
		}



		public ReadOnlyCollection<int> GetVtxEdges(int vID) {
			return vertices_refcount.IsValid(vID) ?
				vertex_edges[vID].AsReadOnly() : null;
		}

		public int GetVtxEdgeCount(int vID) {
			return vertices_refcount.IsValid(vID) ?
				vertex_edges[vID].Count : -1;
		}


		public int GetMaxVtxEdgeCount() {
			int max = 0;
			foreach (int vid in vertices_refcount)
				max = Math.Max(max, vertex_edges[vid].Count);
			return max;
		}





		public int GetEdgeGroup(int eid)
		{
			return edges_refcount.IsValid(eid) ? edges[3 * eid + 2] : -1;
		}

		public void SetEdgeGroup(int eid, int group_id) {
			Debug.Assert(edges_refcount.IsValid(eid));
			if (edges_refcount.IsValid(eid)) {
				edges[3 * eid + 2] = group_id;
				max_group_id = Math.Max(max_group_id, group_id + 1);
				UpdateTimeStamp(false);
			}
		}

		public int AllocateEdgeGroup() {
			return max_group_id++;
		}



		public Index2i GetEdgeV(int eID)
		{
			return edges_refcount.IsValid(eID) ?
				new Index2i(edges[3 * eID], edges[3 * eID + 1]) : InvalidEdge;
		}
		public bool GetEdgeV(int eID, ref Vector2D a, ref Vector2D b)
		{
			if (edges_refcount.IsValid(eID)) {
				int iv0 = 2 * edges[3 * eID];
				a.x = vertices[iv0]; a.y = vertices[iv0 + 1];
				int iv1 = 2 * edges[3 * eID + 1];
				b.x = vertices[iv1]; b.y = vertices[iv1 + 1];
				return true;
			}
			return false;
		}



		public int AppendVertex(Vector2D v)
		{
			int vid = vertices_refcount.Allocate();
			int i = 2 * vid;
			vertices.Insert(v[1], i + 1);
			vertices.Insert(v[0], i);
			vertex_edges.Insert(new List<int>(), vid);
			UpdateTimeStamp(true);
			return vid;
		}



		public int AppendEdge(int v0, int v1, int gid = -1){
			return AppendEdge(new Index2i(v0, v1), gid);
		}
		public int AppendEdge(Index2i ev, int gid = -1)
		{
			if (IsVertex(ev[0]) == false || IsVertex(ev[1]) == false) {
				Util.gDevAssert(false);
				return InvalidID;
			}
			if (ev[0] == ev[1]) {
				Util.gDevAssert(false);
				return InvalidID;
			}
			int e0 = FindEdge(ev[0], ev[1]);
			if (e0 != InvalidID)
				return DuplicateEdgeID;

			// increment ref counts and update/create edges
			vertices_refcount.Increment(ev[0]);
			vertices_refcount.Increment(ev[1]);
			max_group_id = Math.Max(max_group_id, gid + 1);

			// now safe to insert edge
			int eid = Add_edge(ev[0], ev[1], gid);

			UpdateTimeStamp(true);
			return eid;
		}

		int Add_edge(int a, int b, int gid) {
			if (b < a) {
				int t = b; b = a; a = t;
			}
			int eid = edges_refcount.Allocate();
			int i = 3 * eid;
			edges.Insert(a, i);
			edges.Insert(b, i + 1);
			edges.Insert(gid, i + 2);

			vertex_edges[a].Add(eid);
			vertex_edges[b].Add(eid);
			return eid;
		}



		public void AppendPolygon(IEnumerable<Vector2D> poly, int gid = -1) {
			int first = -1;
			int prev = -1;
			foreach(Vector2D v in poly) {
				int cur = AppendVertex(v);
				if (prev == -1)
					first = cur;
				else
					AppendEdge(prev, cur, gid);
				prev = cur;
			}
			AppendEdge(prev, first, gid);
		}






		// iterators

		public IEnumerable<int> VertexIndices() {
			foreach (int vid in vertices_refcount)
				yield return vid;
		}
		public IEnumerable<int> EdgeIndices() {
			foreach (int eid in edges_refcount)
				yield return eid;
		}




		public IEnumerable<Vector2D> Vertices() {
			foreach (int vid in vertices_refcount) {
				int i = 2 * vid;
				yield return new Vector2D(vertices[i], vertices[i + 1]);
			}
		}

		// return value is [v0,v1,gid]
		public IEnumerable<Index3i> Edges() {
			foreach (int eid in edges_refcount) {
				int i = 3 * eid;
				yield return new Index3i(edges[i], edges[i + 1], edges[i + 2]);
			}
		}


		public IEnumerable<int> VtxVerticesItr(int vID) {
			if (vertices_refcount.IsValid(vID)) {
				List<int> vedges = vertex_edges[vID];
				int N = vedges.Count;
				for (int i = 0; i < N; ++i)
					yield return Edge_other_v(vedges[i], vID);
			}
		}


		public IEnumerable<int> VtxEdgesItr(int vID) {
			if (vertices_refcount.IsValid(vID)) {
				List<int> vedges = vertex_edges[vID];
				int N = vedges.Count;
				for (int i = 0; i < N; ++i)
					yield return vedges[i];
			}
		}



		public int FindEdge(int vA, int vB) {
			int vO = Math.Max(vA, vB);
			List<int> e0 = vertex_edges[Math.Min(vA, vB)];
			int N = e0.Count;
			for (int i = 0; i < N; ++i ) {
				if (Edge_has_v(e0[i], vO))
					return e0[i];
			}
			return InvalidID;
		}





		public MeshResult RemoveEdge(int eID, bool bRemoveIsolatedVertices)
		{
			if (!edges_refcount.IsValid(eID)) {
				Util.gDevAssert(false);
				return MeshResult.Failed_NotATriangle;
			}

			int i = 3 * eID;
			Index2i ev = new Index2i(edges[i], edges[i + 1]);
			vertex_edges[ev.a].Remove(eID);
			vertex_edges[ev.b].Remove(eID);

			edges_refcount.Decrement(eID);

			// Decrement vertex refcounts. If any hit 1 and we got remove-isolated flag,
			// we need to remove that vertex
			for (int j = 0; j < 2; ++j) {
				int vid = ev[j];
				vertices_refcount.Decrement(vid);
				if (bRemoveIsolatedVertices && vertices_refcount.RefCount(vid) == 1) {
					vertices_refcount.Decrement(vid);
					Util.gDevAssert(vertices_refcount.IsValid(vid) == false);
					vertex_edges[vid] = null;
				}
			}

			UpdateTimeStamp(true);
			return MeshResult.Ok;
		}





		public struct EdgeSplitInfo
		{
			public int vNew;
			public int eNewBN;      // new edge [vNew,vB] (original was AB)
		}
		public MeshResult SplitEdge(int vA, int vB, out EdgeSplitInfo split)
		{
			int eid = FindEdge(vA, vB);
			if (eid == InvalidID) {
				split = new EdgeSplitInfo();
				return MeshResult.Failed_NotAnEdge;
			}
			return SplitEdge(eid, out split);
		}
		public MeshResult SplitEdge(int eab, out EdgeSplitInfo split)
		{
			split = new EdgeSplitInfo();
			if (!IsEdge(eab))
				return MeshResult.Failed_NotAnEdge;

			// look up primary edge
			int eab_i = 3 * eab;
			int a = edges[eab_i], b = edges[eab_i + 1];
			int gid = edges[eab_i + 2];

			// create new vertex
			Vector2D vNew = 0.5 * (GetVertex(a) + GetVertex(b));
			int f = AppendVertex(vNew);

			// rewrite edge bc, create edge af
			int eaf = eab;
			Replace_edge_vertex(eaf, b, f);
			vertex_edges[b].Remove(eab);
			vertex_edges[f].Add(eaf);

			// create new edge fb
			int efb = Add_edge(f, b, gid);

			// update vertex refcounts
			vertices_refcount.Increment(f, 2);

			split.vNew = f;
			split.eNewBN = efb;

			UpdateTimeStamp(true);
			return MeshResult.Ok;
		}



		// [TODO] collapse edge


		public struct EdgeCollapseInfo
		{
			public int vKept;
			public int vRemoved;

			public int eCollapsed;              // edge we collapsed
		}
		public MeshResult CollapseEdge(int vKeep, int vRemove, out EdgeCollapseInfo collapse)
		{
			bool DiscardIsolatedVertices = true;

			collapse = new EdgeCollapseInfo();
			if (IsVertex(vKeep) == false || IsVertex(vRemove) == false)
				return MeshResult.Failed_NotAnEdge;

			int b = vKeep;      // renaming for sanity. We remove a and keep b
			int a = vRemove;

			int eab = FindEdge(a, b);
			if (eab == InvalidID)
				return MeshResult.Failed_NotAnEdge;

			List<int> edges_b = vertex_edges[b];
			List<int> edges_a = vertex_edges[a];

			// get rid of any edges that will be duplicates
			bool done = false;
			while (!done) {
				done = true;
				foreach (int eax in edges_a) {
					int o = Edge_other_v(eax, a);
					if (o != b && FindEdge(b,o) != InvalidID) {
						RemoveEdge(eax, DiscardIsolatedVertices);
						done = false;
						break;
					}
				}
			}

			edges_b.Remove(eab);
			foreach ( int eax in edges_a ) {
				int o = Edge_other_v(eax, a);
				if (o == b)
					continue;       // discard this edge
				Replace_edge_vertex(eax, a, b);
				vertices_refcount.Decrement(a);
				edges_b.Add(eax);
				vertices_refcount.Increment(b);
			}

			edges_refcount.Decrement(eab);
			vertices_refcount.Decrement(b);
			vertices_refcount.Decrement(a);
			if (DiscardIsolatedVertices) {
				vertices_refcount.Decrement(a); // second decrement discards isolated vtx
				Util.gDevAssert(!IsVertex(a));
				vertex_edges[a] = null;
			}

			edges_a.Clear();

			collapse.vKept = vKeep;
			collapse.vRemoved = vRemove;
			collapse.eCollapsed = eab;

			UpdateTimeStamp(true);
			return MeshResult.Ok;
		}



		bool Edge_has_v(int eid, int vid) {
			int i = 3 * eid;
			return (edges[i] == vid) || (edges[i + 1] == vid);
		}
		int Edge_other_v(int eID, int vID) {
			int i = 3 * eID;
			int ev0 = edges[i], ev1 = edges[i + 1];
			return (ev0 == vID) ? ev1 : ((ev1 == vID) ? ev0 : InvalidID);
		}
		int Replace_edge_vertex(int eID, int vOld, int vNew)
		{
			int i = 3 * eID;
			int a = edges[i], b = edges[i + 1];
			if (a == vOld) {
				edges[i] = Math.Min(b, vNew);
				edges[i + 1] = Math.Max(b, vNew);
				return 0;
			} else if (b == vOld) {
				edges[i] = Math.Min(a, vNew);
				edges[i + 1] = Math.Max(a, vNew);
				return 1;
			} else
				return -1;
		}





		// compute vertex bounding box
		public AxisAlignedBox2d GetBounds()
		{
			double x = 0, y = 0;
			foreach (int vi in vertices_refcount) {
				x = vertices[2 * vi]; y = vertices[2 * vi + 1];;
				break;
			}
			double minx = x, maxx = x, miny = y, maxy = y;
			foreach (int vi in vertices_refcount) {
				x = vertices[2 * vi]; y = vertices[2 * vi + 1];;
				if (x < minx) minx = x; else if (x > maxx) maxx = x;
				if (y < miny) miny = y; else if (y > maxy) maxy = y;
			}
			return new AxisAlignedBox2d(minx, miny, maxx, maxy);
		}

		AxisAlignedBox2d cached_bounds;
		int cached_bounds_timestamp = -1;

		//! cached bounding box, lazily re-computed on access if mesh has changed
		public AxisAlignedBox2d CachedBounds {
			get {
				if (cached_bounds_timestamp != Timestamp) {
					cached_bounds = GetBounds();
					cached_bounds_timestamp = Timestamp;
				}
				return cached_bounds;
			}
		}




		public bool IsCompact {
			get { return vertices_refcount.Is_dense && edges_refcount.Is_dense; }
		}
		public bool IsCompactV {
			get { return vertices_refcount.Is_dense; }
		}










		public enum FailMode { DebugAssert, gDevAssert, Throw, ReturnOnly }

		/// <summary>
		// This function checks that the graph is well-formed, ie all internal data
		// structures are consistent
		/// </summary>
		public bool CheckValidity(FailMode eFailMode = FailMode.Throw)
		{
			bool is_ok = true;
			Action<bool> CheckOrFailF = (b) => {
				is_ok = is_ok && b;
			};
			if (eFailMode == FailMode.DebugAssert) {
				CheckOrFailF = (b) => {
					Debug.Assert(b);
					is_ok = is_ok && b;
				};
			} else if (eFailMode == FailMode.gDevAssert) {
				CheckOrFailF = (b) => {
					Util.gDevAssert(b);
					is_ok = is_ok && b;
				};
			} else if (eFailMode == FailMode.Throw) {
				CheckOrFailF = (b) => {
					if (b == false)
						throw new Exception("DGraph2.CheckValidity: check failed");
				};
			}

			// edge verts/tris must exist
			foreach (int eID in EdgeIndices()) {
				CheckOrFailF(IsEdge(eID));
				CheckOrFailF(edges_refcount.RefCount(eID) == 1);
				Index2i ev = GetEdgeV(eID);
				CheckOrFailF(IsVertex(ev[0]));
				CheckOrFailF(IsVertex(ev[1]));
				CheckOrFailF(ev[0] < ev[1]);
			}

			// verify compact check
			bool is_compact = vertices_refcount.Is_dense;
			if (is_compact) {
				for (int vid = 0; vid < vertices.Length / 3; ++vid) {
					CheckOrFailF(vertices_refcount.IsValid(vid));
				}
			}

			// vertex edges must exist and reference this vert
			foreach (int vID in VertexIndices()) {
				CheckOrFailF(IsVertex(vID));

				Vector2D v = GetVertex(vID);
				CheckOrFailF(double.IsNaN(v.LengthSquared) == false);
				CheckOrFailF(double.IsInfinity(v.LengthSquared) == false);

				List<int> l = vertex_edges[vID];
				foreach (int edgeid in l) {
					CheckOrFailF(IsEdge(edgeid));
					CheckOrFailF(Edge_has_v(edgeid, vID));

					int otherV = Edge_other_v(edgeid, vID);
					int e2 = FindEdge(vID, otherV);
					CheckOrFailF(e2 != InvalidID);
					CheckOrFailF(e2 == edgeid);
					e2 = FindEdge(otherV, vID);
					CheckOrFailF(e2 != InvalidID);
					CheckOrFailF(e2 == edgeid);
				}

				CheckOrFailF(vertices_refcount.RefCount(vID) == l.Count + 1);

			}

			return is_ok;
		}



	}
}
