﻿
//https://github.com/arendvw/clipper
#region

using System;
using System.Collections.Generic;
using System.Linq;
using NGonsCore.Clipper642;
using Rhino;
using Rhino.Geometry;

#endregion


// ReSharper disable once CheckNamespace


namespace NGonsCore.Geometry {

    /// <summary>
    /// /// This file contains the glue to connect the Clipper library to RhinoCommon
    /// It depends only on rhinoCommon and the clipper library (included)
    /// </summary>
    /// 

    public static class RhinClip {

        public static Tuple<List<Polyline>,List<Polyline>,List<Polyline>> BooleanIntersection(List<Curve> curves, List<Curve> cutters, double dist, double scale =1e10) {
            //http://www.angusj.com/delphi/clipper/documentation/Docs/Overview/Example.htm

            //  A = NGonsCore.Geometry.Polyline3D.Boolean(ClipType.ctIntersection, polylines, cutters, Plane.WorldXY, 1 / 1e10, true);

            //this.Component.Message = "Boolean Intersection";



            List<Polyline> output = new List<Polyline>();


            List<Polyline> cutters_ = CurvesToPolylines(cutters, dist);

            List<Polyline> innerPolygons = new List<Polyline>();
            List<Polyline> edgePolygons = new List<Polyline>();
            List<Polyline> edgePolygons_ = new List<Polyline>();



            for (int j = 0; j < curves.Count; j++) {

                Polyline polyline = CurveToPolyline(curves[j]);

                NGonsCore.Clipper642.Clipper clipper = new NGonsCore.Clipper642.Clipper();

                var cutter = PolylineToIntPoints(cutters_, scale);
                clipper.AddPaths(cutter, PolyType.ptClip, true);

                var input = PolylineToIntPoint(polyline, scale);
                clipper.AddPath(input, PolyType.ptSubject, true);

                /*
                foreach(IntPoint intpt in input){
                  NGonsCore.Clipper642.Clipper.PointInPolygon(intpt, cutter);
                }
           */


                List<List<IntPoint>> solution = new List<List<IntPoint>>();
                clipper.Execute(ClipType.ctIntersection, solution);

                solution = NGonsCore.Clipper642.Clipper.SimplifyPolygons(solution, PolyFillType.pftEvenOdd);

                List<Polyline> polys = IntPointToPolylines(solution, scale);
                output.AddRange(polys);



                if (polys.Count > 0) {
                    if (polys.Count == polyline.Count) {
                        edgePolygons.AddRange(polys);
                        edgePolygons_.Add(polyline);
                    } else {
                        if (Math.Abs((polys[0].Length - polyline.Length)) < 0.01 && polys[0].Count == polyline.Count) {
                            innerPolygons.Add(polys[0]);
                            //innerPolygons.Add(j);
                        } else {
                            edgePolygons.AddRange(polys);
                            edgePolygons_.Add(polyline);
                            // edgePolygons.Add(j);
                        }
                    }
                }

            }
            //innerPolylines = innerPolygons;
            //edgePolylinesCut = edgePolygons;
            //edgePolylines = edgePolygons_;

            //   A = cutters_;

            return new Tuple<List<Polyline>, List<Polyline>, List<Polyline>>(innerPolygons, edgePolygons, edgePolygons_);





        }

        public static List<Polyline> BooleanDifference(List<Curve> curves, List<Curve> cutters, double dist, double scale = 1e10) {
            //http://www.angusj.com/delphi/clipper/documentation/Docs/Overview/Example.htm

            //  A = NGonsCore.Geometry.Polyline3D.Boolean(ClipType.ctIntersection, polylines, cutters, Plane.WorldXY, 1 / 1e10, true);

            List<Polyline> output = new List<Polyline>();


            List<Polyline> cutters_ = CurvesToPolylines(cutters, dist);
            List<Polyline> polylines_ = CurvesToPolylines(curves, dist);

            Plane cutterPlane = new Plane();
            Transform xform = Transform.PlaneToPlane(cutterPlane,Plane.WorldXY);



            NGonsCore.Clipper642.Clipper clipper = new NGonsCore.Clipper642.Clipper();

            var cutter = PolylineToIntPoints(cutters_, scale);
            clipper.AddPaths(cutter, PolyType.ptClip, true);

            var input = PolylineToIntPoints(polylines_, scale);
            clipper.AddPaths(input, PolyType.ptSubject, true);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctDifference, solution);

