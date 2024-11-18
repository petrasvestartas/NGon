using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore {

    public static class Tetrahedron {
        public static Mesh Create(double r_ = 1) {


            Mesh mesh = new Mesh();

            float length = (float)r_;
            float width = (float)r_;
            float height = (float)r_;


            Point3f p0 = new Point3f(length * .5f, width * .5f, -height * .5f);//
            Point3f p1 = new Point3f(-length * .5f, -width * .5f, -height * .5f);//
            Point3f p2 = new Point3f(-length * .5f, width * .5f, height * .5f);
            Point3f p3 = new Point3f(length * .5f, -width * .5f, height * .5f);

            Point3f[] vertices = new Point3f[] { p0, p1, p2, p3 };

            MeshFace[] mf = new MeshFace[]
       {
           new MeshFace(1,0,3),
           new MeshFace(0,1,2),
           new MeshFace(2,3,0),
           new MeshFace(3,2,1),


       };


            mesh.Faces.AddFaces(mf);
            mesh.Vertices.AddVertices(vertices);
            mesh.RebuildNormals();
            mesh.Clean();


            return mesh;
        }
    }

    public static class Cube {
        public static Mesh Create( double r_ = 1) {

            Mesh mesh = new Mesh();



            float length = (float)r_;
            float width = (float)r_;
            float height = (float)r_;

            #region Vertices
            Point3f p0 = new Point3f(-length * .5f, -width * .5f, height * .5f);
            Point3f p1 = new Point3f(length * .5f, -width * .5f, height * .5f);
            Point3f p2 = new Point3f(length * .5f, -width * .5f, -height * .5f);
            Point3f p3 = new Point3f(-length * .5f, -width * .5f, -height * .5f);

            Point3f p4 = new Point3f(-length * .5f, width * .5f, height * .5f);
            Point3f p5 = new Point3f(length * .5f, width * .5f, height * .5f);
            Point3f p6 = new Point3f(length * .5f, width * .5f, -height * .5f);
            Point3f p7 = new Point3f(-length * .5f, width * .5f, -height * .5f);

            Point3f[] vertices = new Point3f[]
            {
	// Bottom
	p0, p1, p2, p3,
 
	// Left
	p7, p4, p0, p3,
 
	// Front
	p4, p5, p1, p0,
 
	// Back
	p6, p7, p3, p2,
 
	// Right
	p5, p6, p2, p1,
 
	// Top
	p7, p6, p5, p4
            };
            #endregion

            #region Triangles
            MeshFace[] mf = new MeshFace[]
            {
	// Bottom
	new MeshFace(3, 2,1, 0),
    //3, 2, 1,			
 
	// Left
	new MeshFace(3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1),
    //3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	// Front
	new MeshFace(3 + 4 * 2, 2 + 4 * 2,1 + 4 * 2, 0 + 4 * 2),
    //3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	// Back
	new MeshFace(3 + 4 * 3,2 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3),
    //3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	// Right
	new MeshFace(3 + 4 * 4, 2 + 4 * 4,1 + 4 * 4, 0 + 4 * 4),
    //3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	// Top
	new MeshFace(3 + 4 * 5,  2 + 4 * 5,1 + 4 * 5, 0 + 4 * 5)
    //3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

            };
            #endregion



            mesh.Faces.AddFaces(mf);


            mesh.Vertices.AddVertices(vertices);
   

            mesh.RebuildNormals();
            mesh.Clean();


            return mesh;
        }
    }
    public static class Torus {

        public static Mesh Create(int u_ = 24, int v_ = 18, double r0_ = 1, double r1_ = 0.3) {

            


            Mesh mesh = new Mesh();


            float radius1 = (float)r0_;
            float radius2 = (float)r1_;
            int nbRadSeg = Math.Max(3,u_);
            int nbSides = Math.Max(3, v_);

            #region Vertices		
            Point3f[] vertices = new Point3f[(nbRadSeg + 1) * (nbSides + 1)];
            float _2pi = (float)Math.PI * 2f;
            for (int seg = 0; seg <= nbRadSeg; seg++) {
                int currSeg = seg == nbRadSeg ? 0 : seg;

                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3f r1 = new Vector3f((float)Math.Cos(t1) * radius1, 0f, (float)Math.Sin(t1) * radius1);

                for (int side = 0; side <= nbSides; side++) {
                    int currSide = side == nbSides ? 0 : side;

                    Vector3f normale = Vector3f.CrossProduct(r1, Vector3f.YAxis);
                    float t2 = (float)currSide / nbSides * _2pi;
                    
                    Vector3f r2 = new Vector3f((float)Math.Sin(t2) * radius2, (float)Math.Cos(t2) * radius2,0);
                    Transform angleAxis = Transform.Rotation(-t1, Vector3f.YAxis, Point3d.Origin);
                    r2.Transform(angleAxis);

                    vertices[side + seg * (nbSides + 1)] = (r1 + r2).ToPoint3f();
                }
            }
            #endregion

            #region Normales		
            Vector3f[] normales = new Vector3f[vertices.Length];
            for (int seg = 0; seg <= nbRadSeg; seg++) {
                int currSeg = seg == nbRadSeg ? 0 : seg;

                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3f r1 = new Vector3f((float)Math.Cos(t1) * radius1, 0f, (float)Math.Sin(t1) * radius1);

                for (int side = 0; side <= nbSides; side++) {
                    normales[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)].ToVector3f() - r1).Unit();
                }
            }
            #endregion


            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];
            List<MeshFace> mf = new List<MeshFace>();

            int i = 0;
            for (int seg = 0; seg <= nbRadSeg-1; seg++) {
                for (int side = 0; side <= nbSides - 1; side++) {
                    int current = side + seg * (nbSides + 1);
                    int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

            

                    if (i < nbIndexes - 6) {
                        triangles[i++] = current;
                        triangles[i++] = next;
                        triangles[i++] = next + 1;

                        triangles[i++] = current;
                        triangles[i++] = next + 1;
                        triangles[i++] = current + 1;

                        //if (seg % 2 == 0) {

                        //    if (side % 2 == 0) {
                        //        mf.Add(new MeshFace(current, next, next + 1));
                        //        mf.Add(new MeshFace(current, next + 1, current + 1));
                            
                        //    } else {
                        //        mf.Add(new MeshFace(next, next + 1, current + 1));
                        //        mf.Add(new MeshFace(current + 1, current, next));
                        //    }
                        //} else {

                        //    if (side % 2 == 1) {
                        //        mf.Add(new MeshFace(current, next, next + 1));
                        //        mf.Add(new MeshFace(current, next + 1, current + 1));

                        //    } else {
                        //        mf.Add(new MeshFace(next, next + 1, current + 1));
                        //        mf.Add(new MeshFace(current + 1, current, next));
                        //    }
                        //}

                        mf.Add(new MeshFace(current, next, next + 1));
                        mf.Add(new MeshFace(current, next + 1, current + 1));

                        //Quads
                        //mf.Add(new MeshFace(current, next, next + 1, current + 1));
                    }
                }
            }
            #endregion
            mesh.Faces.AddFaces(mf);
            //i = 0;
            //for (i = 0; i < nbIndexes; i += 3) {
            //    //Rhino.RhinoApp.WriteLine(string.Format("{0},{1},{2}", triangles[i + 0], triangles[i + 1], triangles[i + 2]));
            //    mesh.Faces.AddFace(triangles[i + 0], triangles[i + 1], triangles[i + 2]);
            //}

            mesh.Vertices.AddVertices(vertices);
            mesh.Normals.AddRange( normales);

            //mesh = mesh.Diagonalize(out int[] array);


            mesh.RebuildNormals();
            mesh.Clean();

            mesh.Transform(Transform.PlaneToPlane(Plane.WorldZX,Plane.WorldXY));

            return mesh;
        }
   }

    public  static class IcoSphere {
        private struct TriangleIndices {
            public int v1;
            public int v2;
            public int v3;

            public TriangleIndices(int v1, int v2, int v3) {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        // return index of point in the middle of p1 and p2
        private static int getMiddlePoint(int p1, int p2, ref List<Point3f> vertices, ref Dictionary<long, int> cache, float radius) {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            int ret;
            if (cache.TryGetValue(key, out ret)) {
                return ret;
            }

            // not in cache, calculate it
            Point3f point1 = vertices[p1];
            Point3f point2 = vertices[p2];
            Point3f middle = new Point3f
            (
                (point1.X + point2.X) / 2f,
                (point1.Y + point2.Y) / 2f,
                (point1.Z + point2.Z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.Unit() * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }

        public static Mesh Create(int recursionLevel = 0, float radius = 1) {
            //MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            // mesh.Clear();

            List<Point3f> vertList = new List<Point3f>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
            int index = 0;

        

            // create 12 vertices of a icosahedron
            float t = (1f + (float)System.Math.Sqrt(5f)) / 2f;

            vertList.Add(new Point3f(-1f, t, 0f).Unit() * radius);
            vertList.Add(new Point3f(1f, t, 0f).Unit() * radius);
            vertList.Add(new Point3f(-1f, -t, 0f).Unit() * radius);
            vertList.Add(new Point3f(1f, -t, 0f).Unit() * radius);

            vertList.Add(new Point3f(0f, -1f, t).Unit() * radius);
            vertList.Add(new Point3f(0f, 1f, t).Unit() * radius);
            vertList.Add(new Point3f(0f, -1f, -t).Unit() * radius);
            vertList.Add(new Point3f(0f, 1f, -t).Unit() * radius);

            vertList.Add(new Point3f(t, 0f, -1f).Unit() * radius);
            vertList.Add(new Point3f(t, 0f, 1f).Unit() * radius);
            vertList.Add(new Point3f(-t, 0f, -1f).Unit() * radius);
            vertList.Add(new Point3f(-t, 0f, 1f).Unit() * radius);


            // create 20 triangles of the icosahedron
            List<TriangleIndices> faces = new List<TriangleIndices>();

            // 5 faces around point 0
            faces.Add(new TriangleIndices(0, 11, 5));
            faces.Add(new TriangleIndices(0, 5, 1));
            faces.Add(new TriangleIndices(0, 1, 7));
            faces.Add(new TriangleIndices(0, 7, 10));
            faces.Add(new TriangleIndices(0, 10, 11));

            // 5 adjacent faces 
            faces.Add(new TriangleIndices(1, 5, 9));
            faces.Add(new TriangleIndices(5, 11, 4));
            faces.Add(new TriangleIndices(11, 10, 2));
            faces.Add(new TriangleIndices(10, 7, 6));
            faces.Add(new TriangleIndices(7, 1, 8));

            // 5 faces around point 3
            faces.Add(new TriangleIndices(3, 9, 4));
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(3, 2, 6));
            faces.Add(new TriangleIndices(3, 6, 8));
            faces.Add(new TriangleIndices(3, 8, 9));

            // 5 adjacent faces 
            faces.Add(new TriangleIndices(4, 9, 5));
            faces.Add(new TriangleIndices(2, 4, 11));
            faces.Add(new TriangleIndices(6, 2, 10));
            faces.Add(new TriangleIndices(8, 6, 7));
            faces.Add(new TriangleIndices(9, 8, 1));


            // refine triangles
            for (int i = 0; i < recursionLevel; i++) {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (var tri in faces) {
                    // replace triangle by 4 triangles
                    int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                    int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                    int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            mesh.Vertices.AddVertices(vertList);

            List<MeshFace> triList = new List<MeshFace>();
            for (int i = 0; i < faces.Count; i++) {
                triList.Add(new MeshFace(faces[i].v1, faces[i].v2, faces[i].v3));
            }
            mesh.Faces.AddFaces(triList);
            //mesh.uv = new Vector2[vertices.Length];

            Vector3f[] normales = new Vector3f[vertList.Count];
            for (int i = 0; i < normales.Length; i++)
                normales[i] = vertList[i].Unit().ToVector3f();


            mesh.Normals.AddRange(normales);

            Plane plane = new Plane(Point3d.Origin, new Vector3d(0, -0.5257311031, -0.8506508139));
            mesh.Transform(Transform.PlaneToPlane(plane, Plane.WorldXY));



            mesh.RebuildNormals();
            mesh.Clean();

            return mesh;
            //mesh.RecalculateBounds();
            //mesh.Optimize();
        }
    }
}

