﻿using System;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.queries
{
    public class RayIntersection
    {
        private RayIntersection()
        {

        }

        // basic ray-sphere intersection
        public static bool Sphere(Vector3F vOrigin, Vector3F vDirection, Vector3F vCenter, float fRadius, out float fRayT)
        {
            bool bHit = SphereSigned(vOrigin, vDirection, vCenter, fRadius, out fRayT);
            fRayT = Math.Abs(fRayT);
            return bHit;
        }

        public static bool SphereSigned(Vector3F vOrigin, Vector3F vDirection, Vector3F vCenter, float fRadius, out float fRayT)
        {
            fRayT = 0.0f;
            Vector3F m = vOrigin - vCenter;
            float b = m.Dot(vDirection);
            float c = m.Dot(m) - fRadius * fRadius;

            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
            if (c > 0.0f && b > 0.0f)
                return false;
            float discr = b * b - c;

            // A negative discriminant corresponds to ray missing sphere 
            if (discr < 0.0f)
                return false;

            // Ray now found to intersect sphere, compute smallest t value of intersection
            fRayT = -b - (float)Math.Sqrt(discr);

            return true;
        }



        public static bool SphereSigned(Vector3D vOrigin, Vector3D vDirection, Vector3D vCenter, double fRadius, out double fRayT)
        {
            fRayT = 0.0;
            Vector3D m = vOrigin - vCenter;
            double b = m.Dot(vDirection);
            double c = m.Dot(m) - fRadius * fRadius;

            // Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
            if (c > 0.0f && b > 0.0f)
                return false;
            double discr = b * b - c;
            // A negative discriminant corresponds to ray missing sphere 
            if (discr < 0.0)
                return false;
            // Ray now found to intersect sphere, compute smallest t value of intersection
            fRayT = -b - Math.Sqrt(discr);
            return true;
        }


        public static bool InfiniteCylinder(Vector3F vOrigin, Vector3F vDirection, Vector3F vCylOrigin, Vector3F vCylAxis, float fRadius, out float fRayT)
        {
            bool bHit = InfiniteCylinderSigned(vOrigin, vDirection, vCylOrigin, vCylAxis, fRadius, out fRayT);
            fRayT = Math.Abs(fRayT);
            return bHit;
        }
        public static bool InfiniteCylinderSigned(Vector3F vOrigin, Vector3F vDirection, Vector3F vCylOrigin, Vector3F vCylAxis, float fRadius, out float fRayT)
        {
            // [RMS] ugh this is shit...not even sure it works in general, but works for a ray inside cylinder

            fRayT = 0.0f;


            Vector3F AB = vCylAxis;
            Vector3F AO = (vOrigin - vCylOrigin);
            if (AO.DistanceSquared(AO.Dot(AB) * AB) > fRadius * fRadius)
                return false;

            Vector3F AOxAB = AO.Cross(AB);
            Vector3F VxAB = vDirection.Cross(AB);
            float ab2 = AB.Dot(AB);
            float a = VxAB.Dot(VxAB);
            float b = 2 * VxAB.Dot(AOxAB);
            float c = AOxAB.Dot(AOxAB) - (fRadius * fRadius * ab2);

            double discrim = b * b - 4 * a * c;
            if (discrim <= 0)
                return false;
            discrim = Math.Sqrt(discrim);
            fRayT = (-b - (float)discrim) / (2 * a);
            //float t1 = (-b + (float)discrim) / (2 * a);

            return true;
        }

    }
}
