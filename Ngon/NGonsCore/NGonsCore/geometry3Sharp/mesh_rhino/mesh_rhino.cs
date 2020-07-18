using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using NGonsCore.geometry3Sharp;
using NGonsCore.geometry3Sharp.mesh;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh_rhino {
    public static class mesh_rhino {

        public static mesh.DMesh3 ToDMesh3(this Mesh mesh) {
            var dMesh3 = new DMesh3();

            for (int i = 0; i < mesh.Vertices.Count; i++) {
                var vertex = mesh.Vertices[i];
                var normal = mesh.Normals[i];

                NewVertexInfo ni = new NewVertexInfo() {
                    v = new Vector3D(vertex.X, vertex.Z, vertex.Y),
                    n = new Vector3F(normal.X, normal.Z, normal.Y)
                };

                dMesh3.AppendVertex(ni);
            }

            foreach (var face in mesh.Faces) {
                dMesh3.AppendTriangle(face.A, face.B, face.C);
            }

            return dMesh3;
        }

        public static Mesh ToRhinoMesh(this DMesh3 dMesh3) {
            dMesh3 = new DMesh3(dMesh3, true, MeshComponents.All);
            var mesh = new Mesh();

            var vertices = dMesh3.Vertices().Select(v => new Point3d(v.x, v.z, v.y));
            var faces = dMesh3.Triangles().Select(f => new MeshFace(f.a, f.b, f.c));

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);
            mesh.Normals.ComputeNormals();
            mesh.Compact();

            return mesh;
        }

    }
}
