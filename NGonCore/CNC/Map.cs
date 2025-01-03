﻿using NGonCore;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoGeometry {
    public static class Map {


        public static List<Polyline> MappedFromSurfaceToSurface(this List<Polyline> polylines, Surface s, Surface t) {

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


        public static void Clean(this Mesh mesh) {

            mesh.Compact();
            mesh.Vertices.CombineIdentical(true, true);
            mesh.Vertices.CullUnused();

            

            mesh.Weld(3.14159265358979);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            //mesh.UnifyNormals();


            if (mesh.SolidOrientation() == -1)
                mesh.Flip(true, true, true);





        }

        public static List<Polyline> MappedFromMeshToMesh(this List<Polyline> polylines, Mesh s_, Mesh t_) {


            Mesh s = s_.DuplicateMesh();
            Mesh t = t_.DuplicateMesh();



            List<Polyline> mapped = new List<Polyline>();

            try {

                for (int i = 0; i < polylines.Count; i++) {
                    

                    Polyline pol = new Polyline(polylines[i]);
                    

                    for (int j = 0; j < pol.Count; j++) {

                        Point3d pTemp = new Point3d(pol[j]);
                        MeshPoint mp = s.ClosestMeshPoint(pol[j], 0.01);
                        if (mp != null) {
                            pol[j] = t.PointAt(mp);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(t.PointAt(mp));
                        }

                    }//for j


                    mapped.Add(pol);
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(pol);

                }//for i

            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }


            return mapped;

        }


        public static List<Curve> MapCurve(List<Curve> U, Curve C0, Curve B0, Curve C1 = null, Curve B1 = null) {

            if (C1 == null || B1 == null) {

                return MapCurve(U, PolylineUtil.ToPolylineFromCP(C0), PolylineUtil.ToPolylineFromCP(B0));
            }

            return MapCurve(U, PolylineUtil.ToPolylineFromCP(C0), PolylineUtil.ToPolylineFromCP(B0), PolylineUtil.ToPolylineFromCP(C1), PolylineUtil.ToPolylineFromCP(B1));
        }


        /// <summary>
        /// Map one curve to another
        /// </summary>
        /// <param name="U - A list input of curves to be mapped"></param>
        /// <param name="C0 - Target Polyline"></param>
        /// <param name="B0 - Reference Polyline"></param>
        /// <param name="C1 - Target Volume Polyline"></param>
        /// <param name="B1 - Reference Volume Polyline"></param>
        /// <returns></returns>
        public static List<Curve> MapCurve(List<Curve> U, Polyline C0, Polyline B0, Polyline C1 = null, Polyline B1 = null) {




            try {
                if (C0 != null && B0 != null) {
                    int i = 0; int j = 0; int[] x;
                    Mesh mb0 = new Mesh(); Mesh mb1 = new Mesh(); Mesh mt0 = new Mesh(); Mesh mt1 = new Mesh();
                    List<MeshFace> mf = new List<MeshFace>();
                    Rhino.Geometry.Collections.MeshVertexNormalList mn;
                    bool blnI = false; bool BB = false; bool CB = false;
                    List<Curve> arrC = new List<Curve>();

                    mb0.Vertices.Add(B0.CenterPoint());
                    B0.RemoveAt(0);
                    mb0.Vertices.AddVertices(B0);

                    if (B1 != null) {
                        //Print("B1");
                        BB = true;
                        mb1.Vertices.Add(B1.CenterPoint());
                        B1.RemoveAt(0);
                        mb1.Vertices.AddVertices(B1);
                        if (B1.Count != B0.Count) blnI = true;
                    }

                    mt0.Vertices.Add(C0.CenterPoint());
                    C0.RemoveAt(0);
                    mt0.Vertices.AddVertices(C0);
                    if (C0.Count != B0.Count) blnI = true;

                    if (C1 != null) {
                        //Print("C1");
                        CB = true;
                        mt1.Vertices.Add(C1.CenterPoint());
                        C1.RemoveAt(0);
                        mt1.Vertices.AddVertices(C1);
                        if (C1.Count != B0.Count) blnI = true;
                    }

                    if (!blnI) {
                        //Print("!blnI");
                        x = append(B0.Count - 1);//int array

                        for (i = 0; i < B0.Count; i++) {
                            mf.Add(new MeshFace(x[i] + 1, x[i + 1] + 1, 0));
                        }

                        mb0.Faces.AddFaces(mf);
                        mb0.FaceNormals.ComputeFaceNormals();
                        if (BB) {
                            //Print("BB");
                            mb1.Faces.AddFaces(mf);
                            mb1.FaceNormals.ComputeFaceNormals();
                        }

                        mt0.Faces.AddFaces(mf);
                        mt0.FaceNormals.ComputeFaceNormals();
                        mt0.Normals.ComputeNormals();
                        mn = mt0.Normals;

                        if (CB) {
                            //Print("CB");
                            mt1.Faces.AddFaces(mf);
                        }

                        double md = 0; double fx = 0;

                        for (j = 0; j < U.Count; j++) {
                            //Print("BB");
                            var ncrv = U[j].ToNurbsCurve();
                            var plin = ncrv.Points.ControlPolygon();
                            var bbox = new BoundingBox();
                            bbox = plin.BoundingBox;
                            var crn = new List<Point3d>();
                            Point3d tptA = new Point3d(); Point3d tptB = new Point3d();
                            double td0 = 0; double td1 = 0;
                            crn = bbox.GetCorners().ToList();
                            tptA = (crn[0] + crn[1] + crn[2] + crn[3]) / 4;
                            tptB = (crn[4] + crn[5] + crn[6] + crn[7]) / 4;
                            td0 = mb0.ClosestMeshPoint(tptA, 0.0).Point.DistanceTo(tptA);
                            td1 = mb0.ClosestMeshPoint(tptB, 0.0).Point.DistanceTo(tptB);
                            if (td0 > md) md = td0;
                            if (td1 > md) md = td1;
                        }
                        if ((!BB) && (md <= 0.0000000000001)) fx = 0; else fx = 1;

                        for (j = 0; j < U.Count; j++) {
                            NurbsCurve uc;
                            Rhino.Geometry.Collections.NurbsCurvePointList pts;
                            var mp0 = new List<MeshPoint>(); var mp1 = new List<MeshPoint>();
                            var d0 = new List<double>(); var t0 = new List<double>();
                            MeshPoint mp;
                            double d;

                            uc = U[j].ToNurbsCurve();
                            pts = uc.Points;

                            for (i = 0; i < pts.Count; i++) {
                                mp = mb0.ClosestMeshPoint(pts[i].Location, 0.0);

                                d = mp.Point.DistanceTo(pts[i].Location);
                                if (Vector3d.VectorAngle(mb0.FaceNormals[mp.FaceIndex], pts[i].Location - mp.Point) > (Math.PI * 0.5)) d *= -1;

                                mp0.Add(mp);
                                mp1.Add(mp);
                                d0.Add(d);
                                t0.Add(d);
                            }

                            if (BB) {
                                //Print("-BB");
                                for (i = 0; i < pts.Count; i++) {
                                    mp = mb1.ClosestMeshPoint(pts[i].Location, 0.0);
                                    d = mp.Point.DistanceTo(pts[i].Location);
                                    if (Vector3d.VectorAngle(mb1.FaceNormals[mp.FaceIndex], pts[i].Location - mp.Point) < (Math.PI * 0.5)) d *= -1;

                                    mp1[i] = mp;
                                    t0[i] = d0[i] / (d + d0[i]);
                                }
                                md = 1;
                            }

                            if (CB) {
                                //Print("-CB");
                                for (i = 0; i < pts.Count; i++) {
                                    Point3d pt;
                                    Vector3d v;
                                    mp = mp0[i];

                                    v = (mt1.PointAt(mp1[i]) - mt0.PointAt(mp)) * (t0[i] / md);//bug

                                    //Print((v).ToString());

                                    v *= fx;




                                    pt = mt0.PointAt(mp) + v;
                                    uc.Points.SetPoint(i, pt.X, pt.Y, pt.Z, uc.Points[i].Weight);
                                }

                            } else {
                                // Print("--");
                                for (i = 0; i < pts.Count; i++) {
                                    Point3d pt;
                                    Vector3d v;
                                    mp = mp0[i];

                                    v = (Vector3d)mn[mt0.Faces[mp.FaceIndex].A] * mp.T[0];
                                    v += (Vector3d)mn[mt0.Faces[mp.FaceIndex].C] * mp.T[1];
                                    v += (Vector3d)mn[mt0.Faces[mp.FaceIndex].B] * mp.T[2];
                                    v.Unitize();

                                    pt = mt0.PointAt(mp) + v * d0[i];
                                    uc.Points.SetPoint(i, pt.X, pt.Y, pt.Z, uc.Points[i].Weight);
                                }
                            }
                            arrC.Add(uc);


                        }

                        return arrC;



                    }
                }

            } catch (Exception e) {

                Rhino.RhinoApp.WriteLine(e.ToString());
            }

            return new List<Curve>();

        }

        private static int[] append(int j) {
            var srsEnum = Enumerable.Range(0, j + 1);
            var arrVal = new List<int>();
            arrVal = srsEnum.ToList();
            arrVal.Add(0);
            return arrVal.ToArray();
        }

    }
}
