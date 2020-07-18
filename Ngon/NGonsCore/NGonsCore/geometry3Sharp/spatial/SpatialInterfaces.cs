using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.spatial
{
    public interface ISpatial
    {
        bool SupportsNearestTriangle { get; }
        int FindNearestTriangle(Vector3D p, double fMaxDist = double.MaxValue);

        bool SupportsTriangleRayIntersection{ get; }
        int FindNearestHitTriangle(Ray3d ray, double fMaxDist = double.MaxValue);

        bool SupportsPointContainment { get; }
        bool IsInside(Vector3D p);
    }


    public interface IProjectionTarget
    {
        Vector3D Project(Vector3D vPoint, int identifier = -1);
    }

    public interface IIntersectionTarget
    {
        bool HasNormal { get; }
        bool RayIntersect(Ray3d ray, out Vector3D vHit, out Vector3D vHitNormal);
    }

}
