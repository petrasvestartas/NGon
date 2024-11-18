using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhinoGeometry;
using Grasshopper.Kernel.Data;
using Grasshopper;
using RhinoGeometry.Clipper642;
using NGonCore;

namespace RhinoGeometry {
    public static class Offset {




        public static Polyline[] OffsetParallelograms(Polyline X, Polyline Y, double R) {

            Polyline x = new Polyline(X); X.MergeColinearSegments(0.01, true);
            Polyline y = new Polyline(Y); Y.MergeColinearSegments(0.01, true);

            //////////////////////////////////////////////////////////////////////////
            //Get edge planes that are offseted
            //////////////////////////////////////////////////////////////////////////
            List<Plane> offsetPlanes = new List<Plane>();
            for (int i = 0; i < x.Count - 1; i++) {
                Point3d c = (x[i] + x[i + 1]) * 0.5;
                Vector3d XAxis = x[i].DistanceTo(x[i + 1]) < 0.01 ? y[i] - y[i + 1] : x[i] - x[i + 1];
                Vector3d YAxis = x[i] - y[i];
                Vector3d ZAxis = Vector3d.CrossProduct(XAxis, YAxis);
                ZAxis.Unitize();
                Plane plane = new Plane(c + ZAxis * R, XAxis, YAxis);
                offsetPlanes.Add(plane);
            }



            //////////////////////////////////////////////////////////////////////////
            //Intersect plane one by one
            //////////////////////////////////////////////////////////////////////////
            Plane fitPlane0 = Plane.Unset;
            Plane fitPlane1 = Plane.Unset;
            Plane.FitPlaneToPoints(x, out fitPlane0);
            Plane.FitPlaneToPoints(y, out fitPlane1);
            Polyline offsetPolyline0 = new Polyline();
            Polyline offsetPolyline1 = new Polyline();
            int closed = x.IsClosed ? 0 : 1;
            for (int i = 0; i < offsetPlanes.Count - closed; i++) {

                Line l = Line.Unset;
                Rhino.Geometry.Intersect.Intersection.PlanePlane(offsetPlanes[i], offsetPlanes[(i + 1) % offsetPlanes.Count], out l);
                double t;
                Rhino.Geometry.Intersect.Intersection.LinePlane(l, fitPlane0, out t);
                offsetPolyline0.Add(l.PointAt(t));
                Rhino.Geometry.Intersect.Intersection.LinePlane(l, fitPlane1, out t);
                offsetPolyline1.Add(l.PointAt(t));
            }

            if (!x.IsClosed) {

                Line l = Line.Unset;

                Rhino.Geometry.Intersect.Intersection.PlanePlane(fitPlane0, offsetPlanes[0], out l);
                offsetPolyline0.Insert(0, l.ClosestPoint(x[0], false));
                Rhino.Geometry.Intersect.Intersection.PlanePlane(fitPlane0, offsetPlanes[offsetPlanes.Count - 1], out l);
                offsetPolyline0.Add(l.ClosestPoint(x[x.Count - 1], false));

                Rhino.Geometry.Intersect.Intersection.PlanePlane(fitPlane1, offsetPlanes[0], out l);
                offsetPolyline1.Insert(0, l.ClosestPoint(y[0], false));
                Rhino.Geometry.Intersect.Intersection.PlanePlane(fitPlane1, offsetPlanes[offsetPlanes.Count - 1], out l);
                offsetPolyline1.Add(l.ClosestPoint(y[y.Count - 1], false));
            } else {
                offsetPolyline0.Add(offsetPolyline0[0]);
                offsetPolyline1.Add(offsetPolyline1[0]);
            }

            //////////////////////////////////////////////////////////////////////////
            //Output
            //////////////////////////////////////////////////////////////////////////

            //A = offsetPlanes;
            return new Polyline[] { offsetPolyline0, offsetPolyline1 };
        }

