﻿using System;
using System.Collections.Generic;
using NGonsCore.geometry3Sharp.core;
using DMesh3 = NGonsCore.geometry3Sharp.mesh.DMesh3;

namespace NGonsCore.geometry3Sharp.math
{
    public class IndexUtil
    {
        // test if [a0,a1] and [b0,b1] are the same pair, ignoring order
        public static bool same_pair_unordered(int a0, int a1, int b0, int b1)
        {
            return (a0 == b0) ?
                (a1 == b1) :
                (a0 == b1 && a1 == b0);
        }

        // return index of a in tri_verts, or InvalidID if not found
        public static int find_tri_index(int a, int[] tri_verts)
        {
            if (tri_verts[0] == a) return 0;
            if (tri_verts[1] == a) return 1;
            if (tri_verts[2] == a) return 2;
            return DMesh3.InvalidID;
        }
		public static int find_tri_index(int a, Index3i tri_verts)
		{
			if (tri_verts.a == a) return 0;
			if (tri_verts.b == a) return 1;
			if (tri_verts.c == a) return 2;
			return DMesh3.InvalidID;
		}

        // return index of a in tri_verts, or InvalidID if not found
        public static int find_edge_index_in_tri(int a, int b, int[] tri_verts )
        {
            if (same_pair_unordered(a, b, tri_verts[0], tri_verts[1])) return 0;
            if (same_pair_unordered(a, b, tri_verts[1], tri_verts[2])) return 1;
            if (same_pair_unordered(a, b, tri_verts[2], tri_verts[0])) return 2;
            return DMesh3.InvalidID;
        }
        public static int find_edge_index_in_tri(int a, int b, ref Index3i tri_verts )
        {
            if (same_pair_unordered(a, b, tri_verts.a, tri_verts.b)) return 0;
            if (same_pair_unordered(a, b, tri_verts.b, tri_verts.c)) return 1;
            if (same_pair_unordered(a, b, tri_verts.c, tri_verts.a)) return 2;
            return DMesh3.InvalidID;
        }

        // find sequence [a,b] in tri_verts (mod3) and return index of a, or InvalidID if not found
        public static int find_tri_ordered_edge(int a, int b, int[] tri_verts)
        {
            if (tri_verts[0] == a && tri_verts[1] == b) return 0;
            if (tri_verts[1] == a && tri_verts[2] == b) return 1;
            if (tri_verts[2] == a && tri_verts[0] == b) return 2;
            return DMesh3.InvalidID;
        }

        // find sequence [a,b] in tri_verts (mod3) then return the third **value**, or InvalidID if not found
        public static int find_tri_other_vtx(int a, int b, int[] tri_verts)
        {
            for (int j = 0; j < 3; ++j) {
                if (same_pair_unordered(a, b, tri_verts[j], tri_verts[(j + 1) % 3]))
                    return tri_verts[(j + 2) % 3];
            }
            return DMesh3.InvalidID;
        }
		public static int find_tri_other_vtx(int a, int b, Index3i tri_verts)
		{
			for (int j = 0; j < 3; ++j) {
				if (same_pair_unordered(a, b, tri_verts[j], tri_verts[(j + 1) % 3]))
					return tri_verts[(j + 2) % 3];
			}
			return DMesh3.InvalidID;
		}
		public static int find_tri_other_vtx(int a, int b, DVector<int> tri_array, int ti)
		{
			int i = 3*ti;
			for (int j = 0; j < 3; ++j) {
				if (same_pair_unordered(a, b, tri_array[i+j], tri_array[i + ((j + 1) % 3)]))
					return tri_array[i + ((j + 2) % 3)];
			}
			return DMesh3.InvalidID;
		}

        // find sequence [a,b] in tri_verts (mod3) then return the third **index**, or InvalidID if not found
        public static int find_tri_other_index(int a, int b, int[] tri_verts)
        {
            for (int j = 0; j < 3; ++j) {
                if (same_pair_unordered(a, b, tri_verts[j], tri_verts[(j + 1) % 3]))
                    return (j + 2) % 3;
            }
            return DMesh3.InvalidID;
        }

        // set [a,b] to order found in tri_verts (mod3), and return third **value**, or InvalidID if not found
        public static int orient_tri_edge_and_find_other_vtx(ref int a, ref int b, int[] tri_verts)
        {
            for (int j = 0; j < 3; ++j) {
                if (same_pair_unordered(a, b, tri_verts[j], tri_verts[(j + 1) % 3])) {
                    a = tri_verts[j];
                    b = tri_verts[(j + 1) % 3];
                    return tri_verts[(j + 2) % 3];
                }
            }
            return DMesh3.InvalidID;
        }


