using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore {
    public static class TriangularMeshUtil {


        /// <summary>
        /// vertex to vertex connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int[][] VV(this Mesh mesh) {

            int[][] vvSorted = new int[mesh.TopologyVertices.Count][];

            for(int i = 0; i < mesh.TopologyVertices.Count; i++) 
                vvSorted[i] = mesh.TopologyVertices.ConnectedTopologyVertices(i, true);

            return vvSorted;
        }

        /// <summary>
        /// vertex to edges connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int[][] VE(this Mesh mesh) {

            int[][] veSorted = new int[mesh.TopologyVertices.Count][];
            //mesh.TopologyVertices.SortEdges();
            for (int i = 0; i < mesh.TopologyVertices.Count; i++) {

                veSorted[i] = new int[mesh.TopologyVertices.ConnectedEdgesCount(i)];
                mesh.TopologyVertices.SortEdges(i);

                for (int j = 0; j < mesh.TopologyVertices.ConnectedEdgesCount(i); j++) {
                   
                    veSorted[i][j] = mesh.TopologyVertices.ConnectedEdge(i, j);
                }
            }

            return veSorted;
        }


        /// <summary>
        /// vertex to faces connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int[][] VF(this Mesh mesh) {

            int[][] vfSorted = new int[mesh.TopologyVertices.Count][];
            int[][] veSorted = mesh.VE();

            for (int i = 0; i < mesh.TopologyVertices.Count; i++) {

                vfSorted[i] = new int[veSorted[i].Length];
                mesh.TopologyVertices.SortEdges(i);

                for (int j = 0; j < veSorted[i].Length; j++) {

                    int[] f = mesh.TopologyEdges.GetConnectedFaces(veSorted[i][j]);

                    if (j == 0)
                        vfSorted[i][j] = f[0];
                    else if (j > 0 && f.Length == 1)
                        vfSorted[i][j] = f[0];
                    else
                        vfSorted[i][j] = (f[0] == vfSorted[i][j - 1]) ? f[1] : f[0];
                }//for i
            }//for j

            return vfSorted;
        }

        /// <summary>
        /// Geometry - vertex to vertex connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Point3d[][] VV_Pt(this Mesh mesh) {

            Point3d[][] vvSorted_Pt = new Point3d[mesh.TopologyVertices.Count][];
            int[][] vvSorted = mesh.VV();

            for (int i = 0; i < mesh.TopologyVertices.Count; i++) {

                vvSorted_Pt[i] = new Point3d[vvSorted[i].Length];

                for (int j = 0; j < vvSorted[i].Length; j++)
                    vvSorted_Pt[i][j] = mesh.TopologyVertices[vvSorted[i][j]];

        }

            return vvSorted_Pt;
        }

        /// <summary>
        /// Geometry - vertex to edges connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Line[][] VE_Ln(this Mesh mesh) {

            Line[][] veSorted_Ln = new Line[mesh.TopologyVertices.Count][];
            int[][] veSorted = mesh.VE();

            for (int i = 0; i < mesh.TopologyVertices.Count; i++) {

                veSorted_Ln[i] = new Line[veSorted[i].Length];

                for (int j = 0; j < veSorted[i].Length; j++) {

                    Rhino.IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(veSorted[i][j]);
                    int a = -1;
                    int b = -1;
                    if (pair.I == i) {
                        a = pair.I;
                        b = pair.J;
                    } else {
                        b = pair.I;
                        a = pair.J;
                    }
                    veSorted_Ln[i][j] = new Line(mesh.TopologyVertices[a], mesh.TopologyVertices[b]);

                }
            }

            return veSorted_Ln;
        }


        /// <summary>
        /// Geometry - vertex to faces connectivity
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Polyline[][] VF_Po(this Mesh mesh) {

            Polyline[][] vfSorted_Po = new Polyline[mesh.TopologyVertices.Count][];
            int[][] vfSorted = mesh.VF();

            for (int i = 0; i < mesh.TopologyVertices.Count; i++) {

                vfSorted_Po[i] = new Polyline[vfSorted[i].Length];

                for (int j = 0; j < vfSorted[i].Length; j++) {

                    
                    MeshFace face = mesh.Faces[vfSorted[i][j]];
                    if(face.IsQuad)
                        vfSorted_Po[i][j] = new Polyline() {
                            mesh.Vertices[face.A],
                            mesh.Vertices[face.B],
                            mesh.Vertices[face.C],
                            mesh.Vertices[face.D],
                            mesh.Vertices[face.A]
                        };
                    else
                        vfSorted_Po[i][j] = new Polyline() {
                            mesh.Vertices[face.A],
                            mesh.Vertices[face.B],
                            mesh.Vertices[face.C],
                            mesh.Vertices[face.A]
                        };




                }//for i
            }//for j

            return vfSorted_Po;
        }

        /// <summary>
        /// Geometry - topology vertices
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Point3d[] V_Pt(this Mesh mesh) {

            Point3d[] points = new Point3d[mesh.TopologyVertices.Count];

            for (int i = 0; i < mesh.TopologyVertices.Count; i++)
                points[i] = mesh.TopologyVertices[i];


            return points;
        }








    }
}