        public static Polyline OffsetRectangle(Polyline ClosedPolyline, double dist) {



            Polyline pline = new Polyline();

            Line[] lines = ClosedPolyline.SegmentAt(0).Length > ClosedPolyline.SegmentAt(1).Length ? 
                new Line[] { ClosedPolyline.SegmentAt(0), ClosedPolyline.SegmentAt(2) } : 
                new Line[] { ClosedPolyline.SegmentAt(1), ClosedPolyline.SegmentAt(3) };

            lines[1].Flip();

            double distanceBetweenLines = lines[0].From.DistanceTo(lines[1].From);
            double step = Math.Max(1, Math.Ceiling(distanceBetweenLines / dist));
            //(new Line(lines[0].From, lines[1].From)).Bake();
            //lines[0].Bake();
            //lines[1].Bake();
            step += step % 2;
     
           
            //lines.Bake();

            Point3d[] pts1 = PointUtil.InterpolatePoints(lines[0].From, lines[1].From, (int)step, true);
            Point3d[] pts0 = PointUtil.InterpolatePoints(lines[0].To, lines[1].To, (int)step, true);
            //pts0.Bake();

            //List<Polyline> interpolatedLines = PolylineUtil.InterpolateTwoLines(lines[0], lines[1], (int)step);

            for (int i = 0; i < pts0.Length; i++) {
                //pline.Add(pts0[i]);
                //  pline.Add(pts1[i]);
                if (i % 2 ==0) {
                    pline.Add(pts0[i]);
                    pline.Add(pts1[i]);
                    //pline.AddRange(interpolatedLines[i]);
                } else {
                    pline.Add(pts1[i]);
                    pline.Add(pts0[i]);
                    //pline.AddRange(interpolatedLines[i].Flip());
                }

            }
            //pline.Add(ClosedPolyline[3]);

            //pline.AddRange(ClosedPolyline);

           pline.Close();
            pline.AddRange(ClosedPolyline);
            pline.Add(pline[0]);
            pline.CollapseShortSegments(0.01);
          
            //pline.Bake();
            return pline;
        }

        public static List<Polyline> OffsetClipper(List<Polyline> polyline, double dist) {

            Transform transform = GeometryProcessing.OrientTo2D(polyline[0]);
            Transform inverse; transform.TryGetInverse(out inverse);

            List<Polyline> ClosedPolyline2D = TransformPolylines(polyline, transform);

            List<Polyline> ClosedPolylineSorted = SortPolylines(ClosedPolyline2D);
            ClosedPolylineSorted = OffsetPolylines(ClosedPolylineSorted, scale, -Math.Abs(dist));

            for (int i = 0; i < ClosedPolylineSorted.Count; i++)
                ClosedPolylineSorted[i].Transform(inverse);


            return ClosedPolylineSorted;

        }


        public static double scale = 1e7;
        public static  double step = -100;
        public static Random random = new Random(2);

        public static List<Polyline> ClipperOffset(this List<Polyline> ClosedPolylineSorted, double offsetDistance) {

            List<List<IntPoint>> points = PolylineToIntPoints(ClosedPolylineSorted, scale);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            ClipperOffset co = new ClipperOffset();
            co.AddPaths(points, JoinType.jtMiter, EndType.etClosedPolygon);
            co.Execute(ref solution, offsetDistance * 1 * scale);  // important, scale also used here
            

            return IntPointToPolylines(solution, scale);
        }

        public static Tuple<int, double, Line> ClosestPolyline (this Point3d p, List<Polyline> plines) {

            double dist = default(double);
            int id = -1;
            double t = 0;
            Line line = Line.Unset;

            for (int i = 0; i < plines.Count; i++) {

               

                double cpT = plines[i].ClosestParameter(p);
     
                    Point3d cp = plines[i].PointAt(cpT);
                double distCurrent = cp.DistanceToSquared(p);
                if (distCurrent < dist || dist == default(double)) {
                    dist = distCurrent;
                    id = i;
                    t = cpT;
                    line = new Line(p,(cp+p)*0.5);
                }
            }

            return new Tuple<int, double, Line>(id,t,line);

        }

