using System;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.mesh_ops
{
    public class MeshExtrusion
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
        public EdgeLoop Loop;

        // arguments

        // set new position based on original loop vertex position, normal, and index
        public Func<Vector3D, Vector3F, int, Vector3D> PositionF;

        // outputs

        public int[] NewTriangles;
        public EdgeLoop NewLoop;


        public MeshExtrusion(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, EdgeLoop loop)
        {
            Mesh = mesh;
            Loop = loop;

            PositionF = (pos, normal, idx) => {
                return pos + Vector3D.AxisY;
            };
        }


        public virtual ValidationStatus Validate()
        {
            ValidationStatus loopStatus = MeshValidation.IsBoundaryLoop(Mesh, Loop);
            return loopStatus;
        }


        public virtual bool Extrude(int group_id = -1)
        {
            // duplicate loop vertices
            int NV = Loop.Vertices.Length;
            NewLoop = new EdgeLoop(Mesh);
            NewLoop.Vertices = new int[NV];

            for ( int i = 0; i < NV; ++i ) {
                int vid = Loop.Vertices[i];
                NewLoop.Vertices[i] = Mesh.AppendVertex(Mesh, vid);
            }

            // move to offset positions
            for ( int i = 0; i < NV; ++i ) {
                Vector3D v = Mesh.GetVertex(Loop.Vertices[i]);
                Vector3F n = Mesh.GetVertexNormal(Loop.Vertices[i]);
                Vector3D new_v = PositionF(v, n, i);
                Mesh.SetVertex(NewLoop.Vertices[i], new_v);
            }

            // stitch interior
            MeshEditor edit = new MeshEditor(Mesh);
            NewTriangles = edit.StitchLoop(Loop.Vertices, NewLoop.Vertices, group_id);

            return true;
        }


    }
}
