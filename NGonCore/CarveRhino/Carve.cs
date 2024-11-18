//using CarveSharp;
//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NGonCore {

//    public enum CarveOperations : int{

//        Union = 0,
//        Intersection = 1,
//        AMinusB = 2,
//        BMinusA = 3,
//        SymmetricDifferent = 4,
//        All = 5,

//    }
//    public static class CarveExtensionMethods {
//        public static CarveSharp.CarveMesh MeshToCarve(this Mesh m) {
//            double[] verts = new double[m.Vertices.Count * 3];
//            int[] facesizes = new int[m.Faces.Count];
//            List<int> faces = new List<int>();

//            int j = 0;
//            for (int i = 0; i < m.Vertices.Count; ++i) {
//                verts[j] = m.Vertices[i].X; ++j;
//                verts[j] = m.Vertices[i].Y; ++j;
//                verts[j] = m.Vertices[i].Z; ++j;
//            }

//            for (int i = 0; i < m.Faces.Count; ++i) {
//                faces.Add(m.Faces[i].A);
//                faces.Add(m.Faces[i].B);
//                faces.Add(m.Faces[i].C);
//                if (m.Faces[i].IsQuad) {
//                    facesizes[i] = 4;
//                    faces.Add(m.Faces[i].D);
//                } else
//                    facesizes[i] = 3;
//            }

//            CarveSharp.CarveMesh cm = new CarveSharp.CarveMesh();
//            cm.Vertices = verts;
//            cm.FaceIndices = faces.ToArray();
//            cm.FaceSizes = facesizes;

//            return cm;
//        }

//        public static Mesh CarveToMesh(this CarveSharp.CarveMesh cm) {
//            Mesh m = new Mesh();
//            for (int i = 0; i < cm.Vertices.Length; i += 3) {
//                m.Vertices.Add(new Rhino.Geometry.Point3d(
//                  cm.Vertices[i],
//                  cm.Vertices[i + 1],
//                  cm.Vertices[i + 2]));
//            }

//            int j = 0;
//            for (int i = 0; i < cm.FaceSizes.Length; ++i) {
//                if (cm.FaceSizes[i] == 3) {
//                    m.Faces.AddFace(
//                      cm.FaceIndices[j],
//                      cm.FaceIndices[j + 1],
//                      cm.FaceIndices[j + 2]);
//                } else if (cm.FaceSizes[i] == 4) {
//                    m.Faces.AddFace(
//                      cm.FaceIndices[j],
//                      cm.FaceIndices[j + 1],
//                      cm.FaceIndices[j + 2],
//                      cm.FaceIndices[j + 3]);
//                }
//                j += cm.FaceSizes[i];
//            }
//            return m;
//        }

//        public static void MeshToLists(this Mesh m, out double[] verts, out int[] faces) {
//            verts = new double[m.Vertices.Count * 3];
//            faces = new int[m.Faces.Count * 3];

//            int j = 0;
//            for (int i = 0; i < m.Vertices.Count; ++i) {
//                verts[j] = m.Vertices[i].X; ++j;
//                verts[j] = m.Vertices[i].Y; ++j;
//                verts[j] = m.Vertices[i].Z; ++j;
//            }

//            j = 0;
//            for (int i = 0; i < m.Faces.Count; ++i) {
//                faces[j] = m.Faces[i].A; ++j;
//                faces[j] = m.Faces[i].B; ++j;
//                faces[j] = m.Faces[i].C; ++j;
//            }
//        }
//    }

//   public static class CarveLib {
//        /// <summary>
//        /// Make boolean difference between two meshes using the CarveSharp library.
//        /// </summary>
//        /// <param name="mA">Mesh to subtract from.</param>
//        /// <param name="mB">Mesh to subtract.</param>
//        /// <returns>Mesh difference between MeshA and MeshB.</returns>
//        public static Mesh Carve(Mesh mA, Mesh mB, CarveSharp.CarveSharp.CSGOperations Operation) {
//            if (mA == null || mB == null) return null;
//            mA.Weld(Math.PI);
//            mB.Weld(Math.PI);

//            //MeshA.Faces.ConvertQuadsToTriangles();
//            //MeshB.Faces.ConvertQuadsToTriangles();

//            var cmA = mA.MeshToCarve();
//            var cmB = mB.MeshToCarve();

//            var tmp = CarveSharp.CarveSharp.PerformCSG(cmA, cmB, Operation);

//            return tmp.CarveToMesh();
//        }

//        public static Mesh CarveNGon(Mesh MeshA, IEnumerable<Mesh> MeshesB, CarveOperations Operation) {
//            return Carve(MeshA, MeshesB, (CarveSharp.CarveSharp.CSGOperations)((int)Operation));
//        }

//        public static Mesh CarveNGon(Mesh MeshA, Mesh MeshesB, CarveOperations Operation) {

            
//            Mesh result =    Carve(MeshA, MeshesB, (CarveSharp.CarveSharp.CSGOperations)((int)Operation));
            
//            //result.WeldUsingRTree(0.01, false, false);
//            result.RebuildNormals();
//            result.Unweld(0,true);
//            return result;
//        }

//        /// <summary>
//        /// Make boolean difference between a mesh and a 
//        /// collection of meshes using the CarveSharp library.
//        /// </summary>
//        /// <param name="MeshA">Mesh to subtract from.</param>
//        /// <param name="Meshes">Meshes to subtract.</param>
//        /// <returns>Mesh difference.</returns>
//        public static Mesh Carve(Mesh MeshA, IEnumerable<Mesh> Meshes, CarveSharp.CarveSharp.CSGOperations Operation) {
//            MeshA.Weld(3.14159265358979);
//            MeshA.Faces.ConvertQuadsToTriangles();
//            var tmp = MeshA.MeshToCarve();

         

//            foreach (Mesh m in Meshes) {
//                if (m == null) continue;
//                m.Weld(3.14159265358979);
//                m.Faces.ConvertQuadsToTriangles();
//                var mm = m.MeshToCarve();
//                try {
//                    tmp = CarveSharp.CarveSharp.PerformCSG(tmp, mm, Operation);
//                } catch (Exception e) {

//                } finally {

//                }
//            }

//            return  tmp.CarveToMesh();
//        }




//            /// <summary>
//            /// Make boolean difference between two collections 
//            /// of meshes using the CarveSharp library.
//            /// </summary>
//            /// <param name="MeshesA">Meshes A.</param>
//            /// <param name="MeshesB">Meshes B.</param>
//            /// <param name="Operation">Carve operation to perform.</param>
//            /// <returns>Mesh difference.</returns>
//            public static List<Mesh> Carve(IEnumerable<Mesh> MeshesA, IEnumerable<Mesh> MeshesB, CarveSharp.CarveSharp.CSGOperations Operation) {

    



//            List<Mesh> OutMeshes = new List<Mesh>();
//            foreach (Mesh mA in MeshesA) {
//                if (mA == null) continue;
//                mA.Weld(3.14);
//                //mA.Faces.ConvertQuadsToTriangles();
//                CarveMesh temp = mA.MeshToCarve();

//                foreach (Mesh m in MeshesB) {
//                    if (m == null) continue;
//                    m.Weld(3.14);
//                    //m.Faces.ConvertQuadsToTriangles();
//                    var cm = m.MeshToCarve();
//                    temp = CarveSharp.CarveSharp.PerformCSG(temp, cm, Operation);
//                }

//                if (temp != null)
//                    OutMeshes.Add(temp.CarveToMesh());
//            }
//            return OutMeshes;
//        }
//    }
//}
