﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonsCore;
using Rhino.Geometry;

namespace NGonsCore {

    /// <summary>
    /// Courtesy of David Rutten
    /// https://discourse.mcneel.com/t/converting-curves-into-multiple-arcs/20418/4?u=tom_svilans
    /// </summary>
    public static class BiArc {
        /// <summary>
        /// Converts a factor (0.0 ~ 1.0) into a bi-arc ratio value (0.01 ~ 100.0)
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        private static double ToRatio(double factor) {
            factor = Math.Max(factor, 0.0);
            factor = Math.Min(factor, 1.0);

            //remap to a {-3.0 ~ +3.0} domain. (0.001 ~ 1000.0)
            factor = -3.0 + factor * 6.0;

            return Math.Pow(10.0, factor);
        }

        /// <summary>
        /// Joins A and B into a polycurve.
        /// Segment B will be reversed.
        /// </summary>
        private static PolyCurve JoinArcs(Arc A, Arc B) {
            B.Reverse();

            PolyCurve crv = new PolyCurve();
            crv.Append(A);
            crv.Append(B);

            return crv;
        }

        /// <summary>
        /// Solves a BiArc for a specific set of end points and tangents.
        /// If a solution is found that is simpler than a BiArc (i.e. A single Arc or a straight line segment)
        /// then the solution out parameter will contain that solution and the segments will be null.
        /// </summary>
        /// <param name="pA">Start point of biarc</param>
        /// <param name="tA">Start tangent of biarc.</param>
        /// <param name="pB">End point of biarc</param>
        /// <param name="tB">End tangent of biarc.</param>
        /// <param name="segment_A">First biarc segment</param>
        /// <param name="segment_B">Second biarc segment</param>
        /// <param name="solution">Curve representing entire solution.</param>
        /// <returns>true on success, false on failure</returns>
        public static bool Solve(
          Point3d pA, Vector3d tA,
          Point3d pB, Vector3d tB,
          out Arc segment_A,
          out Arc segment_B,
          out Curve solution) {
            return Solve(pA, tA, pB, tB, 0.5, out segment_A, out segment_B, out solution);
        }

