using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;

namespace NGonsCore.geometry3Sharp.queries
{
    public enum ValidationStatus
    {
        Ok,

        NotAVertex,

        NotBoundaryVertex,
        NotBoundaryEdge,

        VerticesNotConnectedByEdge,
        IncorrectLoopOrientation
    }


    public static class MeshValidation
    {

        public static ValidationStatus IsEdgeLoop(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, EdgeLoop loop)
        {
           int N = loop.Vertices.Length;
            for ( int i = 0; i < N; ++i ) {
                if ( ! mesh.IsVertex(loop.Vertices[i]) )
                    return ValidationStatus.NotAVertex;
            }
            for (int i = 0; i < N; ++i) {
                int a = loop.Vertices[i];
                int b = loop.Vertices[(i + 1) % N];

                int eid = mesh.FindEdge(a, b);
                if (eid == NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID)
                    return ValidationStatus.VerticesNotConnectedByEdge;
            }
            return ValidationStatus.Ok;
        }



        public static ValidationStatus IsBoundaryLoop(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, EdgeLoop loop)
        {
            int N = loop.Vertices.Length;

            for ( int i = 0; i < N; ++i ) {
                if ( ! mesh.Vertex_is_boundary(loop.Vertices[i]) )
                    return ValidationStatus.NotBoundaryVertex;
            }

            for ( int i = 0; i < N; ++i ) {
                int a = loop.Vertices[i];
                int b = loop.Vertices[(i + 1) % N];

                int eid = mesh.FindEdge(a, b);
                if (eid == NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID)
                    return ValidationStatus.VerticesNotConnectedByEdge;

                if (mesh.IsBoundaryEdge(eid) == false)
                    return ValidationStatus.NotBoundaryEdge;

                Index2i ev = mesh.GetOrientedBoundaryEdgeV(eid);
                if (!(ev.a == a && ev.b == b))
                    return ValidationStatus.IncorrectLoopOrientation;
            }

            return ValidationStatus.Ok;
        }


    }
}