        public static  DataTree<Polyline>ClipperOffset2DPolylineNTimes(this List<Polyline> ClosedPolylineSorted, double offsetDistance, int n = 100) {


            List<Polyline> Plines = new List < Polyline > (ClosedPolylineSorted);

            //Create dataTree to track the offset
            DataTree<Polyline> dtOffset = new DataTree<Polyline>();
            DataTree<int> dtOffsetNei = new DataTree<int>();
            for (int i = 0; i < Plines.Count; i++) {
                dtOffset.Add(Plines[i], new GH_Path(0));
            }


            List<List<IntPoint>> points = PolylineToIntPoints(Plines, scale);


            for (int i = 0; i < n; i++) {

                //Perform clipper offset
                ClipperOffset co = new ClipperOffset();
                co.AddPaths(points, JoinType.jtMiter, EndType.etClosedPolygon);
                List<List<IntPoint>> solution = new List<List<IntPoint>>();
                co.Execute(ref solution, offsetDistance * 1 * scale);  // important, scale also used here

                if (solution.Count == 0) break;

                //Convert to polylines
                Plines = IntPointToPolylines(solution, scale);
                for (int j = 0; j < Plines.Count; j++) {

                    //check previous and current polylines to know the  neighbours
                    var cp = ClosestPolyline(Plines[j].PointAt(0.0), dtOffset.Branch(i));

                    //dtOffset.Branch(i)[cp.Item1].Insert((int)Math.Ceiling(cp.Item2),cp.Item3.To);

                    //Addregion to previous one
                    //polys[i] = ChangeClosedPolylineSeam(polys[i], seam + (z % 2));
                    //polys[i] = MoveEndPointApart(polys[i], -MoveApart);


                    dtOffset.Add(Plines[j], new GH_Path(i+1));
                    dtOffsetNei.Add(cp.Item1, new GH_Path(i + 1));
                    cp.Item3.Bake();
                }

                //reset intput to start again
                points = solution;

            }


            return dtOffset;
        }

        public static List<Polyline> OffsetMultipleCleanUp(List<Polyline> ClosedPolyline, int Limit, double MoveApart, double OffsetFirst, double Offset) {

            //Get transformation 3D-2D
            Transform transform = GeometryProcessing.OrientTo2D(ClosedPolyline[0]);
            Transform inverse; transform.TryGetInverse(out inverse);

            //Orient polylines and Sort them and offset if needed
            List<Polyline> ClosedPolyline2D = TransformPolylines(ClosedPolyline, transform);
            List<Polyline> ClosedPolylineSorted = SortPolylines(ClosedPolyline2D);
            //ClosedPolylineSorted = OffsetPolylines(ClosedPolylineSorted, scale, -Math.Abs(OffsetFirst));


            //return ClosedPolylineSorted; 

            return ClosedPolylineSorted.ClipperOffset2DPolylineNTimes(-Offset,10).AllData();
        }



