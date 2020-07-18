﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public partial class DMesh3
    {


        // edits

        public MeshResult ReverseTriOrientation(int tID) {
            if (!IsTriangle(tID))
                return MeshResult.Failed_NotATriangle;
            Internal_reverse_tri_orientation(tID);
            UpdateTimeStamp(true);
            return MeshResult.Ok;
        }
        void Internal_reverse_tri_orientation(int tID) {
            Index3i t = GetTriangle(tID);
            Set_triangle(tID, t[1], t[0], t[2]);
            Index3i te = GetTriEdges(tID);
            Set_triangle_edges(tID, te[0], te[2], te[1]);
        }

		public void ReverseOrientation(bool bFlipNormals = true) {
			foreach ( int tid in TriangleIndices() ) {
				Internal_reverse_tri_orientation(tid);
			}
			if ( bFlipNormals && HasVertexNormals ) {
				foreach ( int vid in VertexIndices() ) {
					int i = 3*vid;
					normals[i] = -normals[i];
					normals[i+1] = -normals[i+1];
					normals[i+2] = -normals[i+2];
				}
			}
            UpdateTimeStamp(true);
		}




        /// <summary>
        /// Remove vertex vID, and all connected triangles if bRemoveAllTriangles = true
        /// (if false, them throws exception if there are still any triangles!)
        /// if bPreserveManifold, checks that we will not create a bowtie vertex first
        /// </summary>
        public MeshResult RemoveVertex(int vID, bool bRemoveAllTriangles = true, bool bPreserveManifold = true)
        {
            if (vertices_refcount.IsValid(vID) == false)
                return MeshResult.Failed_NotAVertex;

            if ( bRemoveAllTriangles ) {

                // if any one-ring vtx is a boundary vtx and one of its outer-ring edges is an
                // interior edge then we will create a bowtie if we remove that triangle
                if ( bPreserveManifold ) {
                    foreach ( int tid in VtxTrianglesItr(vID) ) {
                        Index3i tri = GetTriangle(tid);
                        int j = IndexUtil.find_tri_index(vID, tri);
                        int oa = tri[(j + 1) % 3], ob = tri[(j + 2) % 3];
                        int eid = Find_edge(oa,ob);
                        if (Edge_is_boundary(eid))
                            continue;
                        if (Vertex_is_boundary(oa) || Vertex_is_boundary(ob))
                            return MeshResult.Failed_WouldCreateBowtie;
                    }
                }

                List<int> tris = new List<int>();
                GetVtxTriangles(vID, tris, true);
                foreach (int tID in tris) {
                    MeshResult result = RemoveTriangle(tID, false, bPreserveManifold);
                    if (result != MeshResult.Ok)
                        return result;
                }
            }

            if ( vertices_refcount.RefCount(vID) != 1)
                throw new NotImplementedException("DMesh3.RemoveVertex: vertex is still referenced");

            vertices_refcount.Decrement(vID);
            Debug.Assert(vertices_refcount.IsValid(vID) == false);
            vertex_edges[vID] = null;

            UpdateTimeStamp(true);
            return MeshResult.Ok;
        }



        /// <summary>
        /// Remove a tID from the mesh. Also removes any unreferenced edges after tri is removed.
        /// If bRemoveIsolatedVertices is false, then if you remove all tris from a vert, that vert is also removed.
        /// If bPreserveManifold, we check that you will not create a bowtie vertex (and return false).
        ///   If this check is not done, you have to make sure you don't create a bowtie, because other
        ///   code assumes we don't have bowties, and will not handle it properly
        /// </summary>
        public MeshResult RemoveTriangle(int tID, bool bRemoveIsolatedVertices = true, bool bPreserveManifold = true)
        {
            if ( ! triangles_refcount.IsValid(tID) ) {
                Debug.Assert(false);
                return MeshResult.Failed_NotATriangle;
            }

            Index3i tv = GetTriangle(tID);
            Index3i te = GetTriEdges(tID);

            // if any tri vtx is a boundary vtx connected to two interior edges, then
            // we cannot remove this triangle because it would create a bowtie vertex!
            // (that vtx already has 2 boundary edges, and we would add two more)
            if (bPreserveManifold) {
                for (int j = 0; j < 3; ++j) {
                    if (Vertex_is_boundary(tv[j])) {
                        if (Edge_is_boundary(te[j]) == false && Edge_is_boundary(te[(j + 2) % 3]) == false)
                            return MeshResult.Failed_WouldCreateBowtie;
                    }
                }
            }

            // Remove triangle from its edges. if edge has no triangles left,
            // then it is removed.
            for (int j = 0; j < 3; ++j) {
                int eid = te[j];
                Replace_edge_triangle(eid, tID, InvalidID);
                if (edges[4 * eid + 2] == InvalidID) {
                    int a = edges[4 * eid];
                    List<int> edges_a = vertex_edges[a];
                    edges_a.Remove(eid);

                    int b = edges[4 * eid + 1];
                    List<int> edges_b = vertex_edges[b];
                    edges_b.Remove(eid);

                    edges_refcount.Decrement(eid);
                }
            }

            // free this triangle
			triangles_refcount.Decrement( tID );
			Debug.Assert( triangles_refcount.IsValid( tID ) == false );

            // Decrement vertex refcounts. If any hit 1 and we got remove-isolated flag,
            // we need to remove that vertex
            for (int j = 0; j < 3; ++j) {
                int vid = tv[j];
                vertices_refcount.Decrement(vid);
                if ( bRemoveIsolatedVertices && vertices_refcount.RefCount(vid) == 1) {
                    vertices_refcount.Decrement(vid);
                    Debug.Assert(vertices_refcount.IsValid(vid) == false);
                    vertex_edges[vid] = null;
                }
            }

            UpdateTimeStamp(true);
            return MeshResult.Ok;
        }




		public struct EdgeSplitInfo {
			public bool bIsBoundary;
			public int vNew;
			public int eNewBN;      // new edge [vNew,vB] (original was AB)
			public int eNewCN;      // new edge [vNew,vC] (C is "first" other vtx in ring)
			public int eNewDN;		// new edge [vNew,vD] (D is "second" other, which doesn't exist on bdry)
		}
		public MeshResult SplitEdge(int vA, int vB, out EdgeSplitInfo split)
		{
			int eid = Find_edge(vA, vB);
			if ( eid == InvalidID ) {
				split = new EdgeSplitInfo();
				return MeshResult.Failed_NotAnEdge;
			}
			return SplitEdge(eid, out split);
		}
		public MeshResult SplitEdge(int eab, out EdgeSplitInfo split)
		{
			split = new EdgeSplitInfo();
			if (! IsEdge(eab) )
				return MeshResult.Failed_NotAnEdge;

			// look up primary edge & triangle
			int eab_i = 4*eab;
			int a = edges[eab_i], b = edges[eab_i + 1];
			int t0 = edges[eab_i + 2];
            if (t0 == InvalidID)
                return MeshResult.Failed_BrokenTopology;
			Index3i T0tv = GetTriangle(t0);
			int[] T0tv_array = T0tv.array;
			int c = IndexUtil.orient_tri_edge_and_find_other_vtx(ref a, ref b, T0tv_array);

			// create new vertex
			Vector3D vNew = 0.5 * ( GetVertex(a) + GetVertex(b) );
			int f = AppendVertex( vNew );
            if (HasVertexNormals) 
                SetVertexNormal(f, (GetVertexNormal(a) + GetVertexNormal(b)).Normalized);
            if (HasVertexColors)
                SetVertexColor(f, 0.5f * (GetVertexColor(a) + GetVertexColor(b)) );
            if (HasVertexUVs)
                SetVertexUV(f, 0.5f * (GetVertexUV(a) + GetVertexUV(b)));


            // quite a bit of code is duplicated between boundary and non-boundary case, but it
            //  is too hard to follow later if we factor it out...
            if ( Edge_is_boundary(eab) ) {

				// look up edge bc, which needs to be modified
				Index3i T0te = GetTriEdges(t0);
				int ebc = T0te[ IndexUtil.find_edge_index_in_tri(b, c, T0tv_array) ];

				// rewrite existing triangle
				Replace_tri_vertex(t0, b, f);

				// add new second triangle
				int t2 = Add_triangle_only(f,b,c, InvalidID, InvalidID, InvalidID);
				if ( triangle_groups != null )
					triangle_groups.Insert(triangle_groups[t0], t2);

				// rewrite edge bc, create edge af
				Replace_edge_triangle(ebc, t0, t2);
				int eaf = eab; 
				Replace_edge_vertex(eaf, b, f);
				vertex_edges[b].Remove(eab);
				vertex_edges[f].Add(eaf);

				// create new edges fb and fc 
				int efb = Add_edge(f, b, t2);
				int efc = Add_edge(f, c, t0, t2);

				// update triangle edge-nbrs
				Replace_triangle_edge(t0, ebc, efc);
				Set_triangle_edges(t2, efb, ebc, efc);

				// update vertex refcounts
				vertices_refcount.Increment(c);
				vertices_refcount.Increment(f, 2);

				split.bIsBoundary = true;
				split.vNew = f;
                split.eNewBN = efb;
				split.eNewCN = efc;
				split.eNewDN = InvalidID;

				UpdateTimeStamp(true);
				return MeshResult.Ok;

			} else {		// interior triangle branch
				
				// look up other triangle
				int t1 = edges[eab_i + 3];
				Index3i T1tv = GetTriangle(t1);
				int[] T1tv_array = T1tv.array;
				int d = IndexUtil.find_tri_other_vtx( a, b, T1tv_array );

				// look up edges that we are going to need to update
				// [TODO OPT] could use ordering to reduce # of compares here
				Index3i T0te = GetTriEdges(t0);
				int ebc = T0te[IndexUtil.find_edge_index_in_tri( b, c, T0tv_array )];
				Index3i T1te = GetTriEdges(t1);
				int edb = T1te[IndexUtil.find_edge_index_in_tri( d, b, T1tv_array )];

				// rewrite existing triangles
				Replace_tri_vertex(t0, b, f);
				Replace_tri_vertex(t1, b, f);

				// add two new triangles to close holes we just created
				int t2 = Add_triangle_only(f,b,c, InvalidID, InvalidID, InvalidID);
				int t3 = Add_triangle_only(f, d, b, InvalidID, InvalidID, InvalidID);
				if ( triangle_groups != null ) {
					triangle_groups.Insert(triangle_groups[t0], t2);
					triangle_groups.Insert(triangle_groups[t1], t3);
				}

				// update the edges we found above, to point to new triangles
				Replace_edge_triangle(ebc, t0, t2);
				Replace_edge_triangle(edb, t1, t3);

				// edge eab became eaf
				int eaf = eab; //Edge * eAF = eAB;
				Replace_edge_vertex(eaf, b, f);

				// update a/b/f vertex-edges
				vertex_edges[b].Remove( eab );
				vertex_edges[f].Add( eaf );

				// create new edges connected to f  (also updates vertex-edges)
				int efb = Add_edge( f, b, t2, t3 );
				int efc = Add_edge( f, c, t0, t2 );
				int edf = Add_edge( d, f, t1, t3 );

				// update triangle edge-nbrs
				Replace_triangle_edge(t0, ebc, efc);
				Replace_triangle_edge(t1, edb, edf);
				Set_triangle_edges(t2, efb, ebc, efc);
				Set_triangle_edges(t3, edf, edb, efb);

				// update vertex refcounts
				vertices_refcount.Increment( c );
				vertices_refcount.Increment( d );
				vertices_refcount.Increment( f, 4 );

				split.bIsBoundary = false;
				split.vNew = f;
                split.eNewBN = efb;
				split.eNewCN = efc;
				split.eNewDN = edf;

				UpdateTimeStamp(true);
				return MeshResult.Ok;
			}

		}






		public struct EdgeFlipInfo {
			public int eID;
			public int v0,v1;
			public int ov0,ov1;
			public int t0,t1;
		}
		public MeshResult FlipEdge(int vA, int vB, out EdgeFlipInfo flip) {
			int eid = Find_edge(vA, vB);
			if ( eid == InvalidID ) {
				flip = new EdgeFlipInfo();
				return MeshResult.Failed_NotAnEdge;
			}
			return FlipEdge(eid, out flip);
		}
		public MeshResult FlipEdge(int eab, out EdgeFlipInfo flip) 
		{
			flip = new EdgeFlipInfo();
			if (! IsEdge(eab) )
				return MeshResult.Failed_NotAnEdge;
			if ( Edge_is_boundary(eab) )
				return MeshResult.Failed_IsBoundaryEdge;

			// find oriented edge [a,b], tris t0,t1, and other verts c in t0, d in t1
			int eab_i = 4*eab;
			int a = edges[eab_i], b = edges[eab_i + 1];
			int t0 = edges[eab_i + 2], t1 = edges[eab_i + 3];
			int[] T0tv = GetTriangle(t0).array;
			int[] T1tv = GetTriangle(t1).array;
			int c = IndexUtil.orient_tri_edge_and_find_other_vtx( ref a, ref b, T0tv );
			int d = IndexUtil.find_tri_other_vtx(a, b, T1tv);
			if ( c == InvalidID || d == InvalidID ) {
				return MeshResult.Failed_BrokenTopology;
			}

			int flipped = Find_edge(c,d);
			if ( flipped != InvalidID )
				return MeshResult.Failed_FlippedEdgeExists;

			// find edges bc, ca, ad, db
			int ebc = Find_tri_neighbour_edge(t0, b, c);
			int eca = Find_tri_neighbour_edge(t0, c, a);
			int ead = Find_tri_neighbour_edge(t1,a,d);
			int edb = Find_tri_neighbour_edge(t1,d,b);

			// update triangles
			Set_triangle(t0, c, d, b);
			Set_triangle(t1, d, c, a);

			// update edge AB, which becomes flipped edge CD
			Set_edge_vertices(eab, c, d);
			Set_edge_triangles(eab, t0,t1);
			int ecd = eab;

			// update the two other edges whose triangle nbrs have changed
			if ( Replace_edge_triangle(eca, t0,t1) == -1 )
				throw new ArgumentException("DMesh3.FlipEdge: first replace_edge_triangle failed");
			if ( Replace_edge_triangle(edb, t1, t0) == -1 )
				throw new ArgumentException("DMesh3.FlipEdge: second replace_edge_triangle failed");

			// update triangle nbr lists (these are edges)
			Set_triangle_edges(t0, ecd, edb, ebc);
			Set_triangle_edges(t1, ecd, eca, ead);

			// remove old eab from verts a and b, and decrement ref counts
			if ( vertex_edges[a].Remove(eab) == false ) 
				throw new ArgumentException("DMesh3.FlipEdge: first vertex_edges remove failed");
			if ( vertex_edges[b].Remove(eab) == false ) 
				throw new ArgumentException("DMesh3.FlipEdge: second vertex_edges remove failed");
			vertices_refcount.Decrement(a);
			vertices_refcount.Decrement(b);
			if ( IsVertex(a) == false || IsVertex(b) == false )
				throw new ArgumentException("DMesh3.FlipEdge: either a or b is not a vertex?");

			// add new edge ecd to verts c and d, and increment ref counts
			vertex_edges[c].Add(ecd);
			vertex_edges[d].Add(ecd);
			vertices_refcount.Increment(c);
			vertices_refcount.Increment(d);

			// success! collect up results
			flip.eID = eab;
			flip.v0 = a; flip.v1 = b;
			flip.ov0 = c; flip.ov1 = d;
			flip.t0 = t0; flip.t1 = t1;

			UpdateTimeStamp(true);
			return MeshResult.Ok;
		}



		void Debug_fail(string s) {
		#if DEBUG
			System.Console.WriteLine("DMesh3.CollapseEdge: check failed: " + s);
			Debug.Assert(false);
			//throw new Exception("DMesh3.CollapseEdge: check failed: " + s);
		#endif
		}


		void Check_tri(int t) {
			Index3i tv = GetTriangle(t);
			if ( tv[0] == tv[1] || tv[0] == tv[2] || tv[1] == tv[2] )
				Debug.Assert(false);
		}
		void Check_edge(int e) {
			Index2i tv = GetEdgeT(e);
			if ( tv[0] == -1 )
				Debug.Assert(false);
		}


		public struct EdgeCollapseInfo {
			public int vKept;
			public int vRemoved;
			public bool bIsBoundary;

            public int eCollapsed;              // edge we collapsed
            public int tRemoved0, tRemoved1;    // tris we removed (second may be invalid)
            public int eRemoved0, eRemoved1;    // edges we removed (second may be invalid)
            public int eKept0, eKept1;          // edges we kept (second may be invalid)
		}
		public MeshResult CollapseEdge(int vKeep, int vRemove, out EdgeCollapseInfo collapse)
		{
			collapse = new EdgeCollapseInfo();
				
			if ( IsVertex(vKeep) == false || IsVertex(vRemove) == false )
				return MeshResult.Failed_NotAnEdge;

			int b = vKeep;		// renaming for sanity. We remove a and keep b
			int a = vRemove;
			List<int> edges_b = vertex_edges[b];

			int eab = Find_edge( a, b );
			if (eab == InvalidID)
				return MeshResult.Failed_NotAnEdge;

			int t0 = edges[4*eab+2];
            if (t0 == InvalidID)
                return MeshResult.Failed_BrokenTopology;
			Index3i T0tv = GetTriangle(t0);
			int c = IndexUtil.find_tri_other_vtx(a, b, T0tv);

			// look up opposing triangle/vtx if we are not in boundary case
			bool bIsBoundaryEdge = false;
			int d = InvalidID;
			int t1 = edges[4*eab+3];
			if (t1 != InvalidID) {
				Index3i T1tv = GetTriangle(t1);
				d = IndexUtil.find_tri_other_vtx( a, b, T1tv );
				if (c == d)
					return MeshResult.Failed_FoundDuplicateTriangle;
			} else {
				bIsBoundaryEdge = true;
			}

			// We cannot collapse if edge lists of a and b share vertices other
			//  than c and d  (because then we will make a triangle [x b b].
			//  Unfortunately I cannot see a way to do this more efficiently than brute-force search
			//  [TODO] if we had tri iterator for a, couldn't we check each tri for b  (skipping t0 and t1) ?
			List<int> edges_a = vertex_edges[a];
			int edges_a_count = 0;
			foreach (int eid_a in edges_a) {
				int vax =  Edge_other_v(eid_a, a);
				edges_a_count++;
				if ( vax == b || vax == c || vax == d )
					continue;
				foreach (int eid_b in edges_b) {
					if ( Edge_other_v(eid_b, b) == vax )
						return MeshResult.Failed_InvalidNeighbourhood;
				}
			}


			// We cannot collapse if we have a tetrahedron. In this case a has 3 nbr edges,
			//  and edge cd exists. But that is not conclusive, we also have to check that
			//  cd is an internal edge, and that each of its tris contain a or b
			if (edges_a_count == 3 && bIsBoundaryEdge == false) {
				int edc = Find_edge( d, c );
				int edc_i = 4*edc;
				if (edc != InvalidID && edges[edc_i+3] != InvalidID ) {
					int edc_t0 = edges[edc_i+2];
					int edc_t1 = edges[edc_i+3];

				    if ( (Tri_has_v(edc_t0,a) && Tri_has_v(edc_t1, b)) 
					    || (Tri_has_v(edc_t0, b) && Tri_has_v(edc_t1, a)) )
					return MeshResult.Failed_CollapseTetrahedron;
				}

			} else if (edges_a_count == 2 && bIsBoundaryEdge == true) {
				// cannot collapse edge if we are down to a single triangle
				if ( edges_b.Count == 2 && vertex_edges[c].Count == 2 )
					return MeshResult.Failed_CollapseTriangle;
			}

			// [RMS] this was added from C++ version...seems like maybe I never hit
			//   this case? Conceivably could be porting bug but looking at the
			//   C++ code I cannot see how we could possibly have caught this case...
			//
			// cannot collapse an edge where both vertices are boundary vertices
			// because that would create a bowtie
			//
			// NOTE: potentially scanning all edges here...couldn't we
			//  pick up eac/bc/ad/bd as we go? somehow?
			if ( bIsBoundaryEdge == false && Vertex_is_boundary(a) && Vertex_is_boundary(b) )
				return MeshResult.Failed_InvalidNeighbourhood;


			// 1) remove edge ab from vtx b
			// 2) find edges ad and ac, and tris tad, tac across those edges  (will use later)
			// 3) for other edges, replace a with b, and add that edge to b
			// 4) replace a with b in all triangles connected to a
			int ead = InvalidID, eac = InvalidID;
			int tad = InvalidID, tac = InvalidID;
			foreach (int eid in edges_a) {
				int o = Edge_other_v(eid, a);
				if (o == b) {
					if (edges_b.Remove(eid) != true )
						Debug_fail("remove case o == b");
				} else if (o == c) {
					eac = eid;
					if ( vertex_edges[c].Remove(eid) != true )
						Debug_fail("remove case o == c");
					tac = Edge_other_t(eid, t0);
				} else if (o == d) {
					ead = eid;
					if ( vertex_edges[d].Remove(eid) != true )
						Debug_fail("remove case o == c, step 1");
					tad = Edge_other_t(eid, t1);
				} else {
					if ( Replace_edge_vertex(eid, a, b) == -1 )
						Debug_fail("remove case else");
					edges_b.Add(eid);
				}

				// [TODO] perhaps we can already have unique tri list because of the manifold-nbrhood check we need to do...
				for (int j = 0; j < 2; ++j) {
					int t_j = edges[4*eid + 2 + j];
					if (t_j != InvalidID && t_j != t0 && t_j != t1) {
						if ( Tri_has_v(t_j, a) ) {
							if ( Replace_tri_vertex(t_j, a, b) == -1 )
								Debug_fail("remove last check");
							vertices_refcount.Increment(b);
							vertices_refcount.Decrement(a);
						}
					}
				}
			}

            int ebc = InvalidID, ebd = InvalidID;
			if (bIsBoundaryEdge == false) {

				// remove all edges from vtx a, then remove vtx a
				edges_a.Clear();
				Debug.Assert( vertices_refcount.RefCount(a) == 3 );		// in t0,t1, and initial ref
				vertices_refcount.Decrement( a, 3 );
				Debug.Assert( vertices_refcount.IsValid( a ) == false );

				// remove triangles T0 and T1, and update b/c/d refcounts
				triangles_refcount.Decrement( t0 );
				triangles_refcount.Decrement( t1 );
				vertices_refcount.Decrement( c );
				vertices_refcount.Decrement( d );
				vertices_refcount.Decrement( b, 2 );
				Debug.Assert( triangles_refcount.IsValid( t0 ) == false );
				Debug.Assert( triangles_refcount.IsValid( t1 ) == false );

				// remove edges ead, eab, eac
				edges_refcount.Decrement( ead );
				edges_refcount.Decrement( eab );
				edges_refcount.Decrement( eac );
				Debug.Assert( edges_refcount.IsValid( ead ) == false );
				Debug.Assert( edges_refcount.IsValid( eab ) == false );
				Debug.Assert( edges_refcount.IsValid( eac ) == false );

				// replace t0 and t1 in edges ebd and ebc that we kept
				ebd = Find_edge( b, d );
				ebc = Find_edge( b, c );

				if( Replace_edge_triangle(ebd, t1, tad ) == -1 )
					Debug_fail("isboundary=false branch, ebd replace triangle");

				if ( Replace_edge_triangle(ebc, t0, tac ) == -1 )
					Debug_fail("isboundary=false branch, ebc replace triangle");

				// update tri-edge-nbrs in tad and tac
				if (tad != InvalidID) {
					if ( Replace_triangle_edge(tad, ead, ebd ) == -1 )
						Debug_fail("isboundary=false branch, ebd replace triangle");
				}
				if (tac != InvalidID) {
					if ( Replace_triangle_edge(tac, eac, ebc ) == -1 )
						Debug_fail("isboundary=false branch, ebd replace triangle");
				}

			} else {

				//  this is basically same code as above, just not referencing t1/d

				// remove all edges from vtx a, then remove vtx a
				edges_a.Clear();
				Debug.Assert( vertices_refcount.RefCount( a ) == 2 );		// in t0 and initial ref
				vertices_refcount.Decrement( a, 2 );
				Debug.Assert( vertices_refcount.IsValid( a ) == false );

				// remove triangle T0 and update b/c refcounts
				triangles_refcount.Decrement( t0 );
				vertices_refcount.Decrement( c );
				vertices_refcount.Decrement( b );
				Debug.Assert( triangles_refcount.IsValid( t0 ) == false );

				// remove edges eab and eac
				edges_refcount.Decrement( eab );
				edges_refcount.Decrement( eac );
				Debug.Assert( edges_refcount.IsValid( eab ) == false );
				Debug.Assert( edges_refcount.IsValid( eac ) == false );

				// replace t0 in edge ebc that we kept
				ebc = Find_edge( b, c );
				if ( Replace_edge_triangle(ebc, t0, tac ) == -1 )
					Debug_fail("isboundary=false branch, ebc replace triangle");

				// update tri-edge-nbrs in tac
				if (tac != InvalidID) {
					if ( Replace_triangle_edge(tac, eac, ebc ) == -1 )
						Debug_fail("isboundary=true branch, ebd replace triangle");
				}
			}

			collapse.vKept = vKeep;
			collapse.vRemoved = vRemove;
			collapse.bIsBoundary = bIsBoundaryEdge;
            collapse.eCollapsed = eab;
            collapse.tRemoved0 = t0; collapse.tRemoved1 = t1;
            collapse.eRemoved0 = eac; collapse.eRemoved1 = ead;
            collapse.eKept0 = ebc; collapse.eKept1 = ebd;

			UpdateTimeStamp(true);
			return MeshResult.Ok;
		}













        // internal

        void Set_triangle(int tid, int v0, int v1, int v2)
        {
			int i = 3*tid;
            triangles[i] = v0;
            triangles[i + 1] = v1;
            triangles[i + 2] = v2;
        }
        void Set_triangle_edges(int tid, int e0, int e1, int e2)
        {
			int i = 3*tid;
            triangle_edges[i] = e0;
            triangle_edges[i + 1] = e1;
            triangle_edges[i + 2] = e2;
        }

        int Add_edge(int vA, int vB, int tA, int tB = InvalidID)
        {
            if (vB < vA) {
                int t = vB; vB = vA; vA = t;
            }
            int eid = edges_refcount.Allocate();
			int i = 4*eid;
            edges.Insert(vA, i);
            edges.Insert(vB, i + 1);
            edges.Insert(tA, i + 2);
            edges.Insert(tB, i + 3);

            vertex_edges[vA].Add(eid);
            vertex_edges[vB].Add(eid);
            return eid;
        }

		int Replace_tri_vertex(int tID, int vOld, int vNew) {
			int i = 3*tID;
			if ( triangles[i] == vOld ) { triangles[i] = vNew; return 0; }
			if ( triangles[i+1] == vOld ) { triangles[i+1] = vNew; return 1; }
			if ( triangles[i+2] == vOld ) { triangles[i+2] = vNew; return 2; }
			return -1;
		}

		int Add_triangle_only(int a, int b, int c, int e0, int e1, int e2) {
			int tid = triangles_refcount.Allocate();
			int i = 3*tid;
			triangles.Insert(c, i + 2);
			triangles.Insert(b, i + 1);
			triangles.Insert(a, i);
			triangle_edges.Insert(e2, i+2);
			triangle_edges.Insert(e1, i+1);
			triangle_edges.Insert(e0, i+0);	
			return tid;
		}






		void Set_edge_vertices(int eID, int a, int b) {
			int i = 4*eID;
			edges[i] = Math.Min(a,b);
			edges[i + 1] = Math.Max(a,b);
		}
		void Set_edge_triangles(int eID, int t0, int t1) {
			int i = 4*eID;
			edges[i + 2] = t0;
			edges[i + 3] = t1;
		}

		int Replace_edge_vertex(int eID, int vOld, int vNew) {
			int i = 4*eID;
			int a = edges[i], b = edges[i+1];
			if ( a == vOld ) {
				edges[i] = Math.Min(b, vNew);
				edges[i+1] = Math.Max(b, vNew);
				return 0;
			} else if ( b == vOld ) {
				edges[i] = Math.Min(a, vNew);
				edges[i+1] = Math.Max(a, vNew);
				return 1;				
			} else
				return -1;
		}


		int Replace_edge_triangle(int eID, int tOld, int tNew) {
			int i = 4*eID;
			int a = edges[i+2], b = edges[i+3];
			if ( a == tOld ) {
				if ( tNew == InvalidID ) {
					edges[i+2] = b;
					edges[i+3] = InvalidID;
				} else 
					edges[i+2] = tNew;
				return 0;
			} else if ( b == tOld ) {
				edges[i+3] = tNew;
				return 1;				
			} else
				return -1;
		}

		int Replace_triangle_edge(int tID, int eOld, int eNew) {
			int i = 3*tID;
			if ( triangle_edges[i] == eOld ) {
				triangle_edges[i] = eNew;
				return 0;
			} else if ( triangle_edges[i+1] == eOld ) {
				triangle_edges[i+1] = eNew;
				return 1;
			} else if ( triangle_edges[i+2] == eOld ) {
				triangle_edges[i+2] = eNew;
				return 2;
			} else
				return -1;
		}



    }
}
