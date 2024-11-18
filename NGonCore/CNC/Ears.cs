using NGonCore;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoGeometry {
    public static class Ears {

        public static Polyline[] Notches(Polyline x0, Polyline x1, double R, byte[] T) {


            int closed = x0.IsClosed ? 0 : 1;
            Polyline xA = new Polyline(x0.GetRange(0, x0.Count - (Math.Abs(closed - 1))));
            Polyline xB = new Polyline(x1.GetRange(0, x1.Count - (Math.Abs(closed - 1))));



            List<Line> l0 = new List<Line>(x0.Count);
            List<Line> l1 = new List<Line>(x0.Count);//fillet
            List<Line> l2 = new List<Line>(x0.Count);//fillet
            List<Polyline> plines = new List<Polyline>(x0.Count);
            List<Plane> planes = new List<Plane>();

            ///////////////////////////////////////////////////////////////////////
            //For notches or fillet, if fillet twice bigger radius is needed 
            //also see this if statement if (T[0] == 4 && angle_ > 0) { because corner is  inversed
            ///////////////////////////////////////////////////////////////////////
            bool fillet = false;
            double R_ = T[0] == 4 ? R * 2 : R;

            for (int i = closed; i < xA.Count - closed; i++) {



                ///////////////////////////////////////////////////////////////////////
                //Get extension lines
                ///////////////////////////////////////////////////////////////////////
                Line normal = new Line(xA[i], xB[i]);

                int n = xA.Count;
                int next = (i + 1) % n;
                int nextNext = (i + 2) % n;
                int prev = (((i - 1) % n) + n) % n;

                Point3d p0 = xA[i];
                Point3d p1 = xA[next];
                Point3d p2 = xA[prev];
                Point3d p3 = xA[nextNext];

                ///////////////////////////////////////////////////////////////////
                ///Measure Concavity
                ///////////////////////////////////////////////////////////////////
                Polyline polygon = new Polyline() { p2, p0, p1 };
                polygon.Transform(Transform.PlaneToPlane(new Plane(p0, normal.Direction), Plane.WorldXY));
                Vector3d nextEdge = polygon.SegmentAt(1).Direction;
                Vector3d previousEdge = polygon.SegmentAt(0).Direction;
                var angle_ = ((Math.Atan2(nextEdge.X, nextEdge.Y) - Math.Atan2(previousEdge.X, previousEdge.Y) + Math.PI * 2) % (Math.PI * 2)) - Math.PI;
                if (T[0] == 4 && angle_ > 0) {
                    l0.Add(Line.Unset);
                    l1.Add(Line.Unset);
                    l2.Add(Line.Unset);

                    continue;
                } else if(T[0] != 4 && angle_ < 0) {
                    l0.Add(Line.Unset);
                    l1.Add(Line.Unset);
                    l2.Add(Line.Unset);

                    continue;
                }
           

                ///////////////////////////////////////////////////////////////////////
                //Get Perpependicular points
                ///////////////////////////////////////////////////////////////////////

                Plane normalPl = new Plane(normal.From, normal.Direction);
                Line normal1 = new Line(xA[next], xB[next]);
                Line normal2 = new Line(xA[prev], xB[prev]);
                double t;
                Rhino.Geometry.Intersect.Intersection.LinePlane(normal, normalPl, out t);
                p0 = normal.PointAt(t);
                Rhino.Geometry.Intersect.Intersection.LinePlane(normal1, normalPl, out t);
                p1 = normal1.PointAt(t);
                Rhino.Geometry.Intersect.Intersection.LinePlane(normal2, normalPl, out t);
                p2 = normal2.PointAt(t);


                ///////////////////////////////////////////////////////////////////////
                //Compute offset vectors on perpedicular points
                ///////////////////////////////////////////////////////////////////////
                Vector3d v0 = p0 - p1;
                Vector3d v1 = p0 - p2;
                v0.Unitize();
                v1.Unitize();


                ///////////////////////////////////////////////////////////////////////
                //Depending on angle the offset changes, sin gives this distance value
                ///////////////////////////////////////////////////////////////////////
                double angle = Vector3d.VectorAngle(v0, v1, new Plane(p0, v0, v1));
                double r = R_ * (1 / Math.Sin(angle));


                ///////////////////////////////////////////////////////////////////////
                //Notch - Line Directions A-B
                ///////////////////////////////////////////////////////////////////////
                Plane pl0 = NGonCore.PlaneUtil.AlignPlane(normalPl, v0);
                Plane pl1 = NGonCore.PlaneUtil.AlignPlane(normalPl, v1);

                //for sharp angles extension is needed
                double extension0 = angle < (Math.PI * 0.5) ? NGonCore.VectorUtil.VectorProjection(v0 * r, v1 * r).Length : 0;
                double extension1 = angle < (Math.PI * 0.5) ? NGonCore.VectorUtil.VectorProjection(v1 * r, v0 * r).Length : 0;
                extension0 *= 0;
                extension1 *= 0;

                Line normalXAxis0 = new Line(x0[i] + pl0.XAxis * (r + extension0), x1[i] + pl0.XAxis * (r + extension0));
                Line normalXAxis1 = new Line(x0[i] + pl1.XAxis * (r + extension1), x1[i] + pl1.XAxis * (r + extension1));

                ///////////////////////////////////////////////////////////////////////
                //Notch -  Diaognal
                ///////////////////////////////////////////////////////////////////////
                Vector3d bisectorDir = (pl0.XAxis * r + pl1.XAxis * r);
                Plane pl2normalXAxis2Fillet = NGonCore.PlaneUtil.AlignPlane(normalPl, bisectorDir);
                double length = bisectorDir.Length;
                bisectorDir *= (length - R_) / length;
                Line normalXAxis2 = new Line(x0[i] + bisectorDir, x1[i] + bisectorDir);

                ///////////////////////////////////////////////////////////////////////
                //Notch -  Fillet
                ///////////////////////////////////////////////////////////////////////
                Vector3d bisectorDirFillet = (pl0.XAxis * r + pl1.XAxis * r);
                Plane pl2 = NGonCore.PlaneUtil.AlignPlane(normalPl, bisectorDirFillet);
                double lengthFillet = bisectorDir.Length;
                bisectorDirFillet *= -(length) / length;

                Line normalXAxis2Fillet = new Line(x0[i] + bisectorDirFillet * ((length - R_) / length), x1[i] + bisectorDirFillet * ((length - R_) / length));
                Line normalXAxis2FilletCenter = new Line(x0[i] + bisectorDirFillet, x1[i] + bisectorDirFillet);
                Line normalXAxis2Fillet0 = new Line(normalXAxis2FilletCenter.From + pl0.XAxis * (r + extension0), normalXAxis2FilletCenter.To + pl0.XAxis * (r + extension0));
                Line normalXAxis2Fillet1 = new Line(normalXAxis2FilletCenter.From + pl1.XAxis * (r + extension0), normalXAxis2FilletCenter.To + pl1.XAxis * (r + extension0));


                ///////////////////////////////////////////////////////////////////////
                //Output
                ///////////////////////////////////////////////////////////////////////
                planes.Add(pl0);
                planes.Add(pl1);
                planes.Add(pl2);

          
                if (T.Length == 1) {

                    switch (T[0]) {
                        case (2):
                        case (3):
                            bool flag = T[0] == 3 ?
                              p0.DistanceToSquared(p1) < p0.DistanceToSquared(p2) :
                              p0.DistanceToSquared(p1) > p0.DistanceToSquared(p2);
                            if (flag)
                                l0.Add(normalXAxis0);
                            else
                                l0.Add(normalXAxis1);
                            break;
                        case (1):
                            l0.Add(normalXAxis2);
                            break;
                        case (4):
                            fillet = true;
                            l0.Add(normalXAxis2Fillet0);
                            l1.Add(normalXAxis2Fillet);
                            l2.Add(normalXAxis2Fillet1);
                     
                            break;
                        case (10):
                        default:
                            l0.Add(Line.Unset);
                            break;
                    }



                } else {

                    switch (T[i % T.Length]) {
                        case (2):
                            l0.Add(normalXAxis0);
                            break;
                        case (3):
                            l0.Add(normalXAxis1);
                            break;
                        case (4):
                            fillet = true;
                            l0.Add(normalXAxis2Fillet0);
                            l1.Add(normalXAxis2Fillet);
                            l2.Add(normalXAxis2Fillet1);
                           
                            break;
                        case (1):
                            l0.Add(normalXAxis2);
                            break;
                        case (10):
                        default:
                            l0.Add(Line.Unset);
                            break;

                    }
                }

                //break;
            }


            ///////////////////////////////////////////////////////////////////////
            //Add Notches to a Pair of Polylines
            ///////////////////////////////////////////////////////////////////////
            Plane fitPlane = Plane.Unset;
            Plane.FitPlaneToPoints(xA, out fitPlane);
            Plane fitPlaneB = Plane.Unset;
            Plane.FitPlaneToPoints(xB, out fitPlaneB);
            Polyline xA_Notch = new Polyline();
            Polyline xB_Notch = new Polyline();
      


            int count = 0;
            for (int i = closed; i < xA.Count - closed; i++) {

                //Extend bottom line to compensate drilling bit thickness and angle
                Vector3d extensionV = xA[i] - xB[i];
                double extension = Math.Abs(Math.Tan(Vector3d.VectorAngle(extensionV, fitPlane.ZAxis)) * R_);
                extensionV.Unitize();
                extensionV *= extension;

                //Fillet corner
                if (fillet) {

                    if (l0[count] != Line.Unset) {

                 
                        double t = 0;
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l0[count], fitPlane, out t);
                        xA_Notch.Add(l0[count].PointAt(t) + extensionV);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l1[count], fitPlane, out t);
                        xA_Notch.Add(l1[count].PointAt(t) + extensionV);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l2[count], fitPlane, out t);
                        xA_Notch.Add(l2[count].PointAt(t) + extensionV);

                        Rhino.Geometry.Intersect.Intersection.LinePlane(l0[count], fitPlaneB, out t);
                        xB_Notch.Add(l0[count].PointAt(t));
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l1[count], fitPlaneB, out t);
                        xB_Notch.Add(l1[count].PointAt(t));
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l2[count], fitPlaneB, out t);
                        xB_Notch.Add(l2[count].PointAt(t));


                    } else {
                        xA_Notch.Add(xA[i] + extensionV);
                        xB_Notch.Add(xB[i]);
                    }

                    //Notch corner
                } else {

                    //Add notches and come back to original position
                    xA_Notch.Add(xA[i] + extensionV);
                    xB_Notch.Add(xB[i]);

                    if (l0[count] != Line.Unset) {
                        double t = 0;

                        //Top Outline
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l0[count], fitPlane, out t);
                        xA_Notch.Add(l0[count].PointAt(t) + extensionV);
                        xA_Notch.Add(xA[i] + extensionV);

                        //Bottom Outline
                        Rhino.Geometry.Intersect.Intersection.LinePlane(l0[count], fitPlaneB, out t);
                        xB_Notch.Add(l0[count].PointAt(t));
                        xB_Notch.Add(xB[i]);
                    }
                }

                count++;
            }


            if (fillet) {
                xA_Notch.Close();
                xB_Notch.Close();
            } else {
                if (closed == 0) {
                    xA_Notch.Add(xA_Notch[0]);
                    xB_Notch.Add(xB_Notch[0]);
                } else {
                    xA_Notch.Insert(0, x0[0]);
                    xA_Notch.Add(x0[x0.Count - 1]);
                    xB_Notch.Insert(0, x1[0]);
                    xB_Notch.Add(x1[x1.Count - 1]);
                }
            }

          

            ///////////////////////////////////////////////////////////////////////
            //Output
            ///////////////////////////////////////////////////////////////////////
            return new Polyline[] { xA_Notch, xB_Notch };

        }

            public static List<Line> DrillingHoleForConvexCorners(Polyline x, Polyline x1, double R = 5, byte[] notchesTypes = null) {

            Plane plane = x.plane();// GeometryProcessing.GetPlane(x);
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


                ///////////////////////////////////////////////////////////////////
                ///Measure angle
                ///////////////////////////////////////////////////////////////////
                double angle = Vector3d.VectorAngle(vec0, vec1, plane);
                angle %= Math.PI;//For cnc must be removed?

                //if ( (angle) <= Math.PI * 0.5) continue;
                //Rhino.RhinoApp.WriteLine(angle.ToString());
                if ((angle) > Math.PI * 0.501) continue;//For cnc must be removed?


                ///////////////////////////////////////////////////////////////////
                ///Notche as a drilling line
                ///////////////////////////////////////////////////////////////////
                Vector3d vecAv = vec0 - vec1;
                vecAv.Unitize();
                vecAv *= R;

                Vector3d vecAv_ = vec0_ - vec1_;
                vecAv_.Unitize();
                vecAv_ *= R;

                //Line drillingAxis = new Line(x[i] + vecAv, x1[i] + vecAv_);
                Rhino.RhinoApp.WriteLine(notchesTypes.Length.ToString() + " " + n.ToString());
                Line drillingAxis = Line.Unset;
                if (notchesTypes != null) {
                    if (notchesTypes.Length == n) {
                      
                        if (notchesTypes[i] == 2) {
                            drillingAxis = new Line(x[i] + vec0 * R, x1[i] + vec0 * R);
                        } else if (notchesTypes[i] == 3) {
                            drillingAxis = new Line(x[i] - vec1 * R, x1[i] - vec1 * R);
                        } else if (notchesTypes[i] == 1) {
                            drillingAxis = new Line(x[i] + vecAv, x1[i] + vecAv_);
                        } else if (notchesTypes[i] == 10) {
                            continue;
                        }

                    }
                }

                if (drillingAxis == Line.Unset)
                    drillingAxis = new Line(x[i] + vecAv, x1[i] + vecAv_);


                double extensionDist = extension(drillingAxis, new Line(plane.Origin, plane.Origin + plane.XAxis), new Line(plane.Origin, plane.Origin + plane.YAxis), R);

                drillingAxis.To += drillingAxis.UnitTangent * extensionDist;
                drillingAxis.From -= drillingAxis.UnitTangent * extensionDist;




                ///////////////////////////////////////////////////////////////////
                ///Output
                ///////////////////////////////////////////////////////////////////
                lines.Add(drillingAxis);
                pts.Add(x[i] + vecAv);




            }

            return lines;
        }















        public static double extension(Line CutAxis, Line ToolPath, Line ToolPath1, double R) {


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
