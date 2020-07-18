using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Meta.Numerics.Matrices;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace NGonsCore {
    public static class MeshUtilSimple {


        // List<int>[] mc = MeshCollisions(mesh);
        public static List<int>[] MeshCollisions(List<Mesh> mesh) {

            List<BoundingBox> box = new List<BoundingBox>();
            for (int i = 0; i < mesh.Count; i++) {
                box.Add(mesh[i].GetBoundingBox(false));
            }


            List<int>[] collisions = new List<int>[mesh.Count];
            for (int i = 0; i < mesh.Count; i++) {

                collisions[i] = new List<int>();

                for (int j = 0; j < mesh.Count; j++) {

                    if (i == j)
                        continue;

                    if (BoundingBoxUtil.BoxBox(box[i], box[j]))
                        if (Rhino.Geometry.Intersect.Intersection.MeshMeshFast(mesh[i], mesh[j]) != null)
                            collisions[i].Add(j);

                }//for j
            }//for i

            return collisions;
        }


        /// <summary>
        /// Voir https://en.wikipedia.org/wiki/Kernel_(statistics)
        /// </summary>
        /// <param name="arg_meshECEF"></param>
        /// <param name="arg_tab_values">Values to be smoothed at mesh vertex</param>
        /// <param name="arg_KernelSize">Size of kernel, same unit as mesh</param>
        /// <param name="arg_logInterpolation">if true, arg_tab_values will be smoothed using thes log values</param>
        /// <returns>array of smoothed datz</returns>
        public static double[] FilterGaussianKDE(Mesh arg_meshECEF, double[] arg_tab_values, double arg_KernelSize, bool arg_logInterpolation) {
            if (arg_meshECEF.Vertices.Count != arg_tab_values.Length) {
                throw new Exception("Filtre KDE : Il n'y a pas le même nombre de points entre le maillage et les données sous forme tabulaire ");
            }
            double[] filteredData = new double[arg_meshECEF.Vertices.Count];
            double n = (double)arg_meshECEF.Vertices.Count;
            int i = 0;
            foreach (Point3d ptECEF in arg_meshECEF.Vertices) {
                int j = 0;
                double sumOfKernel = 0.0;
                foreach (Point3d pt2ECEF in arg_meshECEF.Vertices) {
                    double distSquared = pt2ECEF.DistanceToSquared(ptECEF);
                    double kernel = 0;

                    if (arg_KernelSize > 1e-6) {
                        kernel = Math.Exp(-distSquared / (2 * arg_KernelSize * arg_KernelSize));//Kernel de type Gaussien
                    } else {
                        if (distSquared < 1e-6) {
                            kernel = 1;
                        } else {
                            kernel = 0.0;
                        }
                    }
                    if (arg_logInterpolation) {
                        filteredData[i] = filteredData[i] + Math.Log10(arg_tab_values[j]) * kernel;
                    } else {
                        filteredData[i] = filteredData[i] + arg_tab_values[j] * kernel;
                    }
                    sumOfKernel = sumOfKernel + kernel;
                    j++;
                }
                if (arg_logInterpolation) {
                    filteredData[i] = Math.Pow(10, filteredData[i] / sumOfKernel);
                } else {
                    filteredData[i] = filteredData[i] / sumOfKernel;
                }
                i++;

            
            }
            return filteredData;
        }

        public static Mesh ProjectNakedEdgeToCurves(this Mesh M, List<Curve> C, double T, int I) {

            double distance = T * T;
            Mesh mesh = M.DuplicateMesh();
            bool[] naked = mesh.GetNakedEdgePointStatus();

            foreach (Curve c in C) {
                for (int i = 0; i < naked.Length; i++) {
                    if (naked[i]) {
                        double t;
                        c.ClosestPoint(mesh.Vertices[i], out t);
                        Point3d cp = c.PointAt(t);
                        if (cp.DistanceToSquared(M.Vertices[i]) < distance) {
                            mesh.Vertices[i] = (Point3f)cp;

                        }
                    }
                }
            }

            for (int i = 0; i < I; i++) {
                mesh.Smooth(0.5, true, true, true, true, SmoothingCoordinateSystem.Object);
            }

          return mesh;
        }

        public static Mesh RemoveByCurves(this Mesh M, List<Curve> C) {

                Mesh M2 = M.DuplicateMesh();
            double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                List<int> toDelete = new List<int>();

                for (int i = 0; i < M.Faces.Count; i++) {
                    bool keep = false;
                    for (int j = 0; j < C.Count; j++) {
                        if (C[j].Contains(M.Faces.GetFaceCenter(i)) != PointContainment.Outside) {
                            keep = !keep;
                        }
                    }

                    if (!keep) {
                        toDelete.Add(i);
                    }
                }

                M2.Faces.DeleteFaces(toDelete);

                toDelete.Clear();

                for (int i = 0; i < M2.Faces.Count; i++) {
                    if (M2.Faces.AdjacentFaces(i).Length < 2) {
                        toDelete.Add(i);
                    }
                }



                return M2;
            }

        public static Mesh TriangulateMeshByCurves(this Mesh M, List<Curve> C) {
            Mesh M2 = new Mesh();
            M2.Vertices.AddVertices(M.Vertices);

            for (int i = 0; i < M.Faces.Count; i++) {
                MeshFace f = M.Faces[i];
                if (f.IsQuad) {
                    Vector3d tangent = tangentOfClosestCurve(C, M.Faces.GetFaceCenter(i));
                    Point3f fa, fb, fc, fd;
                    M.Faces.GetFaceVertices(i, out fa, out fb, out fc, out fd);

                    double angle0 = Math.Min(Vector3d.VectorAngle(tangent, fc - fa), Vector3d.VectorAngle(tangent, fa - fc));
                    double angle1 = Math.Min(Vector3d.VectorAngle(tangent, fb - fd), Vector3d.VectorAngle(tangent, fd - fb));

                    if (angle1 > angle0) {
                        M2.Faces.AddFace(M.Faces[i].A, M.Faces[i].B, M.Faces[i].C);
                        M2.Faces.AddFace(M.Faces[i].C, M.Faces[i].D, M.Faces[i].A);
                    } else {
                        M2.Faces.AddFace(M.Faces[i].B, M.Faces[i].C, M.Faces[i].D);
                        M2.Faces.AddFace(M.Faces[i].D, M.Faces[i].A, M.Faces[i].B);
                    }

                } else {
                    M2.Faces.AddFace(f);
                }
            }
            return M2;
        }

        public static Vector3d tangentOfClosestCurve(List<Curve> Crvs, Point3d P) {
            double shortestDist = double.MaxValue;
            Vector3d V = new Vector3d();
            foreach (Curve C in Crvs) {
                double t;
                C.ClosestPoint(P, out t);
                double dist = C.PointAt(t).DistanceTo(P);
                if (dist < shortestDist) {
                    shortestDist = dist;
                    V = C.TangentAt(t);
                }
            }
            return V;
        }

        public static  Mesh StarMesh(this Polyline p) {

            Mesh mesh = new Mesh();

            Point3d center = p.CenterPoint();


            p.RemoveAt(p.Count - 1);

            mesh.Vertices.AddVertices(p);
            mesh.Vertices.Add(center);

            int v = 0;
            for (int i = 0; i < p.Count; i++) {
                mesh.Faces.AddFace(v + i, v + ((i + 1) % p.Count), v + p.Count);
            }

            return mesh;
        }

        public static int[] CommonMeshEdgeOfMeshFace(this Mesh M, int F0, int F1) {

            int[] e0 = M.TopologyEdges.GetEdgesForFace(F0);
            int[] e1 = M.TopologyEdges.GetEdgesForFace(F1);

            //    int[] e0 = M._fe(F0);
            //    int[] e1 = M._fe(F1);


            var common = e0.Intersect(e1);

            if (common.Count() > 0) {

                int commonEdge = common.First();

                int a = 0;
                int b = 0;

                for (int i = 0; i < e0.Length; i++) {
                    if (commonEdge == e0[i]) {
                        a = i;
                        break;
                    }
                }

                for (int i = 0; i < e1.Length; i++) {
                    if (commonEdge == e1[i]) {
                        b = i;
                        break;
                    }
                }

                return new int[] { a, b };


            } else
                return new int[] { -1, -1 };
        }

        public static Point3f[] EdgeMidPointsPoint3f(this Mesh mesh) {
            Point3f[] pts = new Point3f[mesh.TopologyEdges.Count];

            for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                pts[i] = (Point3f)mesh.TopologyEdges.EdgeLine(i).PointAt(0.5);

            return pts;
        }
        public static Point3d[] EdgeMidPoints(this Mesh mesh) {
            Point3d[] pts = new Point3d[mesh.TopologyEdges.Count];

            for (int i = 0; i < mesh.TopologyEdges.Count; i++)
                pts[i] = mesh.TopologyEdges.EdgeLine(i).PointAt(0.5);

            return pts;
        }

        public static Mesh Stellate(this Mesh QuadMesh, out int[] BaseFaceArray) {

            Mesh m = QuadMesh.DuplicateMesh();

            BaseFaceArray = QuadMesh.Faces.ToIntArray(false);

            m.Faces.Clear();

            int vertexCount = 0 + m.Vertices.Count;
            for (int i = 0; i < BaseFaceArray.Length; i += 4) {

                Point3d c = QuadMesh.Faces.GetFaceCenter((int)(i * 0.25));
                m.Vertices.Add(c);
                m.Faces.AddFace(BaseFaceArray[i + 0], BaseFaceArray[i + 1], vertexCount);
                m.Faces.AddFace(BaseFaceArray[i + 1], BaseFaceArray[i + 2], vertexCount);
                m.Faces.AddFace(BaseFaceArray[i + 2], BaseFaceArray[i + 3], vertexCount);
                m.Faces.AddFace(BaseFaceArray[i + 3], BaseFaceArray[i + 0], vertexCount);
                vertexCount++;
            }

            m.RebuildNormals();
            return m;

        }

        public static Mesh SortDirections(this Mesh QuadMesh) {


            Mesh mesh = QuadMesh.DuplicateMesh();
            mesh.Vertices.CombineIdentical(true, true);
            mesh.Weld(3.14159265358979);

            Mesh mesh1 = new Mesh();
            mesh1.Vertices.AddVertices(mesh.Vertices);
            bool[] flagArray = new bool[mesh.Faces.Count];
            bool[] flagArray1 = new bool[mesh.Faces.Count];
            mesh1.Faces.AddFaces(mesh.Faces);

            flagArray[0] = true;
            int num = 1;

            for (int i = 0; num < mesh.Faces.Count && i < mesh.Faces.Count; i++) {
                int num1 = 0;
                int num2 = 0;

                while (num2 < mesh.Faces.Count) {

                    if (!flagArray[num2] || flagArray1[num2]) {
                        num2++;
                    } else {
                        num1 = num2;
                        break;
                    }

                }


                for (int j = 0; j < (int)mesh.Faces.AdjacentFaces(num1).Length; j++) {

                    int num3 = mesh.Faces.AdjacentFaces(num1)[j];

                    if (!flagArray[num3]) {

                        int num4 = 0;

                        if (mesh1.Faces[num3].A == mesh1.Faces[num1].A)
                            num4++;

                        if (mesh1.Faces[num3].B == mesh1.Faces[num1].B)
                            num4++;

                        if (mesh1.Faces[num3].C == mesh1.Faces[num1].C)
                            num4++;

                        if (mesh1.Faces[num3].D == mesh1.Faces[num1].D)
                            num4++;


                        if (num4 == 1) {
                            Rhino.Geometry.Collections.MeshFaceList faces = mesh1.Faces;
                            MeshFace item = mesh.Faces[num3];

                            int b = item.B;
                            item = mesh.Faces[num3];
                            int c = item.C;
                            item = mesh.Faces[num3];
                            int d = item.D;
                            item = mesh.Faces[num3];

                            faces.SetFace(num3, b, c, d, item.A);//
                        }

                        flagArray[num3] = true;
                        num++;

                    }
                }
                flagArray1[num1] = true;
            }
            return mesh1;





        }

        public static Mesh Diagonalize(this Mesh QuadMesh) {

            bool[] flagArray = null;
            Line line;
            MeshFace face;
            Mesh mesh = QuadMesh;

            mesh.Vertices.CombineIdentical(true, true);
            mesh.Weld(3.14159265358979);

            if (mesh != null) {

                Point3d[] point3dArray = mesh.Vertices.ToPoint3dArray();
                Mesh mesh1 = new Mesh();

                for (int i = 0; i < mesh.TopologyEdges.Count; i++) {
                    int[] connectedFaces = mesh.TopologyEdges.GetConnectedFaces(i, out flagArray);
                    if (!flagArray[0]) {
                        MeshVertexList vertices = mesh1.Vertices;
                        line = mesh.TopologyEdges.EdgeLine(i);
                        vertices.Add(line.To);
                        MeshVertexList meshVertexList = mesh1.Vertices;
                        line = mesh.TopologyEdges.EdgeLine(i);
                        meshVertexList.Add(line.From);
                    } else {
                        MeshVertexList vertices1 = mesh1.Vertices;
                        line = mesh.TopologyEdges.EdgeLine(i);
                        vertices1.Add(line.From);
                        MeshVertexList meshVertexList1 = mesh1.Vertices;
                        line = mesh.TopologyEdges.EdgeLine(i);
                        meshVertexList1.Add(line.To);
                    }
                    Point3d item = new Point3d();
                    if (!mesh.Faces.GetFace(connectedFaces[0]).IsQuad) {
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        Point3d point3d = point3dArray[face.A];
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        Point3d point3d1 = point3d + point3dArray[face.B];
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        item = 0.333333333333333 * (point3d1 + point3dArray[face.C]);
                    } else {
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        Point3d point3d2 = point3dArray[face.A];
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        Point3d point3d3 = point3d2 + point3dArray[face.B];
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        Point3d point3d4 = point3d3 + point3dArray[face.C];
                        face = mesh.Faces.GetFace(connectedFaces[0]);
                        item = 0.25 * (point3d4 + point3dArray[face.D]);
                    }

                    mesh1.Vertices.Add(item);
                    int count = mesh1.Vertices.Count;
                    if ((int)connectedFaces.Length != 1) {
                        if (!mesh.Faces.GetFace(connectedFaces[1]).IsQuad) {
                            MeshVertexList vertices2 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            Point3d item1 = vertices2[face.A];
                            MeshVertexList meshVertexList2 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            Point3d item2 = item1 + meshVertexList2[face.B];
                            MeshVertexList vertices3 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            item = 0.333333333333333 * (item2 + vertices3[face.C]);
                        } else {
                            MeshVertexList meshVertexList3 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            Point3d item3 = meshVertexList3[face.A];
                            MeshVertexList vertices4 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            Point3d item4 = item3 + vertices4[face.B];
                            MeshVertexList meshVertexList4 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            Point3d item5 = item4 + meshVertexList4[face.C];
                            MeshVertexList vertices5 = mesh.Vertices;
                            face = mesh.Faces.GetFace(connectedFaces[1]);
                            item = 0.25 * (item5 + vertices5[face.D]);
                        }

                        mesh1.Vertices.Add(item);
                        count = mesh1.Vertices.Count;
                        mesh1.Faces.AddFace(count - 4, count - 1, count - 3, count - 2);
                    } else {
                        mesh1.Faces.AddFace(count - 2, count - 1, count - 3);
                    }
                }

                mesh1.Vertices.CombineIdentical(true, true);


                return mesh1;
            }
            return mesh;
        }

        public static Mesh FaceFaceOffset(this Mesh mesh, double dist)
        {


            mesh.FaceNormals.ComputeFaceNormals();
            Mesh mesh1 = mesh.DuplicateMesh();
            mesh.GetNakedEdgePointStatus();
            Vector3d[] item = new Vector3d[mesh1.Vertices.Count];

            try
            {


                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    int[] vertexFaces = mesh.Vertices.GetVertexFaces(i);
                    Vector3d center = new Vector3f();
                    if ((int)vertexFaces.Length == 1)
                    {
                        center = mesh.FaceNormals[vertexFaces[0]];
                    }
                    else if ((int)vertexFaces.Length != 2)
                    {
                        Point3d point3d = new Point3d(0, 0, 0);
                        Point3d[] point3dArray = new Point3d[3];
                        for (int j = 0; j < 3; j++)
                        {
                            point3dArray[j] = 100 * (point3d + mesh.FaceNormals[vertexFaces[j]]);
                        }
                        Circle circle = new Circle(point3dArray[0], point3dArray[1], point3dArray[2]);
                        center = (Vector3f)(circle.Center - point3d);
                    }
                    else
                    {
                        Vector3d vector3d = mesh.FaceNormals[vertexFaces[0]];
                        Vector3d item1 = mesh.FaceNormals[vertexFaces[1]];
                        vector3d.Unitize();
                        item1.Unitize();
                        Vector3d vector3d1 = 0.5 * (vector3d + item1);
                        Vector3d vector3d2 = Vector3d.CrossProduct(mesh.FaceNormals[vertexFaces[0]], mesh.FaceNormals[vertexFaces[1]]);
                        vector3d2.Unitize();
                        center = vector3d1 - (vector3d2 * (vector3d1 * vector3d2));
                    }
                    center.Unitize();
                    item[i] = center;
                    item[i] /= (item[i] * mesh.FaceNormals[vertexFaces[0]]);
                    mesh1.Vertices[i] = (Point3f)(mesh1.Vertices[i] + (Vector3f)(item[i] * dist));
                }

                //Lattice
                /*
                Mesh mesh2 = new Mesh();
                for (int k = 0; k < mesh.TopologyEdges.Count; k++)
                {
                    Line line = mesh.TopologyEdges.EdgeLine(k);
                    Line line1 = mesh1.TopologyEdges.EdgeLine(k);
                    int num1 = mesh2.Vertices.Add(line.From);
                    int num2 = mesh2.Vertices.Add(line.To);
                    int num3 = mesh2.Vertices.Add(line1.To);
                    int num4 = mesh2.Vertices.Add(line1.From);
                    mesh2.Faces.AddFace(num1, num2, num3, num4);
                }
                */
            }catch(Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            return mesh1;

        }

        public static bool IsNaked(this Mesh M, int meshEdge)
        {
           return M.TopologyEdges.GetConnectedFaces(meshEdge).Length == 1;

        }
        public static DataTree<Polyline> SectionMesh(Mesh M, Plane P, double D, double tolerance = 0.01) {

            //GetBoundingBox
            Box box = Box.Unset;
            BoundingBox bbox = M.GetBoundingBox(P, out box);
            int n = (int)Math.Ceiling(box.Z.Length / D);


            //Store Planes
            var plns = new List<Plane>();

            //Output cuts
            DataTree<Polyline> cuts = new DataTree<Polyline>();

            //Mesh Plane intersction
            for (int i = 0; i < n; i++) {
                Plane plane = new Plane(box.PointAt(0, 0, 0) + P.ZAxis * i * D + P.ZAxis * tolerance, P.XAxis, P.YAxis);
                plns.Add(plane);
                Polyline[] sections = Rhino.Geometry.Intersect.Intersection.MeshPlane(M, plane);
                if (sections != null) {
                    cuts.AddRange(sections, new GH_Path(i));
                }

            }
            return cuts;

        }

        public static Point3d SurfaceRay(Surface M, Line l) {

            
            List<Point3d> rayPoints = new List<Point3d>();
            List<GeometryBase> g = new List<GeometryBase>();
            g.Add(M);


            Line line = new Line(l.From, l.To);
            Ray3d ray = new Ray3d(l.From, l.Direction);
            Point3d[] p = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, g, 1);

            if (p != null) {
                rayPoints.Add(p[0]);
            }

            line = new Line(l.To, l.From);
            ray = new Ray3d(l.To, line.Direction);
            p = Rhino.Geometry.Intersect.Intersection.RayShoot(ray, g, 1);


            if (p != null) {
                rayPoints.Add(p[0]);
            }

            if (rayPoints.Count == 1)
                return rayPoints[0];

            if (rayPoints.Count == 2) {
                double d0 = l.From.DistanceToSquared(rayPoints[0]);
                double d1 = l.From.DistanceToSquared(rayPoints[1]);

                if (d0 > d1)
                    return rayPoints[1];
                else
                    return rayPoints[0];

            }

            return l.From;

        }

        public static Point3d MeshRay(Mesh M, Line l) {

            List<Point3d> rayPoints = new List<Point3d>();


            Line line = new Line(l.From, l.To);
            Ray3d ray = new Ray3d(l.From, line.Direction);
            double num = Rhino.Geometry.Intersect.Intersection.MeshRay(M, ray);

            if (num > 0 && Rhino.RhinoMath.IsValidDouble(num)) {
                Point3d p = ray.PointAt(num);
                rayPoints.Add(ray.PointAt(num));
            }

            line = new Line(l.To, l.From);
            ray = new Ray3d(l.To, line.Direction);
            num = Rhino.Geometry.Intersect.Intersection.MeshRay(M, ray);


            if (num > 0 && Rhino.RhinoMath.IsValidDouble(num)) {
                rayPoints.Add(ray.PointAt(num));
            }

            if (rayPoints.Count == 1)
                return rayPoints[0];

            if (rayPoints.Count == 2) {
                double d0 = l.From.DistanceToSquared(rayPoints[0]);
                double d1 = l.From.DistanceToSquared(rayPoints[1]);

                if (d0 > d1)
                    return rayPoints[1];
                else
                    return rayPoints[0];

            }

            return l.From;

        }

        public static List<Polyline> GetFacePolylines(this Mesh mesh) {

            List<Polyline> polylines = new List<Polyline>();
            foreach (MeshFace mf in mesh.Faces) {
                Polyline polyline = new Polyline();
                if (mf.IsQuad)
                    polyline.AddRange(new Point3d[] { mesh.Vertices[mf.A], mesh.Vertices[mf.B], mesh.Vertices[mf.C], mesh.Vertices[mf.D], mesh.Vertices[mf.A] });
                else
                    polyline.AddRange(new Point3d[] { mesh.Vertices[mf.A], mesh.Vertices[mf.B], mesh.Vertices[mf.C], mesh.Vertices[mf.A] });
                if (polyline.IsValid)
                    polylines.Add(polyline);
            }
            return polylines;

        }

        public static Polyline[] GetFacePolylinesArray(this Mesh mesh) {

            Polyline[] polylines = new Polyline[mesh.Faces.Count];
            int i = 0;
            foreach (MeshFace mf in mesh.Faces) {

                Polyline polyline = new Polyline();
                if (mf.IsQuad)
                    polyline.AddRange(new Point3d[] { mesh.Vertices[mf.A], mesh.Vertices[mf.B], mesh.Vertices[mf.C], mesh.Vertices[mf.D], mesh.Vertices[mf.A] });
                else
                    polyline.AddRange(new Point3d[] { mesh.Vertices[mf.A], mesh.Vertices[mf.B], mesh.Vertices[mf.C], mesh.Vertices[mf.A] });
                polylines[i++] = polyline;
            }
            return polylines;

        }


        public static int AddFace(this Mesh mesh, IEnumerable<int> indices) {

            int counter = 0;

            int[] ids = new int[4];

            foreach (int i in indices) {
                ids[counter++] = i;

                if (counter == 4)
                    break;
            }


            if (counter == 4)
                mesh.Faces.AddFace(ids[0], ids[1], ids[2], ids[3]);
            else if (counter == 3)
                mesh.Faces.AddFace(ids[0], ids[1], ids[2]);

            return counter;
        }

        public static MeshFace meshFace( IEnumerable<int> indices) {

            int counter = 0;

            int[] ids = new int[4];

            foreach (int i in indices) {
                ids[counter++] = i;

                if (counter == 4)
                    break;
            }


            if (counter == 4)
                return new MeshFace(ids[0], ids[1], ids[2], ids[3]);
            else if (counter == 3)
                return new MeshFace(ids[0], ids[1], ids[2]);

            return new MeshFace();
        }

        public static List<Mesh> ExplodeMesh(this Mesh M, bool E = false) {

            List<Mesh> meshesExploded = new List<Mesh>();
            Mesh meshes = new Mesh();
            if (M != null && M.IsValid) {

                if (M.VertexColors.Count != M.Vertices.Count) {

                    M.VertexColors.Clear();
                    for (int i = 0; i < M.Vertices.Count; i++) {
                        M.VertexColors.Add(200, 200, 200);
                    }
                }

                if (M.Ngons.Count > 0) {





                    int counter = 0;
                    foreach (MeshNgon ngon in M.Ngons) {

                        //Component.Message = counter++.ToString();
                        UInt32[] faceID = ngon.FaceIndexList();
                        UInt32[] vertexID = ngon.BoundaryVertexIndexList();

                        Dictionary<int, Point3d> vertices = new Dictionary<int, Point3d>();
                        List<int> id = new List<int>();
                        List<Point3d> points = new List<Point3d>();
                        Mesh ngonMesh = new Mesh();

                        int R = 0;
                        int G = 0;
                        int B = 0;

                        //Add unique vertices
                        //      foreach(UInt32 f in faceID){
                        //        if(M.Faces[(int) f].IsTriangle){
                        //          if(!vertices.ContainsKey(M.Faces[(int) f].A)) vertices.Add(M.Faces[(int) f].A, M.Vertices[M.Faces[(int) f].A]);
                        //          if(!vertices.ContainsKey(M.Faces[(int) f].B)) vertices.Add(M.Faces[(int) f].B, M.Vertices[M.Faces[(int) f].B]);
                        //          if(!vertices.ContainsKey(M.Faces[(int) f].C)) vertices.Add(M.Faces[(int) f].C, M.Vertices[M.Faces[(int) f].C]);
                        //        } else {
                        //          if(!vertices.ContainsKey(M.Faces[(int) f].D)) vertices.Add(M.Faces[(int) f].D, M.Vertices[M.Faces[(int) f].D]);
                        //        }
                        //      }
                        int a = -1;
                        int b = -1;
                        int c = -1;
                        int d = -1;

                        foreach (UInt32 f in faceID) {

                            a = M.Faces[(int)f].A;
                            b = M.Faces[(int)f].B;
                            c = M.Faces[(int)f].C;

                            if (!id.Contains(a)) {
                                id.Add(a);
                                ngonMesh.Vertices.Add(M.Vertices[a]);
                                R += M.VertexColors[a].R;
                                G += M.VertexColors[a].G;
                                B += M.VertexColors[a].B;
                            }
                            if (!id.Contains(b)) {
                                id.Add(b);
                                ngonMesh.Vertices.Add(M.Vertices[b]);
                                R += M.VertexColors[b].R;
                                G += M.VertexColors[b].G;
                                B += M.VertexColors[b].B;
                            }
                            if (!id.Contains(c)) {
                                id.Add(c);
                                ngonMesh.Vertices.Add(M.Vertices[c]);
                                R += M.VertexColors[c].R;
                                G += M.VertexColors[c].G;
                                B += M.VertexColors[c].B;
                            }

                            if (M.Faces[(int)f].IsQuad) {
                                d = M.Faces[(int)f].D;
                                if (!id.Contains(d)) {
                                    id.Add(d);
                                    ngonMesh.Vertices.Add(M.Vertices[d]);
                                    R += M.VertexColors[d].R;
                                    G += M.VertexColors[d].G;
                                    B += M.VertexColors[d].B;
                                }
                            }
                        }

                        //Add unique vertices to mesh
                        int n = id.Count;
                        R /= n;
                        G /= n;
                        B /= n;
                        for (int i = 0; i < n; i++) {
                            ngonMesh.VertexColors.Add(R, G, B);
                        }


                        //Add faces
                        foreach (UInt32 f in faceID) {

                            //if(M.Faces[(int) f].IsTriangle){
                            a = id.IndexOf(M.Faces[(int)f].A); //Check index of vertices.Add(M.Faces[(int) f].A in a dictionary of key values
                            b = id.IndexOf(M.Faces[(int)f].B);
                            c = id.IndexOf(M.Faces[(int)f].C);

                            if (M.Faces[(int)f].IsQuad) {
                                d = id.IndexOf(M.Faces[(int)f].D);
                            }

                            if (M.Faces[(int)f].IsTriangle)
                                ngonMesh.Faces.AddFace(a, b, c);
                            else
                                ngonMesh.Faces.AddFace(a, b, c, d);

                        }

                        //Add outlines
                        List<int> boundaryID = new List<int>();
                        foreach (UInt32 v in vertexID) {
                            boundaryID.Add(id.IndexOf((int)v));
                        }

                        MeshNgon ngon_ = MeshNgon.Create(boundaryID, Enumerable.Range(0, faceID.Length).ToArray());
                        ngonMesh.Ngons.AddNgon(ngon_);
                        if (!E)
                            meshes.Append(ngonMesh);
                        else {
                            meshesExploded.Add(ngonMesh);
                            meshesExploded[counter].FaceNormals.ComputeFaceNormals();
                            meshesExploded[counter].Normals.ComputeNormals();
                            meshesExploded[counter].Compact();
                        }
                        counter++;
                    }

                } else {
                    int v = 0;
                    for (int i = 0; i < M.Faces.Count; i++) {



                        int r = 0;
                        int g = 0;
                        int b = 0;

                        if (!E) {
                            meshes.Vertices.Add(M.Vertices[M.Faces[i].A]);
                            meshes.Vertices.Add(M.Vertices[M.Faces[i].B]);
                            meshes.Vertices.Add(M.Vertices[M.Faces[i].C]);
                        } else {
                            meshesExploded.Add(new Mesh());
                            meshesExploded[i].Vertices.Add(M.Vertices[M.Faces[i].A]);
                            meshesExploded[i].Vertices.Add(M.Vertices[M.Faces[i].B]);
                            meshesExploded[i].Vertices.Add(M.Vertices[M.Faces[i].C]);
                        }


                        r += M.VertexColors[M.Faces[i].A].R;
                        g += M.VertexColors[M.Faces[i].A].G;
                        b += M.VertexColors[M.Faces[i].A].B;
                        r += M.VertexColors[M.Faces[i].B].R;
                        g += M.VertexColors[M.Faces[i].B].G;
                        b += M.VertexColors[M.Faces[i].B].B;
                        r += M.VertexColors[M.Faces[i].C].R;
                        g += M.VertexColors[M.Faces[i].C].G;
                        b += M.VertexColors[M.Faces[i].C].B;


                        if (M.Faces[i].IsTriangle) {


                            if (!E) {
                                meshes.Faces.AddFace(v + 0, v + 1, v + 2);
                            } else {
                                meshesExploded[i].Faces.AddFace(0, 1, 2);
                            }
                            r /= 3;
                            g /= 3;
                            b /= 3;
                            if (!E) {
                                meshes.VertexColors.Add(r, g, b);
                                meshes.VertexColors.Add(r, g, b);
                                meshes.VertexColors.Add(r, g, b);
                            } else {
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].FaceNormals.ComputeFaceNormals();
                                meshesExploded[i].Normals.ComputeNormals();
                                meshesExploded[i].Compact();
                            }
                            v += 3;
                        } else {

                            if (!E) {
                                meshes.Vertices.Add(M.Vertices[M.Faces[i].D]);
                            } else {
                                meshesExploded[i].Vertices.Add(M.Vertices[M.Faces[i].D]);
                            }

                            r += M.VertexColors[M.Faces[i].D].R;
                            g += M.VertexColors[M.Faces[i].D].G;
                            b += M.VertexColors[M.Faces[i].D].B;

                            if (!E) {
                                meshes.Faces.AddFace(v + 0, v + 1, v + 2, v + 3);
                            } else {
                                meshesExploded[i].Faces.AddFace(0, 1, 2, 3);
                            }

                            r /= 4;
                            g /= 4;
                            b /= 4;
                            if (!E) {
                                meshes.VertexColors.Add(r, g, b);
                                meshes.VertexColors.Add(r, g, b);
                                meshes.VertexColors.Add(r, g, b);
                                meshes.VertexColors.Add(r, g, b);
                            } else {
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].VertexColors.Add(r, g, b);
                                meshesExploded[i].FaceNormals.ComputeFaceNormals();
                                meshesExploded[i].Normals.ComputeNormals();
                                meshesExploded[i].Compact();
                            }
                            v += 4;
                        }

                    }

                }
                if (!E) {
                    meshes.FaceNormals.ComputeFaceNormals();
                    meshes.Normals.ComputeNormals();
                    meshes.Compact();
                }
                if (E)
                    return meshesExploded;
                else
                    return new List<Mesh> { meshes };
            }

            return null;

        }

        public static List<Polyline> GetNakedPolylines(this Mesh M, bool close = true) {
            List<Polyline> polylines = new List<Polyline>();

            List<List<int>> allnv_ = M.GetNakedVerticesID();

            foreach (List<int> i in allnv_) {
                Polyline polyline = new Polyline();
                foreach (int j in i) {
                    polyline.Add(M.Vertices[j]);
                }
                if (close)
                    polyline.Close();
                polylines.Add(polyline);
            }

            return polylines;
        }

        public static List<Polyline> GetNakedPolylines(this Mesh M, List<List<int>> nakedVertices) {
            List<Polyline> polylines = new List<Polyline>();


            foreach (List<int> i in nakedVertices) {
                Polyline polyline = new Polyline();
                foreach (int j in i) {
                    polyline.Add(M.Vertices[j]);
                }
                polyline.Close();
                polylines.Add(polyline);
            }

            return polylines;
        }

        public static List<List<int>> GetNakedVerticesID(this Mesh M, int Stop = 1000) {
            List<List<int>> allnv_ = new List<List<int>>();

 




            if (M != null) {

                if (M.Vertices.Count == 3) {
                    allnv_.Add(new List<int> { M.Faces[0].A, M.Faces[0].B, M.Faces[0].C });
                    return allnv_;
                         }

                    Mesh mesh = M.DuplicateMesh();
                bool[] ne = M.GetNakedEdgePointStatus();
                //Rhino.RhinoApp.WriteLine((ne==null).ToString());
                //Rhino.RhinoApp.WriteLine(ne.Length.ToString());
                for (int k = 0; k < 100; k++) {

                    List<int> allnv = new List<int>();
                    int last = -1;




                    //Take first naked point
                    int fv = -1;
                    for (int i = 0; i < M.Vertices.Count; i++) {
                        if (ne[i]) {
                            fv = i;
                            ne[i] = false;
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(M.Vertices[i]);
                            break;
                        }
                    }

                    if (fv == -1)
                        break;

                    if (fv > -1) {

                        int first = fv;
                        int second = -1;
                        allnv.Add(fv);
                        last = fv;

                        // I guess what you could do is Mesh.GetNakedEdgePointStatus() to get all the vertices’
                        //  (not topology vertices!) naked edge statuses.
                        // Then start at one vertex and enumerate all edges that radiate from that vertex.
                        //Take the edge that has a naked opposite vertex and keep going from that vertex until you reach the beginning.
                        for (int j = 0; j < Stop; j++) {
                            int[] cv = M.Vertices.GetConnectedVertices(last);


                            bool end = true;//for continuing loop
                            foreach (int i in cv)


                                if (!allnv.Contains(i) && ne[i]) {

                                    int topEID = M.TopologyEdges.GetEdgeIndex(M.TopologyVertices.TopologyVertexIndex(last), M.TopologyVertices.TopologyVertexIndex(i));
                                    if (M.TopologyEdges.GetConnectedFaces(topEID).Length != 1)
                                        continue;

                                    if (j == 0)
                                        second = i;

                                    allnv.Add(i);
                                    last = i;
                                    ne[i] = false;
                                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(M.Vertices[i]);
                                    //Print(i.ToString());
                                    end = false;//for continuing loop
                                    break;
                                }

                            if (end)
                                break;
                        }

                        //Check winding and reverse if not the same
                        if (allnv.Count > 1) {

                            int topEID = M.TopologyEdges.GetEdgeIndex(M.TopologyVertices.TopologyVertexIndex(first), M.TopologyVertices.TopologyVertexIndex(second));
                            int a = M.TopologyVertices.MeshVertexIndices(M.TopologyEdges.GetTopologyVertices(topEID).I)[0];
                            int b = M.TopologyVertices.MeshVertexIndices(M.TopologyEdges.GetTopologyVertices(topEID).J)[0];
                            //Print(a.ToString() + " " + first.ToString());
                            if (a != first)
                                allnv.Reverse();
                        }


                    }//if



                    //if (k == 0) {

                    //    MeshNgon ngon = MeshNgon.Create(allnv, Enumerable.Range(0, M.Faces.Count).ToList());
                    //    mesh.Ngons.AddNgon(ngon);
                    //}
                    //B = allnv;
                    if (allnv.Count == 1)
                        break;
                    allnv_.Add(allnv);
                    allnv = new List<int>();
                   // A = mesh;
                }//for k

            }//null
             // C = allnv_;
            return allnv_;
        }

     
        public static Tuple<Mesh, List<MeshFace>> TriangulateOpenPolyline(this Polyline polyline)
        {
            //Rhino.RhinoApp.WriteLine("n" + polyline.Count.ToString());
            if (polyline.Count < 3 || !polyline.IsValid)
                return new Tuple<Mesh, List<MeshFace>>(null, null);
         

            Polyline P = new Polyline(polyline);

            if (P[0].DistanceToSquared(P[P.Count - 1]) < 0.001)
            {
                P.RemoveAt(0);
            }

            //Rhino.RhinoApp.WriteLine("n" + P.Count.ToString());
            if (P.Count == 3) {
                Mesh mesh = new Mesh();
                mesh.Vertices.AddVertices(P);
                mesh.Faces.AddFace(2,1,0);
                MeshFace mf = (new MeshFace(2, 1, 0));
                List<MeshFace> mfList = new List<MeshFace>();
                mfList.Add(mf);
                //mesh.Bake();
                return new Tuple<Mesh, List<MeshFace>>( mesh, mfList);
            }

         

            // Find best plane for triangulation and remap points
            // to WorldXY for easier maths.
            Plane m_plane;
            //Plane.FitPlaneToPoints(P, out m_plane);
            m_plane = P.GetPlane();

            List<Point3d> m_points = new List<Point3d>();

            for (int i = 0; i < P.Count; ++i)
            {
                Point3d p;
                m_plane.RemapToPlaneSpace(P[i], out p);
                m_points.Add(p);
            }

            // Counter for triangulation loop.
            int N = m_points.Count;

            // Constant point count.
            int num_points = N;

            // Orient m_points clockwise
            if (SignedPolygonArea(m_points) < 0)
                m_points.Reverse();

            // Instead of removing points at every iteration, we set a
            // flag instead, and then skip the flagged vertices in
            // subsequent steps.

            List<MeshFace> m_faces = new List<MeshFace>(); // New mesh faces.
            bool[] flags = new bool[N]; // Vertex flags.

            // from http://www.csharphelper.com/examples/howto_polygon_geometry.zip
            // Triangulation loop
            int A = 0; int B = 0; int C = 0;

            // Critical loop.
            bool success = false;
            while (N > 2)
            {
                success = false;
                num_points = m_points.Count;

                for (A = 0; A < num_points; ++A)
                {
                    if (flags[A])
                        continue;

                    B = (A + 1) % num_points;
                    while (flags[B])
                        B = (B + 1) % num_points;

                    C = (B + 1) % num_points;
                    while (flags[C])
                        C = (C + 1) % num_points;

                    if (FormsEar(m_points, A, B, C))
                    {
                        success = true;
                        break;
                    }
                }

                if (success)
                {
                    m_faces.Add(new MeshFace(A, B, C));
                    flags[B] = true;
                }
                N--;
            }

            Mesh m = new Mesh();

            // Transform points back to original positions.
            Transform xform = Transform.PlaneToPlane(Plane.WorldXY, m_plane);
            for (int i = 0; i < m_points.Count; ++i)
                m_points[i] = m_plane.PointAt(m_points[i].X, m_points[i].Y, m_points[i].Z);

            // Construct mesh.
            m.Vertices.AddVertices(m_points);
            m.Faces.AddFaces(m_faces);
            m.Compact();

            //Rhino.RhinoApp.WriteLine("Mesh faces: {0}", m.Faces.Count);

            // Output
            //Pg = m;
            //Mfs = m_faces;
            return new Tuple<Mesh, List<MeshFace>>(m, m_faces);
        }
        // from http://blackpawn.com/texts/pointinpoly/default.html
        private static bool PointInTriangle(Point3d P, Point3d A, Point3d B, Point3d C) {
            // Compute vectors
            Vector3d v0 = C - A;
            Vector3d v1 = B - A;
            Vector3d v2 = P - A;

            // Compute dot products
            double dot00 = v0 * v0;
            double dot01 = v0 * v1;
            double dot02 = v0 * v2;
            double dot11 = v1 * v1;
            double dot12 = v1 * v2;

            // Compute barycentric coordinates
            double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }


        // from http://www.csharphelper.com/examples/howto_polygon_geometry.zip
        private static bool FormsEar(List<Point3d> points, int A, int B, int C) {

            // Get angle of center vertex
            double BAx = points[A].X - points[B].X;
            double BAy = points[A].Y - points[B].Y;
            double BCx = points[C].X - points[B].X;
            double BCy = points[C].Y - points[B].Y;

            double dot = (BAx * BCx + BAy * BCy);
            double cross = (BAx * BCy - BAy * BCx);

            double angle = Math.Atan2(cross, dot);
            //Rhino.RhinoApp.WriteLine("Angle: {0}", angle);

            if (angle < 0) {
                return false;
            }

            for (int i = 0; i < points.Count; ++i) {
                if ((i != A) && (i != B) && (i != C)) {
                    if (PointInTriangle(points[i], points[A], points[B], points[C])) {
                        // This point is in the triangle
                        // do this is not an ear.
                       // Rhino.RhinoApp.WriteLine("Point is in triangle.");
                        return false;
                    }
                }
            }

            return true;
        }

        // from http://www.csharphelper.com/examples/howto_polygon_geometry.zip
        private static double SignedPolygonArea(List<Point3d> points) {
            // Add the first point to the end.
            List<Point3d> pts = new List<Point3d>(points);
            pts.Add(pts[0]);

            int num_points = points.Count;

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++) {
                area +=
                  (pts[i + 1].X - pts[i].X) *
                  (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
        }


        public static double averageMeshEdgeLength(this Mesh mesh) {
            double l = 0.0;

            Rhino.Geometry.Collections.MeshTopologyEdgeList topedge = mesh.TopologyEdges;
            int count = topedge.Count;

            for (int i = 0; i < count; i++) {
                Rhino.IndexPair pa = topedge.GetTopologyVertices(i);
                Point3f p1 = mesh.TopologyVertices[pa.I];
                double le = mesh.TopologyVertices[pa.I].DistanceTo(mesh.TopologyVertices[pa.J]);
                l = l + le;
            }

            return ((l / count) * 1.25);
        }

        public static int[] GetFaceArray(Mesh mesh, int facdId) {
            if (mesh.Faces[facdId].IsQuad)
                return new int[] { mesh.Faces[facdId].A , mesh.Faces[facdId].B, mesh.Faces[facdId].C , mesh.Faces[facdId].D};
            else
                return new int[] { mesh.Faces[facdId].A, mesh.Faces[facdId].B, mesh.Faces[facdId].C};
        }

        public static Tuple<Polyline[], Polyline[], Plane[], Plane[]> OffsetTriangleMesh(this Mesh M, double D, double C) {
            Polyline[] outlines = new Polyline[M.Faces.Count];
            Polyline[] outlinesO = new Polyline[M.Faces.Count];


            //Only for triangulated meshes

            Mesh MO = M.Offset(D);

            //Get plane per each edge
            Plane[] EP = new Plane[M.TopologyEdges.Count];


            for (int i = 0; i < M.TopologyEdges.Count; i++) {

                Line l0 = M.TopologyEdges.EdgeLine(i);
                Line l1 = MO.TopologyEdges.EdgeLine(i);
                Plane plane = new Plane(l0.From, l0.To, l1.PointAt(0.5));
                plane.Origin = l0.PointAt(0.5);
                EP[i] = plane;

            }

            //Intersect plane with a pair of planes

            Plane[] FP = new Plane[M.Faces.Count];
            M.FaceNormals.ComputeFaceNormals();
            for (int i = 0; i < M.Faces.Count; i++) {

                int[] TE = M.TopologyEdges.GetEdgesForFace(i);
                List<Plane> sidePlanes = new List<Plane>();

                foreach (int j in TE)
                    sidePlanes.Add(EP[j]);

                Plane facePlane = new Plane(M.Faces.GetFaceCenter(i), M.FaceNormals[i]);
                FP[i] = facePlane;
                Plane facePlaneO = new Plane(M.Faces.GetFaceCenter(i), M.FaceNormals[i]);
                facePlaneO.Translate(facePlaneO.ZAxis * D);

                outlines[i] = NGonsCore.PolylineUtil.PolylineFromPlanes(facePlane, sidePlanes, true);
                outlinesO[i] = NGonsCore.PolylineUtil.PolylineFromPlanes(facePlaneO, sidePlanes, true);
                outlines[i] = PolylineUtil.Chamfer(outlines[i], C);
                outlinesO[i] = PolylineUtil.Chamfer(outlinesO[i], C);



            }


            return new Tuple<Polyline[], Polyline[], Plane[], Plane[]>(outlines, outlinesO,EP,FP);
  
        }

        public static List<Curve> GetEdgesAsCurves(this Mesh m) {

            List<Curve> curves = new List<Curve>(m.TopologyEdges.Count);

            for (int i = 0; i < m.TopologyEdges.Count; i++)
                curves.Add(m.TopologyEdges.EdgeLine(i).ToNurbsCurve());

            return curves;
        }

        public static List<Line> GetEdges(this Mesh m) {

            List<Line> curves = new List<Line>(m.TopologyEdges.Count);

            for (int i = 0; i < m.TopologyEdges.Count; i++)
                curves.Add(m.TopologyEdges.EdgeLine(i));

            return curves;
        }

        public static List<Point3d>[] GetHingePoints (this Mesh m){

            List<Point3d>[] hingePoints = new List<Point3d>[4] { new List<Point3d>(), new List<Point3d>(), new List<Point3d>(), new List<Point3d>() };

            m.Faces.ConvertQuadsToTriangles();
            Point3d[] point3dArray = m.Vertices.ToPoint3dArray();

            for (int i = 0; i < m.TopologyEdges.Count; i++) {

                int[] connectedFaces = m.TopologyEdges.GetConnectedFaces(i);
                if (connectedFaces.Length == 2) {

                    Line line = m.TopologyEdges.EdgeLine(i);
                    hingePoints[0].Add(line.From);
                    hingePoints[1].Add(line.To);

                    int[] v1 = m.TopologyVertices.IndicesFromFace(connectedFaces[0]);
                    int[] v2 = m.TopologyVertices.IndicesFromFace(connectedFaces[1]);

                    var topologyVertices = m.TopologyEdges.GetTopologyVertices(i);
                    int[] numArray = m.TopologyVertices.ConnectedTopologyVertices(topologyVertices.I, true); //Sorted because look down

                    for (int j = 0; j < numArray.Length; j++) {
                        if (numArray[j] == topologyVertices.J) {
                            //Because it is sorted you can take previous and coming vertex
                            //             -------* this is numArray[(j - 1 + numArray.Length) % numArray.Length]
                            //       ------
                            // ------
                            //*-------------------* this is topologyVertices.J
                            // ------
                            //       ------
                            //             -------* this is numArray[(j - 1 + numArray.Length) % numArray.Length]
                            int num = numArray[(j - 1 + numArray.Length) % numArray.Length];
                            int num1 = numArray[(j + 1 + numArray.Length) % numArray.Length];

                            //Retrieving mesh vertex location
                            int[] numArray1 = m.TopologyVertices.MeshVertexIndices(num);
                            int[] numArray2 = m.TopologyVertices.MeshVertexIndices(num1);
                            hingePoints[2].Add(point3dArray[numArray1[0]]);
                            hingePoints[3].Add(point3dArray[numArray2[0]]);
                        }
                    }

                }




            }

            return hingePoints;

        }









        public struct Mesh_Cuvature_Utilities_Debugging {

            public readonly List<List<double>> x;
            public readonly List<double> y;

            public Mesh_Cuvature_Utilities_Debugging(List<List<double>> x, List<double> y) {
                this.x = x;
                this.y = y;
            }


            //It lays down the mouse vertices connected with a vertex for a number of ring
            //IN: meshtopologyvertex, index(int), ring(int)
            //Return: the associated list(including the point itself)
            public static List<int> SingleVertexConnection(Rhino.Geometry.Collections.MeshTopologyVertexList meshtopologyvertex, int indice, int ring) {
                List<int> Listaconnessi = new List<int>() { indice };
                List<int> Last = new List<int>() { indice };
                List<int> LastNmeno1 = new List<int>();

                for (int i = 0; i < ring; i++) {
                    //#seach neighbours. If it can not find, put i = 1
                    List<int> LastN = new List<int>();
                    List<int> lasto = Last;

                    foreach (int ii in Last) {
                        int[] vert2 = meshtopologyvertex.ConnectedTopologyVertices(ii);
                        if (vert2 == null) {
                            Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(meshtopologyvertex[ii]);
                            break;
                        }
                        foreach (int j in vert2) {
                            if (!Last.Contains(j)) {
                                if (!LastNmeno1.Contains(j)) {
                                    if (!LastN.Contains(j)) {
                                        LastN.Add(j);
                                        Listaconnessi.Add(j);
                                    }
                                }
                            }
                        }
                    }
                    LastNmeno1 = lasto;
                    Last = LastN;
                }
                return Listaconnessi;
            }

            public static List<int> SingleVertexConnectionSphere(MeshTopologyVertexList meshtopologyvertex, int indice, double Radius) {
                //It establishes the mouse vertices connected with a vertex within a sphere of radius Radius
                //IN: meshtopologyvertex, index(int), Radius(Float)
                //Return: the associated list(including the point itself)

                int Flag = 0;
                List<int> Listaconnessi = new List<int>() { indice };
                List<int> Last = new List<int>() { indice };
                Point3f pt = meshtopologyvertex[indice];
                List<int> LastNmeno1 = new List<int>();

                while (Flag == 0) {
                    //#seach neighbours. If it can not find, put i = 1
                    List<int> LastN = new List<int>();
                    List<int> Lasto = Last;

                    foreach (int ii in Last) {
                        int[] vert2 = meshtopologyvertex.ConnectedTopologyVertices(ii);
                        foreach (int j in vert2) {
                            if (!Last.Contains(j)) {
                                if (!LastNmeno1.Contains(j)) {
                                    if (!LastN.Contains(j)) {
                                        if ((pt - meshtopologyvertex[j]).Length < Radius) {
                                            LastN.Add(j);
                                            Listaconnessi.Add(j);
                                        }

                                    }

                                }

                            }
                        }

                    }
                    LastNmeno1 = Lasto;
                    if (LastN.Count == 0) Flag = 1;
                    Last = LastN;
                }

                return Listaconnessi;

            }

            public static List<List<int>> RingConnection(Rhino.Geometry.Collections.MeshTopologyVertexList topvertex, int d3) {
                List<List<int>> connection = new List<List<int>>();
                for (int i = 0; i < topvertex.Count; i++) {
                    List<int> con = SingleVertexConnection(topvertex, i, d3);
                    connection.Add(con);
                }
                return connection;
            }

            public static List<List<int>> SphereConnection(MeshTopologyVertexList topvertex, double d4) {
                List<List<int>> connection = new List<List<int>>();
                for (int i = 0; i < topvertex.Count; i++) {
                    List<int> con = SingleVertexConnectionSphere(topvertex, i, d4);
                    connection.Add(con);
                }
                return connection;
            }

            //Calculates the Weingarten matrix for the function x ^ 2 + y ^ 2 + C * B * x * y + D or A * C * x ^ 2 + y ^ 2 + B * x * y or 7 cubic returns eigenvalues and eigenvectors
            public static List<double> Weingarten0(RowVector coef) {
                SymmetricMatrix I = new SymmetricMatrix(2);
                I[0, 0] = 2 * coef[0];
                I[0, 1] = coef[1];
                I[1, 1] = 2 * coef[2];
                RealEigensystem E = I.Eigensystem();
                List<double> k = new List<double>();
                for (int i = 0; i < 2; i++) {
                    k.Add(E.Eigenvalue(i));
                }
                return k;
            }

            // Calculates the Weingarten matrix for the function x ^ 2 + y ^ 2 + B * C * x * y + D returns eigenvalues and eigenvectors
            public static List<double> Weingarten1(RowVector coef) {
                SymmetricMatrix I = new SymmetricMatrix(2);
                I[0, 0] = 2 * coef[0];
                I[0, 1] = coef[2];
                I[1, 1] = 2 * coef[1];
                RealEigensystem E = I.Eigensystem();
                List<double> k = new List<double>();
                for (int i = 0; i < 2; i++) {
                    k.Add(E.Eigenvalue(i));
                }
                return k;
            }

            // Calculates the Weingarten matrix for the function x ^ 2 + y ^ 2 + B * C * x * y + D returns eigenvalues and eigenvectors
            public static List<double> Weingarten2(RowVector coef) {
                SymmetricMatrix I = new SymmetricMatrix(2);

                double p1 = coef[0];
                double p2 = coef[1];
                double p3 = coef[2];
                double p4 = coef[3];
                double p5 = coef[4];
                double cost = 1 / (Math.Pow((Math.Pow(p4, 2) + Math.Pow(p5, 2) + 1), 1.5));
                I[0, 0] = (2 * p1 * (Math.Pow(p5, 2) + 1) - p3 * p4 * p5) / cost;
                I[0, 1] = (p3 * (Math.Pow(p5, 2) + 1) - 2 * p2 * p4 * p5) / cost;
                I[1, 0] = (p3 * (Math.Pow(p4, 2) + 1) - 2 * p1 * p4 * p5) / cost;
                I[1, 1] = (2 * p2 * (Math.Pow(p4, 2) + 1) - p3 * p4 * p5) / cost;
                double[] k = I.Eigenvalues();

                List<double> kList = new List<double>();
                for (int i = 0; i < 2; i++) {
                    kList.Add(k[i]); //original E[0].Re ??? What does this "Re" Mean?
                                     //if(i == 0) RhinoApp.WriteLine(k[i].ToString());
                }
                return kList;
            }

            public static RowVector fittingF0(List<Point3d> points, Point3d pt) {
                //Find the coefficients (a, b, c, d) fitting the equation a * x ^ 2 + b * x ^ 2 + c * x * y + d on a" "set of points"
                SymmetricMatrix I = new SymmetricMatrix(3);
                ColumnVector v = new ColumnVector(3);

                foreach (Point3d p in points) {
                    double a = p.X;
                    double b = p.Y;
                    double c = p.Z;
                    I[0, 0] = I[0, 0] + Math.Pow(a, 4);
                    I[0, 1] = I[0, 1] + Math.Pow(a, 3) * b;
                    I[0, 2] = I[0, 2] + Math.Pow(a, 2) * Math.Pow(b, 2);
                    I[1, 2] = I[1, 2] + a * Math.Pow(b, 3);
                    I[2, 2] = I[2, 2] + Math.Pow(b, 4);
                    v[0] = v[0] + Math.Pow(a, 2) * c;
                    v[1] = v[1] + a * b * c;
                    v[2] = v[2] + Math.Pow(b, 2) * c;
                }
                I[1, 1] = I[0, 2];
                RowVector e1;

                try {
                    CholeskyDecomposition CD = I.CholeskyDecomposition();
                    ColumnVector Out = CD.Solve(v);
                    e1 = Out.Transpose();
                } catch (Exception e) {
                    e1 = new RowVector(new List<double>() { 0, 0, 0 });
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                    throw new Exception(String.Format("Exception: {0} Trace: {1}", "not permitted in point solution", e.StackTrace));
                }
                return e1;
            }

            public static RowVector fittingF1(List<Point3d> points, Point3d pt) {
                //Find the coefficients (a, b, c, d) fitting the equation a*x^2+b*x^2+c*x*y+d on a" "set of points"
                SymmetricMatrix I = new SymmetricMatrix(4);
                ColumnVector v = new ColumnVector(4);

                foreach (Point3d p in points) {
                    double a = p.X;
                    double b = p.Y;
                    double c = p.Z;
                    I[0, 0] = I[0, 0] + Math.Pow(a, 4);
                    I[0, 1] = I[0, 1] + Math.Pow(a, 2) * Math.Pow(b, 2);
                    I[0, 2] = I[0, 2] + Math.Pow(a, 3) * b;
                    I[0, 3] = I[0, 3] + Math.Pow(a, 2);
                    I[1, 1] = I[1, 1] + Math.Pow(b, 4);
                    I[1, 2] = I[1, 2] + a * Math.Pow(b, 3);
                    I[1, 3] = I[1, 3] + Math.Pow(b, 2);
                    I[2, 3] = I[2, 3] + a * b;
                    I[3, 3] = I[3, 3] + 1;
                    v[0] = v[0] + Math.Pow(a, 2) * c;
                    v[1] = v[1] + Math.Pow(b, 2) * c;
                    v[2] = v[2] + a * b * c;
                    v[3] = v[3] + c;
                }
                I[2, 2] = I[0, 1];
                RowVector e1;

                try {
                    CholeskyDecomposition CD = I.CholeskyDecomposition();
                    ColumnVector Out = CD.Solve(v);
                    e1 = Out.Transpose();
                } catch (Exception e) {
                    e1 = new RowVector(new List<double>() { 0, 0, 0, 0 });
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                    throw new Exception(String.Format("Exception: {0} Trace: {1}", "not permitted in point solution", e.StackTrace));
                }
                return e1;
            }


            public static RowVector fittingF2(List<Point3d> points, Point3d pt) {
                //Find the coefficients (a, b, c, d, e, f) fitting the equation a * x ^ 2 + b * x ^ 2 + c * x * y + d * x * y + f + and on a set of points
                SymmetricMatrix I = new SymmetricMatrix(6);
                ColumnVector v = new ColumnVector(6);

                foreach (Point3d p in points) {
                    double a = p.X;
                    double b = p.Y;
                    double c = p.Z;
                    I[0, 0] = I[0, 0] + Math.Pow(a, 4);
                    I[0, 1] = I[0, 1] + Math.Pow(a, 2) * Math.Pow(b, 2);
                    I[0, 2] = I[0, 2] + Math.Pow(a, 3) * b;
                    I[0, 3] = I[0, 3] + Math.Pow(a, 3);
                    I[0, 4] = I[0, 4] + Math.Pow(a, 2) * b;
                    I[0, 5] = I[0, 5] + Math.Pow(a, 2);
                    I[1, 1] = I[1, 1] + Math.Pow(b, 4);
                    I[1, 2] = I[1, 2] + a * Math.Pow(b, 3);
                    I[1, 3] = I[1, 3] + a * Math.Pow(b, 2);
                    I[1, 4] = I[1, 4] + Math.Pow(b, 3);
                    I[1, 5] = I[1, 5] + Math.Pow(b, 2);
                    I[2, 5] = I[2, 5] + a * b;
                    I[3, 5] = I[3, 5] + a;
                    I[4, 5] = I[4, 5] + b;
                    I[5, 5] = I[5, 5] + 1;
                    v[0] = v[0] + Math.Pow(a, 2) * c;
                    v[1] = v[1] + Math.Pow(b, 2) * c;
                    v[2] = v[2] + a * b * c;
                    v[3] = v[3] + a * c;
                    v[4] = v[4] + b * c;
                    v[5] = v[5] + c;
                }
                I[2, 2] = I[0, 1];
                I[2, 3] = I[0, 4];
                I[2, 4] = I[1, 3];
                I[3, 3] = I[0, 5];
                I[3, 4] = I[2, 5];
                I[4, 4] = I[1, 5];

                RowVector e1;

                try {
                    CholeskyDecomposition CD = I.CholeskyDecomposition();
                    ColumnVector Out = CD.Solve(v);
                    e1 = Out.Transpose();
                } catch (Exception e) {
                    e1 = new RowVector(new List<double>() { 0, 0, 0, 0, 0, 0 });
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                    throw new Exception(String.Format("Exception: {0} Trace: {1}", "not permitted in point solution", e.StackTrace));
                }
                return e1;
            }

            public static RowVector fittingF3(List<Point3d> points, Point3d pt) {
                //Find the coefficients (a, b, c, d, e, f) fitting the equation a * x ^ 2 + b * x ^ 2 + c * x * y + d * x * y + f + and on a set of points
                SymmetricMatrix I = new SymmetricMatrix(7);
                ColumnVector v = new ColumnVector(7);

                foreach (Point3d p in points) {
                    double a = p.X;
                    double b = p.Y;
                    double c = p.Z;
                    I[0, 0] = I[0, 0] + Math.Pow(a, 4);
                    I[0, 1] = I[0, 1] + Math.Pow(a, 3) * b;
                    I[0, 2] = I[0, 2] + Math.Pow(a, 2) * Math.Pow(b, 2);
                    I[0, 3] = I[0, 3] + Math.Pow(a, 5);
                    I[0, 4] = I[0, 4] + Math.Pow(a, 4) * b;
                    I[0, 5] = I[0, 5] + Math.Pow(a, 3) * Math.Pow(b, 2);
                    I[0, 6] = I[0, 6] + Math.Pow(a, 2) * Math.Pow(b, 3);
                    I[1, 2] = I[1, 2] + a * Math.Pow(b, 3);
                    I[1, 6] = I[1, 6] + a * Math.Pow(b, 4);
                    I[2, 2] = I[2, 2] + Math.Pow(b, 4);
                    I[2, 6] = I[2, 6] + Math.Pow(b, 5);
                    I[3, 3] = I[3, 3] + Math.Pow(a, 6);
                    I[3, 4] = I[3, 4] + Math.Pow(a, 5) * b;
                    I[3, 5] = I[3, 5] + Math.Pow(a, 4) * Math.Pow(b, 2);
                    I[3, 6] = I[3, 6] + Math.Pow(a, 3) * Math.Pow(b, 3);
                    I[4, 6] = I[4, 6] + Math.Pow(a, 2) * Math.Pow(b, 4);
                    I[5, 6] = I[5, 6] + a * Math.Pow(b, 5);
                    I[6, 6] = I[6, 6] + Math.Pow(b, 6);
                    v[0] = v[0] + Math.Pow(a, 2) * c;
                    v[1] = v[1] + a * b * c;
                    v[2] = v[2] + Math.Pow(b, 2) * c;
                    v[3] = v[3] + Math.Pow(a, 3) * c;
                    v[4] = v[4] + Math.Pow(a, 2) * b * c;
                    v[5] = v[5] + a * Math.Pow(b, 2) * c;
                    v[6] = v[6] + Math.Pow(b, 3) * c;
                }
                I[1, 1] = I[0, 2];
                I[1, 3] = I[0, 4];
                I[1, 4] = I[0, 5];
                I[2, 3] = I[0, 5];
                I[1, 5] = I[0, 6];
                I[2, 4] = I[0, 6];
                I[2, 5] = I[1, 6];
                I[4, 4] = I[3, 5];
                I[4, 5] = I[3, 6];
                I[5, 5] = I[4, 6];
                RowVector e1;

                try {
                    CholeskyDecomposition CD = I.CholeskyDecomposition();
                    ColumnVector Out = CD.Solve(v);
                    e1 = Out.Transpose();
                } catch (Exception e) {
                    e1 = new RowVector(new List<double>() { 0, 0, 0, 0, 0, 0 });
                    Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                    throw new Exception(String.Format("Exception: {0} Trace: {1}", "not permitted in point solution", e.StackTrace));
                }
                return e1;

            }

            public static List<double> Curvature0(List<Point3d> pts, Point3d pt, Point3d nor) {

                List<double> K12;

                if (pts.Count < 3) {
                    Rhino.RhinoApp.WriteLine("Insufficient points for the calculation of the fitting point in the" + pt.ToString());
                    K12 = new List<double>(2) { 0, 0 };
                    return K12;
                }

                Plane rc = new Plane();
                Plane.FitPlaneToPoints(pts, out rc);
                Plane fitp = new Plane();
                Plane.FitPlaneToPoints(pts, out fitp);
                fitp.Origin = pt;
                Plane world = Plane.WorldXY;
                Transform xform = Rhino.Geometry.Transform.ChangeBasis(world, fitp);

                K12 = new List<double>(2) { 0, 0 };
                nor.Transform(xform);

                List<Point3d> ptsDuplicate = new List<Point3d>();

                for (int i = 0; i < pts.Count; i++) {
                    /////////////////////////
                    Point3d pTemp = pts[i];
                    pTemp.Transform(xform);//"David Rutten" - This is the one major drawback of value-types over reference-types, and there's no way around it
                    pts[i] = pTemp;
                    ////////////////////////
                    if (nor[2] > 0) ptsDuplicate.Add(new Point3d(pts[i].X, -pts[i].Y, -pts[i].Z));
                    else ptsDuplicate.Add(new Point3d(pts[i].X, pts[i].Y, pts[i].Z));
                }

                RowVector coef = fittingF0(ptsDuplicate, pt);
                K12 = Weingarten0(coef);
                return K12;
            }

            public static List<double> Curvature1(List<Point3d> pts, Point3d pt, Point3d nor) {
                List<double> K12;

                if (pts.Count < 4) {
                    Rhino.RhinoApp.WriteLine("Insufficient points for the calculation of the fitting point in the" + pt.ToString());
                    K12 = new List<double>(2) { 0, 0 };
                    return K12;
                }

                Plane rc = new Plane();
                Plane.FitPlaneToPoints(pts, out rc);
                Plane fitp = new Plane();
                Plane.FitPlaneToPoints(pts, out fitp);
                fitp.Origin = pt;
                Plane world = Plane.WorldXY;
                Transform xform = Rhino.Geometry.Transform.ChangeBasis(world, fitp);

                K12 = new List<double>(2) { 0, 0 };
                nor.Transform(xform);

                List<Point3d> ptsDuplicate = new List<Point3d>();

                for (int i = 0; i < pts.Count; i++) {
                    /////////////////////////
                    Point3d pTemp = pts[i];
                    pTemp.Transform(xform);//"David Rutten" - This is the one major drawback of value-types over reference-types, and there's no way around it
                    pts[i] = pTemp;
                    ////////////////////////
                    if (nor[2] > 0) ptsDuplicate.Add(new Point3d(pts[i].X, -pts[i].Y, -pts[i].Z));
                    else ptsDuplicate.Add(new Point3d(pts[i].X, pts[i].Y, pts[i].Z));
                }

                RowVector coef = fittingF1(ptsDuplicate, pt);
                K12 = Weingarten1(coef);
                return K12;
            }


            public static List<double> Curvature2(List<Point3d> pts, Point3d pt, Point3d nor) {
                List<double> K12;

                if (pts.Count < 6) {
                    Rhino.RhinoApp.WriteLine("Insufficient points for the calculation of the fitting point in the" + pt.ToString());
                    K12 = new List<double>(2) { 0, 0 };
                    return K12;
                }

                Plane rc = new Plane();
                Plane.FitPlaneToPoints(pts, out rc);
                Plane fitp = new Plane();
                Plane.FitPlaneToPoints(pts, out fitp);
                fitp.Origin = pt;
                Plane world = Plane.WorldXY;
                Transform xform = Rhino.Geometry.Transform.ChangeBasis(world, fitp);

                K12 = new List<double>(2) { 0, 0 };
                nor.Transform(xform);

                List<Point3d> ptsDuplicate = new List<Point3d>();

                for (int i = 0; i < pts.Count; i++) {
                    /////////////////////////
                    Point3d pTemp = pts[i];
                    pTemp.Transform(xform);//"David Rutten" - This is the one major drawback of value-types over reference-types, and there's no way around it
                    pts[i] = pTemp;
                    ////////////////////////
                    if (nor[2] > 0) ptsDuplicate.Add(new Point3d(pts[i].X, -pts[i].Y, -pts[i].Z));
                    else ptsDuplicate.Add(new Point3d(pts[i].X, pts[i].Y, pts[i].Z));
                }

                RowVector coef = fittingF2(ptsDuplicate, pt);
                K12 = Weingarten2(coef);
                return K12;
            }

            public static List<double> Curvature3(List<Point3d> pts, Point3d pt, Point3d nor) {
                List<double> K12;

                if (pts.Count < 7) {
                    Rhino.RhinoApp.WriteLine("Insufficient points for the calculation of the fitting point in the" + pt.ToString());
                    K12 = new List<double>(2) { 0, 0 };
                    return K12;
                }

                Plane rc = new Plane();
                Plane.FitPlaneToPoints(pts, out rc);
                Plane fitp = new Plane();
                Plane.FitPlaneToPoints(pts, out fitp);
                fitp.Origin = pt;
                Plane world = Plane.WorldXY;
                Transform xform = Rhino.Geometry.Transform.ChangeBasis(world, fitp);

                K12 = new List<double>(2) { 0, 0 };
                nor.Transform(xform);

                List<Point3d> ptsDuplicate = new List<Point3d>();

                for (int i = 0; i < pts.Count; i++) {
                    /////////////////////////
                    Point3d pTemp = pts[i];
                    pTemp.Transform(xform);//"David Rutten" - This is the one major drawback of value-types over reference-types, and there's no way around it
                    pts[i] = pTemp;
                    ////////////////////////
                    if (nor[2] > 0) ptsDuplicate.Add(new Point3d(pts[i].X, -pts[i].Y, -pts[i].Z));
                    else ptsDuplicate.Add(new Point3d(pts[i].X, pts[i].Y, pts[i].Z));
                }

                RowVector coef = fittingF3(ptsDuplicate, pt);
                K12 = Weingarten0(coef);
                return K12;
            }

            public static List<double>[] CurvaM0(MeshTopologyVertexList topvertex, List<List<int>> connection, Mesh mesh) {
                int t = 0;
                List<double> Kmax = new List<double>();
                List<double> Kmin = new List<double>();

                foreach (List<int> i in connection) {
                    List<Point3f> points = new List<Point3f>();
                    foreach (int j in i) {
                        points.Add(topvertex[j]);
                    }
                    List<Point3d> pts = points.ConvertAll(Convert3fTo3d);
                    Point3d pt = new Point3d(topvertex[t].X, topvertex[t].Y, topvertex[t].Z);
                    int[] ind = topvertex.MeshVertexIndices(t);
                    Vector3d nn = mesh.Normals[ind[0]];
                    Point3d nor = new Point3d(nn.X + pt.X, nn.Y + pt.Y, nn.Z + pt.Z);

                    List<double> K = Curvature0(pts, pt, nor);
                    K.Sort();
                    Kmax.Add(K[1]);
                    Kmin.Add(K[0]);
                    t++;

                }
                return new List<double>[2] { Kmax, Kmin };
            }

            public static List<double>[] CurvaM1(MeshTopologyVertexList topvertex, List<List<int>> connection, Mesh mesh) {

                int t = 0;
                List<double> Kmax = new List<double>();
                List<double> Kmin = new List<double>();

                foreach (List<int> i in connection) {
                    List<Point3f> points = new List<Point3f>();
                    foreach (int j in i) {
                        points.Add(topvertex[j]);
                    }
                    List<Point3d> pts = points.ConvertAll(Convert3fTo3d);
                    Point3d pt = new Point3d(topvertex[t].X, topvertex[t].Y, topvertex[t].Z);
                    int[] ind = topvertex.MeshVertexIndices(t);
                    Vector3d nn = mesh.Normals[ind[0]];
                    Point3d nor = new Point3d(nn.X + pt.X, nn.Y + pt.Y, nn.Z + pt.Z);

                    List<double> K = Curvature1(pts, pt, nor);
                    K.Sort();
                    Kmax.Add(K[1]);
                    Kmin.Add(K[0]);
                    t++;

                }
                return new List<double>[2] { Kmax, Kmin };
            }


            public static List<double>[] CurvaM2(Rhino.Geometry.Collections.MeshTopologyVertexList topvertex, List<List<int>> connection, Mesh mesh) {

                int t = 0;
                List<double> Kmax = new List<double>();
                List<double> Kmin = new List<double>();

                foreach (List<int> i in connection) {

                    List<Point3f> points = new List<Point3f>();
                    foreach (int j in i) {
                        points.Add(topvertex[j]);
                    }

                    List<Point3d> pts = new List<Point3d>();
                    foreach (Point3f p in points) {
                        pts.Add(new Point3d(p.X, p.Y, p.Z));
                    }

                    Point3d pt = new Point3d(topvertex[t].X, topvertex[t].Y, topvertex[t].Z);
                    int[] ind = topvertex.MeshVertexIndices(t);
                    Vector3d nn = mesh.Normals[ind[0]];
                    Point3d nor = new Point3d(nn.X + pt.X, nn.Y + pt.Y, nn.Z + pt.Z);
                    List<double> K = Curvature2(pts, pt, nor);
                    K.Sort();
                    Kmax.Add(K[1]);
                    Kmin.Add(K[0]);
                    t++;

                }
                return new List<double>[2] { Kmax, Kmin };
            }

            public static List<double>[] CurvaM3(MeshTopologyVertexList topvertex, List<List<int>> connection, Mesh mesh) {

                int t = 0;
                List<double> Kmax = new List<double>();
                List<double> Kmin = new List<double>();

                foreach (List<int> i in connection) {
                    List<Point3f> points = new List<Point3f>();
                    foreach (int j in i) {
                        points.Add(topvertex[j]);
                    }
                    List<Point3d> pts = points.ConvertAll(Convert3fTo3d);
                    Point3d pt = new Point3d(topvertex[t].X, topvertex[t].Y, topvertex[t].Z);
                    int[] ind = topvertex.MeshVertexIndices(t);
                    Vector3d nn = mesh.Normals[ind[0]];
                    Point3d nor = new Point3d(nn.X + pt.X, nn.Y + pt.Y, nn.Z + pt.Z);

                    List<double> K = Curvature3(pts, pt, nor);
                    K.Sort();
                    Kmax.Add(K[1]);
                    Kmin.Add(K[0]);
                    t++;

                }
                return new List<double>[2] { Kmax, Kmin };
            }



            public static Mesh_Cuvature_Utilities_Debugging MainK(bool d1, bool d2, int d3, double d4, int d5, int curvatureType, Mesh mesh, Rhino.Geometry.Collections.MeshTopologyVertexList topvertex) {
                topvertex.SortEdges();
                List<List<int>> connection = new List<List<int>>();
                List<double> Kmax = new List<double>();
                List<double> Kmin = new List<double>();
                List<double>[] values = new List<double>[2];

                if (d1 == true) { connection = RingConnection(topvertex, d3); }
                if (d2 == true) { connection = SphereConnection(topvertex, d4); }

                if (d5 == 0) {
                    Kmax = CurvaM0(topvertex, connection, mesh)[0];
                    Kmin = CurvaM0(topvertex, connection, mesh)[1];
                }

                if (d5 == 1) {
                    Kmax = CurvaM1(topvertex, connection, mesh)[0];
                    Kmin = CurvaM1(topvertex, connection, mesh)[1];
                }


                if (d5 == 2) {
                    Kmax = CurvaM2(topvertex, connection, mesh)[0];
                    Kmin = CurvaM2(topvertex, connection, mesh)[1];
                }

                if (d5 == 3) {
                    Kmax = CurvaM3(topvertex, connection, mesh)[0];
                    Kmin = CurvaM3(topvertex, connection, mesh)[1];
                }

                List<List<double>> KT = new List<List<double>>();
                List<double> Kmed = new List<double>();

                for (int i = 0; i < Kmax.Count; i++) {
                    List<double> Kall = new List<double>() { Kmax[i], Kmin[i], Kmin[i] * Kmax[i], (Kmin[i] + Kmax[i]) / 2, Math.Abs(Kmin[i]) + Math.Abs(Kmax[i]), Kmax[i] - Kmin[i], Math.Sqrt(Math.Pow(Kmin[i], 2) + Math.Pow(Kmax[i], 2)) };
                    KT.Add(Kall);
                    Kmed.Add(Kall[curvatureType]);
                }
                return new Mesh_Cuvature_Utilities_Debugging(KT, Kmed);
            }


            //-----------------Convert-----------------//
            public static Point3d Convert3fTo3d(Point3f point) {
                return new Point3d(point.X, point.Y, point.Z);
            }
        }











    }
}
