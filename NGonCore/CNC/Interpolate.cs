using NGonCore;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoGeometry {
    public static class Interpolate {


        public static Polyline[] InterpolateTwoPolylinesToOnePath(Polyline C0, Polyline C1, int Infeed, double R, bool soft,List<Line> DrillLines = null ) {


            Polyline c0 = new Polyline(C0);
            Polyline c1 = new Polyline(C1);


            Plane.FitPlaneToPoints(C0, out Plane plane0);
            Plane.FitPlaneToPoints(C1, out Plane plane1);


            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(c0);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(c1);
            foreach (Line ll in DrillLines) {
                Line l = ll;
                l= l.ExtendLine(l.Length, l.Length);
        
                Point3d intersectionP0 = PlaneUtil.LinePlane(l, plane0);
                Point3d cp0 = C0.PointAt( Math.Round(C0.ClosestParameter(intersectionP0),3));
                Polyline notch0 = new Polyline() { cp0, intersectionP0, cp0 };
                c0.InsertPolyline(notch0);


                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(notch0);
                //notch0.Bake();

                
                Point3d intersectionP1 = PlaneUtil.LinePlane(l, plane1);
                Point3d cp1 = C1.PointAt(Math.Round(C1.ClosestParameter(intersectionP1), 3));
                Polyline notch1 = new Polyline() { cp1, intersectionP1, cp1 };

      
                c1.InsertPolyline(notch1);
              
                //notch1.Bake();
            }

            c0.MergeColinearSegments(0.01, false);
            c1.MergeColinearSegments(0.01, false);

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(c0);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPointCloud(c1);


            //Polyline plane
            Plane plane = new Plane();
            Plane.FitPlaneToPoints(C1, out plane);
            plane.Origin = C1.CenterPoint();



            Polyline remappedPolyline0 = new Polyline();
            Polyline remappedPolyline1 = new Polyline();

            for (int i = 0; i < c0.SegmentCount; i++) {

                //Create vertical lines
                Point3d p0a = c0.SegmentAt(i).From;
                Point3d p0b = c1.SegmentAt(i).From;

                Point3d p1a = c0.SegmentAt(i).To;
                Point3d p1b = c1.SegmentAt(i).To;

                Line cutAxis0 = new Line(p0a, p0b);
                Line cutAxis1 = new Line(p1a, p1b);
                Line cutAxis0_ = new Line(p0a, p0b);//copy
                Line cutAxis1_ = new Line(p1a, p1b);//copy

                //Extend value depending if of angle between segments
                int extendOrNot = 1;

                if (p0a.DistanceToSquared(p1a) > 0.01) {
                    Line perpLine = new Line(p0a, c1.SegmentAt(i).ClosestPoint(p0a, false));//Perpendicular line between two segments
                    double shearAngle = Rhino.RhinoMath.ToDegrees(Vector3d.VectorAngle(perpLine.Direction, plane.ZAxis)); //Angle between plane normal and segments
                    extendOrNot = (shearAngle > -0.01 && shearAngle < 0.01 || shearAngle > 180 - 0.01 && shearAngle < 180 + 0.01) ? 0 : 1;
                }


                double dist0 = extension(cutAxis0, plane, R);
                double dist1 = extension(cutAxis1, plane, R);

                cutAxis0 = new Line(cutAxis0.From, cutAxis0.To + cutAxis0.UnitTangent * dist0 * extendOrNot);//extendOrNot
                cutAxis1 = new Line(cutAxis1.From, cutAxis1.To + cutAxis1.UnitTangent * dist1 * extendOrNot);//extendOrNot

                Line movedSemgent1 = new Line(cutAxis0.To, cutAxis1.To);

                //Take end points of lines and reconstruct the tool path
                if (i == 0) {


                    remappedPolyline1.Add(cutAxis0.To);//
                    remappedPolyline1.Add(cutAxis1.To);//

                    remappedPolyline0.Add(cutAxis0.From);
                    remappedPolyline0.Add(cutAxis1.From);

                } else {

                    if (remappedPolyline1.Last.DistanceToSquared(cutAxis0.To) > 0.01) {
                        remappedPolyline1.Add(cutAxis0.To);
                        remappedPolyline0.Add(cutAxis0.From);
                    }

                    remappedPolyline1.Add(cutAxis0.To);
                    remappedPolyline1.Add(cutAxis1.To);

                    remappedPolyline0.Add(cutAxis0.From);
                    remappedPolyline0.Add(cutAxis1.From);


                }

            }

            Polyline[] interpolated = InterpolatePolylinesZigZag(remappedPolyline0, remappedPolyline1, Math.Max(1, Infeed),soft);


            return interpolated;

        }




        public static Polyline[] InterpolatePolylinesZigZag(Polyline path, Polyline normal, int n , bool soft ) {

            n--;

            Polyline[] interpolatedPolylines = new Polyline[2 + n];

            for (int j = 0; j < 2 + n; j++)
                interpolatedPolylines[j] = new Polyline();

            for (int j = 0; j < path.Count; j++) {

                Point3d[] pts = InterpolatePoints(path[j], normal[j], n, true);

                for (int k = 0; k < 2 + n; k++) {
                    interpolatedPolylines[k].Add(pts[k]);
                }

            }


            Polyline ZigZag = new Polyline();
            Polyline ZigZagNormal = new Polyline();

            for (int i = 0; i < 1 + n; i++) {

                if (i % 2 == 0) {

                    ZigZag.AddRange(interpolatedPolylines[i + 1]);
                    if(soft) ZigZag.Add(interpolatedPolylines[i + 1][1]);
                    ZigZagNormal.AddRange(interpolatedPolylines[i]);
                    if (soft) ZigZagNormal.Add(interpolatedPolylines[i][1]);
                } else {
                    Polyline temp = new Polyline(interpolatedPolylines[i + 1]);
                    temp.Reverse();
                    ZigZag.AddRange(temp);
                    if (soft) ZigZag.Add(temp[temp.Count-2]);
                    Polyline tempNormal = new Polyline(interpolatedPolylines[i]);
                    tempNormal.Reverse();
                    ZigZagNormal.AddRange(tempNormal);
                    if (soft) ZigZagNormal.Add(tempNormal[tempNormal.Count - 2]);
                }

            }


            return new Polyline[] { ZigZag, ZigZagNormal };
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