            public static Polyline OffsetMultiple (List<Polyline> ClosedPolyline, int Limit, double MoveApart, double OffsetFirst, double Offset, bool softMove = false, bool milling = true) {

            try {


                Transform transform = GeometryProcessing.OrientTo2D(ClosedPolyline[0]);
                Transform inverse; transform.TryGetInverse(out inverse);

                List<Polyline> ClosedPolyline2D = TransformPolylines(ClosedPolyline, transform);
                List<Polyline> ClosedPolylineSorted = SortPolylines(ClosedPolyline2D);
                ClosedPolylineSorted = OffsetPolylines(ClosedPolylineSorted, scale, -Math.Abs(OffsetFirst));
          
                

                step = -Math.Abs(Offset);

                List<List<IntPoint>> points = PolylineToIntPoints(ClosedPolylineSorted, scale);
                List<List<IntPoint>> input = points;
                List<Polyline> out0_ = new List<Polyline>();
                out0_.AddRange(ClosedPolylineSorted);

                DataTree<Polyline> dtClipper = new DataTree<Polyline>();
                List<int> nei = new List<int>();



                for (int i = 0; i < ClosedPolylineSorted.Count; i++) {
                    dtClipper.Add(ClosedPolylineSorted[i], new GH_Path(i));
                }

                int lastCount = ClosedPolylineSorted.Count;
                bool flag = true;



                int z = 0;
                while (input.Count > 0 && z < Limit) {
                    //Rhino.RhinoApp.WriteLine(input.Count.ToString());

                    //out0_.Bake();

                    List<List<IntPoint>> solution = new List<List<IntPoint>>();
                    ClipperOffset co = new ClipperOffset();
                    co.AddPaths(input, JoinType.jtMiter, EndType.etClosedPolygon);
                    if(!milling)
                        co.Execute(ref solution, step * 1 * scale);  // important, scale also used here
                    else
                        co.Execute(ref solution, step * 1 * scale);  // important, scale also used here
                    List<Polyline> polys = IntPointToPolylines(solution, scale);

               

                    double seam = (softMove) ? 0.5 : 0.01;

                    for (int i = 0; i < solution.Count; i++) {

                        //Find the closest region
                        int id = 0;
                        Point3d pt = polys[i].PointAt(seam + (z % 2));
                        Point3d cp = ClosedPolylineSorted[0].ClosestPoint(pt);
                        double sqDist = pt.DistanceToSquared(cp);

                        for (int j = 1; j < ClosedPolylineSorted.Count; j++) {

                            cp = ClosedPolylineSorted[j].ClosestPoint(pt);
                            double sqDistCurr = pt.DistanceToSquared(cp);

                            if (sqDistCurr < sqDist) {
                                id = j;
                                sqDist = sqDistCurr;
                            }

                        }

                        //Add region to previous one
                        polys[i] = ChangeClosedPolylineSeam(polys[i], seam + (z % 2));
                        polys[i] = MoveEndPointApart(polys[i], -MoveApart);


                        if (z % 2 == 1)
                            seam=(softMove) ? 0.5 : 0.01;
                        else
                            seam=(softMove) ? 0.5 : (1-0.01);

                        InsertPolyline(ClosedPolylineSorted[id], polys[i]);
                        //Rhino.RhinoApp.WriteLine(polys[i].Count.ToString());
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(ClosedPolylineSorted[id]);
                        //Rhino.RhinoApp.WriteLine(ClosedPolylineSorted[id].Count.ToString());

                    }

                   
                    input = solution;
                    z++;
                }












                //Change seam to outwards
                for (int i = 0; i < ClosedPolylineSorted.Count; i++) {
                    ClosedPolylineSorted[i].CollapseShortSegments(0.01);
                    ClosedPolylineSorted[i].DeleteShortSegments(0.01);
                    Point3d pt = ClosedPolylineSorted[i].BoundingBox.PointAt(0, 0, 0);
                    pt = ClosedPolylineSorted[i].PointAt(0);
                    double t = ClosedPolylineSorted[i].ClosestParameter(pt);
                    //ClosedPolylineSorted[i] = ChangeClosedPolylineSeam(ClosedPolylineSorted[i], t);
                }

                //Find closest polyline and attach to it

                Polyline spiral = new Polyline(ClosedPolylineSorted[0]);
                ClosedPolylineSorted.RemoveAt(0);
                int zz = 0;


                while (ClosedPolylineSorted.Count > 0 && zz < 3) {

                    int id = 0;
                    Point3d pt = ClosedPolylineSorted[0][0];
                    Point3d cp = spiral.ClosestPoint(pt);
                    double sqDist = pt.DistanceToSquared(cp);

                    for (int j = 1; j < ClosedPolylineSorted.Count; j++) {
                        pt = ClosedPolylineSorted[j][0];
                        cp = spiral.ClosestPoint(ClosedPolylineSorted[j][0]);
                        double sqDistCurr = pt.DistanceToSquared(cp);
                        if (sqDistCurr < sqDist) {
                            id = j;
                            sqDist = sqDistCurr;
                        }
                    }

                    //Rhino.RhinoApp.WriteLine(id.ToString());
                    ClosedPolylineSorted[id] = MoveEndPointApart(ClosedPolylineSorted[id], -MoveApart);
                    InsertPolyline(spiral, ClosedPolylineSorted[id]);
                    ClosedPolylineSorted.RemoveAt(id);
                    zz++;
                }


                // Component.Message = z.ToString();

                //spiral.MergeColinearSegments(0.01, true);
                spiral.CollapseShortSegments(0.01);
                spiral.DeleteShortSegments(0.01);

                spiral.Transform(inverse);
                // A = spiral;
                return spiral;

            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            return new Polyline();
        }




        public static Polyline[] OffsetPolyline(Polyline c0, Polyline c1, double toolr = 10, int infeed = 1) {

            if (toolr == 0)
                return new Polyline[] { c0, c1 };

            Transform transform = GeometryProcessing.OrientTo2D(c0);

            Polyline ply0 = new Polyline(c0);
            ply0.Transform(transform);

            Polyline ply1 = new Polyline(c1);
            ply1.Transform(transform);

            Transform inverse; transform.TryGetInverse(out inverse);


            Plane ply0Plane = GeometryProcessing.GetPlane(ply0);
            Plane ply1Plane = GeometryProcessing.GetPlane(ply1);

            Polyline pl0Offset = new Polyline();
            Polyline pl1Offset = new Polyline();

            string orient = (GeometryProcessing.IsClockwiseClosedPolylineOnXYPlane(ply0)) ? "CounterClockwise" : "Clockwise";


            if (ply0.IsValid && ply1.IsValid) {

                Point3d[] vrts = ply0.ToArray();
                Point3d[] uvrts = ply1.ToArray();

                Plane notPairingPlane;
                Plane.FitPlaneToPoints(vrts, out notPairingPlane);


                for (int k = infeed; k >= 1; k--)                           // iterate infeeds
                {
                    //ncc.Add("(start infeed no" + k.ToString() + ")");
                    for (int i = 0; i != vrts.Length - 1; i++)              // iterate segments
                    {
                        Point3d p0 = vrts[i]; //#always
                        Point3d p0u = uvrts[i];
                        Point3d p2b = new Point3d();
                        Point3d p1b = new Point3d();
                        Point3d p1 = new Point3d();
                        Point3d p2 = new Point3d();
                        Point3d p1u = new Point3d();

                        if (i == 0) {
                            p2b = vrts[vrts.Length - 3];
                            p1b = vrts[vrts.Length - 2];
                            p1 = vrts[i + 1];
                            p2 = vrts[i + 2];
                            p1u = uvrts[i + 1];
                        } else if (i == 1) {
                            p2b = vrts[vrts.Length - 2];
                            p1b = vrts[i - 1];
                            p1 = vrts[i + 1];
                            p2 = vrts[i + 2];
                            p1u = uvrts[i + 1];
                        } else if (i == vrts.Length - 2) {
                            p2b = vrts[i - 2];
                            p1b = vrts[i - 1];
                            p1 = vrts[0];
                            p2 = vrts[1];
                            p1u = uvrts[0];
                        } else if (i == vrts.Length - 3) {
                            p2b = vrts[i - 2];
                            p1b = vrts[i - 1];
                            p1 = vrts[i + 1];
                            p2 = vrts[0];
                            p1u = uvrts[i + 1];
                        } else {
                            p2b = vrts[i - 2];
                            p1b = vrts[i - 1];
                            p1 = vrts[i + 1];
                            p2 = vrts[i + 2];
                            p1u = uvrts[i + 1];
                        }

                        // ## DET TOOLPATH

                        Vector3d n1b = Rhino.Geometry.Vector3d.CrossProduct(p1b - p0, p0u - p0);        //#Srf Normal (last)
                        Vector3d n0 = Rhino.Geometry.Vector3d.CrossProduct(p0 - p1, p0u - p0);          //#Srf Normal (current)
                        Vector3d n1 = Rhino.Geometry.Vector3d.CrossProduct(p2 - p1, p1u - p1);          //#Srf Normal (next)
                        n1b.Unitize();
                        n1b *= toolr * -1;
                        n0.Unitize();
                        n0 *= toolr;
                        n1.Unitize();
                        n1 *= toolr;



                        Plane pl0 = new Plane(p0, (n1b + n0) / 2);                         //# ext bisector plane last/current
                        Plane pl1 = new Plane(p1, ((n0 + n1) / 2));                        //# ext bisector plane current/next




                        Line ln0 = new Line(p0 + (n0 * -1), p1 + (n0 * -1));                                     //# toolpath Line

                        double pm0;
                        double pm1;
                        Rhino.Geometry.Intersect.Intersection.LinePlane(ln0, pl0, out pm0);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(ln0, pl1, out pm1);
                        Point3d pt0 = ln0.PointAt(pm0);                                                 //# intersection with Plane 0
                        Point3d pt1 = ln0.PointAt(pm1);                                                 //# intersection with Plane 1






                        Point3d pt6 = new Point3d();
                        bool boolN = false;

                        Vector3d n44 = p1u - p1;
                        n44.Unitize();
                        Point3d ptXX = pt1 + n44 * 45;

                        Line XXA = new Line(p1, p1u);

                        Point3d ptC = XXA.ClosestPoint(pt1, false);
                        double l0 = ptC.DistanceTo(pt1) - toolr;                                        //# offset dist
                        Vector3d nnn = ptC - pt1;
                        nnn.Unitize();
                        Point3d pt3 = pt1 + nnn * l0;
                        Point3d pt4 = pt3 + (p1u - p1);
                        Line ln1 = new Line(pt3, pt4);                                                 //# cylinder axis

                        //## IDENTIFY INSIDE CORNERS
                        Vector3d r1l = p2b - p1b;                                 //# last back
                        Vector3d r1n = p0 - p1b;                                  //# last front
                        Vector3d al = p1b - p0;                                   //# current back
                        Vector3d an = p1 - p0;                                    //# current front
                        Vector3d bl = p0 - p1;                                    //# next back
                        Vector3d bn = p2 - p1;                                    //# next front
                        r1l.Unitize();
                        r1n.Unitize();
                        al.Unitize();
                        an.Unitize();
                        bl.Unitize();
                        bn.Unitize();

                        Vector3d cpr1 = Vector3d.CrossProduct(r1l, r1n);                           //# +- look 1 back
                        Vector3d cp0 = Vector3d.CrossProduct(al, an);                              //# +- look current
                        Vector3d cp1 = Vector3d.CrossProduct(bl, bn);                              //# +- look 1 ahead


                        if (orient == "Clockwise") {
                            if (cpr1.Z < 0 && cp0.Z < 0 && cp1.Z > 0)     //# --+
                                boolN = true;
                            else if (cpr1.Z < 0 && cp0.Z > 0 && cp1.Z > 0)   //# -++
                                boolN = true;
                            else if (cpr1.Z > 0 && cp0.Z < 0 && cp1.Z > 0)       //# +-+
                                boolN = true;
                        } else if (orient == "CounterClockwise") {
                            if (cp0.Z > 0)       //# +-+
                                boolN = true;
                        }



                        Point3d[] pts = { pt0, pt1, pt1 + (p1u - p1), pt0 + (p0u - p0) };

                        Vector3d nh0 = ((pts[3] - pts[0]) / infeed) * (k - 1);
                        Vector3d nh1 = ((pts[2] - pts[1]) / infeed) * (k - 1);

                        Point3d p21 = pts[1] + nh1;  // infeed pts
                        Point3d p30 = pts[0] + nh0;



                        int IPDtemp = 3;                                                                        // division for sim mach
                        if (Math.Abs(pts[0].DistanceTo(pts[1])) <= Math.Abs(pts[3].DistanceTo(pts[2])) + 0.5 && Math.Abs(pts[0].DistanceTo(pts[1])) >= Math.Abs(pts[3].DistanceTo(pts[2])) - 0.5) {
                            IPDtemp = 1;                                                                        // simple cut
                        }

                        double t;
                        Line line = new Line(pts[0], pts[3]);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(line, ply0Plane, out t);
                        pl0Offset.Add(line.PointAt(t));
                        Rhino.Geometry.Intersect.Intersection.LinePlane(line, ply1Plane, out t);
                        pl1Offset.Add(line.PointAt(t));

                    }
                }
            }

            pl0Offset.Add(pl0Offset[0]);
            pl1Offset.Add(pl1Offset[0]);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pl0Offset);
            pl0Offset.Transform(inverse);
            pl1Offset.Transform(inverse);
            return new Polyline[] { pl0Offset, pl1Offset };

        }








