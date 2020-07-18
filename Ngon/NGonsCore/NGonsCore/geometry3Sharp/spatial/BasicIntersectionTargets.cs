using System;
using NGonsCore.geometry3Sharp.intersection;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.spatial
{

    public class TransformedIntersectionTarget : IIntersectionTarget
    {
        DMeshIntersectionTarget BaseTarget = null;

        public Func<Ray3d, Ray3d> MapToBaseF = null;
        public Func<Vector3D, Vector3D> MapFromBasePosF = null;
        public Func<Vector3D, Vector3D> MapFromBaseNormalF = null;


        public bool HasNormal { get { return BaseTarget.HasNormal; } }
        public bool RayIntersect(Ray3d ray, out Vector3D vHit, out Vector3D vHitNormal)
        {
            Ray3d baseRay = MapToBaseF(ray);
            if ( BaseTarget.RayIntersect(baseRay, out vHit, out vHitNormal) ) {
                vHit = MapFromBasePosF(vHit);
                vHitNormal = MapFromBasePosF(vHitNormal);
                return true;
            }
            return false;
        }
    }





    public class DMeshIntersectionTarget : IIntersectionTarget
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh { get; set; }
        public ISpatial Spatial { get; set; }
        public bool UseFaceNormal = true;

        public DMeshIntersectionTarget() { }
        public DMeshIntersectionTarget(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, ISpatial spatial)
        {
            Mesh = mesh;
            Spatial = spatial;
        }


        public bool HasNormal { get { return true; } }
        public bool RayIntersect(Ray3d ray, out Vector3D vHit, out Vector3D vHitNormal)
        {
            vHit = Vector3D.Zero;
            vHitNormal = Vector3D.AxisX;
            int tHitID = Spatial.FindNearestHitTriangle(ray);
            if (tHitID == NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID)
                return false;
            IntrRay3Triangle3 t = MeshQueries.TriangleIntersection(Mesh, tHitID, ray);
            vHit = ray.PointAt(t.RayParameter);
            if ( UseFaceNormal == false && Mesh.HasVertexNormals)
                vHitNormal = Mesh.GetTriBaryNormal(tHitID, t.TriangleBaryCoords.x, t.TriangleBaryCoords.y, t.TriangleBaryCoords.z);
            else
                vHitNormal = Mesh.GetTriNormal(tHitID);
            return true;
        }

    }
}
