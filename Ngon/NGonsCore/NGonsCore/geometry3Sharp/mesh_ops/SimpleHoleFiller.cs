using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.mesh_ops
{
    public class SimpleHoleFiller
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
        public EdgeLoop Loop;

        public int NewVertex;
        public int[] NewTriangles;


        public SimpleHoleFiller(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, EdgeLoop loop)
        {
            Mesh = mesh;
            Loop = loop;

            NewVertex = NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID;
            NewTriangles = null;
        }



        public virtual ValidationStatus Validate()
        {
            ValidationStatus loopStatus = MeshValidation.IsBoundaryLoop(Mesh, Loop);
            return loopStatus;
        }


        public virtual bool Fill(int group_id = -1)
        {
            if (Loop.Vertices.Length < 3)
                return false;

            // this just needs one triangle
            if ( Loop.Vertices.Length == 3 ) {
                Index3i tri = new Index3i(Loop.Vertices[0], Loop.Vertices[2], Loop.Vertices[1]);
                int new_tid = Mesh.AppendTriangle(tri, group_id);
                if (new_tid == NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID)
                    return false;
                NewTriangles = new int[1] { new_tid };
                NewVertex = NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID;
                return true;
            }

            // [TODO] 4-case? could check nbr normals to figure out best internal edge...


            // compute centroid
            Vector3D c = Vector3D.Zero;
            for (int i = 0; i < Loop.Vertices.Length; ++i)
                c += Mesh.GetVertex(Loop.Vertices[i]);
            c *= 1.0 / Loop.Vertices.Length;

            // add centroid vtx
            NewVertex = Mesh.AppendVertex(c);

            // stitch triangles
            MeshEditor editor = new MeshEditor(Mesh);
            try {
                NewTriangles = editor.AddTriangleFan_OrderedVertexLoop(NewVertex, Loop.Vertices, group_id);
            } catch {
                NewTriangles = null;
            }

            // if fill failed, back out vertex-add
            if ( NewTriangles == null ) {
                Mesh.RemoveVertex(NewVertex);
                NewVertex = NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID;
            }

            return true;

        }


    }
}
