using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore {
    public static class Fabircation {


        public static Transform OrientTo2D(Polyline polyline) {

            Plane plane = NGonsCore.PolylineUtil.plane(polyline);
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

        public static Polyline[] OffsetPolyline(Polyline c0, Polyline c1, double toolr = 10, bool notch = false) {

            if (toolr == 0)
                return new Polyline[] { c0, c1 };

            if(c0.Count==2)
                return new Polyline[] { c0, c1 };


            Transform transform = OrientTo2D(c0);

            Polyline ply0 = new Polyline(c0);
            Polyline ply1 = new Polyline(c1);

            bool closed = c0[0].DistanceToSquared(c0[c0.Count - 1]) < 0.001;
            if (!closed) {
                ply0.Close();
                ply1.Close();
            }

            ply0.Transform(transform);
            ply1.Transform(transform);

            Transform inverse; transform.TryGetInverse(out inverse);


            Plane ply0Plane = ply0.plane();
            Plane ply1Plane = ply1.plane();

            Polyline pl0Offset = new Polyline();
            Polyline pl1Offset = new Polyline();

            string orient = (IsClockwiseClosedPolylineOnXYPlane(ply0)) ? "CounterClockwise" : "Clockwise";


            if (ply0.IsValid && ply1.IsValid) {

                Point3d[] vrts = ply0.ToArray();
                Point3d[] uvrts = ply1.ToArray();

                Plane notPairingPlane;
                Plane.FitPlaneToPoints(vrts, out notPairingPlane);

                //int k = 1;
                //int infeed = 1;
                //Rhino.RhinoApp.WriteLine(k.ToString());
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

                    //Vector3d nh0 = ((pts[3] - pts[0]) / infeed) * (k - 1);
                    //Vector3d nh1 = ((pts[2] - pts[1]) / infeed) * (k - 1);

                    Point3d p21 = pts[1]; //+ nh1;  // infeed pts
                    Point3d p30 = pts[0]; //+ nh0;



                    int IPDtemp = 3;                                                                        // division for sim mach
                    if (Math.Abs(pts[0].DistanceTo(pts[1])) <= Math.Abs(pts[3].DistanceTo(pts[2])) + 0.5 && Math.Abs(pts[0].DistanceTo(pts[1])) >= Math.Abs(pts[3].DistanceTo(pts[2])) - 0.5) {
                        IPDtemp = 1;                                                                        // simple cut
                    }

                    // List<Point3d> ptsl = IBOIS.Utilities.GeometryProcessing.DividePoints(pts[0], pts[1], IPDtemp);
                    // List<Point3d> ptsm = IBOIS.Utilities.GeometryProcessing.DividePoints(p30, p21, IPDtemp);
                    //List<Point3d> ptsu = IBOIS.Utilities.GeometryProcessing.DividePoints(pts[3], pts[2], IPDtemp);

                    //  Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pts[0]);




                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(p30,p21));
                    //      Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pts[0], pts[1]));
                    //        Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pts[3], pts[2]));

                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pts[0], pts[3]));
                    double t;
                    Line line = new Line(pts[0], pts[3]);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(line, ply0Plane, out t);
                    pl0Offset.Add(line.PointAt(t));
                    Rhino.Geometry.Intersect.Intersection.LinePlane(line, ply1Plane, out t);
                    pl1Offset.Add(line.PointAt(t));

                }
            }



            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pl0Offset);

            if (notch) {
                //notches
                List<Line> notches = DrillingHoleForConvexCorners(ply0, ply1, -toolr);

                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pl0Offset);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pl1Offset);

                int counter = 0;
                for (int i = 0; i < notches.Count; i++) {

                    if ((!closed) && (i == 0 || i == (notches.Count - 1))) continue;
                    if (notches[i] == Line.Unset) continue;

                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(notches[i]);

                    pl0Offset.Insert(i + 1 + counter, notches[i].From);//i+1 because after point
                    pl1Offset.Insert(i + 1 + counter, notches[i].To);
                    counter++;
                    pl0Offset.Insert(i + 1 + counter, pl0Offset[i - 1 + counter]);
                    pl1Offset.Insert(i + 1 + counter, pl1Offset[i - 1 + counter]);
                    counter++;
                }
            }

            if (closed) {
                pl0Offset.Close();
                pl1Offset.Close();
            }

            pl0Offset.Transform(inverse);
            pl1Offset.Transform(inverse);
            return new Polyline[] { pl0Offset, pl1Offset };

        }

        public static List<Line> DrillingHoleForConvexCorners(Polyline x, Polyline x1, double R = 5) {


            Plane plane = x.plane();
            int n = x.SegmentCount;
            //List<Circle> circles = new List<Circle>();
            List<Point3d> pts = new List<Point3d>();

            List<Line> lines = new List<Line>();

            for (int i = 0; i < n; i++) {

                int prev = (((i - 1) % n) + n) % n;

                Vector3d vec0 = x.SegmentAt(i).UnitTangent;
                Vector3d vec1 = x.SegmentAt(prev).UnitTangent;

                Vector3d vec0_ = x1.SegmentAt(i).UnitTangent;
                Vector3d vec1_ = x1.SegmentAt(prev).UnitTangent;


                double angle = Vector3d.VectorAngle(vec0, vec1, plane);

                //determine if angle is convex, if it is not continue
                if (angle <= Math.PI * 0.5) {
                    lines.Add(Line.Unset);
                    continue;

                }

                Vector3d vecAv = vec0 - vec1;
                vecAv.Unitize();
                vecAv *= R;


                Vector3d vecAv_ = vec0_ - vec1_;
                vecAv_.Unitize();
                vecAv_ *= R;

                Line drillingAxis = new Line(x[i] + vecAv, x1[i] + vecAv_);
                //Line drillingAxis = new Line(x[i], x1[i]);

                double extensionDist = extension(drillingAxis, plane, R);

                drillingAxis.To += drillingAxis.UnitTangent * extensionDist;
                drillingAxis.From -= drillingAxis.UnitTangent * extensionDist;
                lines.Add(drillingAxis);


                pts.Add(x[i] + vecAv);

            }

            return lines;
        }

        public static double extension(Line CutAxis, Plane plane_, double R) {

            Line ToolPath = new Line(plane_.Origin, plane_.Origin + plane_.XAxis);
            Line ToolPath1 = new Line(plane_.Origin, plane_.Origin + plane_.YAxis);


            Vector3d vCutAxis = CutAxis.Direction;
            Vector3d vToolPath = ToolPath.Direction;

            // if (vCutAxis.IsParallelTo(vToolPath, 0.001) != 0)
            //return 0;

            Plane basePlane = new Plane(CutAxis.To, ToolPath.Direction, ToolPath1.Direction);
            Point3d cp = basePlane.ClosestPoint(CutAxis.PointAt(0.5));
            vToolPath = cp - basePlane.ClosestPoint(CutAxis.PointAt(0.0));


            //Dirty way of orineting vector to xy axis
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


    }
}
