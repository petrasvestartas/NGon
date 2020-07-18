using System;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public class MeshNormals
    {
        public IMesh Mesh;
        public DVector<Vector3D> Normals;

        /// <summary>
        /// By default this is Mesh.GetVertex(). Can override to provide
        /// alternate vertex source.
        /// </summary>
        public Func<int, Vector3D> VertexF;



        public enum NormalsTypes
        {
            Vertex_OneRingFaceAverage_AreaWeighted
        }
        public NormalsTypes NormalType;


        public MeshNormals(IMesh mesh, NormalsTypes eType = NormalsTypes.Vertex_OneRingFaceAverage_AreaWeighted)
        {
            Mesh = mesh;
            NormalType = eType;
            Normals = new DVector<Vector3D>();
            VertexF = Mesh.GetVertex;
        }


        public void Compute()
        {
            Compute_FaceAvg_AreaWeighted();
        }


        public void CopyTo(NGonsCore.geometry3Sharp.mesh.DMesh3 SetMesh)
        {
            if (SetMesh.MaxVertexID < Mesh.MaxVertexID)
                throw new Exception("MeshNormals.Set: SetMesh does not have enough vertices!");
            if (!SetMesh.HasVertexNormals)
                SetMesh.EnableVertexNormals(Vector3F.AxisY);
            int NV = Mesh.MaxVertexID;
            for ( int vi = 0; vi < NV; ++vi ) {
                if ( Mesh.IsVertex(vi) && SetMesh.IsVertex(vi) ) {
                    SetMesh.SetVertexNormal(vi, (Vector3F)Normals[vi]);
                }
            }
        }




        // TODO: parallel version, cache tri normals
        void Compute_FaceAvg_AreaWeighted()
        {
            int NV = Mesh.MaxVertexID;
            if ( NV != Normals.Size) 
                Normals.resize(NV);
            for (int i = 0; i < NV; ++i)
                Normals[i] = Vector3D.Zero;

            int NT = Mesh.MaxTriangleID;
            for (int ti = 0; ti < NT; ++ti) {
                if (Mesh.IsTriangle(ti) == false)
                    continue;
                Index3i tri = Mesh.GetTriangle(ti);
                Vector3D va = Mesh.GetVertex(tri.a);
                Vector3D vb = Mesh.GetVertex(tri.b);
                Vector3D vc = Mesh.GetVertex(tri.c);
                Vector3D N = math.MathUtil.Normal(va, vb, vc);
                double a = math.MathUtil.Area(va, vb, vc);
                Normals[tri.a] += a * N;
                Normals[tri.b] += a * N;
                Normals[tri.c] += a * N;
            }

            for ( int vi = 0; vi < NV; ++vi) {
                if (Normals[vi].LengthSquared > math.MathUtil.ZeroTolerancef)
                    Normals[vi].Normalize();
            }

        }




        public static void QuickCompute(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            MeshNormals normals = new MeshNormals(mesh);
            normals.Compute();
            normals.CopyTo(mesh);
        }



    }
}
