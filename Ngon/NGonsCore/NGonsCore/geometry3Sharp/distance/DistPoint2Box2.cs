using System;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.distance
{
    // ported from WildMagic 5's DistPoint2Box2
    // https://www.geometrictools.com/Downloads/Downloads.html

    public class DistPoint2Box2
    {
        Vector2D point;
        public Vector2D Point
        {
            get { return point; }
            set { point = value; DistanceSquared = -1.0; }
        }

        Box2d box;
        public Box2d Box
        {
            get { return box; }
            set { box = value; DistanceSquared = -1.0; }
        }

        public double DistanceSquared = -1.0;

        public Vector2D BoxClosest;


        public DistPoint2Box2(Vector2D PointIn, Box2d boxIn )
        {
            point = PointIn; box = boxIn;
        }

        public DistPoint2Box2 Compute()
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

            // Work in the box's coordinate system.
            Vector2D diff = point - box.Center;

            // Compute squared distance and closest point on box.
            double sqrDistance = (double)0;
            double delta;
            Vector2D closest = Vector2D.Zero;
            int i;
            for (i = 0; i < 2; ++i) {
                closest[i] = diff.Dot(box.Axis(i));
                if (closest[i] < -box.Extent[i]) {
                    delta = closest[i] + box.Extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = -box.Extent[i];
                } else if (closest[i] > box.Extent[i]) {
                    delta = closest[i] - box.Extent[i];
                    sqrDistance += delta * delta;
                    closest[i] = box.Extent[i];
                }
            }

            BoxClosest = box.Center;
            for (i = 0; i < 2; ++i) {
                BoxClosest += closest[i] * box.Axis(i);
            }

            DistanceSquared = sqrDistance;
            return sqrDistance;
        }
    }
}
