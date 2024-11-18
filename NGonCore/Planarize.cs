using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Geometry.ConvexHull;
using Rhino.Geometry;

namespace NGonCore {
    public static class Planarize {



        public static Mesh Solve(Mesh m, int iterations, double tolerance, int type = 0) {

            Mesh mesh = m.DuplicateMesh();

            iterations = (iterations < 0) ? 0 : iterations;

            bool[] planar = new bool[mesh.Ngons.Count];
            double deviation = 1.7976931348623157E+308;

            Plane[] NGonPlanes = null;


                NGonPlanes = GetPlanes(mesh, tolerance, ref deviation, ref planar);


            HashSet<int> allNGonV = mesh.GetAllNGonsVertices();
            int[][] faceMap = mesh.GetNGonsConnectedToNGonVertices(allNGonV);
            bool[] nakedStatus = mesh.GetNakedNGonPointStatus(allNGonV);
            Vector3f[] vertexNormals = mesh.GetNgonNormals3f();
            List<Point3d> pts = new List<Point3d>();

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < allNGonV.Count; j++) {

                    if (!(nakedStatus[j] && type == 1)) {

                        int num8 = 0;

                        for (int k = 0; k < faceMap[j].Length; k++)
                            if (planar[faceMap[j][k]])
                                num8++;


                        if (num8 != faceMap[j].Length) {

                            Point3d[] avPoints = new Point3d[faceMap[j].Length];

                            //Loop through all ngon vertices and connected planes
                            for (int l = 0; l < faceMap[j].Length; l++)
                                avPoints[l] = NGonPlanes[faceMap[j][l]].ClosestPoint(mesh.Vertices[allNGonV.ElementAt(j)]);

                            mesh.Vertices[allNGonV.ElementAt(j)] = (Point3f)NGonCore.PointUtil.AveragePoint(avPoints);

                        }//if
                    }
                }


                Plane[] array5 = GetPlanes(mesh, tolerance, ref deviation, ref planar);
                AssignOrigins(NGonPlanes, ref array5);
                NGonPlanes = array5;
                array5 = null;

                if (deviation <= tolerance)
                    break;

            }

            return mesh;
        }

        private static bool AssignOrigins(Plane[] Source, ref Plane[] Target) {
            for (int i = 0; i < Target.Length; i++)
                Target[i].Origin = Source[i].Origin;
            return true;
        }

        private static Plane[] GetPlanes(Mesh m, double tolerance, ref double deviation, ref bool[] planar) {

            deviation = 0.0;

            Plane[] planes = Enumerable.Repeat(Plane.Unset, m.Ngons.Count).ToArray();
            Point3d[] centers = m.GetNGonCenters();
            bool[] byrefplan = new bool[planar.Length];
            double[] byrefdev = new double[planar.Length];



            Parallel.For(0, m.Ngons.Count, delegate (int i) {

                Point3d[] array = m.Ngons.NgonBoundaryVertexList(m.Ngons[i], false);

                double dev = 0.0;
                Plane.FitPlaneToPoints(array, out planes[i], out dev);
                byrefplan[i] = (dev <= tolerance);
                byrefdev[i] = dev;
                planes[i].Origin = centers[i];
            });

            planar = byrefplan;

            for (int j = 0; j < byrefdev.Length; j++)
                deviation = Math.Max(deviation, byrefdev[j]);

            return planes;

        }

    }
}