        public static List<Polyline> OffsetPolylines(List<Polyline> polylines, double scale, double d) {

            var input = PolylineToIntPoints(polylines, scale);

            List<List<IntPoint>> solution_ = new List<List<IntPoint>>();
            ClipperOffset co_ = new ClipperOffset();
            co_.AddPaths(input, JoinType.jtMiter, EndType.etClosedPolygon);
            co_.Execute(ref solution_, d * 1 * scale);  // important, scale also used here

            List<Polyline> polys = IntPointToPolylines(solution_, scale);
            return polys;

        }

        public static  List<Polyline> TransformPolylines(List<Polyline> polylines, Transform t) {


            List<Polyline> polylinesTransformed = new List<Polyline>();

            for (int i = 0; i < polylines.Count; i++) {

                Polyline temp = new Polyline(polylines[i]);
                temp.Transform(t);
                polylinesTransformed.Add(temp);

            }

            //Transform inverse;  t.TryGetInverse(out inverse);

            return polylinesTransformed;

        }

        public static bool IsClockwiseClosedPolylineOnXYPlane(Polyline polygon) {
            double sum = 0;

            for (int i = 0; i < polygon.Count - 1; i++)
                sum += (polygon[i + 1].X - polygon[i].X) * (polygon[i + 1].Y + polygon[i].Y);

            return sum > 0;
        }

