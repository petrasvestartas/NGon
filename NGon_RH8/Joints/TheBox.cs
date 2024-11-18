using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino;
using NGonCore;

namespace NGon_RH8.Joints {

    public class TheBox {

        //Input
        private Polyline p0 = new Polyline();
        private Polyline p1 = new Polyline();
        private double dist = 0.03;
        private List<double> MidSrfPos = new List<double> { 0.5 };
        private bool bisector = false;
        private double Parallel = 1;
        private double divisions = 2;
        private double angle90 = Math.PI * 0.5;
        private double angle45 = Math.PI * 0.25;
        private double angle135 = Math.PI * 0.75;
        private int n = 0;
        private int ID = 0;


        //Output
        public Plane[] sidePlanes;
        public Plane[] sidePlanesR90;
        public Plane[][] sidePlanesR90SideJoints;
        public Plane[] sidePlanesO;
        public Plane[] sidePlanesBi;
        public Plane topPlane0;
        public Plane topPlane1;
        public Plane[] topPlanes;
        public Point3d c0, c1;
        public bool[] sidePlanesBiFlags;

        //Outlines without joints
        public Polyline[][] midOutlines0;
        public Polyline[][] midOutlines1;
        public Polyline[][] sideOutlines0;
        public Polyline[][] sideOutlines1;//for creating dovetails

        //Outline with joints
        public Polyline[][] misOutlinesJoints;//Repesent mid panels with finger joints
        public List<Polyline>[] sideOutlinesJoints0;//n - number of sides
        public List<Polyline>[] sideOutlinesJoints1;//n - number of sides

        //Line Segments for joints
        public Line[][] jointLines;


        public TheBox(Polyline p0, Polyline p1, double dist, List<double> MidSrfPos, bool bisector, double Parallel, double divisions, int ID) {

            this.p0 = p0;
            this.p1 = p1;
            this.dist = dist;
            this.MidSrfPos = MidSrfPos;
            this.bisector = bisector;
            this.Parallel = Parallel;
            this.divisions = Math.Max(2, divisions);
            this.ID = ID;
            this.n = p0.Count - 1;

            sidePlanes = new Plane[p0.Count - 1];
            sidePlanesR90 = new Plane[p0.Count - 1];
            sidePlanesO = new Plane[p0.Count - 1];
            sidePlanesBi = new Plane[p0.Count - 1];
            jointLines = new Line[p0.Count - 1][];
            topPlanes = new Plane[MidSrfPos.Count];
        }

        public void ConstructPlanes() {
            //Center Point
            this.c0 = p0.CenterPoint();
            this.c1 = p1.CenterPoint();
            Point3d c = (c0 + c1) * 0.5;

            //Top and Bottom Planes


            Plane.FitPlaneToPoints(p0, out topPlane0);
            Plane.FitPlaneToPoints(p1, out topPlane1);
            this.topPlane0.Origin = c0;
            this.topPlane1.Origin = c1;

            topPlane0.Transform(Rhino.Geometry.Transform.Rotation(Vector3d.VectorAngle(p0[0] - p0[1], topPlane0.XAxis), topPlane0.ZAxis, topPlane0.Origin));
            topPlane1.Transform(Rhino.Geometry.Transform.Rotation(Vector3d.VectorAngle(p1[0] - p1[1], topPlane1.XAxis), topPlane1.ZAxis, topPlane1.Origin));

            if ((topPlane0.Origin + topPlane0.ZAxis).DistanceToSquared(c) < (topPlane0.Origin - topPlane0.ZAxis).DistanceToSquared(c)) {
                topPlane0.Flip();
            }

            if ((topPlane1.Origin + topPlane1.ZAxis).DistanceToSquared(c) < (topPlane1.Origin - topPlane1.ZAxis).DistanceToSquared(c))
                topPlane1.Flip();

            if (Parallel > 0.0) {
                Vector3d vec = topPlane1.Origin - topPlane0.Origin;
                vec.Unitize();
                vec *= Parallel;
                Plane tempPlane = topPlane0;
                tempPlane.Flip();
                tempPlane.Translate(vec);
                topPlane1 = tempPlane;
            }


            bool flipOrNotToFlip = p0.ToPolylineCurve().ClosedCurveOrientation(topPlane0) > 0;

            //Side Planes
            this.sidePlanes = new Plane[p0.Count - 1];
            this.sidePlanesR90 = new Plane[p0.Count - 1];
            this.sidePlanesO = new Plane[p0.Count - 1];
            this.sidePlanesBi = new Plane[p0.Count - 1];

            for (int i = 0; i < p0.Count - 1; i++) {
                Point3d origin = (p0[i] + p0[i + 1] + p1[i + 1] + p1[i]) * 0.25;
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(p0[i], p0[i + 1]));

                Vector3d v0 = p0[i] - p0[i + 1];//Red
                Vector3d v1 = p0[i] - p1[i];
                Plane plane = new Plane(origin, v0, v1);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(origin, origin - plane.XAxis));



