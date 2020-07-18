using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;


namespace NGonsCore
{
    public static class PointUtil
    {





        public static List<Point3d> IsPointsCloseToCurve(this Curve C, List<Point3d> P, ref List<int> ID, double T = 0.01) {
            List<Point3d> cp = new List<Point3d>();
            List<int> cpID = new List<int>();
            double tolSQ = T * T;



            BoundingBox bbox = C.GetBoundingBox(false);
            bbox.Inflate(bbox.Diagonal.Length * 0.01);
            double t;
            for (int i = 0; i < P.Count; i++) {
                //if (bbox.Contains(P[i])) {

                    if (!C.IsClosed) {
                        C.ClosestPoint(P[i], out t);
                        Point3d p = C.PointAt(t);
                        if (p.DistanceToSquared(P[i]) < tolSQ) {
                            cp.Add(P[i]);
                            cpID.Add(i);
                        }
                    } else {
                        C.ClosestPoint(P[i], out t);
                        C.TryGetPlane(out Plane plane);
                        var containment = C.ToNurbsCurve().Contains(P[i],plane ,T);

                        if (containment == PointContainment.Inside || containment == PointContainment.Coincident) {
                            cp.Add(P[i]);
                            cpID.Add(i);
                        }
                    }


                //}
            }


            ID = cpID;
            return cp;


        }



        public  static  List<Point3d> IsPointsInsideMesh (this Mesh M, List<Point3d> P, ref List<int> ID, double T = 0.01) {

            List<Point3d> cp = new List<Point3d>();
            List<int> cpID = new List<int>();
            double tolSQ = T * T;



            Mesh mesh = M.DuplicateMesh();
            if (mesh.SolidOrientation() == -1)
                mesh.Flip(true, true, true);

            BoundingBox bbox = M.GetBoundingBox(false);


            bbox.Inflate(bbox.Diagonal.Length * 0.01);


            for (int i = 0; i < P.Count; i++) {
                if (mesh.IsClosed) {
                    if (mesh.IsPointInside(P[i], T, false)) {
                        cp.Add(P[i]);
                        cpID.Add(i);
                    }

                } else {
                    Point3d p = mesh.ClosestPoint(P[i]);
                    if (p.DistanceToSquared(P[i]) < tolSQ) {
                        cp.Add(P[i]);
                        cpID.Add(i);
                    }
                }





            }
            ID = cpID;
            return cp;

     
        }

        public static List<Curve> IsCurvesInsideMesh(this Mesh M, List<Curve> C, ref List<int> ID, double T = 0.01, bool center = true) {

            List<Curve> cp = new List<Curve>();
            List<int> cpID = new List<int>();
            double tolSQ = T * T;



            Mesh mesh = M.DuplicateMesh();
            if (mesh.SolidOrientation() == -1)
                mesh.Flip(true, true, true);

            BoundingBox bbox = M.GetBoundingBox(false);


            bbox.Inflate(bbox.Diagonal.Length * 0.01);

            for (int i = 0; i < C.Count; i++) {

                Polyline pline;
                bool flag = C[i].TryGetPolyline(out pline);

                //if (flag) {

                    List<Point3d> P = center ? new List<Point3d> { pline.CenterPoint() } : pline.ToList();

                    for (int j = 0; j< P.Count; j++) {
                        if (mesh.IsClosed) {
                            if (mesh.IsPointInside(P[j], T, false)) {
                                cp.Add(C[i]);
                                cpID.Add(i);
                                break;
                            }

                        } else {
                            Point3d p = mesh.ClosestPoint(P[j]);
                            if (p.DistanceToSquared(P[j]) < tolSQ) {
                                cp.Add(C[i]);
                                cpID.Add(i);
                                break;
                            }
                        }


                    }


               // }
            }


            ID = cpID;
            return cp;


        }



        public static Vector3f ToVector3f(this Point3f p) {
            Vector3f v = new Vector3f(p.X, p.Y, p.Z);
            return v;
        }

        public static Vector3d ToVector3d(this Point3d p) {
            Vector3d v = new Vector3d(p.X, p.Y, p.Z);
            return v;
        }

        public static Point3f ToPoint3f(this Vector3f p) {
            Point3f v = new Point3f(p.X, p.Y, p.Z);
            return v;
        }

        public static Point3d ToPoint3d(this Vector3d p) {
            Point3d v = new Point3d(p.X, p.Y, p.Z);
            return v;
        }

