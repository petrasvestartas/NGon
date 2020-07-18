using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace NGonsCore{

    public struct TopologyEdge : IComparable<TopologyEdge> {
        private int _from;

        private int _to;

        private int _min;

        private int _max;

        private int _right;

        private int _left;

        public int From {
            get {
                return this._from;
            }
        }

        public bool IsNaked {
            get {
                bool flag;
                flag = (!(this._left == -1 | this._right == -1) ? false : true);
                return flag;
            }
        }

        public int LeftFace {
            get {
                return this._left;
            }
        }

        public int NakedSides {
            get {
                int num = 0;
                if (this._left == -1) {
                    num = checked(num + 1);
                }
                if (this._right == -1) {
                    num = checked(num + 1);
                }
                return num;
            }
        }

        public int OtherFace(int ThisFace) {
            //get{
                int num;

                if (this._left != ThisFace)
                    num = (this._right != ThisFace ? -1 : this._left);
               else 
                    num = this._right;
               
                return num;
            //}
        }

        public int RightFace {
            get {
                return this._right;
            }
        }

        public int To {
            get {
                return this._to;
            }
        }

        public TopologyEdge(int From, int To, int LeftFace = -1, int RightFace = -1) {
            this = new TopologyEdge() {
                _from = From,
                _to = To,
                _min = Math.Min(From, To),
                _max = Math.Max(From, To),
                _left = LeftFace,
                _right = RightFace
            };
        }

        public int CompareTo(TopologyEdge other) {
            int num;
            int num1 = other.Minimum();
            int num2 = this.Minimum();
            if (num2 > num1) {
                num = 1;
            } else if (num2 != num1) {
                num = -1;
            } else {
                int num3 = other.Maximum();
                int num4 = this.Maximum();
                if (num4 <= num3) {
                    num = (num4 != num3 ? -1 : 0);
                } else {
                    num = 1;
                }
            }
            return num;
        }

        public bool IsValid() {
            return !(this.From == -1 | this.To == -1);
        }

        public int Maximum() {
            return this._max;
        }

        public int Minimum() {
            return this._min;
        }

        public static object operator ==(TopologyEdge A, TopologyEdge B) {
            object obj;
            obj = (!(A.Minimum() == B.Minimum() & A.Maximum() == B.Maximum()) ? false : true);
            return obj;
        }

        public static object operator !=(TopologyEdge A, TopologyEdge B) {
            return A != B;
        }

        public void Orient() {
            int num = Math.Min(this.From, this.To);
            int num1 = Math.Max(this.From, this.To);
            this._from = num;
            this._to = num1;
        }

        public override string ToString() {
            return string.Concat(new string[] { "{From:",this.From.ToString(), " To:", this.To.ToString(), "}" });
        }
    }




    public static class PolylineUtil    {

        public static Polyline ToPolyline(this Line l) {
            return new Polyline(new Point3d[] { l.From,l.To});
        }

        public static List<Polyline> Duplicate(this List<Polyline> p) {
            List<Polyline> polylines = new List<Polyline>();
            foreach (Polyline p_ in p)
                polylines.Add(new Polyline(p_));
            return polylines;
        }

        /// <summary>
        /// Takes 4 point rectangle 1st - 3rd or 2nd - 4th lines and creates zigzag 
        ///returns good result only when rectangle edges are equal length    
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="flip"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Polyline ZigZag(this Polyline rectangle, bool flip, double dist, int divisions = -1) {

            Line l0 = (flip) ? new Line(rectangle[0], rectangle[1]) : new Line(rectangle[1], rectangle[2]);
            Line l1 = (flip) ? new Line(rectangle[3], rectangle[2]) : new Line(rectangle[0], rectangle[3]);

            int n = (int)Math.Ceiling(l0.Length / dist);
         
            n += (n % 2);
 

            //rectangle.Bake();

            double dist_ = l0.Length / n;

            Polyline zigzag = new Polyline();
            Vector3d v0 = l0.Direction.UnitVector() * dist_;


            for (int i = 0; i < n; i++) {
                if (i % 2 == 0) {
                    zigzag.Add(l0.From + (v0 * i));
                    zigzag.Add(l1.From + (v0 * (i)));

                } else {
                    zigzag.Add(l1.From + (v0 * (i)));
                    zigzag.Add(l0.From + (v0 * (i)));

                }//if
            }//for

            //Finish the zigzag end
            //if (dist_ * n != l0.Length) {
                if (n % 2 == 0) {
                    zigzag.Add(l0.To);
                    zigzag.Add(l1.To);

                } else {
                    zigzag.Add(l1.To);
                    zigzag.Add(l0.To);

                }//if
            //}
            //zigzag.Bake();
            zigzag[0] = (zigzag[0] + zigzag[1]) * 0.5;
            zigzag[zigzag.Count-1] = (zigzag[zigzag.Count - 1] + zigzag[zigzag.Count - 2]) * 0.5;
            
            return zigzag;
        }
        public static Polyline Flip(this Polyline polyline) {
            Polyline p = polyline.Duplicate();
            p.Reverse();
            return p;
        }

        public static void Orient(this Polyline polyline,Plane source, Plane target) {
            Transform transform = Rhino.Geometry.Transform.PlaneToPlane(source, target);
            polyline.Transform(transform);
        }

        public static List<Line> Transform(this List<Line> lines, Transform t) {

            List<Line> linesTransform = new List<Line>();

            for (int i = 0; i < lines.Count; i++) {
                Line l = lines[i];
                l.Transform(t);
                linesTransform.Add(l);
            }

            return linesTransform;

        }

        public static Line[] Transform(this Line[] lines, Transform t) {

            Line[] linesTransform = new Line[lines.Length];

            for (int i = 0; i < lines.Length; i++) {
                Line l = lines[i];
                l.Transform(t);
                linesTransform[i] = l;
            }

            return linesTransform;

        }

        public static List<Line[]> Transform(this List<Line[]> lines, Transform t) {

            List<Line[]> linesTransformed = new List<Line[]>(lines.Count);

            foreach (Line[] ln in lines) {

                linesTransformed.Add(ln.Transform(t));
            }
            return linesTransformed;
        }

        public static void Transform(this IEnumerable<Polyline> polylines, Transform t) {
            foreach (Polyline p in polylines)
                p.Transform(t);
        }

        public static Polyline Translate(this Polyline p,Vector3d v) {
            Polyline polyline = new Polyline(p);
            polyline.Transform(Rhino.Geometry.Transform.Translation(v));
            return polyline;
        }

        public static List<Polyline> Polygons(this IEnumerable<Circle> curves, int n, double rotation = Math.PI * 0.25, bool sqrt = true) {
            List<Polyline> polygons = new List<Polyline>();
            foreach (var c in curves)
                polygons.Add(Polygon(n, c, rotation, sqrt));
            return polygons;
        }

        public static List <Polyline> Polygons(this IEnumerable<Curve> curves, int n, double rotation = Math.PI * 0.25, bool sqrt = true) {
            List<Polyline> polygons = new List<Polyline>();
            foreach (var c in curves)
                polygons.Add(Polygon(n,c, rotation,sqrt));
            return polygons;
        }

        public static Polyline Polygon(int n, Circle circle, double rotation = Math.PI * 0.25, bool sqrt = true) {


                return Polygon(n, circle.Radius, circle.Plane, rotation, sqrt);

        }

        public static Polyline Polygon(int n, Curve c, double rotation = Math.PI * 0.25, bool sqrt = true) {

            if (c.TryGetCircle(out Circle circle)) {
                return Polygon(n, circle.Radius, circle.Plane, rotation, sqrt);

            }

            return new Polyline();
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

        public static Point3d CenterPoint(Polyline polyline) {

            int Count = polyline.Count;

            if (Count == 0) { return Point3d.Unset; }

            if (Count == 1) { return polyline[0]; }



            Point3d center = Point3d.Origin;

            double weight = 0.0;

            int stop = (Count - 1);
            if (polyline[0].DistanceToSquared(polyline[polyline.Count - 1]) > 0.001) {
                //Rhino.RhinoApp.WriteLine(polyline[0].DistanceToSquared(polyline[polyline.Length - 1]).ToString());
                stop++;
            }
            for (int i = 0; i < stop; i++) {

                Point3d A = polyline[i];

                Point3d B = polyline[(i + 1) % Count];

                double d = A.DistanceTo(B);

                center += d * 0.5 * (A + B);

                weight += d;

            }

            center /= weight;

            return center;

        }

        public static Polyline[] IntersectTwoPlates( Polyline plateA0,  Polyline plateA1,  Polyline plateB0,  Polyline plateB1) {


            double extend = 0;

            var pA0B0 = PolylinePlane(plateA0, plateB0);
            var pA0B1 = PolylinePlane(plateA0, plateB1);
            var pA1B0 = PolylinePlane(plateA1, plateB0);
            var pA1B1 = PolylinePlane(plateA1, plateB1);

            if (pA0B0[0] != Point3d.Unset && pA0B0[1] != Point3d.Unset && pA0B1[0] != Point3d.Unset && pA0B1[1] != Point3d.Unset &&
                pA1B0[0] != Point3d.Unset && pA1B0[1] != Point3d.Unset && pA1B1[0] != Point3d.Unset && pA1B1[1] != Point3d.Unset
                ) {


                Point3d pA0B0Mid = PointUtil.AveragePoint(pA0B0);
                Point3d pA0B1Mid = PointUtil.AveragePoint(pA0B1);
                Point3d pA1B0Mid = PointUtil.AveragePoint(pA1B0);
                Point3d pA1B1Mid = PointUtil.AveragePoint(pA1B1);

                Vector3d v0 = (pA0B0[0]- pA0B0[1]).UnitVector()* extend;
                Vector3d v1 = (pA0B1[0] - pA0B1[1]).UnitVector() * extend;
                Vector3d v2 = (pA1B0[0] - pA1B0[1]).UnitVector() * extend;
                Vector3d v3 = (pA1B1[0] - pA1B1[1]).UnitVector() * extend;

                Polyline u0 = new Polyline(new Point3d[] { pA0B0[0], pA0B0Mid-v0, pA0B1Mid - v1, pA0B1[0] });
                Polyline u1 = new Polyline(new Point3d[] { pA1B0[0], pA1B0Mid - v2, pA1B1Mid - v3, pA1B1[0] });

                Polyline u2 = new Polyline(new Point3d[] { pA0B0[1], pA0B0Mid+v0, pA1B0Mid+v2, pA1B0[1] });
                Polyline u3 = new Polyline(new Point3d[] { pA0B1[1], pA0B1Mid+v1, pA1B1Mid+v3, pA1B1[1] });

                return new Polyline[] {u0,u1,u2,u3 };

            }


            return new Polyline[0];



        }

        public static Point3d[] PolylinePlane(Polyline p0, Polyline p1) {

            Line[] segmentsP0 = p0.GetSegments();
            Line[] segmentsP1 = p1.GetSegments();

            Plane pl0 = p0.plane();
            Plane pl1 = p1.plane();

          //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(p0);
         //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(p1);


            Point3d ptP0 = Point3d.Unset;
            Point3d ptP1 = Point3d.Unset;

            foreach (Line line in segmentsP0) {
               
                double t;
                if (Intersection.LinePlane(line, pl1, out t)) {
                    if (t > 1 || t < 0) continue;
                    Point3d pTemp = line.PointAt(t);

                    if (p1.ToNurbsCurve().Contains(pTemp, pl1, 0.01) == PointContainment.Inside) {
                        ptP0 = pTemp;
                        break;
                    }
                }
            }

            foreach (Line lineP1 in segmentsP1) {
      
                double t;
                if (Intersection.LinePlane(lineP1, pl0, out t)) {
                    if (t > 1 || t < 0) continue;
                    Point3d pTemp = lineP1.PointAt(t);
                    if (p0.ToNurbsCurve().Contains(pTemp, pl0, 0.01) == PointContainment.Inside) {
                        ptP1 = pTemp;
                        break;
                    }
                }
            }

            return new Point3d[] { ptP0, ptP1 };
        }

        public static Line[] LoftLine(Polyline p0, Polyline p1) {
            Line[] line= new Line[ p0.Count - 1];

            for(int i = 0; i < p0.Count - 1; i++) {
                line[i] = new Line(p0[i], p1[i]);
            }
            return line;
        }

        public static void InsertPoint(this Polyline polyline, Point3d p) {
            polyline.Insert((int)Math.Ceiling(polyline.ClosestParameter(p)), p);
        }
        public static List<Polyline> MappedFromSurfaceToSurface(this List<Polyline> polylines, Surface s, Surface t ) {

            s.SetDomain(0, new Interval(0, 1));
            s.SetDomain(1, new Interval(0, 1));
            t.SetDomain(0, new Interval(0, 1));
            t.SetDomain(1, new Interval(0, 1));
            //  Rhino.RhinoDoc.ActiveDoc.Objects.AddSurface(s);

            List<Polyline> mapped = new List<Polyline>();

            for (int i = 0; i < polylines.Count; i++) {

                Polyline pol = new Polyline(polylines[i]);

                bool flag = true;

                for (int j = 0; j < pol.Count; j++) {
                    double u, v;
                    Point3d pTemp = new Point3d(pol[j]);
                    s.ClosestPoint(pol[j], out u, out v);

                    pol[j] = t.PointAt(u, v);

                    if (s.PointAt(u, v).DistanceTo(pTemp) > 0.01) {
                        flag = false;
                        break;
                    }

                }//for j
                if (flag) {
                    mapped.Add(pol);
                }
            }//for i
            return mapped;

        }


        public static List<Polyline> MappedFromMeshToMesh(this List<Polyline> polylines, Mesh s, Mesh t) {

            //    s.SetDomain(0, new Interval(0, 1));
            //    s.SetDomain(1, new Interval(0, 1));
            //    t.SetDomain(0, new Interval(0, 1));
            //    t.SetDomain(1, new Interval(0, 1));
            //  Rhino.RhinoDoc.ActiveDoc.Objects.AddSurface(s);

            List<Polyline> mapped = new List<Polyline>();

            for (int i = 0; i < polylines.Count; i++) {

                Polyline pol = new Polyline(polylines[i]);

                bool flag = true;

                for (int j = 0; j < pol.Count; j++) {
                    //point3d = mesh.PointAt(index, t, num, t1, num1);
                    //vector3d = mesh.NormalAt(index, t, num, t1, num1);

                    Point3d pTemp = new Point3d(pol[j]);

                    MeshPoint mp = s.ClosestMeshPoint(pol[j], 10.01);
                    if (mp == null) {
                        flag = false;
                        break;
                    }

                    //   Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(mp.Point);

                    pol[j] = t.PointAt(mp);

                    if (s.PointAt(mp).DistanceTo(pTemp) > 0.01) {
                        flag = false;
                        break;
                    }

                }//for j

                



                //try to trim curve
                if (!flag) {

                    //How to check if curve is on the first?
                    MeshPoint mp0 = s.ClosestMeshPoint(pol[0],10);
                    MeshPoint mp1 = s.ClosestMeshPoint(pol[pol.Count-1], 10);


                }


                if (flag) {
                    mapped.Add(pol);
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pol);
                }
            }//for i
            return mapped;

        }

        public static Polyline OutlineFromFaceEdgeCorner(Plane facePlane, Plane[] edgePlanes, Plane[] bisePlanes, int T = 1, double tolerance = 0.1) {
            Polyline polyline = new Polyline();

            switch (T) {
                case (2):

                for (int j = 0; j < edgePlanes.Length; j++) {

                    Plane currPlane = edgePlanes[j];
                    Plane nextPlane = edgePlanes[MathUtil.Wrap(j + 1, edgePlanes.Length)];

                  
                    if (Vector3d.VectorAngle(currPlane.XAxis, nextPlane.XAxis) < tolerance) {
                        Vector3d vv = new Vector3d(currPlane.XAxis);
                        vv.Rotate(Math.PI * 0.5, currPlane.YAxis);
                        nextPlane = new Plane(bisePlanes[j].Origin, vv, currPlane.YAxis);
                    }

                    Line line = PlaneUtil.PlanePlane(currPlane, nextPlane);
                    polyline.Add(PlaneUtil.LinePlane(line, facePlane));

                }
                polyline.Close();
                break;

                default:
                for (int j = 0; j < bisePlanes.Length; j++) {
                    Point3d pt;
                    Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(facePlane, bisePlanes[j], edgePlanes[j], out pt);
                    polyline.Add(pt);
                }
                polyline.Close();
                break;
            }


            return polyline;
        }

        public static Polyline ProjectPolyline(this Polyline p, bool averagePlane = true) {


            Polyline p_ = new Polyline(p.Count);

            Plane plane = p.plane();
            if (!averagePlane)
                Plane.FitPlaneToPoints(p, out plane);

            int closed = (p[0].DistanceToSquared(p[p.Count - 1]) < 0.001) ? 1 : 0;

            for (int i = 0; i < p.Count - closed; i++) {
                p_.Add(plane.ClosestPoint(p[i]));
            }

            if (closed == 1)
                p_.Close();


            return p_;

        }

        public static List<Curve> ShatterCurve(Curve C, List<Point3d> P, double D = 0.01) {
            List<Curve> curves = new List<Curve>();
            try {
             
                List<double> shatterT = new List<double>();

                Curve c = C.DuplicateCurve();
                //c.Domain = new Interval(0, 1);
                double a = c.Domain.T0;
                double b = c.Domain.T1;

                for (int i = 0; i < P.Count; i++) {
                    double t;
                    bool flag = c.ClosestPoint(P[i], out t, D);

                    if (flag) {
                        shatterT.Add((double)Math.Round(t,5));
                       // shatterT.Add(t);
                    }
                }

                if (shatterT.Count > 0) {

                    shatterT.Sort();
                    shatterT = shatterT.Distinct().ToList();


                    if (shatterT[0] == a && shatterT[shatterT.Count - 1] == b)
                        shatterT.RemoveAt(0);


                    if (c.PointAtStart.DistanceToSquared(c.PointAtEnd) < 0.001) {
                        for (int i = 0; i < shatterT.Count; i++) {
                            //Rhino.RhinoApp.WriteLine(shatterT[i].ToString() +  " " + shatterT[(i + 1) % shatterT.Count].ToString()) ;

             
                            if(shatterT[shatterT.Count - 1] ==b) {
                                Curve curve = c.Trim(shatterT[NGonsCore.MathUtil.Wrap(i - 1, shatterT.Count)], shatterT[i]);
                                curves.Add(curve);
                            } else {
                                Curve curve = c.Trim(shatterT[i], shatterT[(i + 1) % shatterT.Count]);
                                curves.Add(curve);
                            }

                        }
                    } else {

                        if (shatterT[0] != a)
                            shatterT.Insert(0, a);

                        if (shatterT[shatterT.Count - 1] != b)
                            shatterT.Insert(shatterT.Count, b);

                        for (int i = 0; i < shatterT.Count - 1; i++) {
                            Curve curve = c.Trim(shatterT[i], shatterT[(i + 1)]);
                            curves.Add(curve );
                        }


                    }


                } else {
                    curves.Add(c);
                }
               

            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());            }
            return curves;
        }




        public static Polyline IntersectPlanarLines(Line[] lines, bool close = true) {

            Polyline polyline = new Polyline();

            for (int i = 0; i < lines.Length; i++) {

                double a, b;
                Rhino.Geometry.Intersect.Intersection.LineLine(lines[i], lines[(i + 1) % lines.Length], out a, out b, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                Point3d p = lines[i].PointAt(a);
                polyline.Add(p);

            }

            if (close)
                polyline.Add(polyline[0]);

            return polyline;

        }

        public static Polyline IntersectPlanarLines(Line[] lines, Plane projectToPlane, bool close = true) {

            for (int i = 0; i < lines.Length; i++) {
                lines[i].From = projectToPlane.ClosestPoint(lines[i].From);
                lines[i].To = projectToPlane.ClosestPoint(lines[i].To);
            }

            return IntersectPlanarLines(lines, close);

        }


        public static List<Polyline> JoinPolylines(IEnumerable<Polyline> Polylines, ref List<List<int>> Groups) {
            List<Point3d> point3ds = new List<Point3d>();
            SortedList<TopologyEdge, Polyline> topologyEdges = new SortedList<TopologyEdge, Polyline>();
            SortedList<TopologyEdge, int> topologyEdges1 = new SortedList<TopologyEdge, int>();
            SortedList<TopologyEdge, int> item = new SortedList<TopologyEdge, int>();
            int num = checked(Polylines.Count<Polyline>() - 1);
            for (int i = 0; i <= num; i = checked(i + 1)) {
                point3ds.Add(Polylines.ElementAtOrDefault<Polyline>(i)[0]);
                point3ds.Add(Polylines.ElementAtOrDefault<Polyline>(i)[checked(Polylines.ElementAtOrDefault<Polyline>(i).Count - 1)]);
                TopologyEdge topologyEdge = new TopologyEdge(checked(point3ds.Count - 2), checked(point3ds.Count - 1), -1, -1);
                topologyEdges1[topologyEdge] = i;
                topologyEdges.Add(topologyEdge, Polylines.ElementAtOrDefault<Polyline>(i));
            }
            Graphs.DuplicateRemover duplicateRemover = new Graphs.DuplicateRemover(point3ds, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 2, 123);
            duplicateRemover.Solve();
            SortedList<TopologyEdge, Polyline> topologyEdges2 = new SortedList<TopologyEdge, Polyline>();
            List<int> nums = new List<int>();
            int num1 = 0;
            int count = checked(topologyEdges.Keys.Count - 1);
            for (int j = 0; j <= count; j = checked(j + 1)) {
                TopologyEdge item1 = topologyEdges.Keys[j];
                TopologyEdge topologyEdge1 = new TopologyEdge(duplicateRemover.Map[item1.From], duplicateRemover.Map[item1.To], -1, -1);
                if (!topologyEdges2.ContainsKey(topologyEdge1)) {
                    item[topologyEdge1] = topologyEdges1[item1];
                    topologyEdges2.Add(topologyEdge1, topologyEdges[item1]);
                } else {
                    topologyEdges2.Remove(topologyEdge1);
                    num1 = checked(num1 + 2);
                    nums.Add(topologyEdge1.From);
                    nums.Add(topologyEdge1.To);
                }
            }
            List<int>[] listArrays = new List<int>[checked(checked((int)duplicateRemover.UniquePoints.Length - 1) + 1)];
            int length = checked((int)listArrays.Length - 1);
            for (int k = 0; k <= length; k = checked(k + 1)) {
                listArrays[k] = new List<int>();
            }
            int count1 = checked(point3ds.Count - 1);
            for (int l = 0; l <= count1; l = checked(l + 2)) {
                listArrays[duplicateRemover.Map[l]].Add(duplicateRemover.Map[checked(l + 1)]);
                listArrays[duplicateRemover.Map[checked(l + 1)]].Add(duplicateRemover.Map[l]);
            }
            bool[] flagArray = new bool[checked(checked((int)duplicateRemover.UniquePoints.Length - 1) + 1)];
            int count2 = num1;
            int num2 = checked(nums.Count - 1);
            for (int m = 0; m <= num2; m = checked(m + 1)) {
                flagArray[nums[m]] = true;
            }
            SortedList<int, List<int>> nums1 = new SortedList<int, List<int>>()
            {
                { 0, new List<int>() },
                { 1, new List<int>() },
                { 2, new List<int>() }
            };
            int length1 = checked((int)listArrays.Length - 1);
            for (int n = 0; n <= length1; n = checked(n + 1)) {
                if (!flagArray[n]) {
                    nums1[listArrays[n].Count].Add(n);
                }
            }
            List<int> nums2 = new List<int>();
            nums2.AddRange(nums1[1]);
            nums2.AddRange(nums1[2]);
            List<Polyline> polylines = new List<Polyline>();
            while (count2 < (int)flagArray.Length) {
                int count3 = checked(nums2.Count - 1);
                for (int o = 0; o <= count3; o = checked(o + 1)) {
                    if (!flagArray[nums2[o]]) {
                        List<int> nums3 = new List<int>();
                        nums3.Clear();
                        Polyline polyline = new Polyline();
                        List<int> nums4 = (List<int>)PointUtil.Walk(nums2[o], listArrays);
                        int num3 = checked(nums4.Count - 2);
                        for (int p = 0; p <= num3; p = checked(p + 1)) {
                            TopologyEdge topologyEdge2 = new TopologyEdge(nums4[p], nums4[checked(p + 1)], -1, -1);
                            Polyline polyline1 = topologyEdges2[topologyEdge2];
                            int item2 = item[topologyEdge2];
                            int num4 = topologyEdges2.IndexOfKey(topologyEdge2);
                            if (topologyEdges2.Keys[num4].From != topologyEdge2.From) {
                                polyline1.Reverse();
                            }
                            if (polyline.Count <= 0) {
                                polyline.AddRange(polyline1);
                            } else if (polyline[checked(polyline.Count - 1)] != polyline1[0]) {
                                polyline.AddRange(polyline1);
                            } else {
                                int count4 = checked(polyline1.Count - 1);
                                for (int q = 1; q <= count4; q = checked(q + 1)) {
                                    polyline.Add(polyline1[q]);
                                }
                            }
                            nums3.Add(item2);
                        }
                        Groups.Add(nums3);
                        polylines.Add(polyline);
                        count2 = checked(count2 + nums4.Count);
                        int count5 = checked(nums4.Count - 1);
                        for (int r = 0; r <= count5; r = checked(r + 1)) {
                            flagArray[nums4[r]] = true;
                        }
                    }
                }
            }
            return polylines;
        }

        private static Polyline OrderPolyline(this Polyline polyline) {

            if (polyline != null) {
                if (polyline.IsValid) {
                    bool flag = IsClockwiseClosedPolylineOnXYPlane(polyline);
                    //isClockWise = flag;
                    if (flag) {
                        Polyline polyline0 = new Polyline(polyline);
                        polyline0.Reverse();


                        return polyline0;

                    } else {
                        return polyline;
                    }
                }
   
                
            }
            return polyline;
        }

        public static bool IsClockwiseClosedPolylineOnXYPlane(this Polyline polygon) {
            double sum = 0;

            for (int i = 0; i < polygon.Count - 1; i++)
                sum += (polygon[i + 1].X - polygon[i].X) * (polygon[i + 1].Y + polygon[i].Y);

            return sum > 0;
        }

        public static Polyline Chamfer(this Polyline polyline, double value = 0.001) {

            Line[] lines = polyline.GetSegments();

            if (value == 0)
                return polyline;

            Polyline p = new Polyline();

            if (value < 0) {
                foreach (Line l in lines) {
                    Line lShorter = NGonsCore.CurveUtil.ExtendLine(l, -Math.Abs(value));
                    p.Add(lShorter.From);
                    p.Add(lShorter.To);
                }
            } else {
                foreach (Line l in lines) {
                    p.Add(l.PointAt(value));
                    p.Add(l.PointAt(1 - value));
                }
            }

            p.Add(p[0]);

            List<Point3d> points = new List<Point3d>();

            for (int i = 1; i < p.Count - 1; i += 2) {
                points.Add(new Point3d(
                  (p[i + 1].X + p[i].X) * 0.5,
                  (p[i + 1].Y + p[i].Y) * 0.5,
                  (p[i + 1].Z + p[i].Z) * 0.5
                  )
                  );
            }

            return p;


        }


        public static void InsertPolyline(this Polyline x, IEnumerable<Polyline> y) {

            foreach (Polyline poly in y) {
                double t0 = x.ClosestParameter(poly[0]);
                double t1 = x.ClosestParameter(poly.Last);
                if (t0 > t1)
                    poly.Reverse();
                x.InsertRange((int)Math.Ceiling(t0), poly);
            }
        }


        public static void InsertPolyline(this Polyline x, Polyline poly) {


            double t0 = x.ClosestParameter(poly[0]);
            double t1 = x.ClosestParameter(poly.Last);
            if (t0 > t1)
                poly.Reverse();
            x.InsertRange((int)Math.Ceiling(t0), poly);

        }

        public static List<Polyline> InterpolateTwoLines(Line l0, Line l1, int n = 1) {
            List<Polyline> squares = new List<Polyline>();

            if (n > 0) {


                Point3d[] interpolatePt0 = PointUtil.InterpolatePoints(l0.From, l0.To, n); //bottom interpolation
                Point3d[] interpolatePt1 = PointUtil.InterpolatePoints(l1.From, l1.To, n); //top inerpolation

                for (int i = 0; i < n + 1; i++) {
                    Polyline polyline = new Polyline(new[] {
                    interpolatePt0[i],
                    interpolatePt0[i+1],
                    interpolatePt1[i+1],
                    interpolatePt1[i],
                    interpolatePt0[i]
                });
                    squares.Add(polyline);
                }

                return squares;

            } else {


                squares.Add(new Polyline(new[] {
                    l0.From,
                    l0.To,
                    l1.To,
                    l1.From,
                    l0.From
                    }));


            }

            return squares;


        }


        public static Line PlanePlanePlanePlane(Plane planeToIntersectLine0, Plane planeToIntersectLine1, Plane planeToCutLine0, Plane planeToCutLine1) {

            Rhino.Geometry.Intersect.Intersection.PlanePlane(planeToIntersectLine0, planeToIntersectLine1, out Line intersectedLine);
            Rhino.Geometry.Intersect.Intersection.LinePlane(intersectedLine, planeToCutLine0, out double t0);
            Rhino.Geometry.Intersect.Intersection.LinePlane(intersectedLine, planeToCutLine1, out double t1);
            Line line = new Line(intersectedLine.PointAt(t0), intersectedLine.PointAt(t1));
            return line;
        }

        public static Polyline PlaneLines(Plane plane, IEnumerable<Line> lines, bool close = true) {


            Polyline polyline = new Polyline();

            foreach (Line l in lines) {

                Rhino.Geometry.Intersect.Intersection.LinePlane(l, plane, out double t);
                polyline.Add(l.PointAt(t));
            }
            polyline.Add(polyline[0]);
            return polyline;
        }
        public static Plane MovePlane(Plane Base, double Thickness) {
            return new Plane(Base.Origin + Base.Normal * (Thickness / 2), Base.Normal);
        }

        public static Point3d IntPtPln1(Line line, Plane plane) {
            double pm;
            Rhino.Geometry.Intersect.Intersection.LinePlane(line, plane, out pm);
            return line.PointAt(pm);
        }

        public static Line IntPtPln(Point3d origin, Vector3d axis, Plane planeBot, Plane planeTop) {
            Line lineTEMP = new Line(origin, origin + axis);
            double pmBot;
            double pmTop;
            Rhino.Geometry.Intersect.Intersection.LinePlane(lineTEMP, planeBot, out pmBot);
            Rhino.Geometry.Intersect.Intersection.LinePlane(lineTEMP, planeTop, out pmTop);

            return new Line(lineTEMP.PointAt(pmBot), lineTEMP.PointAt(pmTop));
        }

        public static Line tweenLine(Line l0, Line l1, double t = 0.5) {
            return new Line(MathUtil.Lerp(l0.From , l1.From, t) , MathUtil.Lerp(l0.To , l1.To, t));
        }

        public static Polyline tweenPolylines(Polyline l0, Polyline l1, double t = 0.5) {

            Polyline p = new Polyline(l0);

            for(int i = 0; i< l0.Count; i++) {
                p[i] = MathUtil.Lerp(l0[i], l1[i], t);
            }


            return p;
        }

        public static Polyline PolylineFromPlanes(Plane basePlane, List<Plane> sidePlanes, bool close = true) {

            Polyline polyline = new Polyline();

            for (int i = 0; i < sidePlanes.Count - 1; i++) {
                Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(basePlane, sidePlanes[i], sidePlanes[i + 1], out Point3d pt);
                polyline.Add(pt);
            }

            Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(basePlane, sidePlanes[sidePlanes.Count - 1], sidePlanes[0], out Point3d pt1);
            polyline.Add(pt1);

            if (close)
                polyline.Add(polyline[0]);

            return polyline;

        }


        /// <summary>
        /// Move two planes by z axis and output one that is closer to target point
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="point"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Plane MovePolylinePlaneToPoint(this Polyline polyline, Point3d point, double dist)
        {

            Plane planeA = polyline.plane();
            planeA.Translate(planeA.ZAxis * dist);

            Plane planeB = polyline.plane();
            planeB.Translate(planeB.ZAxis * -dist);

            double dA = PointUtil.FastDistance(point, planeA.Origin);
            double dB = PointUtil.FastDistance(point, planeB.Origin);

            if (dA > dB)
                return planeB;

            return planeA;
        }

        //Only works for fully closed objects
        public static Line[] MaleFemale(this Polyline[] twoPlanarPolys, double dist)
        {
            Mesh mesh = Loft(twoPlanarPolys, 0.001);

            //Ngons planes
            //Get ngons planes and copy it to have offset and original set
            Plane[] planesO = mesh.GetNgonPlanes();

            //1.Get all edges
            int[][] nGonTV = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(nGonTV);

            //2.Get all edges in ngon faces
            int[][] ngonEdges = mesh.GetNGonFacesEdges(nGonTV);

            //3.Get ngons connected to edges
            int[][] edgeNgons = mesh.GetNgonsConnectedToNGonsEdges(allE);

            //loop through one face edges and get lines
            int i = 0;

            //Offset planes instead of current one
            Plane[] planes = mesh.GetNgonPlanes();
            planes.MovePlaneArrayByAxis(dist, i);

            Line[] lines = new Line[ngonEdges[i].Length];

            for (int j = 0; j < ngonEdges[i].Length; j++)
            {
                //Get edge faces
                int[] ngons = edgeNgons[ngonEdges[i][j]];
                Intersection.PlanePlane(planes[ngons[0]], planes[ngons[1]], out lines[i]);

            }


            //Mark edges id for insection vector?? If none supplied use perpedicular




            //Intersect 2 planes

            //Intersect plane line
            //Intersect other plane line

            return lines;

        }

        //Only works for fully closed objects
        public static Line[][] OffsetClosedMesh(this Polyline[] twoPlanarPolys, double dist)
        {

            //Loft two polylines to get solid box to extract normals
            Mesh mesh = Loft(twoPlanarPolys, 0.001);
            int[][] boundaries = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(boundaries);
            HashSet<int> allTV = mesh.GetAllNGonsTopoVertices();



            //Get ngons planes and copy it to have offset and original set
            Plane[] planesO = mesh.GetNgonPlanes();
            Plane[] planes = mesh.GetNgonPlanes();

            for (int j = 0; j < planes.Length; j++)
                planes[j].Translate(planes[j].Normal * -dist);


            //Get all edges end id
            int[][] pairs = mesh.GetAllNGonEdges_TopoVertices(boundaries, allE);

            //Now since we have closed volue we can compute planeplaneplane intersection


            //This is an array of all vertices
            //But they are not in order 2 6 4 0 12 and so on because numbers represent mesh vertices not ngon vertices
            var vToFDictionary = mesh.GetNGonsConnectedToNGonTopologyVerticesDictionary(allTV);



            //These are all ngons all ngons topology edges - order matches -> allE numbers
            Dictionary<int, Line> lines = new Dictionary<int, Line>(allE.Count);

            for (int i = 0; i < allE.Count; i++)
            {
                int vertexIDA = pairs[i][0];
                int vertexIDB = pairs[i][1];
                int[] adjFA = vToFDictionary[vertexIDA];
                int[] adjFB = vToFDictionary[vertexIDB];
                Intersection.PlanePlanePlane(planes[adjFA[0]], planes[adjFA[1]], planes[adjFA[2]], out Point3d a);
                Intersection.PlanePlanePlane(planes[adjFB[0]], planes[adjFB[1]], planes[adjFB[2]], out Point3d b);
                lines.Add(allE.ElementAt(i), new Line(a, b));
                //lines[i] = new Line(a, b);
            }

            //Output
            int[][] NGonsE = mesh.GetNGonFacesEdges(boundaries);

            Line[][] faceLines = new Line[NGonsE.Length][];

            for (int i = 0; i < NGonsE.Length; i++)
            {
                faceLines[i] = new Line[NGonsE[i].Length];
                for (int j = 0; j < NGonsE[i].Length; j++)
                {
                    faceLines[i][j] = lines[(NGonsE[i][j])];

                }
            }



            return faceLines;
        }
        
        public static Tuple<Polyline[][]> SingleDegreeJoints(this Polyline[] twoPlanarPolys, double dist, double jointSize, int d)
        {

            //Loft two polylines to get solid box to extract normals
            Mesh mesh = Loft(twoPlanarPolys, 0.001);
            Plane[] planesO = mesh.GetNgonPlanes();
            Plane[] planes = new Plane[planesO.Length - 2];
            Plane[] planesOO = new Plane[planesO.Length - 2];
            Array.Copy(planesO, 0, planes, 0, planesO.Length - 2);
            Array.Copy(planesO, 0, planesOO, 0, planesO.Length - 2);


            Plane[] biPlanesO = planes.BisectorPlaneArray();
            Plane[] biPlanes = biPlanesO;

            for (int j = 0; j < planes.Length; j++)
            {
                planes[j].Translate(planes[j].Normal * -dist);
                biPlanes[j].Translate(biPlanes[j].Normal * -jointSize);
            }




            //Take one segment of a line
            //Move it towards negative direction of plane normal
            //Intersect with neighbours polyline segmensts


            Polyline[][] po = new Polyline[twoPlanarPolys[0].SegmentCount][];

            for (int i = 0; i < twoPlanarPolys[0].SegmentCount; i++)
                //for (int i = 0; i < 1; i++)
            {


                int n = twoPlanarPolys[0].SegmentCount;
                int prev = (i - 1).Wrap(n);
                int next = (i + 1).Wrap(n);

                //Take original segments:
                Line lineT = twoPlanarPolys[0].SegmentAt(i);
                Line lineB = twoPlanarPolys[1].SegmentAt(i);

                //Create line from interesecting neighbour lines with current plane
                Line lineTO = IntersectionPlaneTwoLines(planes[i], twoPlanarPolys[0].SegmentAt(prev), twoPlanarPolys[0].SegmentAt(next));
                Line lineBO = IntersectionPlaneTwoLines(planes[i], twoPlanarPolys[1].SegmentAt(prev), twoPlanarPolys[1].SegmentAt(next));

                //Trim/Create new horizontal lines that polylines with bisect planes 
                //Trim that polylines to know connection offset 
                lineTO = lineTO.IntersectionLineTwoPlanes(planes[prev], planes[next]);
                lineBO = lineBO.IntersectionLineTwoPlanes(planes[prev], planes[next]);


                Line a = IntersectionLineTwoPlanes(lineT, biPlanes[i], biPlanes[prev]);
                Line b = IntersectionLineTwoPlanes(lineB, biPlanes[i], biPlanes[prev]);

                Line aO = IntersectionLineTwoPlanes(lineTO, biPlanes[i], biPlanes[prev]);
                Line bO = IntersectionLineTwoPlanes(lineBO, biPlanes[i], biPlanes[prev]);


                Line chamferA = new Line(a.From, aO.From);
                Line chamferB = new Line(b.From, bO.From);

                Intersection.LinePlane(chamferA, planesOO[next], out double t1);
                Intersection.LinePlane(chamferB, planesOO[next], out double t2);
                chamferA.From = chamferA.PointAt(t1);
                chamferB.From = chamferB.PointAt(t2);

                a.From = chamferA.From;
                b.From = chamferB.From;






                //Construct vector to trim and extend segments
                //po[i] = new[]
                //{
                //    lineT.ToP(),
                //    lineB.ToP(),
                //    lineTO.ToP(),
                //    lineBO.ToP()
                //};

                //Interpolate points for divisions this will not result in perpedicular elements
                po[i] = new[] {
                    //DovetailPolyline(lineA, lineB, lineA_, lineB_, d),
                    // DovetailPolyline(lineTO,lineBO,aO,  bO, d),

  
                    DovetailPolylineShifted(  new []
                        {
                            lineT.To ,
                            lineB.To,
                            a.From,//+ new Point3d(0,0,-4),
                            b.From,//+ new Point3d(0,0,4),
                            a.To ,
                            b.To,
                            lineT.From,
                            lineB.From
                        } ,
                        d),

                    DovetailPolylineShifted(  new []
                        {
                            lineTO.To,
                            lineBO.To,
                            aO.From,
                            bO.From,
                            aO.To,
                            bO.To,
                            lineTO.From,
                            lineBO.From,
                        } ,
                        d),

                    lineT.ToP(),
                    lineB.ToP(),
                    lineTO.ToP(),
                    lineBO.ToP(),
                    chamferA.ToP(),
                    chamferB.ToP()

                    //    //new Polyline(new[]{lineTO.From,lineTO.To}),
                    //    //new Polyline(new[]{lineBO.From,lineBO.To}),
                    //    //new Polyline(new[]{aO.From,aO.To}),
                    //    //new Polyline(new[]{bO.From,bO.To}),

                };

            }


            return new Tuple<Polyline[][]>(po);
        }
        
        public static Tuple<Polyline[][]> Dovetail(this Polyline[] twoPlanarPolys, double dist, int d)
        {

            //Loft two polylines to get solid box to extract normals
            Mesh mesh = Loft(twoPlanarPolys,0.001);
            Plane[] planesO = mesh.GetNgonPlanes();
            Plane[] planes = mesh.GetNgonPlanes();

            for (int j = 0; j < planes.Length; j++)
                planes[j].Translate(planes[j].Normal * -dist);


            //Take one segment of a line
            //Move it towards negative direction of plane normal
            //Intersect with neighbours polyline segmensts


            Polyline[][] po = new Polyline[twoPlanarPolys[0].SegmentCount][];

            for (int j = 0; j < twoPlanarPolys[0].SegmentCount; j++)
            {

                int i = j;
                int n = twoPlanarPolys[0].SegmentCount;
                int prev = (i - 1).Wrap(n);
                int next = (i + 1).Wrap(n);

                //Trip original segments:
                Line lineA_O = twoPlanarPolys[0].SegmentAt(i).IntersectionLineTwoPlanes(planes[prev], planes[next]);
                Line lineB_O = twoPlanarPolys[1].SegmentAt(i).IntersectionLineTwoPlanes(planes[prev], planes[next]);

                //Offset polyline by intersection points
                Line lineA = IntersectionPlaneTwoLines(planes[i], twoPlanarPolys[0].SegmentAt(prev), twoPlanarPolys[0].SegmentAt(next));
                Line lineB = IntersectionPlaneTwoLines(planes[i], twoPlanarPolys[1].SegmentAt(prev), twoPlanarPolys[1].SegmentAt(next));

                //Trim that polylines to know connection offset 
                Line lineA_ = lineA.IntersectionLineTwoPlanes(planes[prev], planes[next]);
                Line lineB_ = lineB.IntersectionLineTwoPlanes(planes[prev], planes[next]);


                //Interpolate points for divisions this will not result in perpedicular elements
                po[i] = new[] {
                    DovetailPolyline(lineA, lineB, lineA_, lineB_, d),
                    DovetailPolyline(twoPlanarPolys[0].SegmentAt(i), twoPlanarPolys[1].SegmentAt(i),lineA_O, lineB_O, d)
                };

            }


            return new Tuple<Polyline[][]>(po);
        }

        public static Polyline DovetailPolyline(Line lineA, Line lineB, Line lineA_, Line lineB_, int d)
        {
            Point3d[] interA = PointUtil.InterpolatePoints(lineA.From, lineB.From, d);
            Point3d[] interA_ = PointUtil.InterpolatePoints(lineA_.From, lineB_.From, d);



            Point3d[] interB;
            Point3d[] interB_;

            if (d % 2 == 1)
            {
                interB = PointUtil.InterpolatePoints(lineB.To, lineA.To, d);
                interB_ = PointUtil.InterpolatePoints(lineB_.To, lineA_.To, d);
            }
            else
            {
                interB_ = PointUtil.InterpolatePoints(lineB.To, lineA.To, d);
                interB = PointUtil.InterpolatePoints(lineB_.To, lineA_.To, d);
            }



            Polyline poly = new Polyline();
            Polyline temp = new Polyline();

            for (int j = 0; j < interA.Length - 1; j++)
            {
                if (j % 2 == 0)
                {
                    poly.Add(interA[j]);
                    poly.Add(interA[j + 1]);
                    temp.Add(interB[j]);
                    temp.Add(interB[j + 1]);
                }
                else
                {
                    poly.Add(interA_[j]);
                    poly.Add(interA_[j + 1]);
                    temp.Add(interB_[j]);
                    temp.Add(interB_[j + 1]);
                }
            }

            poly.AddRange(temp);
            poly.Close();
            return poly;
        }

        public static Polyline DovetailPolylineShifted(Point3d[] pts, int d)
        {
            Point3d[] interA = PointUtil.InterpolatePoints(pts[0], pts[1], d);
            Point3d[] interA_ = PointUtil.InterpolatePoints(pts[2], pts[3], d);



            Point3d[] interB;
            Point3d[] interB_;

            if (d % 2 == 1)
            {
                interB = PointUtil.InterpolatePoints(pts[5], pts[4], d);
                interB_ = PointUtil.InterpolatePoints(pts[7], pts[6], d);
            }
            else
            {
                interB_ = PointUtil.InterpolatePoints(pts[5], pts[4], d);
                interB = PointUtil.InterpolatePoints(pts[7], pts[6], d);
            }



            Polyline poly = new Polyline();
            Polyline temp = new Polyline();

            for (int j = 0; j < interA.Length - 1; j++)
            {
                if (j % 2 == 0)
                {
                    poly.Add(interA[j]);
                    poly.Add(interA[j + 1]);
                    temp.Add(interB[j]);
                    temp.Add(interB[j + 1]);
                }
                else
                {
                    poly.Add(interA_[j]);
                    poly.Add(interA_[j + 1]);
                    temp.Add(interB_[j]);
                    temp.Add(interB_[j + 1]);
                }
            }

            poly.AddRange(temp);
            poly.Close();
            return poly;
        }

        public static Line IntersectionPlaneTwoLines(Plane p, Line lnA, Line lnB)
        {
            Intersection.LinePlane(lnA, p, out double t1);
            Intersection.LinePlane(lnB, p, out double t2);
            return new Line(lnA.PointAt(t1), lnB.PointAt(t2));
        }

        public static double[] IntersectionPlaneTwoLinesT(Plane p, Line lnA, Line lnB)
        {
            Intersection.LinePlane(lnA, p, out double t1);
            Intersection.LinePlane(lnB, p, out double t2);
            return new[] { t1, t2 };
        }

        public static Line IntersectionLineTwoPlanes(this Line line, Plane pa, Plane pb)
        {
            Intersection.LinePlane(line, pa, out double t1);
            Intersection.LinePlane(line, pb, out double t2);
            return new Line(line.PointAt(t1), line.PointAt(t2));
        }

        public static double[] IntersectionLineTwoPlanesT(this Line line, Plane pa, Plane pb)
        {
            Intersection.LinePlane(line, pa, out double t1);
            Intersection.LinePlane(line, pb, out double t2);
            return new[] { t1, t2 };
        }

        public static Mesh Loft(Polyline[] twoPolys, double weld, bool cap = true) {
            
            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(twoPolys[0]);
            mesh.Vertices.AddVertices(twoPolys[1]);

            if (cap)
            {
                Mesh Top = MeshCreate.MeshFromPolylines(new[] {twoPolys[0]}, weld);
                Polyline p = new Polyline(twoPolys[1]);


                p.CollapseShortSegments(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                Mesh Bottom = Mesh.CreateFromClosedPolyline(p);
                Bottom.Flip(true, true, true);
                int[] tempV = Enumerable.Range(0, Bottom.Vertices.Count).ToArray();
                int[] tempF = Enumerable.Range(0, Bottom.Faces.Count).ToArray();
                Bottom.Ngons.AddNgon(MeshNgon.Create(tempV, tempF));
                Bottom.Normals.ComputeNormals();


                int n = twoPolys[1].Count;

                if (twoPolys[0].Count >= 2)
                {
                    int f = mesh.Faces.Count - 1;
                    if (twoPolys[0].Count == twoPolys[1].Count)
                        for (int j = 0; j < n - 1; j++)
                        {
                            mesh.Faces.AddFace( n + j, n + j + 1, j + 1,  j );
                            f++;
                            mesh.Ngons.AddNgon(MeshNgon.Create(new[]  {  n + j,  n + j + 1, j + 1,  j }, new[] {f}));
                        }
                }


                mesh.Append(Top);
                mesh.Append(Bottom);
            }
            else
            {
                int n = twoPolys[1].Count;

                if (twoPolys[0].Count >= 2) {
                    int f = mesh.Faces.Count - 1;
                    if (twoPolys[0].Count == twoPolys[1].Count)
                        for (int j = 0; j < n - 1; j++) {
                            mesh.Faces.AddFace( n + j, n + j + 1,  j + 1, j );
                            f++;
                            mesh.Ngons.AddNgon(MeshNgon.Create(new[]  {  n + j,n + j + 1, j + 1,  j   }, new[] { f }));
                        }
                }
            }


            //This is optional.
            if (weld > 0)
                mesh.WeldUsingRTree(weld);
            else
                mesh.Flip(true,true,true);
 
            return mesh;
        }

        public static Polyline[] ToPolylines(this IEnumerable<Curve> nurbsCurves, bool collapseShortSegments = true) {

            Polyline[] p = new Polyline[nurbsCurves.Count()];

            for (int i = 0; i < nurbsCurves.Count(); i++) {
                nurbsCurves.ElementAt(i).TryGetPolyline(out Polyline polyline);
                p[i] = nurbsCurves.ElementAt(i).ToPolyline(collapseShortSegments);
            }

            return p;

        }


        public static Polyline ToPolyline(this Curve curve, bool collapseShortSegments = true) {

            curve.TryGetPolyline(out Polyline polyline);
            if (collapseShortSegments)
                polyline.CollapseShortSegments(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance );
            return polyline;

        }


        public static Polyline[] ToPolylinesFromCP(this IEnumerable<Curve> curves, double collapseShortSegments = 0.01) {

            Polyline[] p = new Polyline[curves.Count()];

            int j = 0;
            foreach (Curve curve in curves) {
                p[j++] = ToPolylineFromCP(curve, collapseShortSegments);
            }

            return p;
        }

        public static Polyline ToPolylineFromCP(this Curve curve, double collapseShortSegments = 0.01) {

            Polyline polyline = new Polyline();
            if (curve.TryGetPolyline(out polyline)) {
                polyline.CollapseShortSegments(collapseShortSegments);
                return polyline;
            }


            NurbsCurve c = curve.ToNurbsCurve();

            Point3d[] points = new Point3d[c.Points.Count];

            for (int i = 0; i < c.Points.Count; i++)
                c.Points.GetPoint(i, out points[i]);

             polyline = new Polyline(points);


            //What the fuck these two lines
            c = polyline.ToNurbsCurve();
            c.TryGetPolyline(out polyline);
           
            

            if (collapseShortSegments>0)
                polyline.CollapseShortSegments(collapseShortSegments);
            polyline = new Polyline(polyline);

            //polyline.CollapseShortSegments(1);
            return polyline;


        }

        public static int[] append(int j) {
            var srsEnum = Enumerable.Range(0, j + 1);
            var arrVal = new List<int>();
            arrVal = srsEnum.ToList();
            arrVal.Add(0);
            return arrVal.ToArray();
        }


        public static Plane[] Planes(this Polyline[] polylines)
        {
            Plane[] p = new Plane[polylines.Length];

            for (int i = 0; i < polylines.Length; i++) {
                //p[i] = new Plane(polylines[i].CenterPoint(), polylines[i].Normal());
                p[i] = polylines[i].GetPlane();
            }

            return p;
        }

        public static Plane plane(this Polyline polylines)
        {
            return polylines.GetPlane();
            //return new Plane(polylines.CenterPoint(), polylines.AverageNormal());
        }

        public static Vector3d AverageNormal(this Polyline p)
        {
            //PolyFace item = this[index];
            int len = p.Count - 1;
            Vector3d vector3d = new Vector3d();
            int count = checked(len - 1);

            for (int i = 0; i <= count; i++)
            {
                int num = ((i - 1) + len) % len;
                int item1 = (checked(i + 1) + len) % len;
                Point3d point3d = p[num];
                Point3d point3d1 = p[item1];
                Point3d item2 = p[i];
                vector3d = vector3d + Vector3d.CrossProduct(new Vector3d(item2 - point3d), new Vector3d(point3d1 - item2));
            }

            if (vector3d.X == 0 & vector3d.Y == 0 & vector3d.Z == 0)
                vector3d.Unitize();

            return vector3d;
        }

        public static Vector3d Normal(this Polyline p)
        {
            var n = Vector3d.Unset;
            if (null != p && p.Count - 1 > 3)
            {
                var a = p[2] - p[0];
                var b = p[3] - p[1];
                n = Vector3d.CrossProduct(a, b);
                n.Unitize();
            }
            return n;
        }

        public static Polyline[][] LoftPolylines(Polyline[][] polylines)
        {
            Polyline[][] p = new Polyline[polylines.Length][];

            for (int i = 0; i < polylines.Length; i++)
            {
                if (polylines[i].Length >= 2)
                {
                    if (polylines[i][0].Count == polylines[i][1].Count)
                    {
                        p[i] = new Polyline[polylines[i][0].Count - 1];

                        for (int j = 0; j < polylines[i][0].Count - 1; j++)
                            p[i][j] = new Polyline() { polylines[i][0][j], polylines[i][0][j + 1], polylines[i][1][j + 1], polylines[i][1][j], polylines[i][0][j] };
                    }
                    else
                        p[i] = new Polyline[] { new Polyline() };
                }
            }

            return p;
        }

        public static Polyline[] LoftTwoPolylines(Polyline[] polylines)
        {
            Polyline[] p = new Polyline[polylines[0].Count - 1];

            if (polylines.Length >= 2)
            {
                if (polylines[0].Count == polylines[1].Count)
                    for (int j = 0; j < polylines[0].Count - 1; j++)
                        p[j] = new Polyline() { polylines[0][j], polylines[0][j + 1], polylines[1][j + 1], polylines[1][j], polylines[0][j] };

                else
                    p = new Polyline[] { new Polyline() };
            }
            return p;
        }

        public static void ShiftPolyline(this Polyline A, int n)
        {

            A.RemoveAt(A.Count - 1);

            for (int j = 0; j < n; j++)
            {

                int len = A.Count; //self explanatory 
                var tmp = A[len - 1]; //save last element value
                for (int i = len - 1; i > 0; i--) //starting from the end to begining
                    A[i] = A[i - 1]; //assign value of the previous element
                A[0] = tmp; //now "rotate" last to first.
            }

            A.Add(A[0]);

        }

        public static void Close(this Polyline p)
        {
            if(p.Count > 2)
                p.Add(p[0]);
        }

        public static Polyline ToP(this Line line)
        {
            return new Polyline(new[] { line.From, line.To });
        }

        public static Curve[] ToCurveArray(this Polyline[] polyline)
        {
            Curve[] curves = new Curve[polyline.Length];

            for (int i = 0; i < polyline.Length; i++)
                curves[i] = polyline[i].ToNurbsCurve();

            return curves;
        }

        private static Random random = new Random();

        public static Polyline ExpandDuplicatedPoints (this Polyline polyline, double tolerance=1E-5)
        {
            Polyline poly = new Polyline(polyline.ToArray());


            Plane.FitPlaneToPoints(poly, out Plane plane);

            for (int i = 1; i < poly.Count-1; i+=2)
            {
                double dist = poly[i].DistanceTo(poly[i + 1]);
                double dist2 = poly[i].DistanceTo(poly[MathUtil.Wrap(i - 1,poly.Count)]);
                if (dist < tolerance) {
                    Point3d pt = poly[i];
                    double t = poly.ClosestParameter(pt);
                    poly[i] = poly.PointAt(t + 0.00001);
                }

                if (dist2 < tolerance) {
                    Point3d pt = poly[i];
                    double t = poly.ClosestParameter(pt);
                    poly[i] = poly.PointAt(t - 0.00001);
                }
            }

            return poly;

        }

      
        public static Rhino.Geometry.Mesh LoftPolylineWithHoles(Polyline[] polylines0, Polyline[] polylines1) {

            Mesh mesh0 = NGonsCore.MeshCreate.MeshFromClosedPolylineWithHoles(polylines0);
            Mesh mesh1 = NGonsCore.MeshCreate.MeshFromClosedPolylineWithHoles(polylines1);
            //mesh0.Bake();
            //polylines0[0].Bake();
           // polylines1[0].Bake();

            mesh1.Flip(true, true, true);
            Mesh m = new Mesh();
            m.Append(mesh0);
            m.Append(mesh1);

            int n = 0;

            foreach (var p in polylines0)
                n += p.Count - 1;

            int c = 0;

            for (int i = 0; i < polylines0.Length; i++) {
                int counter = polylines0[i].Count - 1;

                for (int j = 0; j < counter; j++) {
                    if (j == counter - 1)
                        m.Faces.AddFace(c + j, c + 0, c + n + 0, c + n + j);
                    else
                        m.Faces.AddFace(c + j, c + j + 1, c + n + (j + 1), c + n + j);
                }


                c += counter;
            }

            //m.Clean();
            if (m.SolidOrientation() == -1)
                m.Flip(true, true, true);
            
            //m.Vertices.CombineIdentical(true, true);
           // m.Vertices.CullUnused();
            //m.Weld(3.14159265358979);
            m.FaceNormals.ComputeFaceNormals();
            m.Normals.ComputeNormals();

            // m.WeldFull(0.001);

            //m.Unweld(0, true);

            m.UnifyNormals();
            m.FaceNormals.UnitizeFaceNormals();
            m.Unweld(0.001, true);
            return m;


        }

        public static Rhino.Geometry.Mesh[][] LoftPolylineWithHoles(Polyline[][][] polylines0, Polyline[][][] polylines1) {

            Mesh[][] meshes = new Mesh[polylines0.Length][];

            for (int i = 0; i < polylines0.Length; i++) {

                meshes[i] = new Mesh[polylines0[i].Length];

                for (int j = 0; j < polylines0[i].Length; j++) {
                    meshes[i][j] = LoftPolylineWithHoles(polylines0[i][j], polylines1[i][j]);
                }

            }

            return meshes;
        }


        public static Rhino.Geometry.Mesh LoftTwoCurves (Curve C0, Curve C1, int N = 2, bool E = true) {

            if (N < 1)
                N = 2;

            Mesh mesh = new Mesh();


            if (C0 != null && C1!=null) {

                Polyline p0;
                Polyline p1;

                bool f0 = C0.TryGetPolyline(out p0);
                bool f1 = C1.TryGetPolyline(out p1);

                if ((f0 && f1 && p0.Count == p1.Count) == false) {

                    p0 = divideByCount(C0, N, C0.IsClosed);
                    p1 = divideByCount(C1, N, C0.IsClosed);
                }

                //If 2 polylines and number of points the same


                mesh.Vertices.AddVertices(p0);
                mesh.Vertices.AddVertices(p1);
                int n = p0.Count;

                int counter = 0;
                for (int i = 0; i < n - 1; i++) {
                    mesh.Faces.AddFace(i, i + 1, i + 1 + n, i + n);
                    mesh.Ngons.AddNgon(MeshNgon.Create(new int[] { i, i + 1, i + 1 + n, i + n }, new int[] { counter++ }));
                }

                if (p0.IsClosed && p1.IsClosed && E) {

                    Mesh m0 = Mesh.CreateFromClosedPolyline(p0);
                    Mesh m1 = Mesh.CreateFromClosedPolyline(p1);

                    int fc0 = mesh.Faces.Count;
                    int fc1 = m0.Faces.Count;

                    foreach (MeshFace f in m0.Faces)
                        mesh.Faces.AddFace(f);


                    mesh.Ngons.AddNgon(MeshNgon.Create(Enumerable.Range(0, n - 1).ToList(), Enumerable.Range(fc0, fc1).ToList()));

                    fc0 = mesh.Faces.Count;
                    fc1 = m1.Faces.Count;

                    foreach (MeshFace f in m1.Faces)
                        mesh.Faces.AddFace(f.A + n, f.B + n, f.C + n);

                    mesh.Ngons.AddNgon(MeshNgon.Create(Enumerable.Range(n, n - 1).ToList(), Enumerable.Range(fc0, fc1).ToList()));
                }//if polyline is closed


            }



            if (mesh.IsValid) {
                mesh.Clean();
                mesh.Normals.ComputeNormals();
                mesh.UnifyNormals();
                mesh.UnifyNormalsNGons();
                mesh.Weld(0.001);
            }

            return mesh;


        }


        public static Polyline divideByCount(Curve curve, int n, bool close) {

            curve.Domain = new Interval(0, 1);

            if (n < 1)
                return null;

            switch (n) {
                case (1):
                if (curve.IsClosed)
                    return new Polyline(new[] { curve.PointAt(0.0), curve.PointAt(0.5) });
                else
                    return new Polyline(new[] { curve.PointAt(0.0), curve.PointAt(1.0) });

                case (2):
                if (curve.IsClosed)
                    return new Polyline(new[] { curve.PointAt(0.0), curve.PointAt(0.33333), curve.PointAt(0.66666) });
                else
                    return new Polyline(new[] { curve.PointAt(0.0), curve.PointAt(0.5), curve.PointAt(1.0) });

                default:

                Point3d[] pts;
                curve.DivideByCount(n, true, out pts);

                Polyline polyline = new Polyline(pts);
                if (close)
                    polyline.Add(polyline[0]);

                return polyline;
            }


        }





    }
}
