using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public static class CylinderUtil {


        public static bool[] IsPointInsideCylinder(Cylinder cylinder, Point3d[] points, double tolerance) {
            bool[] pointStatus = new bool[points.Length];
            //pointStatus[i] = true => points[i] is inside of cylinder
            Vector3d N = cylinder.Axis;
            N.Unitize(); // N should be already unit size vector, but for any case...
            Point3d C1 = cylinder.Center;
            //we find base of cylinder = circle 
            Circle baseCircle = cylinder.CircleAt(cylinder.Height1);
            double radiusWithTolerance = baseCircle.Radius + tolerance;
            double SquareRadiusWithTolerance = radiusWithTolerance * radiusWithTolerance;

            // Any point T(T.X; T.Y; T.Z) on cylinder.Axis has parametrical formula:
            //   T = C1+ t * N  where t is parameter (t is real number)
            // In that case coordiantes of T point can be writen as: 
            // (1)
            //       T.X = C1.X + t*N.X
            //       T.Y = C1.Y + t*N.Y    
            //       T.Z = C1.Z + t*N.Z

            // So, for point C1 parameter is t_C1 = 0
            double t_C1 = 0;
            // and for point C2 parameter is t_C2 = cylinder.TotalHeight,  Point3d C2 = C1 + t_C2 * N
            double t_C2 = cylinder.TotalHeight;

            // Let P(P.X; P.Y; P.Z) be any point in space
            //  and T(T.X; T.Y; T.Z) be projection of point P to axis, so T= C1 + t * N for one and just one "t".
            // Because T is projection of P on axis we know that vector P-T is orthogonal to axis,
            //  so it must be (P-T)*N = 0  (DOT PRODUCT of two orthogonal vectors is 0)
            // (2)
            //      (P-T) * N =  0   =>  we will use algebraic definition of dot product to calcualte it: 
            //        ( P.X -T.X ) * N.X  +  (P.Y - T.Y) * N.Y  +  (P.Z - P.Z) * N.Z = 0
            //   and if we substitute T.X, T.Y and T.Z in previous formula with presentation from (1) and do all calculus we get
            //      t = (N.X * (P.X - C1.X) + N.Y * (P.Y - C1.Y) + N.Z * (P.Z - C1.Z)) / (N.X*N.X + N.Y*N.Y + N.Z*N.Z)
            //   becasue N is unit vector value of (N.X*N.X + N.Y*N.Y + N.Z*N.Z) = N.SquareLength = 1
            //   so our parameter t for point T becomes:
            //  (3)
            //      t = (N.X * (P.X - C1.X) + N.Y * (P.Y - C1.Y) + N.Z * (P.Z - C1.Z))
            //
            //  And now we can use formula (1) to calucalte T.X, T.Y and T.Z  if we need it
            //
            //  So now we can calculate square distance of point P to point T by:
            //  (4)
            //      SquareDistancePT = ( P.X -T.X ) * ( P.X -T.X )  +  (P.Y - T.Y) * (P.Y - T.Y)  +  (P.Z - P.Z) * (P.Z - P.Z)
            //  
            //  Now we have everything we need to chcek if point P is inside cylinder.
            //  First condition: if distance between point P and axis is larger than Cylinder radius than point IS NOT inside cylinder,
            //                   this statement is equivalent for square distances so we can write it down as
            //      if (SquareDistancePT> SquareRadiusWithTolerance) { //point is not inside cylinder}
            //
            //   In case that SquareDistancePT<= SquareRadiusWithTolerance  point may be inside cylinder so
            //      we have to check if point T (projection of point P to axis) is inside cylinder.
            //   It is obvious that point T for which we have determined parameter t by (3) IS NOT inside cylinder if:
            //      if (t < t_C1 || t > t_C2) {//point is not inside cylinder} 

            //  Before we use parameters t_C1 and T_C2 in previuos formula we should add tolerance to them as follows:
            //     t_C1 = t_C1 - tolerance
            //     t_C2 = t_C2 + tolerance
            t_C1 = t_C1 - tolerance;
            t_C2 = t_C2 + tolerance;
            // So we have everything to calculate if an arbitrary point P is inside cylinder
            for (int i = 0; i < points.Length; i++) {
                double t = N.X * (points[i].X - C1.X) + N.Y * (points[i].Y - C1.Y) + N.Z * (points[i].Z - C1.Z);
                double TX = C1.X + t * N.X;
                double TY = C1.Y + t * N.Y;
                double TZ = C1.Z + t * N.Z;
                var SquareDistancePT = (points[i].X - TX) * (points[i].X - TX) +
                                       (points[i].Y - TY) * (points[i].Y - TY) +
                                       (points[i].Z - TZ) * (points[i].Z - TZ);

                if (t < t_C1 || t > t_C2 || SquareDistancePT > SquareRadiusWithTolerance) {
                    pointStatus[i] = false; //point not inside cylinder
                } else {
                    pointStatus[i] = true;
                }
            }
            //
            return pointStatus;
        }



        public static bool[] IsPointInsideCylinder02(Cylinder cylinder, Point3d[] points, double tolerance) {
            bool[] pointStatus = new bool[points.Length];
            //pointStatus[i] = true => points[i] is inside of cylinder
            Vector3d N = cylinder.Axis;
            N.Unitize(); // N should be already unit size vector, but for any case...
            Point3d C1 = cylinder.Center;
            //we find base of cylinder = circle 
            Circle baseCircle = cylinder.CircleAt(cylinder.Height1);
            double radiusWithTolerance = baseCircle.Radius + tolerance;
            double SquareRadiusWithTolerance = radiusWithTolerance * radiusWithTolerance;

            double t_C1 = 0;
            double t_C2 = cylinder.TotalHeight;
            t_C1 = t_C1 - tolerance;
            t_C2 = t_C2 + tolerance;

            Parallel.For(0, points.Length, i =>
            {
                double t = N.X * (points[i].X - C1.X) + N.Y * (points[i].Y - C1.Y) + N.Z * (points[i].Z - C1.Z);
                double TX = C1.X + t * N.X;
                double TY = C1.Y + t * N.Y;
                double TZ = C1.Z + t * N.Z;
                var SquareDistancePT = (points[i].X - TX) * (points[i].X - TX) +
                        (points[i].Y - TY) * (points[i].Y - TY) +
                        (points[i].Z - TZ) * (points[i].Z - TZ);
                if (t < t_C1 || t > t_C2 || SquareDistancePT > SquareRadiusWithTolerance) {
                    pointStatus[i] = false; //point not inside cylinder
                } else {
                    pointStatus[i] = true;
                }
            });
            //
            return pointStatus;
        }




    }
}