            List<Polyline> polys = IntPointToPolylines(solution, scale);



            output.AddRange(polys);

            return output;
        }


        public static Polyline SimplifyCurve(Curve C, double scale = 1e10) {
            Polyline polyline = CurveToPolyline(C, 0);

            Plane plane;
            Plane.FitPlaneToPoints(polyline, out plane);
            Transform xform = Transform.PlaneToPlane(plane, Plane.WorldXY);
            polyline.Transform(xform);

            var input = PolylineToIntPoint(polyline, scale);
            var solution = NGonsCore.Clipper642.Clipper.SimplifyPolygon(input, PolyFillType.pftEvenOdd);

            List<Polyline> polys = IntPointToPolylines(solution, scale);

            Polyline simplifiedPolyline = new Polyline();
            double len = -1;
            foreach (Polyline pp in polys) {
                double tempLen = pp.Length;
                if (tempLen > len) {
                    simplifiedPolyline = pp;
                    len = tempLen;
                }
            }

            xform.TryGetInverse(out xform);
            simplifiedPolyline.Transform(xform);

            // P = simplifiedPolyline;

            return simplifiedPolyline;

        }


        public static List<Polyline> CurvesToPolylines(List<Curve> c, double dist = 0) {

            List<Polyline> polylines = new List<Polyline>(c.Count);
            foreach (Curve curve in c) {
                polylines.Add(CurveToPolyline(curve, dist));
            }
            return polylines;
        }

        public static Polyline CurveToPolyline(Curve c, double dist = 0) {

            Polyline polyline = new Polyline();



            bool flag = c.TryGetPolyline(out polyline);
            if (!flag) {
                if (dist <= 0) {

                    PolylineCurve pc = c.ToPolyline(c.SpanCount, 0, 0, 0);
                    polyline = pc.ToPolyline();
                    //Point3d[] pts;
                    //c.DivideByCount(c.SpanCount, true, out pts);
                    //polyline = new Polyline(pts);
                } else {
                    //        Point3d[] pts;
                    //        c.DivideByCount(Math.Min((int) Math.Ceiling(c.GetLength() / dist), 100), true, out pts);
                    //        polyline = new Polyline(pts);

                    PolylineCurve pc = c.ToPolyline(0, 0, 0, dist);
                    polyline = pc.ToPolyline();
                }
                polyline.Add(polyline[0]);
            }
            return polyline;
        }

        // double scale = 1e10;
        public static List<List<IntPoint>> PolylineToIntPoints(List<Polyline> p, double scale = 1e10) {



            List<List<IntPoint>> polygons = new List<List<IntPoint>>();
            foreach (Polyline pp in p) {
                polygons.Add(PolylineToIntPoint(pp, scale));
            }

            return polygons;
        }

        public static List<IntPoint> PolylineToIntPoint(Polyline p, double scale = 1e10) {

            List<IntPoint> polygon = new List<IntPoint>();
            int closed = (p[0].DistanceToSquared(p[p.Count - 1]) < 0.001) ? 1 : 0;

            for (int i = 0; i < p.Count - closed; i++) {
                polygon.Add(new IntPoint(p[i].X * scale, p[i].Y * scale));
            }
            return polygon;
        }



        public static List<Polyline> IntPointToPolylines(List<List<IntPoint>> p, double scale = 1e10) {
            List<Polyline> polygons = new List<Polyline>();
            foreach (List<IntPoint> pp in p) {
                polygons.Add(IntPointToPolyline(pp, scale));
            }
            return polygons;
        }

