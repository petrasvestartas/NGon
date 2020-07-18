using System;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.shapes3;

namespace NGonsCore.geometry3Sharp.distance
{
    // ported from WildMagic 5
    // https://www.geometrictools.com/Downloads/Downloads.html

    public class DistPoint3Circle3
    {
        Vector3D point;
        public Vector3D Point
        {
            get { return point; }
            set { point = value; DistanceSquared = -1.0; }
        }

        Circle3d circle;
        public Circle3d Circle
        {
            get { return circle; }
            set { circle = value; DistanceSquared = -1.0; }
        }

        public double DistanceSquared = -1.0;

        public Vector3D CircleClosest;
        public bool AllCirclePointsEquidistant;


        public DistPoint3Circle3(Vector3D PointIn, Circle3d circleIn )
        {
            point = PointIn; circle = circleIn;
        }

        public DistPoint3Circle3 Compute()
        {
            GetSquared();
            return this;
        }

        public double Get()
        {
            return Math.Sqrt(GetSquared());
        }


        public double GetSquared()
        {
            if (DistanceSquared >= 0)
                return DistanceSquared;

            // Projection of P-C onto plane is Q-C = P-C - Dot(N,P-C)*N.
            Vector3D PmC = point - circle.Center;
            Vector3D QmC = PmC - circle.Normal.Dot(PmC) * circle.Normal;
            double lengthQmC = QmC.Length;
            if (lengthQmC > math.MathUtil.Epsilon) {
                CircleClosest = circle.Center + circle.Radius * QmC / lengthQmC;
                AllCirclePointsEquidistant = false;
            } else {
                // All circle points are equidistant from P.  Return one of them.
                CircleClosest = circle.Center + circle.Radius * circle.PlaneX;
                AllCirclePointsEquidistant = true;
            }

            Vector3D diff = point - CircleClosest;
            double sqrDistance = diff.Dot(diff);

            // Account for numerical round-off error.
            if (sqrDistance < 0) {
                sqrDistance = 0;
            }
            DistanceSquared = sqrDistance;
            return sqrDistance;
        }
    }
}
