using System;
using System.Collections.Generic;

namespace NGonsCore.geometry3Sharp.mesh
{
    public static class MeshIterators
    {

        public static IEnumerable<int> FilteredVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, Func<NGonsCore.geometry3Sharp.mesh.DMesh3, int, bool> FilterF )
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (FilterF(mesh, i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> FilteredEdges(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, Func<NGonsCore.geometry3Sharp.mesh.DMesh3, int, bool> FilterF )
        {
            int N = mesh.MaxEdgeID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsEdge(i) ) {
                    if (FilterF(mesh, i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> FilteredTriangles(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, Func<NGonsCore.geometry3Sharp.mesh.DMesh3, int, bool> FilterF )
        {
            int N = mesh.MaxTriangleID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsTriangle(i) ) {
                    if (FilterF(mesh, i))
                        yield return i;
                }
            }
        }



        public static IEnumerable<int> BoundaryVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (mesh.IsBoundaryVertex(i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> InteriorVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (mesh.IsBoundaryVertex(i) == false)
                        yield return i;
                }
            }
        }



        public static IEnumerable<int> GroupBoundaryVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (mesh.IsGroupBoundaryVertex(i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> GroupJunctionVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (mesh.IsGroupJunctionVertex(i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> BoundaryEdges(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxEdgeID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsEdge(i) ) {
                    if (mesh.IsBoundaryEdge(i))
                        yield return i;
                }
            }
        }


        public static IEnumerable<int> GroupBoundaryEdges(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxEdgeID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsEdge(i) ) {
                    if (mesh.IsGroupBoundaryEdge(i))
                        yield return i;
                }
            }
        }




        public static IEnumerable<int> BowtieVertices(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            int N = mesh.MaxVertexID;
            for ( int i = 0; i < N; ++i ) {
                if ( mesh.IsVertex(i) ) {
                    if (mesh.IsBowtieVertex(i))
                        yield return i;
                }
            }
        }



    }
}
