using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore {
    public static class LineUtil {

        public static  Tuple<List<Line>, List<Line>, List<Line>, List<double>, List<double>, List<int>, List<int>> LineLine(List<Line> L, bool Bbox, double tolerance) {

            List<Line> lines = new List<Line>();

            HashSet<long> pairs = new HashSet<long>();

            List<int> pairA = new List<int>();
            List<int> pairB = new List<int>();
            List<double> pairA_T = new List<double>();
            List<double> pairB_T = new List<double>();

            List<Line> pairA_L = new List<Line>();
            List<Line> pairB_L = new List<Line>();


            for (int i = 0; i < L.Count; i++) {
                for (int j = 0; j < L.Count; j++) {
                    if (i == j) continue;




                    long key0 = GetKey(i, j);
                    long key1 = GetKey(j, i);

                    if (pairs.Contains(key0)) continue;

                    pairs.Add(key0);
                    pairs.Add(key1);

                    if (Bbox) {
                        BoundingBox bbox0 = L[i].BoundingBox;
                        BoundingBox bbox1 = L[j].BoundingBox;
                        if (!Intersects(bbox0, bbox1))
                            continue;
                    }

                    double t0, t1;
                    if (Rhino.Geometry.Intersect.Intersection.LineLine(L[i], L[j], out t0, out t1, tolerance, true)) {
                        lines.Add(new Line(L[i].PointAt(t0), L[j].PointAt(t1)));
                        pairA_T.Add(t0);
                        pairB_T.Add(t1);
                        pairA.Add(i);
                        pairB.Add(j);
                        pairA_L.Add(L[i]);
                        pairB_L.Add(L[j]);
                    }


                }
            }

            return new Tuple<List<Line>, List<Line>, List<Line>, List<double>, List<double>, List<int>, List<int>>(lines,pairA_L,pairB_L,pairA_T,pairB_T,pairA,pairB);

 
        }

        public static long GetKey(int i, int j) {
            return (UInt32)i << 16 | (UInt32)j;
        }

        public static Line Rotate (this Line line, double angle, Plane plane) {
            Line l = line;
            l.Transform(Transform.Rotation(Rhino.RhinoMath.ToRadians(angle),plane.ZAxis,plane.Origin));
            return l;
        }

        public static bool Intersects(BoundingBox current, BoundingBox other) {
            return
              (current.Min.X < other.Max.X) && (current.Max.X > other.Min.X) &&
              (current.Min.Y < other.Max.Y) && (current.Max.Y > other.Min.Y) &&
              (current.Min.Z < other.Max.Z) && (current.Max.Z > other.Min.Z);
        }


        public static Point3d P(this Line line, int i) {
            if (i == 0)
                return line.From;
            else if (i == 1)
                return line.To;
            else
                return Point3d.Unset;
        }

        public static void ChangeEnd(ref Line line, int i, Point3d newPoint){
            if (i == 0)
                line.From = newPoint;
            else if (i == 1)
                line.To = newPoint;

        }

        public static Point3d LineLine(this Line l0, Line l1)
        {
            double a, b;
            Rhino.Geometry.Intersect.Intersection.LineLine(l0, l1, out a, out b, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
            Point3d p = l0.PointAt(a);
            return p;
        }

        public static Line LineLineCP(this Line l0, Line l1)
        {
            double a, b;
            Rhino.Geometry.Intersect.Intersection.LineLine(l0, l1, out a, out b);
            return new Line(l0.PointAt(a), l1.PointAt(b));
        }

        public static Line LineLineCP(this Line l0, Line l1, out double[] t)
        {

            t = new double[] {0,0 };
            Rhino.Geometry.Intersect.Intersection.LineLine(l0, l1, out t[0], out t[1]);
            return new Line(l0.PointAt(t[0]), l1.PointAt(t[1]));
        }

        public static double[] LineLineCPT(this Line l0, Line l1 )
        {
            double a, b;
            Rhino.Geometry.Intersect.Intersection.LineLine(l0, l1, out a, out b);
            return new double[] {a,b};
        }

        public static Point3d Center(this Line l)
        {
            return (l.From+l.To)*0.5;
        }

    }
}