                //if(v0.IsParallelTo(plane.XAxis, 0.1) != 0){
                // plane.Transform(Transform.Rotation(Vector3d.VectorAngle(plane.YAxis, v0), plane.ZAxis, plane.Origin));
                //Rhino.RhinoApp.WriteLine(v0.IsParallelTo(plane.YAxis).ToString());
                //}

                //if( (plane.Origin + plane.ZAxis).DistanceToSquared(c) < (plane.Origin - plane.ZAxis).DistanceToSquared(c))
                if (flipOrNotToFlip) {
                    plane.Flip();
                    plane.Rotate(angle90, plane.ZAxis, plane.Origin);
                }

                this.sidePlanes[i] = new Plane(plane);
                this.sidePlanesO[i] = new Plane(plane);
                this.sidePlanesO[i].Translate(sidePlanesO[i].ZAxis * -dist);
                if (i > 0)
                    this.sidePlanesBi[i] = NGonCore.PlaneUtil.BisectorPlane(sidePlanes[i - 1], sidePlanes[i]);
                if (i == p0.Count - 2)
                    this.sidePlanesBi[0] = NGonCore.PlaneUtil.BisectorPlane(sidePlanes[sidePlanes.Length - 1], sidePlanes[0]);

                this.sidePlanesR90[i] = new Plane(sidePlanes[i]);
                this.sidePlanesR90[i].Transform(Rhino.Geometry.Transform.Rotation(angle90, sidePlanesR90[i].YAxis, sidePlanesR90[i].Origin));//this is causing error x and y axis

            }//for i
        }

        public void ConstructOutlines() {


            //Side Outlines
            this.sideOutlines0 = new Polyline[n][];
            this.sideOutlines1 = new Polyline[n][];
            sidePlanesBiFlags = new bool[n];

            for (int i = 0; i < n; i++) {

                //Check angle between next and previous planes
                double angleNext = Vector3d.VectorAngle(sidePlanes[i].ZAxis, sidePlanes[NGonCore.MathUtil.Wrap(i + 1, n)].ZAxis);
                double anglePrev = Vector3d.VectorAngle(sidePlanes[i].ZAxis, sidePlanes[NGonCore.MathUtil.Wrap(i - 1, n)].ZAxis);
                

                bool isNextAngle = angleNext > angle45*0.5 && angleNext < angle135;  //min > angle < max
                bool isPrevAngle = anglePrev > angle45*0.5 && anglePrev < angle135;  //min > angle < max
                sidePlanesBiFlags[i] = isNextAngle;

                //Get next and previous planes
                Plane nextPlane = (isNextAngle && bisector) ? sidePlanesO[NGonCore.MathUtil.Wrap(i + 1, n)] : sidePlanesBi[NGonCore.MathUtil.Wrap(i + 1, n)];
                Plane prevPlane = (isPrevAngle && bisector) ? sidePlanes[NGonCore.MathUtil.Wrap(i - 1, n)] : sidePlanesBi[NGonCore.MathUtil.Wrap(i, n)];

                //Intersect current plane and surrounding ordered planes
                sideOutlines0[i] = new[]{
            OutlineFromPlanes(sidePlanesO[i], new Plane[]{ topPlane0, nextPlane,topPlane1, prevPlane }),
            OutlineFromPlanes(sidePlanes[i], new Plane[]{ topPlane0, nextPlane,topPlane1, prevPlane })
            };

                nextPlane = (isNextAngle && bisector) ? sidePlanes[NGonCore.MathUtil.Wrap(i + 1, n)] : sidePlanesBi[NGonCore.MathUtil.Wrap(i + 1, n)];
                prevPlane = (isPrevAngle && bisector) ? sidePlanesO[NGonCore.MathUtil.Wrap(i - 1, n)] : sidePlanesBi[NGonCore.MathUtil.Wrap(i, n)];

                sideOutlines1[i] = new[]{
            OutlineFromPlanes(sidePlanesO[i], new Plane[]{ topPlane0, nextPlane,topPlane1, prevPlane }),
            OutlineFromPlanes(sidePlanes[i], new Plane[]{ topPlane0, nextPlane,topPlane1, prevPlane })
            };



            }



            //Top Outlines
            this.midOutlines0 = new Polyline[MidSrfPos.Count][];
            this.midOutlines1 = new Polyline[MidSrfPos.Count][];

            for (int i = 0; i < MidSrfPos.Count; i++) {

                Point3d intPt0 = interpolatedPt(c0, c1, MidSrfPos[i], 1 - MidSrfPos[i]);
                Plane plane = topPlane0;
                plane.Origin = intPt0;
                Plane plane0 = plane;
                Plane plane1 = plane;
                plane0.Translate(plane0.ZAxis * dist * 0.5);
                plane1.Translate(plane0.ZAxis * dist * -0.5);

                midOutlines0[i] = new Polyline[2]{
          OutlineFromPlanes(plane0, sidePlanesO),
          OutlineFromPlanes(plane1, sidePlanesO),
          };

                midOutlines1[i] = new Polyline[2]{
          OutlineFromPlanes(plane0, sidePlanes),
          OutlineFromPlanes(plane1, sidePlanes),
          };
                topPlanes[i] = plane0;
            }

        }

        public void ConstructJointsEdgeJoints(int sideDivisions = 2, double dovetailAngleDegrees = 5) {

            sidePlanesR90SideJoints = new Plane[n][];
            if (sideDivisions % 2 == 1)
                sideDivisions -= 1;
            //sideDivisions = Math.Max(sideDivisions, 0);


            //1 addPlanes at each corner

            for (int i = 0; i < n; i++) {

                //if (sidePlanesBiFlags[i]) { //Check if not bisector
                int next = NGonCore.MathUtil.Wrap(i + 1, n);
                int prev = NGonCore.MathUtil.Wrap(i - 1, n);

                sidePlanesR90SideJoints[i] = new Plane[sideDivisions];

                    //i - side polyline  1 - two polylines
                    //2---1
                    //3---0

                    //Interpolate polyline
                    Point3d[] interpolatedPts = NGonCore.PointUtil.InterpolatePoints(
                    (sideOutlines0[i][0][2] + sideOutlines1[i][1][2]) * 0.5,
                    (sideOutlines0[i][0][3] + sideOutlines1[i][1][3]) * 0.5,
                    sideDivisions, false);

                    for (int j = 0; j < sideDivisions; j++) {
                        int shift = (j % 2 == 0) ? 1 : -1;
                        sidePlanesR90SideJoints[i][j] = new Plane(sidePlanesR90[i]);
                        sidePlanesR90SideJoints[i][j].Rotate(angle90 , sidePlanesR90SideJoints[i][j].XAxis);
                    sidePlanesR90SideJoints[i][j].Rotate( shift * RhinoMath.ToRadians(dovetailAngleDegrees), sidePlanesR90SideJoints[i][j].YAxis);
                    sidePlanesR90SideJoints[i][j].Origin = interpolatedPts[j];
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(sidePlanesR90SideJoints[i][j], new Interval(-0.05, 0.05), new Interval(-0.05, 0.05)));
                    }
                //} else sidePlanesR90SideJoints[i] = null;

            }

            //2 Create zigzag Polylines

            for (int i = 0; i < n; i++) {

                Polyline zigzag0;
                Polyline zigzag1;

                if (i != 0) {
                    zigzag0 = new Polyline() { sideOutlines0[i][0][2] };
                    zigzag1 = new Polyline() { sideOutlines0[i][1][2] };
                } else {
                    zigzag0 = new Polyline() { sideOutlines1[i][0][2] };
                    zigzag1 = new Polyline() { sideOutlines1[i][1][2] };
                }

                int next = NGonCore.MathUtil.Wrap(i + 1, n);
                int prev = NGonCore.MathUtil.Wrap(i - 1, n);

                //Rhino.RhinoApp.WriteLine(sidePlanesBiFlags[i].ToString() + " " + i.ToString());

                if (sidePlanesBiFlags[prev] && i != 0) {
                    for (int j = 0; j < sideDivisions; j++) {
                        zigzag0.AddRange(IntersectTwoLinesWithOnePlane(sideOutlines0[i][0].SegmentAt(2), sideOutlines1[i][0].SegmentAt(2), sidePlanesR90SideJoints[i][j], j));
                        zigzag1.AddRange(IntersectTwoLinesWithOnePlane(sideOutlines0[i][1].SegmentAt(2), sideOutlines1[i][1].SegmentAt(2), sidePlanesR90SideJoints[i][j], j));
                    }
                }

                if (i != 0) {
                    zigzag0.Add(sideOutlines0[i][0][3]);
                    zigzag1.Add(sideOutlines0[i][1][3]);
                } else {
                    zigzag0.Add(sideOutlines1[i][0][3]);
                    zigzag1.Add(sideOutlines1[i][1][3]);
                }

                if (i != n - 1) {
                    zigzag0.Add(sideOutlines0[i][0][0]);
                    zigzag1.Add(sideOutlines0[i][1][0]);
                } else {
                    zigzag0.Add(sideOutlines1[i][0][0]);
                    zigzag1.Add(sideOutlines1[i][1][0]);
                }



                if (sidePlanesBiFlags[i] && i != n-1) {
                    for (int j = sideDivisions-1; j >= 0; j--) {
                        zigzag0.AddRange(IntersectTwoLinesWithOnePlane(sideOutlines0[i][0].SegmentAt(0), sideOutlines1[i][0].SegmentAt(0), sidePlanesR90SideJoints[next][j], j, 1));
                        zigzag1.AddRange(IntersectTwoLinesWithOnePlane(sideOutlines0[i][1].SegmentAt(0), sideOutlines1[i][1].SegmentAt(0), sidePlanesR90SideJoints[next][j], j,1));
                    }
               }


                if (i != n - 1) {
                    zigzag0.Add(sideOutlines0[i][0][1]);
                    zigzag1.Add(sideOutlines0[i][1][1]);
                } else {
                    zigzag0.Add(sideOutlines1[i][0][1]);
                    zigzag1.Add(sideOutlines1[i][1][1]);
                }


                zigzag0.Close();
                zigzag1.Close();

                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(zigzag0);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(zigzag1);

                sideOutlines0[i][0] = zigzag0;
                sideOutlines0[i][1] = zigzag1;
                //RhinoDoc.ActiveDoc.Objects.AddPolyline(zigzag0);



            }


        }

        private Point3d[] IntersectTwoLinesWithOnePlane( Line line0, Line line1, Plane plane, int j, int shift = 0) {

            Point3d[] pts = new Point3d[2];

                Rhino.Geometry.Intersect.Intersection.LinePlane(line0, plane, out double t0);
                Rhino.Geometry.Intersect.Intersection.LinePlane(line1, plane, out double t1);

            if (j % 2 == shift) {
                pts[0] = line0.PointAt(t0);
                pts[1] = line1.PointAt(t1);
            } else {
                pts[1] = line0.PointAt(t0);
                pts[0] = line1.PointAt(t1);
            }
                return pts;
        }

        public void ConstructJoints() {

            this.misOutlinesJoints = new Polyline[MidSrfPos.Count][];//Repesent mid panels with finger joints
                                                                     //Polyline[][] sideOutlines0 = new Polyline[n][];//n - number of side other array is for pair
            this.sideOutlinesJoints0 = new List<Polyline>[n];//n - number of sides
            this.sideOutlinesJoints1 = new List<Polyline>[n];//n - number of sides

            //MidSrfJoints
            for (int i = 0; i < MidSrfPos.Count; i++) {

                Line[] s0 = midOutlines0[i][0].GetSegments();
                Line[] s1 = midOutlines0[i][1].GetSegments();
                Line[] s0_ = midOutlines1[i][0].GetSegments();
                Line[] s1_ = midOutlines1[i][1].GetSegments();


                //Polyline midOutlineDovetails1 = new Polyline();

                this.misOutlinesJoints[i] = new Polyline[2] { new Polyline(), new Polyline() };
                //misOutlinesJoints1[i] = new Polyline[2]{new Polyline(),new Polyline()};

                //Loop thorugh each segment and construct finger joints
                //Gather also holes
                for (int j = 0; j < s0.Length; j++) {

                    int next = NGonCore.MathUtil.Wrap(j + 1, sidePlanesR90.Length);

                    if (i == 0) {
                        this.sideOutlinesJoints0[j] = new List<Polyline>() { sideOutlines0[next][0] };
                        this.sideOutlinesJoints1[j] = new List<Polyline>() { sideOutlines0[next][1] };

                    }


                    double div = divisions;
                    double t0, t1, t2, t3;
                    Plane jointPlane = sidePlanesR90[next];
                    jointPlane.Origin = jointPlane.Origin = s0[j].PointAt(0.5);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(s0[j], jointPlane, out t0);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(s1[j], jointPlane, out t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(s0_[j], jointPlane, out t2);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(s1_[j], jointPlane, out t3);
                    Point3d pt0 = s0[j].PointAt(t0);
                    Point3d pt1 = s1[j].PointAt(t1);
                    Point3d pt0_ = s0_[j].PointAt(t2);
                    Point3d pt1_ = s1_[j].PointAt(t3);

                    //Add Starting Point
                    misOutlinesJoints[i][0].Add(s0[j].From);
                    misOutlinesJoints[i][1].Add(s1[j].From);


                    Point3d cp0 = s0[j].ClosestPoint(s0_[j].From, true);
                    Point3d cp1 = s0[j].ClosestPoint(s0_[j].To, true);
                    //double distance = Math.Min(cp0.DistanceTo(s0[j].PointAt(0.5)),cp1.DistanceTo(s0[j].PointAt(0.5)));
                    double distance = Math.Min(pt0.DistanceTo(s0[j].From), pt0.DistanceTo(s0[j].To));


                    Vector3d dir = -s0[j].UnitTangent * distance * 0.5;
                    //int divisions = (int) Math.Floor(distance / jointWidth);
                    //Vector3d dir = s0[j].UnitTangent *jointWidth;

                    pt0 -= dir;
                    pt1 -= dir;
                    pt0_ -= dir;
                    pt1_ -= dir;

                    if ((distance * 0.25) < Math.Abs(dist))
                        div = 1;

                    double maxDist = maxDist = (dir.Length * 2.4) * 1 / div;

                    dir.Unitize();
                    dir *= maxDist;

                    Polyline TT0 = new Polyline();
                    Polyline TT1 = new Polyline();


                    //Make through tenons
                    for (int k = 0; k < div + 1; k++) {

                        if (k % 2 == 0) {
                            TT0.Add(pt0);
                            TT0.Add(pt0_);
                            TT1.Add(pt1);
                            TT1.Add(pt1_);
                        } else {

                            //Holes
                            Polyline hole1 = new Polyline(new[] { pt1_, pt0_, TT0.Last(), TT1.Last(), pt1_ });
                            Polyline hole0 = new Polyline(new[] { pt1, pt0, TT0[TT0.Count - 2], TT1[TT1.Count - 2], pt1 });
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(hole1);
                            this.sideOutlinesJoints0[j].Add(hole0);
                            this.sideOutlinesJoints1[j].Add(hole1);

                            TT0.Add(pt0_);
                            TT0.Add(pt0);
                            TT1.Add(pt1_);
                            TT1.Add(pt1);


                        }
                        pt0 += dir;
                        pt1 += dir;
                        pt0_ += dir;
                        pt1_ += dir;

                    }
                    TT0.Reverse();
                    TT1.Reverse();
                    this.misOutlinesJoints[i][0].AddRange(TT0);
                    this.misOutlinesJoints[i][1].AddRange(TT1);

                }

                this.misOutlinesJoints[i][0].Close();
                this.misOutlinesJoints[i][1].Close();
            }



        }

        public void ConstructJointsLines() {


            Point3d intPt0 = interpolatedPt(c0, c1, 0.15, 0.85);
            Point3d intPt1 = interpolatedPt(c0, c1, 0.85, 0.15);

            Plane plane0 = topPlane0;
            Plane plane1 = topPlane0;
            plane0.Origin = intPt0;
            plane1.Origin = intPt1;



            //Loop through each rectangular side
            for (int i = 0; i < n; i++) {

                Line l0 = new Line(midOutlines0[0][0][i], midOutlines0[0][1][i]);
                Line l1 = new Line(midOutlines0[0][1][i + 1], midOutlines0[0][0][i + 1]);

                Line intl0 = LineLinePlane(l0, l1, plane0);
                Line intl1 = LineLinePlane(l0, l1, plane1);

                int next = NGonCore.MathUtil.Wrap(i + 1, n);
                jointLines[next] = new Line[] { };
                jointLines[next] = new Line[] { intl0, intl1 };

            }



        }



        private Line LineLinePlane(Line l0, Line l1, Plane plane) {

            double t0, t1;
            Rhino.Geometry.Intersect.Intersection.LinePlane(l0, plane, out t0);
            Rhino.Geometry.Intersect.Intersection.LinePlane(l1, plane, out t1);
            return new Line(l0.PointAt(t0), l1.PointAt(t1));

        }


        /// <summary>
        /// Interpolate points by weights
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="w0"></param>
        /// <param name="w1"></param>
        /// <returns></returns>
        private Point3d interpolatedPt(Point3d p0, Point3d p1, double w0, double w1) {
            Point3d origin = Point3d.Origin;
            origin += p0 * w0;
            origin += p1 * w1;
            double item2 = 0;
            item2 += w0;
            item2 += w1;
            return origin /= item2;
        }


        /// <summary>
        /// Intersect Plane with surrounding ordered Planes
        /// </summary>
        /// <param name="mainPlane"></param>
        /// <param name="planes"></param>
        /// <returns></returns>
        private Polyline OutlineFromPlanes(Plane mainPlane, Plane[] planes) {
            Polyline outline = new Polyline();
            Point3d pt;
            for (int i = 0; i < planes.Length; i++) {
                Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(mainPlane, planes[i], planes[NGonCore.MathUtil.Wrap(i + 1, planes.Length)], out pt);
                outline.Add(pt);
            }
            outline.Close();
            return outline;
        }



    }
}
