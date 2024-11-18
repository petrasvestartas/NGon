using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore
{
    public static class Morph
    {

        public static List<Point3d> MorphPoints(List<Point3d> point3ds, List<Polyline> twoSquares0, List<Polyline> twoSquares1)
        {

            //Mesh m0 = new Mesh();

            Point3d pCenter0 = (twoSquares0[0][0] + twoSquares0[1][2]) * 0.5;
            Point3d pCenter1 = (twoSquares1[0][0] + twoSquares1[1][2]) * 0.5;



            Mesh m0 = MeshUtil.LoftMeshFast(twoSquares0[0], twoSquares0[1], true);
            Mesh m1 = MeshUtil.LoftMeshFast(twoSquares1[0], twoSquares1[1], true);
            m0.Transform(Transform.Scale(pCenter0, 2));
            m1.Transform(Transform.Scale(pCenter1, 2));
            //m0 = m0.WeldUsingRTree(0.01);
            //m1 = m1.WeldUsingRTree(0.01);
            double[][] weights = null;
            twoSquares0[0].Bake();
            //m0.Bake();
            //m1.Bake();




            bool calculateWeights = true;
            m0 = SymmetricTriangulate(m0);
            m1 = SymmetricTriangulate(m1);
            int count = m0.Vertices.Count;



            if (calculateWeights)
            {
                weights = new double[point3ds.Count][];
                Point3d[] point3dArray1 = m0.Vertices.ToPoint3dArray();

                ////////////////////////////////////////////////////////
                //Paralell For
                ////////////////////////////////////////////////////////
                Parallel.For(0, point3ds.Count, (int i) => {

                    weights[i] = new double[count];
                    double num = 0;
                    bool flag = false;

                    foreach (MeshFace face in m0.Faces)
                    {
                        Point3d[] point3dArray = new Point3d[] { point3dArray1[face.A], point3dArray1[face.B], point3dArray1[face.C] };
                        if (Math.Abs((new Plane(point3dArray[0], point3dArray[1], point3dArray[2])).DistanceTo(point3ds[i])) >= 0.0001)
                        {
                            if (flag)
                                continue;

                            Vector3d[] item = new Vector3d[3];
                            Vector3d[] vector3dArray = new Vector3d[3];
                            Vector3d vector3d = new Vector3d();

                            for (int i1 = 0; i1 < 3; i1++)
                                item[i1] = point3dArray[i1] - point3ds[i];


                            bool flag1 = false;

                            for (int j = 0; j < 3; j++)
                            {
                                vector3dArray[j] = Vector3d.CrossProduct(item[j], point3dArray[(j + 1) % 3] - point3dArray[j]);
                                if (vector3dArray[j].IsTiny(0.0001))
                                    flag1 = true;

                                vector3dArray[j].Unitize();
                                vector3d += (0.5 * Vector3d.VectorAngle(item[j], item[(j + 1) % 3]) * vector3dArray[j]);
                            }

                            if (flag1)
                                continue;

                            for (int k = 0; k < 3; k++)
                            {
                                double num1 = (vector3dArray[k] * vector3d) / (vector3dArray[k] * item[(k + 2) % 3]);
                                weights[i][face[(k + 2) % 3]] += num1;
                                num += num1;
                            }
                        }
                        else
                        {
                            Vector3d vector3d1 = point3dArray[1] - point3dArray[0];
                            Vector3d vector3d2 = point3dArray[2] - point3dArray[0];
                            Vector3d item1 = point3ds[i] - point3dArray[0];
                            double num2 = vector3d1 * vector3d1;
                            double num3 = vector3d1 * vector3d2;
                            double num4 = vector3d2 * vector3d2;
                            double num5 = item1 * vector3d1;
                            double num6 = item1 * vector3d2;
                            double num7 = num2 * num4 - num3 * num3;
                            double num8 = (num4 * num5 - num3 * num6) / num7;
                            double num9 = (num2 * num6 - num3 * num5) / num7;
                            double num10 = 1 - num9 - num8;
                            if (num10 <= 0 || num8 <= 0 || num9 <= 0)
                            {
                                continue;
                            }
                            weights[i] = new double[count];
                            weights[i][face[0]] = num10;
                            weights[i][face[1]] = num8;
                            weights[i][face[2]] = num9;
                            num = 1;
                            flag = true;
                        }
                    }

                    for (int l = 0; l < count; l++)
                        weights[i][l] /= num;

                });//parallel for loop
                ////////////////////////////////////////////////////////
            }


            if (weights != null)
            {
                Point3d[] point3dArray2 = new Point3d[point3ds.Count];
                Point3d[] point3dArray3 = m1.Vertices.ToPoint3dArray();

                Parallel.For(0, point3ds.Count, (int i) => {
                    for (int num = 0; num < (int)point3dArray3.Length; num++)
                    {
                        ref Point3d p = ref point3dArray2[i];
                        p += (point3dArray3[num] * weights[i][num]);
                    }
                });

                return point3dArray2.ToList();


            }
            return new List<Point3d>();
        }




        public static List<Polyline> MorphPolylines(List<Polyline> plines, List<Polyline> twoSquares0, List<Polyline> twoSquares1)
        {

            ////////////////////////////////////////////////////
            //Convert polylines to points
            ////////////////////////////////////////////////////
            List<Point3d> point3ds = new List<Point3d>();
            List<int> point3dsBreak = new List<int>();
           
            int c = 0;
            foreach (var pline in plines)
                for(int i = 0; i < pline.Count; i++)
                {
                    point3ds.Add(pline[i]);
                    if(i == pline.Count-1) 
                        point3dsBreak.Add(-c);
                    else
                        point3dsBreak.Add(c);
                    c++;
                }
            ////////////////////////////////////////////////////


            var point3dArray2 = MorphPoints(point3ds, twoSquares0, twoSquares1);



                ////////////////////////////////////////////////////
                //Convert points to plines
                ////////////////////////////////////////////////////
                List<Polyline> plinesDistorted = new List<Polyline>();
                //List<int> point3dsBreak = new List<int>();

                Polyline plineEmpty = new Polyline();
                for(int i = 0; i < point3dArray2.Count; i++)
                {
                    plineEmpty.Add(point3dArray2[i]);

                    if (point3dsBreak[i] < 0)
                    {
                        plinesDistorted.Add(plineEmpty);
                        plineEmpty = new Polyline();
                    }

                }

                ////////////////////////////////////////////////////

                return plinesDistorted;
           
            return new List<Polyline>();
        }




        private static Mesh SymmetricTriangulate(Mesh M)
        {
            if (M.Faces.Count == M.Faces.TriangleCount)
            {
                return M;
            }
            Mesh m0 = new Mesh();
            m0.Vertices.AddVertices(M.Vertices);
            m0.Faces.AddFaces(M.Faces);
            List<MeshFace> meshFaces = new List<MeshFace>();
            List<int> nums = new List<int>();
            for (int i = 0; i < M.Faces.Count; i++)
            {
                if (M.Faces[i].IsQuad)
                {
                    nums.Add(i);
                    int num = m0.Vertices.Add(M.Faces.GetFaceCenter(i));
                    MeshFace item = M.Faces[i];
                    int a = item.A;
                    item = M.Faces[i];
                    meshFaces.Add(new MeshFace(a, item.B, num));
                    item = M.Faces[i];
                    int b = item.B;
                    item = M.Faces[i];
                    meshFaces.Add(new MeshFace(b, item.C, num));
                    item = M.Faces[i];
                    int c = item.C;
                    item = M.Faces[i];
                    meshFaces.Add(new MeshFace(c, item.D, num));
                    item = M.Faces[i];
                    int d = item.D;
                    item = M.Faces[i];
                    meshFaces.Add(new MeshFace(d, item.A, num));
                }
            }
            m0.Faces.AddFaces(meshFaces);
            m0.Faces.DeleteFaces(nums);
            return m0;
        }




    }
}