        public static bool is_same_triangle(int a, int b, int c, ref Index3i tri)
        {
            if (tri.a == a)         return same_pair_unordered(tri.b, tri.c, b, c);
            else if ( tri.b == a )  return same_pair_unordered(tri.a, tri.c, b, c);
            else if ( tri.c == a )  return same_pair_unordered(tri.a, tri.b, b, c);
            return false;
        }


        public static void cycle_indices_minfirst(ref Index3i tri)
        {
            if (tri.b < tri.a && tri.b < tri.c) {
                int a = tri.a, b = tri.b, c = tri.c;
                tri.a = b;
                tri.b = c;
                tri.c = a;
            } else if (tri.c < tri.a && tri.c < tri.b) {
                int a = tri.a, b = tri.b, c = tri.c;
                tri.a = c;
                tri.b = a;
                tri.c = b;
            }
        }


        public static void sort_indices(ref Index3i tri)
        {
            // possibly this can be re-ordered to have fewer tests? ...
            if ( tri.a < tri.b && tri.a < tri.c ) {
                if (tri.b > tri.c) {
                    int b = tri.b; tri.b = tri.c; tri.c = b;
                }
            } else if ( tri.b < tri.a && tri.b < tri.c ) {
                if ( tri.a < tri.c ) {
                    int b = tri.b; tri.b = tri.a; tri.a = b;
                } else {
                    int a = tri.a, b = tri.b, c = tri.c;
                    tri.a = b; tri.b = c; tri.c = a;
                }
            } else if ( tri.c < tri.a && tri.c < tri.b ) {
                if ( tri.b < tri.a ) {
                    int c = tri.c; tri.c = tri.a; tri.a = c;
                } else {
                    int a = tri.a, b = tri.b, c = tri.c;
                    tri.a = c; tri.b = a; tri.c = b;
                }
            }
        }




        /// <summary>
        /// Filter out invalid entries in indices[] list. Will return indices itself if 
        /// none invalid, and bForceCopy == false
        /// </summary>
        public static int[] FilterValid(int[] indices, Func<int, bool> FilterF, bool bForceCopy = false )
        {
            int nValid = 0;
            for ( int i = 0; i < indices.Length; ++i ) {
                if (FilterF(indices[i]))
                    ++nValid;
            }
            if (nValid == indices.Length && bForceCopy == false)
                return indices;
            int[] valid = new int[nValid];
            int vi = 0;
            for ( int i = 0; i < indices.Length; ++i ) {
                if (FilterF(indices[i]))
                    valid[vi++] = indices[i];
            }
            return valid;
        }



        /// <summary>
        /// return trune if CheckF returns true for all members of indices list
        /// </summary>
        public static bool IndicesCheck(int[] indices, Func<int, bool> CheckF)
        {
            for ( int i = 0; i < indices.Length; ++i ) {
                if (CheckF(indices[i]) == false)
                    return false;
            }
            return true;
        }



        /// <summary>
        /// Apply map to indices
        /// </summary>
        public static void Apply(List<int> indices, IIndexMap map)
        {
            int N = indices.Count;
            for (int i = 0; i < N; ++i)
                indices[i] = map[indices[i]];
        }

        public static void Apply(int[] indices, IIndexMap map)
        {
            int N = indices.Length;
            for (int i = 0; i < N; ++i)
                indices[i] = map[indices[i]];
        }

    }





    public static class gIndices
    {
        // Corner vertices of box faces  -  see Box.Corner for points associated w/ indexing
        // Note that 
        public static readonly int[,] BoxFaces = new int[6, 4] {
            { 1, 0, 3, 2 },     // back, -z
            { 4, 5, 6, 7 },     // front, +z
            { 0, 4, 7, 3 },     // left, -x
            { 5, 1, 2, 6 },     // right, +x,
            { 0, 1, 5, 4 },     // bottom, -y
            { 7, 6, 2, 3 }      // top, +y
        };

        // Box Face normal. Use Sign(BoxFaceNormals[i]) * Box.Axis( Abs(BoxFaceNormals[i])-1 )
        //  (+1 is so we can have a sign on X)
        public static readonly int[] BoxFaceNormals = new int[6] { -3, 3, -1, 1, -2, 2 }; 


        // integer indices offsets in x/y/z directions, corresponds w/ BoxFaces directions
        public static readonly Vector3i[] GridOffsets6 = new Vector3i[] {
            new Vector3i( 0, 0,-1), new Vector3i( 0, 0, 1),
            new Vector3i(-1, 0, 0), new Vector3i( 1, 0, 0),
            new Vector3i( 0,-1, 0), new Vector3i( 0, 1, 0)
        };

    }



}
