using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;


namespace NGonCore
{
    public static class CustomMeshUtil
    {

        public static Polyline[][] ProjectPairsPolylines(this Mesh mesh, double value)
        {
            //Polyline[] p = new Polyline[mesh.Ngons[0].BoundaryVertexCount + 2];
            int n = (int)(mesh.Ngons.Count * 0.5);
            Polyline[][] p = new Polyline[n][];
            Plane[] pl = mesh.GetNgonPlanes();


            uint[][] id = mesh.GetNGonsBoundaries();

            for (int i = 0; i < n; i++)
            {
                p[i] = new Polyline[] { new Polyline(), new Polyline() };
                for (int j = 0; j < id[i].Length; j++)
                {
                    Line temp = new Line(mesh.Vertices[(int)id[i][j]], mesh.Vertices[(int)id[i + n][j]]);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i], out double t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i + n], out double t2);
                    p[i][0].Add(temp.PointAt(t1));
                    p[i][1].Add(temp.PointAt(t2));
                }
                p[i][0].Close();
                p[i][1].Close();
            }

            return p;
        }

        public static Mesh[] ProjectPairsMesh(this Mesh mesh, double value, ref Polyline[][]  plinesProjected) {

            int n = (int)(mesh.Ngons.Count * 0.5);
            Polyline[][] p = new Polyline[n][];
            Plane[] pl = mesh.GetNgonPlanes();
            Polyline[] polys = mesh.GetPolylines();
            Mesh[] m = new Mesh[n];
            uint[][] id = mesh.GetNGonsBoundaries();

            for (int i = 0; i < n; i++) {
                p[i] = new[] { new Polyline(), new Polyline() };
                for (int j = 0; j < id[i].Length; j++) {
                    Line temp = new Line(mesh.Vertices[(int)id[i][j]], mesh.Vertices[(int)id[i + n][j]]);
                    double t1, t2;

                    // polys[i].RemoveAt(0);
                    // polys[i + n].RemoveAt(0);
                    Plane plane0 = Plane.Unset;
                    Plane plane1 = Plane.Unset;
                    Plane.FitPlaneToPoints(polys[i], out plane0);
                    Plane.FitPlaneToPoints(polys[i + n], out plane1);




                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, plane0, out t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, plane1, out t2);


                    if (value < 1 && value >= 0) {
                        p[i][0].Add(temp.PointAt(MathUtil.Lerp(0, t1, value)));
                        p[i][1].Add(temp.PointAt(MathUtil.Lerp(1, t2, value)));

                    } else {
                        p[i][0].Add(temp.PointAt(t1));
                        p[i][1].Add(temp.PointAt(t2));
                    }
                }
                p[i][0].Close();
                p[i][1].Close();
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(p[i][0]);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(p[i][1]);
                m[i] = PolylineUtil.Loft(p[i], 0);
            }
            plinesProjected = p;
            return m;
        }


        //public static Mesh[] ProjectPairsMesh(this Mesh mesh, double value) {

        //    int n = (int)(mesh.Ngons.Count * 0.5);
        //    Polyline[][] p = new Polyline[n][];
        //    Plane[] pl = mesh.GetNgonPlanes();
        //    Mesh[] m = new Mesh[n];
        //    uint[][] id = mesh.GetNGonsBoundaries();

        //    for (int i = 0; i < n; i++) {
        //        p[i] = new[] { new Polyline(), new Polyline() };
        //        for (int j = 0; j < id[i].Length; j++) {
        //            Line temp = new Line(mesh.Vertices[(int)id[i][j]], mesh.Vertices[(int)id[i + n][j]]);
        //            Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i], out double t1);
        //            Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i + n], out double t2);

        //            if (value < 1 && value >= 0) {
        //                p[i][0].Add(temp.PointAt(MathUtil.Lerp(0, t1, value)));
        //                p[i][1].Add(temp.PointAt(MathUtil.Lerp(1, t2, value)));

        //            } else {
        //                p[i][0].Add(temp.PointAt(t1));
        //                p[i][1].Add(temp.PointAt(t2));
        //            }
        //        }
        //        p[i][0].Close();
        //        p[i][1].Close();

        //        m[i] = PolylineUtil.Loft(p[i], 0);
        //    }

        //    return m;
        //}

        public static Mesh[] ProjectPairsMesh(this Mesh mesh0, Mesh mesh1, double value) {

            int n = (int)(mesh0.Ngons.Count);
            Polyline[][] p = new Polyline[n][];
            Plane[] pl0 = mesh0.GetNgonPlanes();
            Plane[] pl1 = mesh1.GetNgonPlanes();
            Mesh[] m = new Mesh[n];
            uint[][] id = mesh0.GetNGonsBoundaries();

            for (int i = 0; i < n; i++) {
                p[i] = new[] { new Polyline(), new Polyline() };
                for (int j = 0; j < id[i].Length; j++) {
                    Line temp = new Line(mesh0.Vertices[(int)id[i][j]], mesh1.Vertices[(int)id[i ][j]]);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl0[i], out double t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl1[i], out double t2);

                    if (value < 1 && value >= 0) {
                        p[i][0].Add(temp.PointAt(MathUtil.Lerp(0, t1, value)));
                        p[i][1].Add(temp.PointAt(MathUtil.Lerp(1, t2, value)));

                    } else {
                        p[i][0].Add(temp.PointAt(t1));
                        p[i][1].Add(temp.PointAt(t2));
                    }
                }
                p[i][0].Close();
                p[i][1].Close();

                m[i] = PolylineUtil.Loft(p[i], 0);
            }

            return m;
        }


        public static Mesh[] ProjectPairsMeshToBrep(this Mesh mesh, double value, Mesh target) {
            //Polyline[] p = new Polyline[mesh.Ngons[0].BoundaryVertexCount + 2];
            int n = (int)(mesh.Ngons.Count * 0.5);
            Polyline[][] p = new Polyline[n][];
            Plane[] pl = mesh.GetNgonPlanes();
            Mesh[] m = new Mesh[n];


            uint[][] id = mesh.GetNGonsBoundaries();

            for (int i = 0; i < n; i++) {
                p[i] = new Polyline[] { new Polyline(), new Polyline() };

                for (int j = 0; j < id[i].Length; j++) {
                 
                    Line temp = new Line(mesh.Vertices[(int)id[i][j]], mesh.Vertices[(int)id[i + n][j]]);
                    temp.Transform(Transform.Scale(temp.PointAt(0.5), 1000));
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i], out double t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i + n], out double t2);


                    // Line tempLine =temp;
               
                   Point3d ptA = Rhino.Geometry.Intersect.Intersection.MeshLine(target, temp, out int[] faceIds)[0];
                   t2 = temp.ClosestParameter(ptA);


                    if (value < 1 && value >= 0) {


                        p[i][1].Add(temp.PointAt(MathUtil.Lerp(0, t1, value)));
                        p[i][0].Add(temp.PointAt(MathUtil.Lerp(1, t2, value)));

                    } else {
                        p[i][1].Add(temp.PointAt(t1));
                        //p[i][1].Add(temp.PointAt(t2));
                        p[i][0].Add(ptA);
                    }
                }
                p[i][0].Close();
                p[i][1].Close();

                m[i] = PolylineUtil.Loft(p[i], 0.001);
            }

            return m;
        }





    }
}