        public static List<Polyline> SortPolylines(List<Polyline> polylines) {



            int id = 0;
            double len = polylines[0].BoundingBox.Diagonal.Length;

            for (int i = 1; i < polylines.Count; i++) {
                double tempLen = polylines[i].BoundingBox.Diagonal.Length;
                if (tempLen > len) {
                    id = i;
                    len = tempLen;
                }
            }

            List<Polyline> sortedPolylines = new List<Polyline> { polylines[id] };

            bool flag = IsClockwiseClosedPolylineOnXYPlane(polylines[id]);

            for (int i = 0; i < polylines.Count; i++) {
                if (i != id) {
                    Polyline polyline = new Polyline(polylines[i]);
                    bool flagTemp = IsClockwiseClosedPolylineOnXYPlane(polyline);
                    if (flag == flagTemp)
                        polyline.Reverse();
                    sortedPolylines.Add(polyline);
                }
            }

            return sortedPolylines;
        }

        public static void InsertPolyline(Polyline x, Polyline poly) {

            //Firs find closest points
            double t0 = x.ClosestParameter(poly[0]);
            double t1 = x.ClosestParameter(poly.Last);

            if (t0 > t1) {
                poly.Reverse();

                poly.Add(x.PointAt(t0));
                poly.Insert(0, x.PointAt(t1));

            } else {

                poly.Add(x.PointAt(t1));
                poly.Insert(0, x.PointAt(t0));
            }

            x.InsertRange((int)Math.Ceiling(t0), poly);
            //x.CollapseShortSegments(0.01);
            //  x.DeleteShortSegments(0.01);
        }


