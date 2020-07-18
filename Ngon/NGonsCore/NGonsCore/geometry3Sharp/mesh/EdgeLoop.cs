﻿using System;
using System.Diagnostics;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public class EdgeLoop
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;

        public int[] Vertices;
        public int[] Edges;

        public int[] BowtieVertices;        // this may not be initialized!


        public EdgeLoop(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            Mesh = mesh;
        }
        public EdgeLoop(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int[] vertices, int[] edges, bool bCopyArrays)
        {
            Mesh = mesh;
            if ( bCopyArrays ) {
                Vertices = new int[vertices.Length];
                Array.Copy(vertices, Vertices, Vertices.Length);
                Edges = new int[edges.Length];
                Array.Copy(edges, Edges, Edges.Length);
            } else {
                Vertices = vertices;
                Edges = edges;
            }
        }
        public EdgeLoop(EdgeLoop copy)
        {
            Mesh = copy.Mesh;
            Vertices = new int[copy.Vertices.Length];
            Array.Copy(copy.Vertices, Vertices, Vertices.Length);
            Edges = new int[copy.Edges.Length];
            Array.Copy(copy.Edges, Edges, Edges.Length);
            BowtieVertices = new int[copy.BowtieVertices.Length];
            Array.Copy(copy.BowtieVertices, BowtieVertices, BowtieVertices.Length);
        }


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


        public bool IsInternalLoop()
        {
            int NV = Vertices.Length;
            for (int i = 0; i < NV; ++i ) {
                int eid = Mesh.FindEdge(Vertices[i], Vertices[(i + 1) % NV]);
                Debug.Assert(eid != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID);
                if (Mesh.IsBoundaryEdge(eid))
                    return false;
            }
            return true;
        }


        public bool IsBoundaryLoop()
        {
            int NV = Vertices.Length;
            for (int i = 0; i < NV; ++i ) {
                int eid = Mesh.FindEdge(Vertices[i], Vertices[(i + 1) % NV]);
                Debug.Assert(eid != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID);
                if (Mesh.IsBoundaryEdge(eid) == false)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// find index of vertex vID in Vertices list, or -1 if not found
        /// </summary>
        public int FindVertexIndex(int vID)
        {
            int N = Vertices.Length;
            for (int i = 0; i < N; ++i) {
                if (Vertices[i] == vID)
                    return i;
            }
            return -1;
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


        // Check if Loop2 is the same set of positions on another mesh.
        // Does not require the indexing to be the same
        // Currently doesn't handle loop-reversal
        public bool IsSameLoop(EdgeLoop Loop2, bool bReverse2 = false, double tolerance = math.MathUtil.ZeroTolerance)
        {
            // find a duplicate starting vertex
            int N = Vertices.Length;
            int N2 = Loop2.Vertices.Length;
            if (N != N2)
                return false;

            NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh2 = Loop2.Mesh;

            int start_i = 0, start_j = -1;

            // try to find a unique same-vertex on each loop. Do not
            // use vertices that have duplicate positions.
            bool bFoundGoodStart = false;
            while ( !bFoundGoodStart && start_i < N ) {
                Vector3D start_v = Mesh.GetVertex(start_i);
                int count = Loop2.CountWithinTolerance(start_v, tolerance, out start_j);
                if (count == 1)
                    bFoundGoodStart = true;
                else
                    start_i++;
            }
            if (!bFoundGoodStart)
                return false;       // no within-tolerance duplicate vtx to start at

            for ( int ii = 0; ii < N; ++ii ) {
                int i = (start_i + ii) % N;
                int j = (bReverse2) ? 
                    math.MathUtil.WrapSignedIndex(start_j - ii, N2)
                    : (start_j + ii) % N2;
                Vector3D v = Mesh.GetVertex(Vertices[i]);
                Vector3D v2 = Mesh2.GetVertex(Loop2.Vertices[j]);
                if (v.Distance(v2) > tolerance)
                    return false;
            }

            return true;
        }



        /// <summary>
        /// stores vertices [starti, starti+1, ... starti+count-1] in span, and returns span, or null if invalid range
        /// </summary>
        public int[] GetVertexSpan(int starti, int count, int[] span, bool reverse = false)
        {
            int N = Vertices.Length;
            if (starti < 0 || starti >= N || count > N - 1)
                return null;
            if (reverse) {
                for (int k = 0; k < count; ++k)
                    span[count-k-1] = Vertices[(starti + k) % N];
            } else {
                for (int k = 0; k < count; ++k)
                    span[k] = Vertices[(starti + k) % N];
            }
            return span;
        }





        // utility function
        public static int[] VertexLoopToEdgeLoop(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int[] vertex_loop)
        {
            int NV = vertex_loop.Length;
            int[] edges = new int[NV];
            for ( int i = 0; i < NV; ++i ) {
                int v0 = vertex_loop[i];
                int v1 = vertex_loop[(i + 1) % NV];
                edges[i] = mesh.FindEdge(v0, v1);
            }
            return edges;
        }



    }
}