        public static Point3f Unit(this Point3f p) {
            Vector3f v = new Vector3f(p.X, p.Y, p.Z);
            v.Unitize();
            return new Point3f(v.X, v.Y, v.Z);
        }

        public static Point3d Unit(this Point3d p) {
            Vector3d v = new Vector3d(p.X, p.Y, p.Z);
            v.Unitize();
            return new Point3d(v.X, v.Y, v.Z);
        }

        public static Vector3f Unit(this Vector3f p) {
            Vector3f v = new Vector3f(p.X, p.Y, p.Z);
            v.Unitize();
            return new Vector3f(v.X, v.Y, v.Z);
        }

        public static Vector3d Unit(this Vector3d p) {
            Vector3d v = new Vector3d(p.X, p.Y, p.Z);
            v.Unitize();
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static int ClosestPoint(Point3d P, Point3d[] list) {
            int num = -1;
            double num1 = double.MaxValue;

            for (int i = 0; i < list.Length; i++) {
                if (list[i] != null && list[i].IsValid) {

                    double num2 = P.DistanceToSquared(list[i]);
                    if (num2 < num1) {
                        num1 = num2;
                        num = i;
                    }
                }
            }
            return num;
        }

        public static List<int> ClosestPoints(Point3d P, Point3d[] list, int num) {

            checked {
                int[] array = new int[num - 1 + 1];
                double[] array2 = new double[num - 1 + 1];
                int arg_74_0 = 0;
                int num2 = array.Length - 1;
                for (int i = arg_74_0; i <= num2; i++) {
                    array[i] = -1;
                    array2[i] = 1.7976931348623157E+308;
                }
                int arg_A1_0 = 0;
                int num3 = list.Length - 1;
                for (int j = arg_A1_0; j <= num3; j++) {
                    if (list[j] != null) {
                        if (list[j].IsValid) {
                            double num4 = P.DistanceTo(list[j]);
                            int num5 = System.Array.BinarySearch<double>(array2, num4);
                            if (num5 < 0) {
                                num5 ^= -1;
                            }
                            if (num5 < array.Length) {
                                int arg_102_0 = array.Length - 1;
                                int num6 = num5 + 1;
                                for (int k = arg_102_0; k >= num6; k += -1) {
                                    array[k] = array[k - 1];
                                    array2[k] = array2[k - 1];
                                }
                                array[num5] = j;
                                array2[num5] = num4;
                            }
                        }
                    }
                }

                List<int> list3 = new List<int>(num);
                List<double> dist = new List<double>();

                int arg_161_0 = 0;
                int num7 = array.Length - 1;
                int num8 = arg_161_0;
                while (num8 <= num7 && array[num8] >= 0) {
                    list3.Add(array[num8]);
                    dist.Add(P.DistanceTo(list[array[num8]]));
                    num8++;
                }




                return list3;
            }

        }




        public static Point3d Barycentric(Point3d P, Point3d A, Point3d B, Point3d C) {
            Vector3d b = B - A;
            Vector3d c = C - A;
            Vector3d p = P - A;
            double num = DotProduct(b, b);
            double num1 = DotProduct(b, c);
            double num2 = DotProduct(c, c);
            double num3 = DotProduct(p, b);
            double num4 = DotProduct(p, c);
            double num5 = 1 / (num * num2 - num1 * num1);
            double num6 = (num2 * num3 - num1 * num4) * num5;
            double num7 = (num * num4 - num1 * num3) * num5;
            return new Point3d(num6, num7, 1 - num6 - num7);
        }


        public static Circle InscribedCircle(List<Point3d> Triangle) {
            Circle circle = new Circle();
            Point3d item = Triangle[0];
            double num = item.DistanceTo(Triangle[1]);
            item = Triangle[1];
            double num1 = item.DistanceTo(Triangle[2]);
            item = Triangle[2];
            double num2 = item.DistanceTo(Triangle[0]);
            double num3 = (num + num1 + num2) / 2;
            double num4 = Math.Sqrt(num3 * (num3 - num) * (num3 - num1) * (num3 - num2));
            double num5 = 2 * num4 / (num + num1 + num2);
            circle.Radius=(num5);
            double num6 = Vector3d.VectorAngle(Triangle[1] - Triangle[0], Triangle[2] - Triangle[0]) / 2;
            double num7 = 1 / Math.Sin(num6) * num5;
            Vector3d vector3d = Triangle[1] - Triangle[0];
            Vector3d item1 = Triangle[2] - Triangle[0];
            vector3d.Unitize();
            item1.Unitize();
            Vector3d vector3d1 = vector3d + item1;
            vector3d1.Unitize();
            vector3d1 *= num7;
            Point3d point3d = Triangle[0] + vector3d1;
            Vector3d item2 = Triangle[2] - Triangle[1];
            Vector3d vector3d2 = Triangle[1] - Triangle[0];
            item2.Unitize();
            vector3d2.Unitize();
            Vector3d vector3d3 = Vector3d.CrossProduct(item2, vector3d2);
            circle.Plane=(new Plane(point3d, vector3d3));
            return circle;
        }

        public static Point3d[][] InterpolatePointArrays(Point3d[] From, Point3d[] To, int Steps) {
            Point3d[][] point3dArray = new Point3d[checked(checked(Steps + 1) + 1)][];
            int length = checked((int)point3dArray.Length - 1);
            for (int i = 0; i <= length; i = checked(i + 1)) {
                double num = (double)i / (double)(checked((int)point3dArray.Length - 1));
                Point3d[] point3dArray1 = new Point3d[checked(checked((int)From.Length - 1) + 1)];
                int length1 = checked((int)From.Length - 1);
                for (int j = 0; j <= length1; j = checked(j + 1)) {
                    Point3d from = From[j];
                    Point3d to = To[j];
                    from *= num;
                    to = to * (1 - num);
                    point3dArray1[j] = from + to;
                }
                point3dArray[i] = point3dArray1;
            }
            return point3dArray;
        }



        public static Vector3d SumVector3d(IEnumerable<Vector3d> Vectors) {
            Vector3d vector3d = new Vector3d();
            int num = checked(Vectors.Count<Vector3d>() - 1);
            for (int i = 0; i <= num; i = checked(i + 1)) {
                vector3d += Vectors.ElementAtOrDefault<Vector3d>(i);
            }
            return vector3d;
        }


        public static Line TangentCircle(double R, Line L1, Line L2) {
            Vector3d to = L1.To - L1.From;
            Vector3d vector3d = L2.To - L2.From;
            to.Unitize();
            vector3d.Unitize();
            Vector3d vector3d1 = new Vector3d((to + vector3d) * 0.5);
            double num = Vector3d.VectorAngle(L1.To - L1.From, L2.To - L2.From) * 0.5;
            double r = R * MathUtil.Cot(num);
            return new Line(L1.From, vector3d1, MathUtil.Pitagoras(r, R));
        }

        internal static object Walk(int Start, List<int>[] Neighbors) {
            int start = Start;
            int num = Start;
            int item = Neighbors[num][0];
            List<int> nums = new List<int>()
            {
                num,
                item
            };
            do {
                if (Neighbors[item].Count != 1) {
                    int item1 = Neighbors[item][0];
                    int num1 = Neighbors[item][1];
                    if (item1 != num) {
                        num = item;
                        item = item1;
                    } else if (num1 != num) {
                        num = item;
                        item = num1;
                    }
                    nums.Add(item);
                } else {
                    if (num != start) {
                        break;
                    }
                    nums.Add(Neighbors[item][0]);
                    break;
                }
            }
            while (item != start);
            return nums;
        }
   

    public static double DotProduct(Vector3d a, Vector3d b) {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static void PlanarizePoint(this Point3d A0, Plane newplane) {
            Line line = new Line(A0, A0 + newplane.Normal * 10);
            double param = new double();
            Rhino.Geometry.Intersect.Intersection.LinePlane(line, newplane, out param);
            Point3d A0p = line.PointAt(param);
            A0 = A0p;
        }

        public static Point3d PlanarPoint(this Point3d A0, Plane newplane) {
            Line line = new Line(A0, A0 + newplane.Normal * 10);
            double param = new double();
            Rhino.Geometry.Intersect.Intersection.LinePlane(line, newplane, out param);
            Point3d A0p = line.PointAt(param);
            return A0p;
        }

        public static Line[] ToLineArray(this Point3d[] p, bool closed = false) {

            Line[] lines;

            if (!closed) {

                lines = new Line[p.Length-1];
                for (int i = 0; i < p.Length - 1; i++)
                    lines[i] = new Line(p[i], p[i+1]);
                return lines;

            } else {
                lines = new Line[p.Length];
                for (int i = 0; i < p.Length - 1; i++)
                    lines[i] = new Line(p[i], p[i + 1]);
                lines[p.Length-1]= new Line(p[p.Length-1], p[0]);
                return lines;
            }

        }




        public static double FastDistance(this Point3d p1, Point3d p2)
        {
            return (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.Z - p1.Z) * (p2.Z - p1.Z);
        }



        public static Point3d AveragePoint(IEnumerable<Point3d> P)
        {
            Point3d point3d = new Point3d(0.0, 0.0, 0.0);
            foreach (Point3d pt in P)
            {
                point3d += pt;
            }
            return point3d / (double)P.Count();
        }





        public static Point3d[] InterpolatePoints(Point3d from, Point3d to, int Steps, bool includeEnds = true)
        {
            Point3d[] point3DArray;

            if (includeEnds) {
                point3DArray = new Point3d[Steps + 2];
                point3DArray[0] = from;

                for (int i = 1; i < Steps + 1; i++) {
                    double num = i / (double)(1 + Steps);

                    point3DArray[i] = new Point3d(
                        MathUtil.Lerp(from.X, to.X, num),
                        MathUtil.Lerp(from.Y, to.Y, num),
                        MathUtil.Lerp(from.Z, to.Z, num)
                    );
                }

                point3DArray[point3DArray.Length - 1] = to;
            } else {

                point3DArray = new Point3d[Steps];

                for (int i = 1; i < Steps + 1; i++) {
                    double num = i / (double)(1 + Steps);

                    point3DArray[i-1] = new Point3d(
                        MathUtil.Lerp(from.X, to.X, num),
                        MathUtil.Lerp(from.Y, to.Y, num),
                        MathUtil.Lerp(from.Z, to.Z, num)
                    );
                }
            }

            return point3DArray;


        }

        public static Point3d[] InterpolateLine(this Line line, int Steps, bool includeEnds = true) {
            return InterpolatePoints(line.From, line.To, Steps, includeEnds);
        }

        public static Polyline[] InterpolatePolylines(Polyline from, Polyline to, int Steps, bool includeEnds = true) {

            



            if (from.Count == to.Count) {

                Polyline[] interpolatedPolylines = new Polyline[Steps+(Convert.ToInt32(includeEnds)*2)];

                for (int i = 0; i < Steps + (Convert.ToInt32(includeEnds) * 2);i++) 
                    interpolatedPolylines[i] = new Polyline();
                

                System.Collections.Generic.IEnumerator<Point3d> enum0 = from.GetEnumerator();
                System.Collections.Generic.IEnumerator<Point3d> enum1 = to.GetEnumerator();




    
                while (enum0.MoveNext()) {

                    Point3d[] pt = NGonsCore.PointUtil.InterpolatePoints(enum0.Current, enum1.Current, Steps, includeEnds);

                    for (int i = 0; i < Steps + (Convert.ToInt32(includeEnds) * 2); i++)
                        interpolatedPolylines[i].Add(pt[i]);

                    enum1.MoveNext();
             
                }

                return interpolatedPolylines;

            }

            return null;

        }


            public static List<int> cp(Point3d P, Point3d[] list, int num) {

            checked {
                int[] array = new int[num - 1 + 1];
                double[] array2 = new double[num - 1 + 1];
                int arg_74_0 = 0;
                int num2 = array.Length - 1;
                for (int i = arg_74_0; i <= num2; i++) {
                    array[i] = -1;
                    array2[i] = 1.7976931348623157E+308;
                }
                int arg_A1_0 = 0;
                int num3 = list.Length - 1;
                for (int j = arg_A1_0; j <= num3; j++) {
                    if (list[j] != null) {
                        if (list[j].IsValid) {
                            double num4 = P.DistanceTo(list[j]);
                            int num5 = System.Array.BinarySearch<double>(array2, num4);
                            if (num5 < 0) {
                                num5 ^= -1;
                            }
                            if (num5 < array.Length) {
                                int arg_102_0 = array.Length - 1;
                                int num6 = num5 + 1;
                                for (int k = arg_102_0; k >= num6; k += -1) {
                                    array[k] = array[k - 1];
                                    array2[k] = array2[k - 1];
                                }
                                array[num5] = j;
                                array2[num5] = num4;
                            }
                        }
                    }
                }

                List<int> list3 = new List<int>(num);
                List<double> dist = new List<double>();

                int arg_161_0 = 0;
                int num7 = array.Length - 1;
                int num8 = arg_161_0;
                while (num8 <= num7 && array[num8] >= 0) {
                    list3.Add(array[num8]);
                    dist.Add(P.DistanceTo(list[array[num8]]));
                    num8++;
                }




                return list3;
            }

        }


    }
}
