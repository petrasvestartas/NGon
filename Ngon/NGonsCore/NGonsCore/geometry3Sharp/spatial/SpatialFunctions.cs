using NGonsCore.geometry3Sharp.distance;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.spatial
{

    // collection of utility classes
    public static class SpatialFunctions
    {

        // various offset-surface functions, in class so the compute functions 
        // can be passed to other functions
        public class NormalOffset
        {
            public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
            public ISpatial Spatial;

            public double Distance = 0.01;
            public bool UseFaceNormal = true;

            public Vector3D FindNearestAndOffset(Vector3D pos)
            {
                int tNearestID = Spatial.FindNearestTriangle(pos);
                DistPoint3Triangle3 q = MeshQueries.TriangleDistance(Mesh, tNearestID, pos);
                Vector3D vHitNormal = 
                    (UseFaceNormal == false && Mesh.HasVertexNormals) ?
                        Mesh.GetTriBaryNormal(tNearestID, q.TriangleBaryCoords.x, q.TriangleBaryCoords.y, q.TriangleBaryCoords.z) 
                        : Mesh.GetTriNormal(tNearestID);
                return q.TriangleClosest + Distance * vHitNormal;
            }

        }


    }



}