        public static Polyline IntPointToPolyline(List<IntPoint> p, double scale = 1e10) {

            Polyline polygon = new Polyline();



            for (int i = 0; i < p.Count; i++) {
                polygon.Add(new Point3d(p[i].X / scale, p[i].Y / scale, 0));
            }

            // Print((polygon[0].DistanceToSquared(polygon[polygon.Count - 1])).ToString());

            // if(polygon[0].DistanceToSquared(polygon[polygon.Count - 1]) < 0.001)
            polygon.Add(polygon[0]);

            return polygon;
        }





    }


    public static class PolyNodeHelper {




        // C# Generator sweetness to handle recursion
        /// <summary>
        ///   flatten a polynode tree, return each item.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<PolyNode> Iterate(this PolyNode node) {
            yield return node;
            foreach (PolyNode childNode in node.Childs) {
                foreach (PolyNode childNodeItem in childNode.Iterate()) {
                    yield return childNodeItem;
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for a 3D polyline
    /// </summary>
    public static class Polyline3D {
        #region ClosedFilletType enum

        /// <summary>
        /// 
        /// </summary>
        public enum ClosedFilletType {
            Round,
            Square,
            Miter
        }

        #endregion

        #region OpenFilletType enum

        /// <summary>
        /// 
        /// </summary>
        public enum OpenFilletType {
            Round,
            Square,
            Butt
        }

        #endregion

        /// <summary>
        /// Get a plane from a polyline
        /// </summary>
        /// <param name="crv">The CRV.</param>
        /// <returns></returns>
        public static Plane FitPlane(this Polyline crv) {
            Plane pln;
            Plane.FitPlaneToPoints(crv, out pln);
            return pln;
        }

        /// <summary>
        /// Converts the curves to polyline.
        /// </summary>
        /// <param name="crvs">The CRVS.</param>
        /// <returns></returns>
        public static IEnumerable<Polyline> ConvertCurvesToPolyline(IEnumerable<Curve> crvs) {
            foreach (Curve c in crvs) {
                Polyline pl;
                if (ConvertCurveToPolyline(c, out pl)) {
                    yield return pl;
                }
            }
        }

        /// <summary>
        /// Converts the curve to polyline.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="pl">The pl.</param>
        /// <returns></returns>
        public static bool ConvertCurveToPolyline(Curve c, out Polyline pl) {
            if (c.TryGetPolyline(out pl)) {
                return pl.IsValid && !(pl.Length < RhinoMath.ZeroTolerance);
            }
            // default options.. should perhaps do something with the document tolerance.
            PolylineCurve polylineCurve = c.ToPolyline(0, 0, 0.1, 0, 0, 0, 0, 0, true);
            if (polylineCurve == null) {
                //Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert brep edge to polyline");
                return false;
            }
            if (!polylineCurve.TryGetPolyline(out pl)) {
                //Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert brep edge to polyline - weird shizzle");
                return false;
            }
            return pl.IsValid && !(pl.Length < RhinoMath.ZeroTolerance);
        }

        /// <summary>
        ///   Offset a polyline with a distance.
        /// </summary>
        /// <param name="polyline">polyline input</param>
        /// <param name="distance">offset distance</param>
        /// <param name="outContour">Outer offsets</param>
        /// <param name="outHole">Innter offsets</param>
        public static void Offset(this Polyline polyline, double distance, out List<Polyline> outContour,
          out List<Polyline> outHole) {
            Offset(new Polyline[] { polyline }, distance, out outContour, out outHole);
        }

        /// <summary>
        ///   Offset a list of polylines with a distance
        /// </summary>
        /// <param name="polylines">Input polylines</param>
        /// <param name="distance">Distance to offset</param>
        /// <param name="outContour"></param>
        /// <param name="outHole"></param>
        public static void Offset(IEnumerable<Polyline> polylines, double distance, out List<Polyline> outContour,
          out List<Polyline> outHole) {
            // ReSharper disable once PossibleMultipleEnumeration
            Polyline pl = polylines.First();
            Plane pln = pl.FitPlane();

            //
            // ReSharper disable once PossibleMultipleEnumeration
            Offset(polylines, OpenFilletType.Square, ClosedFilletType.Square, distance, pln, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out outContour, out outHole);
        }

        public static Polyline[] Offset(this IEnumerable<Polyline> polylines, double distance) {

            Polyline[] polylinesOffset = new Polyline[polylines.Count()];




            if (distance != 0) {
                List<Polyline> c0;
                List<Polyline> c1;

                int i = 0;

                foreach(Polyline p in polylines) { 


                    NGonsCore.Geometry.Polyline3D.Offset(p, Math.Abs(distance), out c0, out c1);

                    if (distance < 0) {
                        polylinesOffset[i]=(c1[0]);
                    } else {
                        polylinesOffset[i] = (c0[0]);
                    }

                    i++;

                }


                return polylinesOffset;

            }

            return polylines.ToArray();


        }

        /// <summary>
        /// Offsets the specified polylines.
        /// </summary>
        /// <param name="polylines">The polylines.</param>
        /// <param name="openFilletType">Type of the open fillet.</param>
        /// <param name="closedFilletType">Type of the closed fillet.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="plane">The plane.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="outContour">The out contour.</param>
        /// <param name="outHole">The out hole.</param>
        public static void Offset(IEnumerable<Polyline> polylines, OpenFilletType openFilletType,
          ClosedFilletType closedFilletType, double distance, Plane plane, double tolerance, out List<Polyline> outContour,
          out List<Polyline> outHole) {
            List<List<Polyline>> outContours;
            List<List<Polyline>> outHoles;
            Offset(polylines, new List<OpenFilletType> { openFilletType }, new List<ClosedFilletType> { closedFilletType }, plane,
              tolerance, new List<double> { distance }, 2, 0.25, out outContours, out outHoles);
            outContour = outContours[0];
            outHole = outHoles[0];
        }

        /// <summary>
        /// Determines whether the specified polyline is inside.
        /// </summary>
        /// <param name="polyline">The polyline.</param>
        /// <param name="point">The point.</param>
        /// <param name="pln">The PLN.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public static int IsInside(this Polyline polyline, Point3d point, Plane pln, Double tolerance) {
            return NGonsCore.Clipper642.Clipper.PointInPolygon(point.ToIntPoint2D(pln, tolerance), polyline.ToPath2D(pln, tolerance));
        }

        /// <summary>
        /// Offsets the specified polylines.
        /// </summary>
        /// <param name="polylines">A list of polylines</param>
        /// <param name="openFilletType">Optional: line endtype (Butt, Square, Round)</param>
        /// <param name="closedFilltetType">Optional: join type: Round, Miter (uses miter parameter) or Square</param>
        /// <param name="plane">Plane to project the polylines to</param>
        /// <param name="tolerance">Tolerance: Cutoff point. Eg. point {1.245; 9.244351; 19.3214} with precision {0.1} will be cut
        /// off to {1.2; 9.2; 19.3}.</param>
        /// <param name="distance">Distances to offset set of shapes.</param>
        /// <param name="miter">Miter deterimines how long narrow spikes can become before they are cut off: A miter setting of 2
        /// means not longer than 2 times the offset distance. A miter of 25 will give big spikes.</param>
        /// <param name="arcTolerance">The arc tolerance.</param>
        /// <param name="outContour">The out contour.</param>
        /// <param name="outHoles">The out holes.</param>
        public static void Offset(IEnumerable<Polyline> polylines, List<OpenFilletType> openFilletType,
          List<ClosedFilletType> closedFilltetType, Plane plane, double tolerance, IEnumerable<double> distance,
          double miter, double arcTolerance, out List<List<Polyline>> outContour, out List<List<Polyline>> outHoles) {
            outContour = new List<List<Polyline>>();
            outHoles = new List<List<Polyline>>();
            /*
             * iEndType: How to handle open ended polygons.
             * Open				Closed
             * etOpenSquare		etClosedLine    (fill inside & outside)
             * etOpenRound			etClosedPolygon (fill outside only)
             * etOpenButt
             * 
             * See: http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/EndType.htm
             */

            /*
             * jtJoinType
             * How to fill angles of closed polygons
             * jtRound: Round
             * jtMiter: Square with variable distance
             * jtSquare: Square with fixed distance (jtMiter = 1)
             */

            ClipperOffset cOffset = new ClipperOffset(miter, arcTolerance);
            int i = 0;
            foreach (Polyline pl in polylines) {
                EndType et = EndType.etOpenButt;
                JoinType jt = JoinType.jtSquare;
                if (pl.IsClosed) {
                    et = EndType.etClosedLine;
                } else if (openFilletType.Count != 0) {
                    OpenFilletType oft = IndexOrLast(openFilletType, i);
                    switch (oft) {
                        case OpenFilletType.Butt:
                        et = EndType.etOpenButt;
                        break;
                        case OpenFilletType.Round:
                        et = EndType.etOpenRound;
                        break;
                        case OpenFilletType.Square:
                        et = EndType.etOpenSquare;
                        break;
                    }
                } else {
                    et = EndType.etOpenButt;
                }

                if (closedFilltetType.Count != 0) {
                    ClosedFilletType cft = IndexOrLast(closedFilltetType, i);
                    switch (cft) {
                        case ClosedFilletType.Miter:
                        jt = JoinType.jtMiter;
                        break;
                        case ClosedFilletType.Round:
                        jt = JoinType.jtRound;
                        break;
                        case ClosedFilletType.Square:
                        jt = JoinType.jtSquare;
                        break;
                    }
                } else {
                    jt = JoinType.jtSquare;
                }
                cOffset.AddPath(pl.ToPath2D(plane, tolerance), jt, et);
                i++;
            }

            foreach (double offsetDistance in distance) {
                PolyTree tree = new PolyTree();
                cOffset.Execute(ref tree, offsetDistance / tolerance);

                List<Polyline> holes = new List<Polyline>();
                List<Polyline> contours = new List<Polyline>();

                foreach (PolyNode path in tree.Iterate()) {
                    if (path.Contour.Count == 0) {
                        continue;
                    }
                    Polyline polyline = path.Contour.ToPolyline(plane, tolerance, !path.IsOpen);
                    if (path.IsHole) {
                        holes.Add(polyline);
                    } else {
                        contours.Add(polyline);
                    }
                }

                outContour.Add(contours);
                outHoles.Add(holes);
            }
        }


        public static List<Polyline> Boolean(ClipType clipType, IEnumerable<Polyline> polyA, IEnumerable<Polyline> polyB,
          Plane pln, double tolerance, bool evenOddFilling) {
            NGonsCore.Clipper642.Clipper clipper = new NGonsCore.Clipper642.Clipper();
            PolyFillType polyfilltype = PolyFillType.pftEvenOdd;
            if (!evenOddFilling) {
                polyfilltype = PolyFillType.pftNonZero;
            }

            foreach (Polyline plA in polyA) {
                clipper.AddPath(plA.ToPath2D(pln, tolerance), PolyType.ptSubject, plA.IsClosed);
            }

            foreach (Polyline plB in polyB) {
                clipper.AddPath(plB.ToPath2D(pln, tolerance), PolyType.ptClip, true);
            }

            PolyTree polytree = new PolyTree();

            clipper.Execute(clipType, polytree, polyfilltype, polyfilltype);

            List<Polyline> output = new List<Polyline>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (PolyNode pn in polytree.Iterate()) {
                if (pn.Contour.Count > 1) {
                    output.Add(pn.Contour.ToPolyline(pln, tolerance, !pn.IsOpen));
                }
            }

            return output;
        }

        /// <summary>
        /// Indexes the or last.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static T IndexOrLast<T>(List<T> list, int index) {
            if (list.Count - 1 < index) {
                return list.Last();
            }
            return list[index];
        }

        /// <summary>
        ///   Convert a Polyline to a Path2D.
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pln"></param>
        /// <returns></returns>
        public static List<IntPoint> ToPath2D(this Polyline pl, Plane pln) {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            return pl.ToPath2D(pln, tolerance);
        }

        /// <summary>
        ///   Convert a 3D Polygon to a 2D Path
        /// </summary>
        /// <param name="pl">Polyline to convert</param>
        /// <param name="pln">Plane to project the polyline to</param>
        /// <param name="tolerance">The tolerance at which the plane will be converted. A Path2D consists of integers.</param>
        /// <returns>A 2D Polyline Path</returns>
        public static List<IntPoint> ToPath2D(this Polyline pl, Plane pln, double tolerance) {
            List<IntPoint> path = new List<IntPoint>();
            foreach (Point3d pt in pl) {
                path.Add(pt.ToIntPoint2D(pln, tolerance));
            }
            if (pl.IsClosed) {
                path.RemoveAt(pl.Count - 1);
            }
            return path;
        }

        /// <summary>
        /// To the int point2 d.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <param name="pln">The PLN.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        private static IntPoint ToIntPoint2D(this Point3d pt, Plane pln, double tolerance) {
            double s, t;
            pln.ClosestParameter(pt, out s, out t);
            IntPoint point = new IntPoint(s / tolerance, t / tolerance);
            return point;
        }
    }

    /// <summary>
    ///   Extension methods for Path2D
    /// </summary>
    public static class Path2D {
        /// <summary>
        ///   Convert a path to polyline
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pln"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public static Polyline ToPolyline(this List<IntPoint> path, Plane pln, bool closed) {
            return path.ToPolyline(pln, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, closed);
        }

        /// <summary>
        ///   Convert a 2D Path to a 3D Polyline
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pln"></param>
        /// <param name="tolerance"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public static Polyline ToPolyline(this List<IntPoint> path, Plane pln, double tolerance, bool closed) {
            List<Point3d> polylinepts = new List<Point3d>();

            foreach (IntPoint pt in path) {
                polylinepts.Add(pln.PointAt(pt.X * tolerance, pt.Y * tolerance));
            }

            if (closed && path.Count > 0) {
                polylinepts.Add(polylinepts[0]);
            }
            Polyline pl = new Polyline(polylinepts);
            return pl;
        }
    }
}