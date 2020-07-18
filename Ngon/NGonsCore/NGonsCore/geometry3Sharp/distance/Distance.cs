using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.distance
{
    public static class Distance
    {

        public static Vector3F ClosestPointOnLine(Vector3F p0, Vector3F dir, Vector3F pt)
        {
            float t = (pt - p0).Dot(dir);
            return p0 + t * dir;
        }
        public static float ClosestPointOnLineT(Vector3F p0, Vector3F dir, Vector3F pt)
        {
            float t = (pt - p0).Dot(dir);
            return t;
        }
    }
}
