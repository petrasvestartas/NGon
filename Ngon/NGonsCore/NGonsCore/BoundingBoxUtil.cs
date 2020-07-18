using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore {
    public static class BoundingBoxUtil {

        public static bool BoxBox(BoundingBox a0, BoundingBox b0, double scale = 0.01) {
            BoundingBox b = b0;
            b.Inflate(scale);
            BoundingBox a = a0;
            a.Inflate(scale);

            if (a.Max.X < b.Min.X)
                return false;
            if (a.Min.X > b.Max.X)
                return false;
            if (a.Max.Y < b.Min.Y)
                return false;
            if (a.Min.Y > b.Max.Y)
                return false;
            if (a.Max.Z < b.Min.Z)
                return false;
            if (a.Min.Z > b.Max.Z)
                return false;
            return true;
        }


        public static Box BoundingBox(this IEnumerable<Line> lines, Plane plane, bool midPoints = true) {

            List<Point3d> points = new List<Point3d>(lines.Count()*2);

            if(midPoints)
                foreach (Line l in lines) {
                    points.Add(l.PointAt(0.5));
                }
            else
                foreach (Line l in lines) {
                    points.Add(l.From);
                    points.Add(l.To);
                }


            if(plane != Plane.Unset) { 

                return GetBoundingBoxAligned(points,plane);
            }

            return new Box(new BoundingBox(points));

        }

        

        public static Box[] BoundingBoxes(this IEnumerable<Line> lines, Plane plane0, Plane plane1, bool midPoints = true) {

            List<Point3d> points = new List<Point3d>(lines.Count() * 2);

            if (midPoints)
                foreach (Line l in lines) {
                    points.Add(l.PointAt(0.5));
                } else
                foreach (Line l in lines) {
                    points.Add(l.From);
                    points.Add(l.To);
                }


            if (plane0 != Plane.Unset && plane1 != Plane.Unset) {
                return new Box[] { GetBoundingBoxAligned(points, plane0), GetBoundingBoxAligned(points, plane1) };
            }

            return new Box[] { new Box((new BoundingBox(points))), new Box((new BoundingBox(points))) };

        }

        public static Box GetBoundingBoxAligned(List<Point3d> pts,  Plane plane) {

            Transform transform = Transform.ChangeBasis(Plane.WorldXY, plane);
            BoundingBox b = new BoundingBox(pts,transform);

            Point3d min = b.Min;
            double x = min.X;
            min = b.Max;
            Interval interval = new Interval(x, min.X);
            min = b.Min;
            double y = min.Y;
            min = b.Max;
            Interval interval1 = new Interval(y, min.Y);
            min = b.Min;
            double z = min.Z;
            min = b.Max;
            Box box = new Box(plane, interval, interval1, new Interval(z, min.Z));

            return box;
        }


        public static Box GetBoundingBoxAligned(Brep brep, Plane plane)
        {

            Transform transform = Transform.ChangeBasis(Plane.WorldXY, plane);
            BoundingBox b = brep.GetBoundingBox(transform);

            Point3d min = b.Min;
            double x = min.X;
            min = b.Max;
            Interval interval = new Interval(x, min.X);
            min = b.Min;
            double y = min.Y;
            min = b.Max;
            Interval interval1 = new Interval(y, min.Y);
            min = b.Min;
            double z = min.Z;
            min = b.Max;
            Box box = new Box(plane, interval, interval1, new Interval(z, min.Z));

            return box;
        }

        public static bool Intersects(this BoundingBox current, BoundingBox other) {
            return
              (current.Min.X < other.Max.X) && (current.Max.X > other.Min.X) &&
              (current.Min.Y < other.Max.Y) && (current.Max.Y > other.Min.Y) &&
              (current.Min.Z < other.Max.Z) && (current.Max.Z > other.Min.Z);
        }

        public static bool IntersectRay(this BoundingBox bbox, Ray3d r) {
             double t;
            Point3d dirfrac = Point3d.Unset;
            dirfrac.X = 1.0f / r.Direction.X;
            dirfrac.Y = 1.0f / r.Direction.Y;
            dirfrac.Z = 1.0f / r.Direction.Z;
            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            Point3d lb = bbox.Min;
            Point3d rt = bbox.Max;

            double t1 = (lb.X - r.Position.X) * dirfrac.X;
            double t2 = (rt.X - r.Position.X) * dirfrac.X;
            double t3 = (lb.Y - r.Position.Y) * dirfrac.Y;
            double t4 = (rt.Y - r.Position.Y) * dirfrac.Y;
            double t5 = (lb.Z - r.Position.Z) * dirfrac.Z;
            double t6 = (rt.Z - r.Position.Z) * dirfrac.Z;

            double tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            double tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0) {
                t = tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax) {
                t = tmax;
                return false;
            }

            t = tmin;
            return true;
        }


        public static bool IntersectRay(this BoundingBox bbox, Ray3d r, out double t) {

            Point3d dirfrac = Point3d.Unset;
            dirfrac.X = 1.0f / r.Direction.X;
            dirfrac.Y = 1.0f / r.Direction.Y;
            dirfrac.Z = 1.0f / r.Direction.Z;
            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            Point3d lb = bbox.Min;
            Point3d rt = bbox.Max;

            double t1 = (lb.X - r.Position.X) * dirfrac.X;
            double t2 = (rt.X - r.Position.X) * dirfrac.X;
            double t3 = (lb.Y - r.Position.Y) * dirfrac.Y;
            double t4 = (rt.Y - r.Position.Y) * dirfrac.Y;
            double t5 = (lb.Z - r.Position.Z) * dirfrac.Z;
            double t6 = (rt.Z - r.Position.Z) * dirfrac.Z;

            double tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            double tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0) {
                t = tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax) {
                t = tmax;
                return false;
            }

            t = tmin;
            return true;
        }

        public static bool IntersectRay(this BoundingBox bbox, Ray3d r, out float t) {

            Point3d dirfrac = Point3d.Unset;
            dirfrac.X = 1.0f / r.Direction.X;
            dirfrac.Y = 1.0f / r.Direction.Y;
            dirfrac.Z = 1.0f / r.Direction.Z;
            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            Point3d lb = bbox.Min;
            Point3d rt = bbox.Max;

            double t1 = (lb.X - r.Position.X) * dirfrac.X;
            double t2 = (rt.X - r.Position.X) * dirfrac.X;
            double t3 = (lb.Y - r.Position.Y) * dirfrac.Y;
            double t4 = (rt.Y - r.Position.Y) * dirfrac.Y;
            double t5 = (lb.Z - r.Position.Z) * dirfrac.Z;
            double t6 = (rt.Z - r.Position.Z) * dirfrac.Z;

            double tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            double tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0) {
                t = (float)tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax) {
                t = (float)tmax;
                return false;
            }

            t = (float)tmin;
            return true;
        }

    }
}
