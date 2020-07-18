﻿using System;
using System.Diagnostics;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
	/// <summary>
	/// An EdgeSpan is a continous set of edges in a Mesh that is *not* closed
	/// (that would be an EdgeLoop)
	/// </summary>
    public class EdgeSpan
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
        public EdgeSpan(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            Mesh = mesh;
        }

        public int[] Vertices;
        public int[] Edges;

        public int[] BowtieVertices;


        public int VertexCount {
            get { return Vertices.Length; }
        }
        public int EdgeCount {
            get { return Edges.Length; }
        }

        public Vector3D GetVertex(int i) {
            return Mesh.GetVertex(Vertices[i]);
        }


        public AxisAlignedBox3d GetBounds()
        {
            AxisAlignedBox3d box = AxisAlignedBox3d.Empty;
            for (int i = 0; i < Vertices.Length; ++i)
                box.Contain(Mesh.GetVertex(Vertices[i]));
            return box;
        }


        public bool IsInternalSpan()
        {
            int NV = Vertices.Length;
            for (int i = 0; i < NV-1; ++i ) {
                int eid = Mesh.FindEdge(Vertices[i], Vertices[i + 1]);
                Debug.Assert(eid != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID);
                if (Mesh.IsBoundaryEdge(eid))
                    return false;
            }
            return true;
        }


        public bool IsBoundarySpan()
        {
            int NV = Vertices.Length;
            for (int i = 0; i < NV-1; ++i ) {
                int eid = Mesh.FindEdge(Vertices[i], Vertices[i + 1]);
                Debug.Assert(eid != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID);
                if (Mesh.IsBoundaryEdge(eid) == false)
                    return false;
            }
            return true;
        }


        public int FindNearestVertex(Vector3D v)
        {
            int iNear = -1;
            double fNearSqr = double.MaxValue;
            int N = Vertices.Length;
            for ( int i = 0; i < N; ++i ) {
                Vector3D lv = Mesh.GetVertex(Vertices[i]);
                double d2 = v.DistanceSquared(lv);
                if ( d2 < fNearSqr ) {
                    fNearSqr = d2;
                    iNear = i;
                }
            }
            return iNear;
        }

        // count # of vertices in loop that are within tol of v
        // final param returns last encountered index within tolerance, or -1 if return is 0
        public int CountWithinTolerance(Vector3D v, double tol, out int last_in_tol)
        {
            last_in_tol = -1;
            int count = 0;
            int N = Vertices.Length;
            for (int i = 0; i < N; ++i) {
                Vector3D lv = Mesh.GetVertex(Vertices[i]);
                if (v.Distance(lv) < tol) {
                    count++;
                    last_in_tol = i;
                }
            }
            return count;
        }


        // Check if Spanw is the same set of positions on another mesh.
        // Does not require the indexing to be the same
        public bool IsSameSpan(EdgeSpan Spanw, bool bReverse2 = false, double tolerance = math.MathUtil.ZeroTolerance)
        {
			// [RMS] this is much easier than for a loop, because it has to have 
			//   same endpoints. But don't have time right now.
			throw new NotImplementedException("todo!");
        }





        // utility function
        public static int[] VerticesToEdges(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int[] vertex_span)
        {
            int NV = vertex_span.Length;
            int[] edges = new int[NV-1];
            for ( int i = 0; i < NV-1; ++i ) {
                int v0 = vertex_span[i];
                int v1 = vertex_span[(i + 1)];
                edges[i] = mesh.FindEdge(v0, v1);
            }
            return edges;
        }



    }
}