        public static Point3d LastPt(Polyline polyline) {
            return polyline[polyline.Count - 1];
        }

        public static Polyline ChangeClosedPolylineSeam(Polyline polyline, double t) {

            //Rhino.RhinoApp.WriteLine(polyline.Count.ToString());
            //polyline.Bake();

            Polyline tempPolyline;
            Curve curve = polyline.ToNurbsCurve();
            curve.ChangeClosedCurveSeam(t);
            curve.TryGetPolyline(out tempPolyline);
            return new Polyline(tempPolyline);
        }

        public static Polyline MoveEndPointApart(Polyline polyline, double tt) {


            int last = polyline.Count - 1;
            Vector3d vec0 = polyline[0] - polyline[1];
            Vector3d vec1 = polyline[last] - polyline[last - 1];
            vec0.Unitize();
            vec1.Unitize();

            Point3d p0 = polyline[0] + vec0 * tt;
            Point3d p1 = polyline[last] + vec1 * tt;

            int flip = 1;

            polyline[0] += vec0 * tt * flip;
            polyline[last] += vec1 * tt * flip;


            return polyline;

        }

        public static  List<List<IntPoint>> PolylineToIntPoints(List<Polyline> p, double scale = 1e10) {

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
                {
                 
                    polygons.Add(IntPointToPolyline(pp, scale));
                }
            }
            return polygons;
        }

        public static Polyline IntPointToPolyline(List<IntPoint> p, double scale = 1e10) {

            Polyline polygon = new Polyline();



            for (int i = 0; i < p.Count; i++) {
                polygon.Add(new Point3d(p[i].X / scale, p[i].Y / scale, 0));
            }

            polygon.Add(polygon[0]);

            return polygon;
        }











    }
}
