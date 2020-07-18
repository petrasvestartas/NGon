using NGonsCore.geometry3Sharp.io;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public interface IPointSet
    {
        int VertexCount { get; }
		int MaxVertexID { get; }

        bool HasVertexNormals { get; }
        bool HasVertexColors { get; }

        Vector3D GetVertex(int i);
        Vector3F GetVertexNormal(int i);
        Vector3F GetVertexColor(int i);

        bool IsVertex(int vID);

        // iterators allow us to work with gaps in index space
        System.Collections.Generic.IEnumerable<int> VertexIndices();
    }



    public interface IMesh : IPointSet
    {
        int TriangleCount { get; }
		int MaxTriangleID { get; }

        bool HasVertexUVs { get; }
        Vector2F GetVertexUV(int i);

        NewVertexInfo GetVertexAll(int i);

        bool HasTriangleGroups { get; }

        Index3i GetTriangle(int i);
        int GetTriangleGroup(int i);

        bool IsTriangle(int tID);

        // iterators allow us to work with gaps in index space
        System.Collections.Generic.IEnumerable<int> TriangleIndices();
    }


    public interface IDeformableMesh : IMesh
    {
        void SetVertex(int vID, Vector3D vNewPos);
        void SetVertexNormal(int vid, Vector3F vNewNormal);
    }



    /*
     * Abstracts construction of meshes, so that we can construct different types, etc
     */
    public struct NewVertexInfo
    {
        public Vector3D v;
        public Vector3F n, c;
        public Vector2F uv;
        public bool bHaveN, bHaveUV, bHaveC;

		public NewVertexInfo(Vector3D v) {
			this.v = v; n = c = Vector3F.Zero; uv = Vector2F.Zero;
			bHaveN = bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3D v, Vector3F n) {
			this.v = v; this.n = n; c = Vector3F.Zero; uv = Vector2F.Zero;
			bHaveN = true; bHaveC = bHaveUV = false;
		}
		public NewVertexInfo(Vector3D v, Vector3F n, Vector3F c) {
			this.v = v; this.n = n; this.c = c; uv = Vector2F.Zero;
			bHaveN = bHaveC = true; bHaveUV = false;
		}
		public NewVertexInfo(Vector3D v, Vector3F n, Vector3F c, Vector2F uv) {
			this.v = v; this.n = n; this.c = c; this.uv = uv;
			bHaveN = bHaveC = bHaveUV = true;
		}
    }


    public interface IMeshBuilder
    {
        // return ID of new mesh
        int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups);
        void SetActiveMesh(int id);

        int AppendVertex(double x, double y, double z);
        int AppendVertex(NewVertexInfo info);

        int AppendTriangle(int i, int j, int k);
        int AppendTriangle(int i, int j, int k, int g);


        // material handling

        // return client-side unique ID of material
        int BuildMaterial(GenericMaterial m);

        // do material assignment to mesh, where meshID comes from IMeshBuilder
        void AssignMaterial(int materialID, int meshID);

        // optional
        bool SupportsMetaData { get; }
        void AppendMetaData(string identifier, object data);
    }




}
