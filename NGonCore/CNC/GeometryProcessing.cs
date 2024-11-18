using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public static class GeometryProcessing {


        public static DataTree<Polyline> NotchCircles(this IEnumerable<Line> L, double R, double S) {

            DataTree<Polyline> notch = new DataTree<Polyline>();

            int c = 0;
            foreach (Line l in L) {
                notch.AddRange(NotchCircle(l, R, S), new GH_Path(c++));
            }
            return notch;
        }

        public static DataTree<Polyline> NotchSpirals(this IEnumerable<Line> L, double R, double S) {

            DataTree<Polyline> notch = new DataTree<Polyline>();

            int c = 0;
            foreach (Line l in L) {
                notch.AddRange(NotchSpiral(l,R,S),new GH_Path(c++));
            }
            return notch;
        }

        public static Polyline[] NotchSpiral(this Line L, double R, double S) {
            Vector3d v = L.Direction;
            Plane plane = new Plane(L.From, v);


            Polyline p0 = GeometryProcessing.Polygon((int)(Math.Max(3, R * 1.00 / Math.Max(0.1, S))), (Math.Max(1, R)), plane, 0, false);
            Polyline p1 = new Polyline(p0);


            p1.Transform(Transform.Translation(v));

        
         

            //SPiral


            Polyline[] p = GeometryProcessing.InterpolatePolylines(p0, p1, (int)Math.Max(0, (L.Length / R) - 1));
            //Rhino.RhinoApp.WriteLine(p.Length.ToString());

            Rhino.Geometry.Interval interval = new Rhino.Geometry.Interval(0, 1);
            int n = p0.Count;
            double[] tInterval = new double[n];
            for (int i = 0; i < n; i++) {
                tInterval[i] = interval.ParameterAt((double)i / (double)(n - 1));
            }


            //Spiral
            Polyline spiral = new Polyline();


            for (int i = 0; i < p.Length - 1; i++) {
                for (int j = 0; j < n; j++) {
                    if (i == 1 && j == 0)
                        continue;
                    spiral.Add(GeometryProcessing.Lerp(p[i][j], p[i + 1][j], tInterval[j]));
                }
            }


            for (int j = 0; j < n; j++) {
                if (j == 0 || j == n - 1)
                    continue;
                spiral.Add(p[p.Length - 1][j]);
            }

            spiral.Add(p[0][n - 2]);

            Polyline spiral1 = new Polyline(spiral);
            v.Unitize();
            spiral1.Transform(Transform.Translation(-v * R * 0.1));

            return new Polyline[] { spiral, spiral1 };

        }

            public static Polyline[] NotchCircle(this Line L, double R, double S) {


            Vector3d v = L.Direction;
            Plane plane = new Plane(L.From, v);


            Polyline p0 = GeometryProcessing.Polygon((int)(Math.Max(3, R * 1.00 / Math.Max(0.1, S))), (Math.Max(1, R)), plane, 0, false);
            Polyline p1 = new Polyline(p0);


            p1.Transform(Transform.Translation(v));

            //DA.SetData(0, p1);
            //DA.SetData(1, p0);
            return new Polyline[] {p1,p0 };

            //SPiral


            Polyline[] p = GeometryProcessing.InterpolatePolylines(p0, p1, (int)Math.Max(0, (L.Length / R) - 1));
            //Rhino.RhinoApp.WriteLine(p.Length.ToString());

            Rhino.Geometry.Interval interval = new Rhino.Geometry.Interval(0, 1);
            int n = p0.Count;
            double[] tInterval = new double[n];
            for (int i = 0; i < n; i++) {
                tInterval[i] = interval.ParameterAt((double)i / (double)(n - 1));
            }


            //Spiral
            Polyline spiral = new Polyline();


            for (int i = 0; i < p.Length - 1; i++) {
                for (int j = 0; j < n; j++) {
                    if (i == 1 && j == 0)
                        continue;
                    spiral.Add(GeometryProcessing.Lerp(p[i][j], p[i + 1][j], tInterval[j]));
                }
            }


            for (int j = 0; j < n; j++) {
                if (j == 0 || j == n - 1)
                    continue;
                spiral.Add(p[p.Length - 1][j]);
            }

            spiral.Add(p[0][n - 2]);

            Polyline spiral1 = new Polyline(spiral);
            v.Unitize();
            spiral1.Transform(Transform.Translation(-v * R * 0.1));

        }

        public static Rhino.Geometry.Point3d Lerp(Rhino.Geometry.Point3d p0, Rhino.Geometry.Point3d p1, double amount) {
            return new Rhino.Geometry.Point3d(
                Lerp(p0.X, p1.X, amount),
                Lerp(p0.Y, p1.Y, amount),
                Lerp(p0.Z, p1.Z, amount)
                );
        }

        public static Polyline Polygon(int n, double radius, Plane plane, double rotation = Math.PI * 0.25, bool sqrt = true) {

            Polyline polyline = new Polyline();
            double sector = Math.PI * 2 / n;
            double r = sqrt ? 1 / Math.Sqrt(2) * radius : radius;

            for (int i = 0; i < n; i++) {
                Point3d p = new Point3d(Math.Sin((sector * i) + rotation) * r, Math.Cos((sector * i) + rotation) * r, 0);
                polyline.Add(p);
            }

            polyline.Add(polyline[0]);

            polyline.Transform(Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, plane));

            return polyline;
        }

        public static Curve[] ToCurves(this Polyline[] p) {
            Curve[] c = new Curve[p.Length];
            for (int i = 0; i < p.Length; i++)
                c[i] = p[i].ToNurbsCurve();

            return c;
        }

        public static Polyline[]  SortPolylines (this List <Polyline> p) {

            Polyline[] outlines = new Polyline[p.Count];
            double[] len = new double[p.Count];

            for (int i = 0; i < p.Count; i++) {
                outlines[i] = new Polyline(p[i]);
                len[i] = p[i].BoundingBox.Diagonal.Length;
            }

            Array.Sort(len, outlines);
            Array.Reverse(outlines);

            return outlines;
        }

        //public static Polyline ToPolylineFromCP(this Curve curve, double collapseShortSegments = 0.01) {

        //    Polyline polyline = new Polyline();
        //    if (curve.TryGetPolyline(out polyline)) {
        //        polyline.CollapseShortSegments(collapseShortSegments);
        //        return polyline;
        //    }


        //    NurbsCurve c = curve.ToNurbsCurve();

        //    Point3d[] points = new Point3d[c.Points.Count];

        //    for (int i = 0; i < c.Points.Count; i++)
        //        points[i]=c.Points.ElementAt(i).Location;

        //    polyline = new Polyline(points);


        //    //What the fuck these two lines
        //    c = polyline.ToNurbsCurve();
        //    c.TryGetPolyline(out polyline);



        //    if (collapseShortSegments > 0)
        //        polyline.CollapseShortSegments(collapseShortSegments);
        //    polyline = new Polyline(polyline);

        //    //polyline.CollapseShortSegments(1);
        //    return polyline;


        //}

        public static Transform OrientTo2D(Polyline polyline) {

            Plane plane = GeometryProcessing.GetPlane(polyline);
            Transform t = Transform.PlaneToPlane(plane, Plane.WorldXY);
            return t;
        }

        public static List<Polyline> TransformPolylines(List<Polyline> polylines, Transform t) {


            List<Polyline> polylinesTransformed = new List<Polyline>();

            for (int i = 0; i < polylines.Count; i++) {

                Polyline temp = new Polyline(polylines[i]);
                temp.Transform(t);
                polylinesTransformed.Add(temp);

            }

            return polylinesTransformed;
        }

        public static bool IsClockwiseClosedPolylineOnXYPlane(Polyline polygon) {
            double sum = 0;

            for (int i = 0; i < polygon.Count - 1; i++)
                sum += (polygon[i + 1].X - polygon[i].X) * (polygon[i + 1].Y + polygon[i].Y);

            return sum > 0;
        }

        public static Plane GetPlane(this Polyline polyline, bool AveragePlane = true) {

            //In case use default version

            if (!AveragePlane) {
                // in z case z axis may flip from time to time
                Plane plane_;
                Plane.FitPlaneToPoints(polyline, out plane_);
                plane_.Origin = polyline.CenterPoint();
                return plane_;
            } else {

                Vector3d XAxis = polyline.SegmentAt(0).Direction.Unit();
                Vector3d ZAxis = PolylineUtil.AverageNormal(polyline).Unit();
                Vector3d YAxis = Vector3d.CrossProduct(XAxis, ZAxis);

                return new Plane(polyline.CenterPoint(), XAxis,YAxis);

            }


        }

        //public static Vector3d AverageNormal(this Polyline p) {
        //    //PolyFace item = this[index];
        //    int len = p.Count - 1;
        //    Vector3d vector3d = new Vector3d();
        //    int count = checked(len - 1);

        //    for (int i = 0; i <= count; i++) {
        //        int num = ((i - 1) + len) % len;
        //        int item1 = (checked(i + 1) + len) % len;
        //        Point3d point3d = p[num];
        //        Point3d point3d1 = p[item1];
        //        Point3d item2 = p[i];
        //        vector3d = vector3d + Vector3d.CrossProduct(new Vector3d(item2 - point3d), new Vector3d(point3d1 - item2));
        //    }

        //    if (vector3d.X == 0 & vector3d.Y == 0 & vector3d.Z == 0)
        //        vector3d.Unitize();

        //    return vector3d;
        //}



        public static double DistanceToSquared(this Point3d x1, Point3d x2) {
            return (x1.X - x2.X) * (x1.X - x2.X) + (x1.Y - x2.Y) * (x1.Y - x2.Y) + (x1.Z - x2.Z) * (x1.Z - x2.Z);
        }

        /// <summary>
        /// When cutting in an angle the drilling bit must go deeper
        /// this methods gives value of extendsion
        /// </summary>
        /// <param name="CutAxis - CNC vector"></param>
        /// <param name="ToolPath - path"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public static double CompensateInclination(Line CutAxis, Line ToolPath, double R) {




            Vector3d vCutAxis = CutAxis.Direction;
            Vector3d vToolPath = ToolPath.Direction;

            if (vCutAxis.IsParallelTo(vToolPath, 0.001) != 0)
                return 0;


            //Dirty way of orienting vector to xy axis
            Plane plane = new Plane(ToolPath.To, vToolPath, vCutAxis);
            Transform t = Transform.PlaneToPlane(plane, Plane.WorldXY);
            vCutAxis.Transform(t);
            vToolPath.Transform(t);

            //Get normal
            Vector3d normal = Vector3d.CrossProduct(vCutAxis, vToolPath);


            //Scale Vector to tool radius
            vCutAxis.Unitize();
            vCutAxis *= R;

            //Rotate that vector
            Vector3d vCutAxisRotated = new Vector3d(vCutAxis);
            vCutAxisRotated.Rotate(Math.PI * 0.5, normal);

            //Compare y components
            double scale = Math.Abs((vCutAxisRotated.Y / vCutAxis.Y) * R);

            return scale;

        }

        public static Polyline[] InterpolatePolylines(Polyline path, Polyline normal, int n = 0) {


            Polyline[] interpolatedPolylines = new Polyline[2 + n];

            for (int j = 0; j < 2 + n; j++)
                interpolatedPolylines[j] = new Polyline();

            for (int j = 0; j < path.Count; j++) {
                Point3d[] pts = GeometryProcessing.InterpolatePoints(path[j], normal[j], n, true);
                for (int k = 0; k < 2 + n; k++) {
                    interpolatedPolylines[k].Add(pts[k]);
                }
            }

            return interpolatedPolylines;
        }

        public static Polyline[] InterpolatePolylinesZigZag(Polyline path, Polyline normal, int n = 0) {

            n-- ;

            Polyline[] interpolatedPolylines = new Polyline[2 + n];

            for (int j = 0; j < 2 + n; j++)
                interpolatedPolylines[j] = new Polyline();

            for (int j = 0; j < path.Count; j++) {
                Point3d[] pts = GeometryProcessing.InterpolatePoints(path[j], normal[j], n, true);
                for (int k = 0; k < 2 + n; k++) {
                    interpolatedPolylines[k].Add(pts[k]);
                }
            }


            Polyline ZigZag = new Polyline();
            Polyline ZigZagNormal = new Polyline();

            for (int i = 0; i < 1 + n; i++) {
                if (i % 2 == 0) {
                    ZigZag.AddRange(interpolatedPolylines[i+1]);
                    ZigZagNormal.AddRange(interpolatedPolylines[i]);
                } else {
                    Polyline temp = new Polyline(interpolatedPolylines[i+1]);
                    temp.Reverse();
                    ZigZag.AddRange(temp);
                    Polyline tempNormal = new Polyline(interpolatedPolylines[i]);
                    tempNormal.Reverse();
                    ZigZagNormal.AddRange(tempNormal);
                }
            }


            return new Polyline[] {ZigZag,ZigZagNormal };
        }


        public static double Lerp(double value1, double value2, double amount) {
            return value1 + (value2 - value1) * amount;
        }


        public static Point3d[] InterpolatePoints(Point3d from, Point3d to, int Steps, bool includeEnds = true) {
            Point3d[] point3DArray;

            if (includeEnds) {
                point3DArray = new Point3d[Steps + 2];
                point3DArray[0] = from;

                for (int i = 1; i < Steps + 1; i++) {
                    double num = i / (double)(1 + Steps);

                    point3DArray[i] = new Point3d(
                        Lerp(from.X, to.X, num),
                        Lerp(from.Y, to.Y, num),
                        Lerp(from.Z, to.Z, num)
                    );
                }

                point3DArray[point3DArray.Length - 1] = to;
            } else {

                point3DArray = new Point3d[Steps];

                for (int i = 1; i < Steps + 1; i++) {
                    double num = i / (double)(1 + Steps);

                    point3DArray[i - 1] = new Point3d(
                        Lerp(from.X, to.X, num),
                        Lerp(from.Y, to.Y, num),
                        Lerp(from.Z, to.Z, num)
                    );
                }
            }

            return point3DArray;


        }


        public static Tuple<Polyline, Polyline, List<Curve>> CheckAngle(Polyline P0, Polyline P1, double angleTol = 60) {

            Vector3d vMeasure = Vector3d.ZAxis;
            Point3d[] vrts = P0.ToArray();
            Point3d[] uvrts = P1.ToArray();

            Plane p0;
            Plane p1;
            Plane.FitPlaneToPoints(vrts, out p0);
            Plane.FitPlaneToPoints(uvrts, out p1);

            Plane[] planes = new Plane[uvrts.Length - 1];
            Plane[] planesCompare = new Plane[uvrts.Length - 1];
           

            //Plane[] planes0 = new Plane[uvrts.Length - 1];
            //Plane[] planes1 = new Plane[uvrts.Length - 1];

            //Point3d[] origins = new Point3d[uvrts.Length - 1];
            double[] angles = new double[uvrts.Length - 1];

            List<Curve> sharpPolylines = new List<Curve>();

            for (int i = 0; i < P0.SegmentCount; i++) {

                Point3d midPt = P0.PointAt(i + 0.5);
                Point3d cp = P1.SegmentAt(i).ClosestPoint(midPt, false);
                Point3d origin = (midPt + cp) * 0.5;
                //origins[i] = origin;

                planes[i] = new Plane(origin, P0.SegmentAt(i).Direction, cp - midPt);

       



                planesCompare[i] = new Plane(origin, P0.SegmentAt(i).Direction, vMeasure);

                angles[i] = Rhino.RhinoMath.ToDegrees(Vector3d.VectorAngle(planes[i].ZAxis, planesCompare[i].ZAxis));

                Plane segmentPlane0 = new Plane(P0.SegmentAt(i).PointAt(0.5), P0.SegmentAt(i).Direction, p0.ZAxis);
                Plane segmentPlane1 = new Plane(p0.ClosestPoint(P1.SegmentAt(i).PointAt(0.5)), P1.SegmentAt(i).Direction, p1.ZAxis);

               

                // planes0[i] = segmentPlane0;
                //planes1[i] = segmentPlane1;


                if (angles[i] > angleTol) {
                    planes[i] = (P0.Contains(segmentPlane1.Origin)) ? segmentPlane1 : segmentPlane0;
                    sharpPolylines.Add( (new Polyline(new Point3d[] { P0.PointAt(i), P0.PointAt(i + 1), P1.PointAt(i + 1), P1.PointAt(i), P0.PointAt(i) }) ) .ToNurbsCurve());
                }


            }

            Line[] edges = new Line[uvrts.Length - 1];
            for (int i = 0; i < P0.SegmentCount; i++) {
                Line line = new Line(P0.PointAt(i), P1.PointAt(i));
                edges[i] = line;
            }

         

            Polyline newPoly0 = PolylineFromPlanes(p0, planes, edges);
           Polyline newPoly1 = PolylineFromPlanes(p1, planes, edges);

           //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(newPoly0);
           //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(newPoly1);

            return new Tuple<Polyline, Polyline, List<Curve>>(newPoly0, newPoly1, sharpPolylines);

        }

        public static Polyline PolylineFromPlanes(Plane basePlane, Plane[] sidePlanes, Line[] edges, bool close = true) {

            Polyline polyline = new Polyline();
            Point3d pt, pt1;

            //Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(basePlane, sidePlanes[sidePlanes.Length - 1], sidePlanes[0], out pt1);
            //polyline.Add(pt1);

            for (int i = 0; i < sidePlanes.Length; i++) {
                

                //Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(basePlane, sidePlanes[i], sidePlanes[i + 1], out pt);
                //Plane bi = BisectorPlane(sidePlanes[i], sidePlanes[i + 1]);
                int prev = i - 1;
                if (prev < 0)
                    prev = sidePlanes.Length-1;
                int next = (i + 1) % sidePlanes.Length;
                //Rhino.RhinoApp.WriteLine(i.ToString() + " " + next.ToString());


                Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(basePlane, sidePlanes[i], sidePlanes[next], out pt);

              
                    //Rectangle3d rec = new Rectangle3d(sidePlanes[i],10,10);
               // Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec);
              

                double angle = Vector3d.VectorAngle(sidePlanes[i].ZAxis, sidePlanes[next].ZAxis);
                //Rhino.RhinoApp.WriteLine("angle " + angle.ToString() + " " + i.ToString() + " " + next.ToString() );


                if(angle < 0.001 ) {
                    edges[next].Transform(Transform.Scale(edges[next].PointAt(0.5),2));
                    Rhino.Geometry.Intersect.Intersection.LinePlane(edges[next], basePlane, out double t);
                    pt = edges[next].PointAt(t);
               }

            if (i == 4) {
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(edges[next]);
                }


                polyline.Add(pt);
            }

            //Rhino.RhinoApp.WriteLine((sidePlanes.Length - 1).ToString());
            //Rhino.RhinoApp.WriteLine((edges.Length - 1).ToString());



            if (close)
                polyline.Add(polyline[0]);

            return polyline;

        }

        public static Plane BisectorPlane(Plane a, Plane b, double tolerance = 0.01) {
            double angle = Vector3d.VectorAngle(a.ZAxis, b.ZAxis);

            if (angle > (1 - tolerance) * Math.PI || angle < tolerance) {
                Plane p = new Plane((a.Origin + b.Origin) * 0.5, a.XAxis, a.YAxis);
                p.Rotate(Math.PI * 0.5, p.XAxis);

                return p;

            }

            Line lnA, lnB;
            Rhino.Geometry.Intersect.Intersection.PlanePlane(a, b, out lnA);
            a.Translate(a.ZAxis * 10);
            b.Translate(b.ZAxis * 10);
            Rhino.Geometry.Intersect.Intersection.PlanePlane(a, b, out lnB);
            Plane p_ = new Plane(lnA.From, lnA.To, lnB.PointAt(0.5));
            // p_.Rotate(Math.PI * 0.5, lnA.Direction);
            return p_;

        }


        public static List<Curve> IsCurvesBelowZero(List<Curve> curves, double tolerance = -0.01) {
            List<Curve> badCurves = new List<Curve>();

            foreach (Curve c in curves) {
                if (c.PointAtStart.Z < tolerance || c.PointAtEnd.Z < tolerance || c.PointAtNormalizedLength(0.5).Z < tolerance) {
                    badCurves.Add(c.ToNurbsCurve());
                }
            }

            return badCurves;
        }

        public static List<Curve> IsCurvesBelowZero(List<Line> curves, double tolerance = -0.01) {
            List<Curve> badCurves = new List<Curve>();

            foreach (Line c in curves) {
                if (c.From.Z < tolerance || c.To.Z < tolerance) {
                    badCurves.Add(c.ToNurbsCurve());
                }
            }

            return badCurves;
        }

        public static List<Curve> IsCurvesBelowZero(List<Polyline> curves, double tolerance = -0.01) {
            List<Curve> badCurves = new List<Curve>();

            foreach (Polyline c in curves) {

                foreach(Point3d p in c) {
                    if (p.Z < tolerance) {
                        badCurves.Add(c.ToNurbsCurve());
                        break;
                    }
                }
            }

            return badCurves;
        }


        public static List<List<Curve>> FindPairs(List<Curve> crvs, bool pairing = true) {

            List<List<Curve>> pairs = new List<List<Curve>>();

            //if pairing is turned of the list is just split into pairs without any ordering
            if (!pairing) {
                for (int i = 0; i < crvs.Count; i += 2)
                    pairs.Add(new List<Curve> { crvs[i], crvs[i + 1] });
                return pairs;
            }

            List<Curve> ccrvs = new List<Curve>();

            foreach (Curve crv in crvs) {
                if (crv != null) {
                    if (crv.IsValid) {
                        if (crv.IsClosed) {
                            ccrvs.Add(crv);
                        }
                    }
                }

            }

            List<Curve> uppercrvs = new List<Curve>();
            List<Curve> lowercrvs = new List<Curve>();

            if (ccrvs != null)                                                  // now sort into 2 lists of upper and lower crvs
            {
                foreach (Curve crv in ccrvs) {
                    Point3d testpt = crv.PointAtStart;
                    if (testpt.Z >= -0.1 && testpt.Z <= 0.1) {
                        lowercrvs.Add(crv);
                    } else {
                        uppercrvs.Add(crv);
                    }
                }

                foreach (Curve lcrv in lowercrvs) {
                    Point3d pt0 = lcrv.PointAtStart;
                    List<double> ldst = new List<double>();

                    foreach (Curve ucrv in uppercrvs) {
                        Point3d pt1 = ucrv.PointAtStart;
                        double dst = pt1.DistanceTo(pt0);
                        ldst.Add(dst);
                    }


                    double var1 = 1000;
                    int var2 = 0;
                    for (int i = 0; i != ldst.Count; i++) {
                        if (ldst[i] > 0 && ldst[i] < var1) {
                            var1 = ldst[i];
                            var2 = i;
                        }
                    }

                    List<Curve> pair = new List<Curve>();
                    pair.Add(lcrv);
                    pair.Add(uppercrvs[var2]);
                    pairs.Add(pair);
                }
            }

            return pairs;
        }


        public static List<Point3d> DividePoints(Point3d p1, Point3d p2, int df) {

            Vector3d v1 = p2 - p1;
            double l1 = v1.Length;

            double sl = l1 / df;
            v1.Unitize();
            Vector3d vs = v1 * sl;

            List<Point3d> pts = new List<Point3d>();

            for (int j = 0; j != df + 1; j++) 
                pts.Add(p1 + vs * j);

            return pts;
        }

        public static Point3d VecPlnInt(Point3d p, Vector3d n, double Zval) {
            Line line = new Line(p, p + n);
            Plane plane = new Plane(new Point3d(0, 0, Zval), Plane.WorldXY.Normal);
            double pm;
            Rhino.Geometry.Intersect.Intersection.LinePlane(line, plane, out pm);
            Point3d pt = line.PointAt(pm);
            return pt;
        }




    }
}
