using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore {

    public class Plates : IEnumerable<Plate> {

        List<Plate> plates = new List<Plate>();


        //Ienumerable Methods
        public IEnumerator<Plate> GetEnumerator() {
            return this.plates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.plates.GetEnumerator();
        }

        public Plate this[int index] {
            get { return plates[index % plates.Count]; }
            set { this.plates[index % plates.Count] = value.Duplicate(); }
        }

        public int Count() {
            return this.plates.Count;
        }

        public void Add(Plate plate) {
            this.plates.Add(plate.Duplicate());
        }

        public void AddRange(List<Plate> plates) {
            foreach (Plate p in plates)
                this.plates.Add(p.Duplicate());
        }

        public void Renumber() {
            for (int i = 0; i < plates.Count; i++) {
                plates[i].id = i;
            }
        }

        public override string ToString() {
            return "Plates " + plates.Count().ToString();
        }

        public Plates Duplicate() {
            Plates copy = new Plates();
            foreach (Plate p in plates)
                copy.Add(p.Duplicate());
            return copy;
        }


        public void Transform(Transform xform) {
            foreach (Plate p in plates)
                p.Transform(xform);
        }

        public void RemoveAt(int i) {
            this.plates.RemoveAt(i);
        }


    }

    public  class Plate {

        public Polyline face0;
        public Polyline face1;

        public Polyline faceJoints0;
        public Polyline faceJoints1;

        public int id = -1;

        public Plate() { }

        public Plate(Polyline pline0, Polyline pline1) {
            face0 = new Polyline(pline0);
            face1 = new Polyline(pline1);
            faceJoints0 = new Polyline(pline0);
            faceJoints1 = new Polyline(pline1);
        }

        public void Transform(Transform xform) {
            face0.Transform(xform);
            face1.Transform(xform);
            faceJoints0.Transform(xform);
            faceJoints1.Transform(xform);
        }

        public Plate Duplicate() {
            Plate copy = new Plate();
            copy.face0 = new Polyline(new Polyline(this.face0));
            copy.face1 = new Polyline(new Polyline(this.face1));
            copy.faceJoints0 = new Polyline(new Polyline(this.faceJoints0));
            copy.faceJoints1 = new Polyline(new Polyline(this.faceJoints1));
            copy.id = id;
            return copy;
        }

        public override string ToString() {
            return "Plate " + id.ToString();
        }

    }
    public static class Joinery {

        public static Polyline ZigZagFromRectangle(Rectangle3d rec, double step,double scale=0.9) {
            Polyline pline = new Polyline();
            double step_ = Math.Max(step*1.5,scale);
            Rectangle3d recShrink = new Rectangle3d(rec.Plane,new Interval(-(rec.Width*  0.5 ) + 0, (rec.Width *  0.5 ) - 0), new Interval(-(rec.Height *  0.5) + step_, (rec.Height *  0.5 )- step_));
            int divisions = (int)MathUtil.Constrain(recShrink.Width / step,2,500);
            divisions += divisions % 2;

            Polyline zz = PolylineUtil.ZigZag(recShrink.ToPolyline(), false, step);
            
            return zz;
        }


        public static List<List<Polyline>> Miter(Mesh M,  double D,double JointStep, double scale, ref GH_Structure<GH_Curve> additionalPlines) {


            List<List<Polyline>> zzAll = new List<List<Polyline>>();
            List<List<Line>> zzLinesAll = new List<List<Line>>();
            List<List<Plane>> zzPlanesAll = new List<List<Plane>>();



            Polyline[] plines = M.GetPolylines();
            Plane[] planes = M.GetNgonPlanes();
            int[][] tv = M.GetNGonsTopoBoundaries();
            int[][] fe = M.GetNGonFacesEdges(tv);
            HashSet<int> allE = M.GetAllNGonEdges(tv);
            int[][] ef = M.GetNgonsConnectedToNGonsEdges(allE, true);
            uint[][] finf = M.GetFacesInNGons();
            int[] allEArray = allE.ToArray();

            Dictionary<int, int> meNgonE = new Dictionary<int, int>();
            for (int i = 0; i < allEArray.Length; i++)
                meNgonE.Add(allEArray[i], i);

            Plane[][] bisePlanes;
            Plane[][] edgePlanes;//"ZY"
            M.GetEdgeAndBisectorPlanes(out bisePlanes, out edgePlanes);




      

            for (int i = 0; i < plines.Length; i++) {

                var zz = new List<Polyline>();
                var zzLines = new List<Line>();
                var zzPlanes = new List<Plane>();

                if (D != 0) {

                    Point3d center = M._Plane(i).Origin;

                    for (int j = 0; j < plines[i].Count - 1; j++) {

                       

                        int e = fe[i][j];

                        int[] faces = M.TopologyEdges.GetConnectedFaces(e);
                        int neiFace = faces[0];
                        Vector3d normalDir = Vector3d.Zero;

                        if (faces.Length == 2) {

                            bool flag = false;
                            foreach (var ff in finf[i]) {
                                if (faces[0] == (int)ff) {
                                    flag = true;
                                    break;
                                }
                            }
                            neiFace = flag ? faces[1] : faces[0];

                            normalDir = (M.FaceNormals[faces[1]] + M.FaceNormals[faces[0]]);
                            normalDir.Unitize();

                        } else {
                            continue;
                            normalDir = (M.FaceNormals[faces[0]]);
                            normalDir.Unitize();

                        }

                        Line l = plines[i].SegmentAt(j);
                        Vector3d dir = Vector3d.CrossProduct(M.FaceNormals[neiFace], l.Direction);
                        dir.Unitize();
                        Point3d p0 = l.PointAt(0.5);
                         dir = -normalDir;

                        Vector3d sideVec = Vector3d.CrossProduct(l.Direction, normalDir);
                        Rectangle3d rect = new Rectangle3d(new Plane(l.Center(), sideVec, l.Direction), new Interval(-D * 0.5, D * 0.5), new Interval(-l.Length * 0.5, l.Length * 0.5));
                        Polyline zigzag = ZigZagFromRectangle(rect, JointStep,scale);

                        //Project zigzag points to plane by direction of plane normal
                        for(int k = 0; k < zigzag.Count; k++) {
                            Line zzLine = new Line(zigzag[k], zigzag[k] + rect.Plane.ZAxis);
                              zigzag[k]=PlaneUtil.LinePlane(zzLine, planes[i]);
                        }
                        zzPlanes.Add(rect.Plane);

                        zz.Add(zigzag);

                        int sign = faces.Length == 2 ? -1 : -1;
                        //if (F)
                        //    sign *= -1;
                        double distance = D * sign;

                        Polyline polyline = new Polyline() { l.From, l.To, l.To + dir * distance, l.From + dir * distance, l.From };


                    }//for j
                    zzAll.Add(zz);
                    zzPlanesAll.Add(zzPlanes);
                }

            }

            //Add zigzag to panels
            for (int i = 0; i < plines.Length; i++) {

                plines[i].InsertPolyline(zzAll[i]);
            }

            if (additionalPlines != null) {

                for (int i = 0; i < additionalPlines.Branches.Count; i++) {

                    if (additionalPlines[i].Count != plines.Length)
                        continue;

                    for (int j = 0; j < additionalPlines[i].Count; j++) {

                        //Cast to Polyline
                        Curve curve = additionalPlines[i][j].Value;
                        bool good = curve.TryGetPolyline(out Polyline aPline);

                        if (good) {
                            Plane plane = aPline.GetPlane();

                            for (int k = 0; k < zzAll[j].Count; k++) {
                                Polyline zzPolyline = new Polyline();
                              
                                for (int m = 0; m < zzAll[j][k].Count; m++) {
                                    Line zzLine = new Line(zzAll[j][k][m], zzAll[j][k][m] + zzPlanesAll[j][k].ZAxis);
                                     Point3d zzPoint = PlaneUtil.LinePlane(zzLine, plane);
                                   zzPolyline.Add(zzPoint);
                                   //
                                }
                               //zzPolyline.Bake();
                                aPline.InsertPolyline(zzPolyline);
                            }
                            //aPline.Bake();
                            //break;
                            additionalPlines[i][j] = new GH_Curve(new Polyline(aPline).ToNurbsCurve());
                        }
                        //
                      
                    }

                }//iterate of n-th layer of mesh.

            }//not null

            //zzAll.Clear();



            //return zzAll;

            return new List<List<Polyline>>() { plines.ToList() };

        }



        public static List<List<Polyline>> Miter(Mesh M, double D, double JointStep, double scale, GH_Structure<GH_Curve> additionalPlines) {

            if (additionalPlines.Branches.Count != 2)
                return null;
            if (additionalPlines[0].Count != M.Ngons.Count || additionalPlines[1].Count != M.Ngons.Count)
                return null;


                
            List <List<Polyline>> zzAll = new List<List<Polyline>>();
            List<List<Line>> zzLinesAll = new List<List<Line>>();
            List<List<Plane>> zzPlanesAll = new List<List<Plane>>();



            Polyline[] plines = M.GetPolylines();
            //Plane[] planes = M.GetNgonPlanes();
            int[][] tv = M.GetNGonsTopoBoundaries();
            int[][] fe = M.GetNGonFacesEdges(tv);
            HashSet<int> allE = M.GetAllNGonEdges(tv);
            int[][] ef = M.GetNgonsConnectedToNGonsEdges(allE, false);
            uint[][] finf = M.GetFacesInNGons();
            int[] allEArray = allE.ToArray();

            Dictionary<int, int> meNgonE = new Dictionary<int, int>();
            for (int i = 0; i < allEArray.Length; i++)
                meNgonE.Add(allEArray[i], i);



            //Get planes
            Plane[] planes = new Plane[M.Ngons.Count];
            for (int i = 0; i < additionalPlines[0].Count; i++) {
                Curve curve = additionalPlines[0][i].Value;
                curve.TryGetPolyline(out Polyline pline);
                planes[i] = pline.GetPlane();
            }

            Line[] allEArrayLines = new Line[allEArray.Length];
            Plane[] allEArrayBisector = new Plane[allEArray.Length];

            for (int i = 0; i < M.Ngons.Count; i++) {
                zzAll.Add(new List<Polyline>());
                zzPlanesAll.Add(new List<Plane>());
            }


                //Get line segments and edge planes
                for (int i = 0; i < allEArray.Length; i++) {
                int me = allEArray[i];
                int ne = meNgonE[me];
                int[] edgeFaces = ef[ne];

                if(edgeFaces.Length == 2) {
                    //Rhino.RhinoApp.WriteLine(edgeFaces[0].ToString() + " " + edgeFaces[1].ToString());
                    int[] fe0 = fe[edgeFaces[0]];
                    int[] fe1 = fe[edgeFaces[1]];
                    int localID0 = Array.IndexOf(fe0, me);
                    int localID1 = Array.IndexOf(fe1, me);


                    //Get segments
                    Curve curve = additionalPlines[0][edgeFaces[0]].Value;
                    curve.TryGetPolyline(out Polyline pline0);
                    curve = additionalPlines[1][edgeFaces[0]].Value;
                    curve.TryGetPolyline(out Polyline pline1);
                    Line l0 = PolylineUtil.tweenLine(pline0.SegmentAt(localID0), pline1.SegmentAt(localID0));

                    curve = additionalPlines[0][edgeFaces[1]].Value;
                    curve.TryGetPolyline(out Polyline pline2);
                    curve = additionalPlines[1][edgeFaces[1]].Value;
                    curve.TryGetPolyline(out Polyline pline3);
                    Line l1= PolylineUtil.tweenLine(pline2.SegmentAt(localID1), pline3.SegmentAt(localID1));
                    //l0.Bake();
                    //l1.Bake();
                    l1.Flip();

                    allEArrayLines[ne] = PolylineUtil.tweenLine(l0, l1);
                    Vector3d XAxis = allEArrayLines[ne].Direction;

                    var pointsFit = new List<Point3d>();
                    pointsFit.AddRange(pline0.SegmentAt(localID0).ToP());
                    pointsFit.AddRange(pline1.SegmentAt(localID0).ToP());
                    pointsFit.AddRange(pline2.SegmentAt(localID1).ToP());
                    pointsFit.AddRange(pline3.SegmentAt(localID1).ToP());
                    Plane.FitPlaneToPoints(pointsFit, out Plane fitPlane);
                    fitPlane.Rotate(Vector3d.VectorAngle(fitPlane.XAxis, XAxis,fitPlane),fitPlane.ZAxis);



                    Point3d p0 = pline0.SegmentAt(localID0).PointAt(0.5);
                    Point3d p1 = pline1.SegmentAt(localID0).PointAt(0.5);

                    //Vector3d YAxis = (pline0.SegmentAt(localID0).PointAt(0.5) - pline1.SegmentAt(localID0).PointAt(0.5)) + (pline0.SegmentAt(localID1).PointAt(0.5) - pline1.SegmentAt(localID1).PointAt(0.5));
                    Plane plane = new Plane(allEArrayLines[ne].PointAt(0.5),XAxis, fitPlane.YAxis);
                    allEArrayBisector[ne] = plane;
                    //plane.Bake(1);


                    //Create ZigZag
                    Vector3d sideVec = Vector3d.CrossProduct(XAxis, plane.ZAxis);
                    Rectangle3d rect = new Rectangle3d(new Plane(plane.Origin, plane.ZAxis,XAxis ), new Interval(-D * 0.5, D * 0.5), new Interval(-allEArrayLines[ne].Length * 0.5, allEArrayLines[ne].Length * 0.5));
                    Polyline zigzag0 = ZigZagFromRectangle(rect, JointStep, scale);
                    zzAll[edgeFaces[0]].Add(zigzag0);
                    zzPlanesAll[edgeFaces[0]].Add(new Plane(rect.Plane));
                    //zigzag0.Bake();

                    //plane.Rotate(Math.PI,plane.ZAxis);
                     rect = new Rectangle3d(new Plane(plane.Origin, -plane.ZAxis, -XAxis), new Interval(-D * 0.5, D * 0.5), new Interval(-allEArrayLines[ne].Length * 0.5, allEArrayLines[ne].Length * 0.5));
                    //rect.ToPolyline().Bake();
                    Polyline zigzag1= ZigZagFromRectangle(rect, JointStep, scale);
                    zzAll[edgeFaces[1]].Add(zigzag1);
                    zzPlanesAll[edgeFaces[1]].Add(new Plane(rect.Plane));
                    //zzAll.Add();

                    //zigzag1.Bake();
                } else {
                    //int[] fe0 = fe[edgeFaces[0]];
                    //int localID0 = Array.IndexOf(fe0, me);


                    ////Get segments
                    //Curve curve = additionalPlines[0][edgeFaces[0]].Value;
                    //curve.TryGetPolyline(out Polyline pline0);
                    //curve = additionalPlines[1][edgeFaces[0]].Value;
                    //curve.TryGetPolyline(out Polyline pline1);
                    //Line l0 = PolylineUtil.tweenLine(pline0.SegmentAt(localID0), pline1.SegmentAt(localID0));


                    //allEArrayLines[ne] = l0;
                    //Vector3d XAxis = allEArrayLines[ne].Direction;
                    //Vector3d YAxis = (pline0.SegmentAt(localID0).PointAt(0.5) - pline1.SegmentAt(localID0).PointAt(0.5)) ;
                    //Plane plane = new Plane(allEArrayLines[ne].PointAt(0.5), XAxis, YAxis);
                    //allEArrayBisector[ne] = plane;
                    ////plane.Bake(1);

                    //Vector3d sideVec = Vector3d.CrossProduct(XAxis, plane.ZAxis);
                    //Rectangle3d rect = new Rectangle3d(new Plane(plane.Origin, plane.ZAxis, XAxis ), new Interval(-D * 0.5, D * 0.5), new Interval(-allEArrayLines[ne].Length * 0.5, allEArrayLines[ne].Length * 0.5));
                    //Polyline zigzag = ZigZagFromRectangle(rect, JointStep, scale);
                    ////zigzag.Bake();
                }
                
            }






            ////Plane[][] bisePlanes;
            ////Plane[][] edgePlanes;//"ZY"
            ////M.GetEdgeAndBisectorPlanes(out bisePlanes, out edgePlanes);





            //    for (int i = 0; i < plines.Length; i++) {



            //    var zz = new List<Polyline>();
            //    var zzLines = new List<Line>();
            //    var zzPlanes = new List<Plane>();

            //    if (D != 0) {

            //        Point3d center = M._Plane(i).Origin;

            //        for (int j = 0; j < plines[i].Count - 1; j++) {



            //            int e = fe[i][j];

            //            int[] faces = M.TopologyEdges.GetConnectedFaces(e);
            //            int neiFace = faces[0];
            //            Vector3d normalDir = Vector3d.Zero;

            //            if (faces.Length == 2) {

            //                bool flag = false;
            //                foreach (var ff in finf[i]) {
            //                    if (faces[0] == (int)ff) {
            //                        flag = true;
            //                        break;
            //                    }
            //                }
            //                neiFace = flag ? faces[1] : faces[0];

            //                normalDir = (M.FaceNormals[faces[1]] + M.FaceNormals[faces[0]]);
            //                normalDir.Unitize();

            //            } else {
            //                continue;
            //                normalDir = (M.FaceNormals[faces[0]]);
            //                normalDir.Unitize();

            //            }

            //            Line l = plines[i].SegmentAt(j);
            //            Vector3d dir = Vector3d.CrossProduct(M.FaceNormals[neiFace], l.Direction);
            //            dir.Unitize();
            //            Point3d p0 = l.PointAt(0.5);
            //            dir = -normalDir;

            //            Vector3d sideVec = Vector3d.CrossProduct(l.Direction, normalDir);
            //            Rectangle3d rect = new Rectangle3d(new Plane(l.Center(), sideVec, l.Direction), new Interval(-D * 0.5, D * 0.5), new Interval(-l.Length * 0.5, l.Length * 0.5));
            //            Polyline zigzag = ZigZagFromRectangle(rect, JointStep, scale);

            //            //Project zigzag points to plane by direction of plane normal
            //            for (int k = 0; k < zigzag.Count; k++) {
            //                Line zzLine = new Line(zigzag[k], zigzag[k] + rect.Plane.ZAxis);
            //                zigzag[k] = PlaneUtil.LinePlane(zzLine, planes[i]);
            //            }
            //            zzPlanes.Add(rect.Plane);

            //            zz.Add(zigzag);

            //            int sign = faces.Length == 2 ? -1 : -1;
            //            //if (F)
            //            //    sign *= -1;
            //            double distance = D * sign;

            //            Polyline polyline = new Polyline() { l.From, l.To, l.To + dir * distance, l.From + dir * distance, l.From };


            //        }//for j
            //        zzAll.Add(zz);
            //        zzPlanesAll.Add(zzPlanes);
            //    }

            //}

            //Add zigzag to panels
            //for (int i = 0; i < plines.Length; i++) {

            //    plines[i].InsertPolyline(zzAll[i]);
            //}
            List<List<Polyline>> zzAllNew = new List<List<Polyline>>();
            zzAllNew.Add(new List<Polyline>());
            zzAllNew.Add(new List<Polyline>());
            if (additionalPlines != null) {

                for (int i = 0; i < additionalPlines.Branches.Count; i++) {

                    if (additionalPlines[i].Count != plines.Length)
                        continue;

                    for (int j = 0; j < additionalPlines[i].Count; j++) {

                        //Cast to Polyline
                        Curve curve = additionalPlines[i][j].Value;
                        bool good = curve.TryGetPolyline(out Polyline aPline);
                  
                        if (good) {
                            Plane plane = aPline.GetPlane();

                            for (int k = 0; k < zzAll[j].Count; k++) {
                                Polyline zzPolyline = new Polyline();

                                for (int m = 0; m < zzAll[j][k].Count; m++) {
                                    Line zzLine = new Line(zzAll[j][k][m], zzAll[j][k][m] + zzPlanesAll[j][k].ZAxis);
                                    Point3d zzPoint = PlaneUtil.LinePlane(zzLine, plane);
                                    //zzPoint.Bake();
                                    zzPolyline.Add(zzPoint);
                                    //
                                }
                                //zzPolyline.Bake();
                                aPline.InsertPolyline(zzPolyline);
                            }
                            //aPline.Bake();
                            //break;
                            //additionalPlines[i][j] = new GH_Curve(new Polyline(aPline).ToNurbsCurve());
                            zzAllNew[i].Add(aPline);
                            //zzAllNew[1].Add(aPline);
                        }
                        //

                    }

                }//iterate of n-th layer of mesh.

            }//not null

            //zzAll.Clear();



            return zzAllNew;

            //return new List<List<Polyline>>() { additionalPlines[0], };

        }



    }
}