        /// <summary>
        /// Solves a BiArc for a specific set of end points and tangents.
        /// If a solution is found that is simpler than a BiArc (i.e. A single Arc or a straight line segment)
        /// then the solution out parameter will contain that solution and the segments will be null.
        /// </summary>
        /// <param name="pA">Start point of biarc</param>
        /// <param name="tA">Start tangent of biarc.</param>
        /// <param name="pB">End point of biarc</param>
        /// <param name="tB">End tangent of biarc.</param>
        /// <param name="factor">Factor (between zero and one) that defines the relative weight of the start and end points.</param>
        /// <param name="segment_A">First biarc segment</param>
        /// <param name="segment_B">Second biarc segment</param>
        /// <param name="solution">Curve representing entire solution.</param>
        /// <returns>true on success, false on failure</returns>
        public static bool Solve(
          Point3d pA, Vector3d tA,
          Point3d pB, Vector3d tB,
          double factor,
          out Arc segment_A,
          out Arc segment_B,
          out Curve solution) {
            segment_A = Arc.Unset;
            segment_B = Arc.Unset;
            solution = null;

            if ((!pA.IsValid) || (!tA.IsValid) || (!pB.IsValid) || (!tB.IsValid)) { return false; }
            if (tA.IsTiny(1e-12) || tB.IsTiny(1e-12)) { return false; }

            tA.Unitize();
            tB.Unitize();

            Vector3d span = (pA - pB);
            if (span.IsTiny(1e-12)) { return false; }

            //================================================================
            //From here on out it should be possible to always get a solution.
            //================================================================

            double ratio = ToRatio(factor);

            // If the start and end tangent are parallel, we have a simple solution.
            if (tA.IsParallelTo(tB, 1e-12) == +1) {
                if (tA.IsParallelTo(span, 1e-12) == -1) {
                    // If the span is also parallel to tA, we have a straight line segment
                    solution = new LineCurve(pA, pB);
                    return true;
                } else {
                    // We have two symmetrical arcs. The regular algorithm cannot handle (anti)parallel tangents,
                    // so we need to handle these separately.
                    segment_A = new Arc(pA, tA, pA + span * 0.5);
                    segment_B = new Arc(pB, -tB, pB + span * 0.5);
                    solution = JoinArcs(segment_A, segment_B);
                    return true;
                }
            }
            // Handle anti-parallel tangents here.


            // Attempt single arc solution.
            Arc simple_solution = new Arc(pA, tA, pB);
            if (!simple_solution.IsValid) {
                if (simple_solution.TangentAt(simple_solution.AngleDomain.Max).IsParallelTo(tB, 1e-12) == +1) {
                    segment_A = simple_solution;
                    segment_B = simple_solution;

                    segment_A.Trim(simple_solution.AngleDomain.ParameterIntervalAt(new Interval(0.0, factor)));
                    segment_B.Trim(simple_solution.AngleDomain.ParameterIntervalAt(new Interval(factor, 1.0)));

                    solution = new ArcCurve(simple_solution);
                    return true;
                }

                // If the end tangent of the simple solution doesn't match the provided end tangent,
                // we have to fit a real bi-arc.
            }


            // Hard core bi-arc solver. Code based on cmdMikko.cpp in Rhino source.
            {
                double a = ratio * 2.0 * (tA * tB - 1.0);
                if (Math.Abs(a) < Rhino.RhinoMath.ZeroTolerance) { return false; }

                double b = span * 2.0 * (tA * ratio + tB);
                double c = span * span;
                double d = (b * b) - (4.0 * a * c);

                if (d < 0.0) { return false; }

                int lmax = 0;
                if (d > 0.0) {
                    d = Math.Sqrt(d);
                    lmax = 2;
                }

                double denom = 1.0 / (2.0 * a);

                for (int l = -1; l < lmax; l += 2) {
                    double sol = (-b + l * d) * denom;
                    if (sol > 0.0) {
                        Line ln = new Line(pA + tA * sol * ratio, pB - tB * sol);
                        Point3d pt = ln.PointAt((ratio * sol) / (sol + ratio * sol));

                        if ((pt.DistanceTo(pA) > Rhino.RhinoMath.ZeroTolerance) &&
                          (pt.DistanceTo(pB) > Rhino.RhinoMath.ZeroTolerance)) {
                            segment_A = new Arc(pA, tA, pt);
                            if (!segment_A.IsValid) { return false; }

                            segment_B = new Arc(pB, -tB, pt);
                            if (!segment_B.IsValid) { return false; }

                            solution = JoinArcs(segment_A, segment_B);
                            return true;
                        }
                    }
                }
            }

            return (solution != null);
        }
    }



    public class CCX {
        public double t0;
        public double t1;
        public Point3d p0;
        public Point3d p1;
    }

    public static class CurveUtil {

        public static Plane OrientPlaneTowardsCurveShorterEnd(this Curve curve, Plane plane, double t) {

            Plane planeFlip = new Plane(plane);

            //Split interval, and find shorter one
            Interval curveInterval = curve.Domain;

            Interval[] curveIntervalSplit = new Interval[]{
             new Interval (curve.Domain.T0-1000, t),
             new Interval (t, curve.Domain.T1+1000),
            };

            int shorterInerval = curveIntervalSplit[0].Length < curveIntervalSplit[1].Length ? 0 : 1;

            //Offset point on plane and check if it is on the shorterInerval
            curve.ClosestPoint(plane.Origin + plane.ZAxis, out double tCP);

            if (!curveIntervalSplit[shorterInerval].IncludesParameter(tCP)) {
                planeFlip = planeFlip.FlipAndRotate();
            }

            return planeFlip;
        }

        public static double TCP(this Curve curve, Point3d p) {
            curve.ClosestPoint(p, out double t);
            return t;
        }

        public static double TS(this Curve curve) {
            return curve.Domain.T0;
        }

