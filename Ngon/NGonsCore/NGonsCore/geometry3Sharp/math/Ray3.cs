

#if G3_USING_UNITY
using UnityEngine;
#endif

namespace NGonsCore.geometry3Sharp.math
{
    public struct Ray3d
    {
        public Vector3D Origin;
        public Vector3D Direction;

        public Ray3d(Vector3D origin, Vector3D direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        public Ray3d(Vector3F origin, Vector3F direction)
        {
            this.Origin = origin;
            this.Direction = direction;
            this.Direction.Normalize();     // float cast may not be normalized in double, is trouble in algorithms!
        }

        // parameter is distance along ray
        public Vector3D PointAt(double d) {
            return Origin + d * Direction;
        }


        public double Project(Vector3D p)
        {
            return (p - Origin).Dot(Direction);
        }

        public double DistanceSquared(Vector3D p)
        {
            double t = (p - Origin).Dot(Direction);
            if (t < 0) {
                return Origin.DistanceSquared(p);
            } else {
                Vector3D proj = Origin + t * Direction;
                return (proj - p).LengthSquared;
            }
        }

        public Vector3D ClosestPoint(Vector3D p)
        {
            double t = (p - Origin).Dot(Direction);
            if (t < 0) {
                return Origin;
            } else {
                return Origin + t * Direction;
            }
        }


        // conversion operators
        public static implicit operator Ray3d(Ray3f v)
        {
            return new Ray3d(v.Origin, ((Vector3D)v.Direction).Normalized );
        }
        public static explicit operator Ray3f(Ray3d v)
        {
            return new Ray3f((Vector3F)v.Origin, ((Vector3F)v.Direction).Normalized );
        }


#if G3_USING_UNITY
        public static implicit operator Ray3d(UnityEngine.Ray r)
        {
            return new Ray3d(r.origin, ((Vector3d)r.direction).Normalized);
        }
        public static explicit operator Ray(Ray3d r)
        {
            return new Ray((Vector3)r.Origin, ((Vector3)r.Direction).normalized);
        }
#endif

    }



    public struct Ray3f
    {
        public Vector3F Origin;
        public Vector3F Direction;

        public Ray3f(Vector3F origin, Vector3F direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        // parameter is distance along ray
        public Vector3F PointAt(float d)
        {
            return Origin + d * Direction;
        }

        public float Project(Vector3F p)
        {
            return (p - Origin).Dot(Direction);
        }

        public float DistanceSquared(Vector3F p)
        {
            float t = (p - Origin).Dot(Direction);
            Vector3F proj = Origin + t * Direction;
            return (proj - p).LengthSquared;
        }


#if G3_USING_UNITY
        public static implicit operator Ray3f(UnityEngine.Ray r)
        {
            return new Ray3f(r.origin, r.direction);
        }
        public static implicit operator Ray(Ray3f r)
        {
            return new Ray(r.Origin, r.Direction);
        }
#endif
    }
}
