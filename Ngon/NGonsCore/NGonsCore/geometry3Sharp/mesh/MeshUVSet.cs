using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
	// 
	// Standalone UV mesh
	//   (mainly we are using this as a UV layer for an existing 3D Mesh, so the assumption
	//    is that TriangleUVs has the same # of triangles as that mesh...)
    public class DenseUVMesh
    {
        public DVector<Vector2F> UVs;
        public DVector<Index3i> TriangleUVs;

        public DenseUVMesh()
        {
            UVs = new DVector<Vector2F>();
            TriangleUVs = new DVector<Index3i>();
        }

        public int AppendUV(Vector2F uv)
        {
            int id = UVs.Length;
            UVs.Add(uv);
            return id;
        }

    }
}
