using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.Tesselation {
    public static class TriangleSubdivision {


        public static Mesh SubdivideTriangleMesh(Mesh Mesh, int Iterations, double threshold=0) {

            Mesh mesh = Triangulate(Mesh);


            for (int i = 0; i < Iterations; i++) 
                Subdivide(mesh, threshold);

            return mesh;



        }




        public static Mesh Subdivide(Mesh m, double threshold) {
            List<MeshFace> newFaces = new List<MeshFace>();

            foreach (MeshFace mf in m.Faces) {
                if (threshold == 0) {
                    double mfarea = AreaOfTriangle(m, mf);
                    if (mfarea > threshold) {
                        m.Vertices.AddVertices(FaceMidPoints(m, mf));
                        newFaces.Add(new MeshFace(mf.A, m.Vertices.Count - 3, m.Vertices.Count - 1));
                        newFaces.Add(new MeshFace(m.Vertices.Count - 3, mf.B, m.Vertices.Count - 2));
                        newFaces.Add(new MeshFace(m.Vertices.Count - 1, m.Vertices.Count - 2, mf.C));
                        newFaces.Add(new MeshFace(m.Vertices.Count - 3, m.Vertices.Count - 2, m.Vertices.Count - 1));
                    } else newFaces.Add(mf);
                } else {
                    m.Vertices.AddVertices(FaceMidPoints(m, mf));
                    newFaces.Add(new MeshFace(mf.A, m.Vertices.Count - 3, m.Vertices.Count - 1));
                    newFaces.Add(new MeshFace(m.Vertices.Count - 3, mf.B, m.Vertices.Count - 2));
                    newFaces.Add(new MeshFace(m.Vertices.Count - 1, m.Vertices.Count - 2, mf.C));
                    newFaces.Add(new MeshFace(m.Vertices.Count - 3, m.Vertices.Count - 2, m.Vertices.Count - 1));
                }
            }

            m.Faces.Clear();
            m.Faces.AddFaces(newFaces);
            newFaces.Clear();
            return m;

        }

        public static List<Point3d> FaceMidPoints(Mesh m, MeshFace mf) {
            var rtnlist = new List<Point3d>();
            rtnlist.Add(MidPoint(m.Vertices[mf.A], m.Vertices[mf.B]));
            rtnlist.Add(MidPoint(m.Vertices[mf.B], m.Vertices[mf.C]));
            rtnlist.Add(MidPoint(m.Vertices[mf.C], m.Vertices[mf.A]));
            return rtnlist;
        }

        public static Point3d MidPoint(Point3d pt1, Point3d pt2) {
            return new Point3d(0.5 * (pt1.X + pt2.X), 0.5 * (pt1.Y + pt2.Y), 0.5 * (pt1.Z + pt2.Z));
        }

        public  static Mesh Triangulate(Mesh x) {
            int facecount = x.Faces.Count;
            for (int i = 0; i < facecount; i++) {
                var mf = x.Faces[i];
                if (mf.IsQuad) {
                    double dist1 = x.Vertices[mf.A].DistanceTo(x.Vertices[mf.C]);
                    double dist2 = x.Vertices[mf.B].DistanceTo(x.Vertices[mf.D]);
                    if (dist1 > dist2) {
                        x.Faces.AddFace(mf.A, mf.B, mf.D);
                        x.Faces.AddFace(mf.B, mf.C, mf.D);
                    } else {
                        x.Faces.AddFace(mf.A, mf.B, mf.C);
                        x.Faces.AddFace(mf.A, mf.C, mf.D);
                    }
                }
            }

            var newfaces = new List<MeshFace>();
            foreach (var mf in x.Faces) {
                if (mf.IsTriangle) newfaces.Add(mf);
            }

            x.Faces.Clear();
            x.Faces.AddFaces(newfaces);
            return x;
        }

        public static double AreaOfTriangle(Point3d pt1, Point3d pt2, Point3d pt3) {
            double a = pt1.DistanceTo(pt2);
            double b = pt2.DistanceTo(pt3);
            double c = pt3.DistanceTo(pt1);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public static double AreaOfTriangle(Mesh m, MeshFace mf) {
            return AreaOfTriangle(m.Vertices[mf.A], m.Vertices[mf.B], m.Vertices[mf.C]);
        }

    }
}