        public static double TE(this Curve curve) {
            return curve.Domain.T1;
        }
        public static double TM(this Curve curve) {
            return (curve.Domain.T0 + curve.Domain.T1) * 0.5;
        }

        public static double TX01(this Curve curve, double valueFrom0To1) {
            return MathUtil.RemapNumbers(valueFrom0To1, 0, 1, curve.Domain.Min, curve.Domain.Max);
        }

        public static double TXAB(this Curve curve, double valueFromAToB) {
            return MathUtil.RemapNumbers(valueFromAToB, curve.Domain.Min, curve.Domain.Max, 0, 1);
        }

        public static Circle[] CreateBeamCircles(this Curve centreCurve, List<double> radii, List<Tuple<double, Plane>> frames, int samples, bool rebuild = false, int rebuild_pts = 20, int signX = 1, int signY = 1) {

            List<Point3d> pts = new List<Point3d>();
            double[] t = centreCurve.DivideByCount(samples, true);

            Circle[] circles = new Circle[t.Length];

            for (int i = 0; i < t.Length; ++i) {

                double tRemapped = MathUtil.RemapNumbers(t[i], 0, t.Last(), 0, 1);
                double r = MathUtil.Interpolate(radii, tRemapped);

                Plane p = centreCurve.GetPlane(frames, t[i]);

                pts.Add(p.Origin + p.XAxis * signX * r + p.YAxis * signY * r);

                circles[i] = new Circle(p, r);
            }

            //Curve new_curve = Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform, centreCurve.TangentAtStart, centreCurve.TangentAtEnd);

            //if (new_curve == null)
            //    throw new Exception("FreeformGlulam::CreateOffsetCurve:: Failed to create interpolated curve!");

            //double len = new_curve.GetLength();
            //new_curve.Domain = new Interval(0.0, len);

            //if (rebuild)
            //    new_curve = new_curve.Rebuild(rebuild_pts, new_curve.Degree, true);

            //return new_curve;



            //Circle[] circles = new Circle[radii.Count];

            //for (int i = 0; i < circles.Length; i++)
            //    circles[i] = new Circle(frames[i].Item2, radii[i]);
            return circles;
        }


        public static Curve CreateOffsetCurve(this Curve centreCurve, List<double> radii, List<Tuple<double, Plane>> frames, int samples, bool rebuild = false, int rebuild_pts = 20, int signX = 1, int signY = 1) {
            List<Point3d> pts = new List<Point3d>();
            double[] t = centreCurve.DivideByCount(samples, true);


            for (int i = 0; i < t.Length; ++i) {

                double tRemapped = MathUtil.RemapNumbers(t[i], 0, t.Last(), 0, 1);
                double r = MathUtil.Interpolate(radii, tRemapped);

                Plane p = centreCurve.GetPlane(frames, t[i]);

                pts.Add(p.Origin + p.XAxis * signX * r + p.YAxis * signY * r);
            }

            Curve new_curve = Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform, centreCurve.TangentAtStart, centreCurve.TangentAtEnd);

            if (new_curve == null)
                throw new Exception("FreeformGlulam::CreateOffsetCurve:: Failed to create interpolated curve!");

            double len = new_curve.GetLength();
            new_curve.Domain = new Interval(0.0, len);

            if (rebuild)
                new_curve = new_curve.Rebuild(rebuild_pts, new_curve.Degree, true);

            return new_curve;
        }

        public static Curve[] CreateOffsetCurve4(this Curve centreCurve, List<double> radii, List<Tuple<double, Plane>> frames, int samples, bool rebuild = false, int rebuild_pts = 20) {

            Curve[] crvs = new Curve[] {
            CreateOffsetCurve(centreCurve, radii, frames, samples, rebuild, rebuild_pts, 1, 1),
            CreateOffsetCurve(centreCurve, radii, frames, samples, rebuild, rebuild_pts, -1, 1),
            CreateOffsetCurve(centreCurve, radii, frames, samples, rebuild, rebuild_pts, -1, -1),
            CreateOffsetCurve(centreCurve, radii, frames, samples, rebuild, rebuild_pts, 1, -1),

        };

            return crvs;

        }

