using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public static class Analysis {

        /// <summary>
        /// Loops trough mesh edges, get adjacent faces, computers angle between face normals, and add edges as line if it matches the value
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static List<Line> MeshEdgesByAngle(this Mesh mesh, double d = 0.49) {


            //Display Naked edges
            List<Line> lines = new List<Line>();

            mesh.FaceNormals.ComputeFaceNormals();
            var te = mesh.TopologyEdges;

            for (int i = 0; i < mesh.TopologyEdges.Count; i++) {
                int[] f = mesh.TopologyEdges.GetConnectedFaces(i);
                if (f.Length == 2) {
                    double angle = Vector3d.VectorAngle(mesh.FaceNormals[f[0]], mesh.FaceNormals[f[1]]);

                    if (angle > (0.5 - d) * 3.14159265359 && angle < (0.5 + d) * 3.14159265359)
                        lines.Add(te.EdgeLine(i));
                }

            }


            return lines;
        }

    }
}