        public static Curve CreateOffsetCurve(this Curve centreCurve, List<Tuple<double, Plane>> frames, int samples, double x, double y, bool rebuild = false, int rebuild_pts = 20) {
            List<Point3d> pts = new List<Point3d>();
            double[] t = centreCurve.DivideByCount(samples, true);


            for (int i = 0; i < t.Length; ++i) {
                Plane p = centreCurve.GetPlane(frames, t[i]);
                pts.Add(p.Origin + p.XAxis * x + p.YAxis * y);
            }

            Curve new_curve = Curve.CreateInterpolatedCurve(pts, 3, CurveKnotStyle.Uniform, centreCurve.TangentAtStart, centreCurve.TangentAtEnd);

            if (new_curve == null)
                throw new Exception("FreeformGlulam::CreateOffsetCurve:: Failed to create interpolated curve!");

            double len = new_curve.GetLength();
            new_curve.Domain = new Interval(0.0, len);

            if (rebuild)
                new_curve = new_curve.Rebuild(rebuild_pts, new_curve.Degree, true);

            return new_curve;
        }

        public static Plane[] GetPlanes(this Curve crv_, int divisions, bool bishopFrame = false) {

            int closed = crv_.IsClosed ? 0 : 1;
            Curve crv = crv_.DuplicateCurve();
            crv.Domain = new Interval(0, 1);
            int n = divisions + closed;
            Plane[] planes = new Plane[n];

            for (int i = 0; i < n; i++) {
                double t = MathUtil.RemapNumbers(i, 0, divisions, 0, 1);
                //Print(t.ToString());
                planes[i] = GetPlane(crv, t, bishopFrame);
            }

            //Get plane at one point, measure X-Axis angle and rotate the rest of the planes by the same angle

            return planes;
        }

        public static Plane GetPlane(this Curve crv_, double t, bool bishopFrame = false) {

            Curve crv = crv_.DuplicateCurve();
            crv.Domain = new Interval(0, 1);



            Plane xyz = Plane.Unset;

            if (bishopFrame) {
                //Get derivatives
                Vector3d[] ders = crv.DerivativeAt(t, 3, CurveEvaluationSide.Default);
                Vector3d[] dersOff = crv.DerivativeAt(t + 0.001, 2, CurveEvaluationSide.Default);

                ders[1].Unitize();
                dersOff[1].Unitize();

                Vector3d r = -Vector3d.CrossProduct(ders[1], dersOff[1]);
                r.Unitize();
                Vector3d nrm = -Vector3d.CrossProduct(ders[1], r);
                nrm.Unitize();


                Vector3d crvC = crv.CurvatureAt(t);
                crvC.Unitize();
                xyz = new Plane(crv.PointAt(t), crvC, Vector3d.CrossProduct(crvC, ders[1]));
                //xyz = new Plane(crv.PointAt(t), r, nrm);
            } else {
                //Get perp frame
                Plane plane;
                crv.PerpendicularFrameAt(t, out plane);
                xyz = plane;
            }

            return xyz;
        }

        public static Line ToLine(this Curve curve) {
            return new Line(curve.PointAtStart, curve.PointAtEnd);
        }

        public static Curve RemapCurveDomain(this Curve curve) {
            Curve c = curve.DuplicateCurve();
            c.Domain = new Interval(0, 1);
            return c;
        }

        public static List<Curve> RemapCurvesDomain(this List<Curve> curves) {
            List<Curve> curvesRemapped = new List<Curve>(curves.Count);
            foreach (Curve c in curves)
                curvesRemapped.Add(c.RemapCurveDomain());
            return curvesRemapped;
        }

        public static List<Curve> LinesToCircles(List<Curve> x, int divisions = 10, double radius = 25) {
            var crvs = new List<Curve>();
            var lns = new List<Curve>();

            foreach (Curve c in x) {

                //  Print(c.SpanCount.ToString());
                if (!c.IsLinear(0.01) || ((c.SpanCount + 1) > 2)) {

                    crvs.Add(c);
                } else {

                    Plane pl0;
                    c.PerpendicularFrameAt(0, out pl0);
                    lns.Add(PolylineUtil.Polygon(divisions, radius, pl0).ToNurbsCurve());
                    Plane pl1;
                    c.PerpendicularFrameAt(1, out pl1);
                    lns.Add(PolylineUtil.Polygon(divisions, radius, pl1).ToNurbsCurve());

                }
            }

            crvs.AddRange(lns);
            return crvs;
        }

        public static Tuple<double, double> CurveCurveInter(Curve C0, Curve C1, double t) {

            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C0, C1, t, t);


            Line line = Line.Unset;
            if (ci.Count > 0) {
                Rhino.Geometry.Intersect.IntersectionEvent intersection = ci[0];
                return new Tuple<double, double>(intersection.ParameterA, intersection.ParameterB);
            }

            return null;
        }

        public static double[] GetCurvature(Curve x, int divisions = 10) {
            double[] t = x.DivideByCount(divisions, true);
            double[] curvature = new double[t.Length];
            for (int i = 0; i < t.Length; i++)
                curvature[i] = x.CurvatureAt(t[i]).SquareLength;

            return curvature;
        }

        public static Curve[] SplitAtCorners(this Curve C, double angle, out List<Point3d> Corners) {

            List<double> splitParams = new List<double>();
            List<Point3d> corners = new List<Point3d>();

            if (Vector3d.VectorAngle(C.TangentAtStart, C.TangentAtEnd) > angle) {
                splitParams.Add(0);
                corners.Add(C.PointAtStart);
            }

            bool working = true;
            double tstart = 0;
            while (working == true) {
                double t;
                working = C.GetNextDiscontinuity(Continuity.C1_continuous, tstart, double.MaxValue, out t);
                if (working) {
                    tstart = t;
                    Vector3d va = C.DerivativeAt(t, 1, CurveEvaluationSide.Below)[1];
                    Vector3d vb = C.DerivativeAt(t, 1, CurveEvaluationSide.Above)[1];
                    if (Vector3d.VectorAngle(va, vb) > angle) {
                        splitParams.Add(t);
                        corners.Add(C.PointAt(t));
                    }
                }
            }


            Corners = corners;
            return C.Split(splitParams);
        }



        public static List<Circle> SortCircles(List<Circle> x) {

            if (x.Count == 1)
                return x;

            List<Circle> sortedCircles = new List<Circle>() { x[0] };

            for (int i = 1; i < x.Count; i++) {

                Plane plane = PlaneUtil.ProjectPlaneXPlaneToYPlane(sortedCircles[sortedCircles.Count - 1].Plane, x[i].Plane);
                Circle circle = new Circle(plane, x[i].Radius);
                sortedCircles.Add(circle);
            }

            return sortedCircles;
        }

        public static Tuple<double, double> CurveCurveIntersection(Curve C0, Curve C1, double t) {

            //Rhino.RhinoApp.WriteLine("WTF");
            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C0, C1, t, t);


            Line line = Line.Unset;
            if (ci.Count > 0) {
                Rhino.Geometry.Intersect.IntersectionEvent intersection = ci[0];
                //Rhino.RhinoApp.WriteLine(intersection.ParameterA.ToString() +" " + intersection.ParameterB.ToString());
                return new Tuple<double, double>(intersection.ParameterA, intersection.ParameterB);
            }

            return null;
        }

        public static Tuple<Point3d, Point3d> CurveCurveIntersectionPoints(Curve C0, Curve C1, double t) {

            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C0, C1, t, t);


            Line line = Line.Unset;
            if (ci.Count > 0) {
                Rhino.Geometry.Intersect.IntersectionEvent intersection = ci[0];
                return new Tuple<Point3d, Point3d>(intersection.PointA, intersection.PointB);
            }

            return null;
        }

        public static List<Point3d> CurveCurveIntersectionAllPts(Curve C0, Curve C1, double t = 0.01) {

            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C0, C1, t, t);

            List<Point3d> pts = new List<Point3d>();
            Line line = Line.Unset;
            if (ci.Count > 0) {

                foreach (Rhino.Geometry.Intersect.IntersectionEvent inter in ci) {
                    pts.Add(inter.PointA);
                }
                return pts;
            }

            return null;
        }

        public static CCX CurveCurveIntersectionCCX(Curve C0, Curve C1, double t) {

            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C0, C1, t, t);


            Line line = Line.Unset;
            if (ci.Count > 0) {

                Rhino.Geometry.Intersect.IntersectionEvent intersection = ci[0];
                return new CCX() { t0 = intersection.ParameterA, t1 = intersection.ParameterB, p0 = intersection.PointA, p1 = intersection.PointB };
            }

            return null;
        }

        /// <summary>
        ///  Flip curve comparing the angular difference between n tangent on both curves
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="crv"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Curve GuideCurveDirection(Curve guid, Curve crv, int n = 10) {


            Curve x = guid.DuplicateCurve();
            Curve y = crv.DuplicateCurve();

            Curve r = crv.DuplicateCurve();
            r.Reverse();
            var px = x.DivideCurve(n);
            var py = y.DivideCurve(n);
            var pr = r.DivideCurve(n);
            //Rhino.RhinoApp.WriteLine(px.Length.ToString());


            double tot1 = 0;
            double tot2 = 0;


            for (int i = 0; i < n; i++) {
                var tx = x.CurveTangent(x.CurveClosestPoint(px[i]));
                var ty = y.CurveTangent(y.CurveClosestPoint(py[i]));
                var tr = r.CurveTangent(r.CurveClosestPoint(pr[i]));
                tot1 += Vector3d.VectorAngle(tx, ty);
                tot2 += Vector3d.VectorAngle(tx, tr);

            }


            if (tot1 > tot2)
                y.Reverse();
            return y;

        }

        public static Vector3d CurveTangent(this Curve curve, double parameter) {
            if (curve.Domain.IncludesParameter(parameter))
                return curve.TangentAt(parameter);
            return Vector3d.Unset;
        }

        public static double CurveClosestPoint(this Curve curve, Point3d p) {
            if (curve.ClosestPoint(p, out double t))
                return t;
            return -1;
        }

        public static Point3d[] DivideCurve(this Curve c, int n, bool ends = true) {
            Point3d[] p = new Point3d[0];
            c.DivideByCount(n, ends, out p);
            return p;
        }

        public static Line ExtendLine(Line l, double d) {

            Point3d p0 = l.From;
            Point3d p1 = l.To;

            return new Line(p0 - l.UnitTangent * d, p1 + l.UnitTangent * d);
        }

        public static Curve BooleanOpenCurve(Curve ClosedC, Curve C, out int result) {



            PointContainment cont0 = ClosedC.Contains(C.PointAtEnd, Plane.WorldXY, 0.01);
            PointContainment cont1 = ClosedC.Contains(C.PointAtStart, Plane.WorldXY, 0.01);


            bool isCurveInsideStart = (cont0 == PointContainment.Inside || cont0 == PointContainment.Coincident);
            bool isCurveInsideEnd = (cont1 == PointContainment.Inside || cont1 == PointContainment.Coincident);

            Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurveCurve(C, ClosedC, 0.01, 0.01);
            Interval interval = C.Domain;


            if (isCurveInsideStart && isCurveInsideEnd) {
                result = 2;
                return C;
            } else if (isCurveInsideStart && !isCurveInsideEnd) {
                result = 1;
                if (ci.Count == 0) {
                    result = 0;
                    return null;
                }
                return C.Trim(ci[0].ParameterA, interval.T0);
            } else if (!isCurveInsideStart && isCurveInsideEnd) {
                if (ci.Count == 0) {
                    result = 0;
                    return null;
                }
                result = 1;
                return C.Trim(interval.T1, ci[0].ParameterA);
            } else {
                if (ci.Count == 2) {

                    result = 1;
                    return C.Trim(ci[0].ParameterA, ci[1].ParameterA);
                }
            }
            result = 0;
            return null;
        }


    }
}
