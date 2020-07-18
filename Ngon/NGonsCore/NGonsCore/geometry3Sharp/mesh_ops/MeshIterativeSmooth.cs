using System;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.mesh_ops
{
    public class MeshIterativeSmooth
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
        public int[] Vertices;

        public double Alpha = 0.25f;
        public int Rounds = 10;

		public enum SmoothTypes {
			Uniform, Cotan, MeanValue
		};
		public SmoothTypes SmoothType = SmoothTypes.Uniform;

        // reproject smoothed position to new location
        public Func<Vector3D, Vector3F, int, Vector3D> ProjectF;

        Vector3D[] SmoothedPostions;

        public MeshIterativeSmooth(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int[] vertices, bool bOwnVertices = false)
        {
            Mesh = mesh;
            Vertices = (bOwnVertices) ? vertices : (int[])vertices.Clone();

            SmoothedPostions = new Vector3D[Vertices.Length];

            ProjectF = null;
        }


        public virtual ValidationStatus Validate()
        {
            return ValidationStatus.Ok;
        }


        public virtual bool Smooth()
        {
            int NV = Vertices.Length;

            double a = math.MathUtil.Clamp(Alpha, 0, 1);
            double num_rounds = math.MathUtil.Clamp(Rounds, 0, 10000);

            Func<NGonsCore.geometry3Sharp.mesh.DMesh3, int, double, Vector3D> smoothFunc = mesh.MeshUtil.UniformSmooth;
            if (SmoothType == SmoothTypes.MeanValue)
                smoothFunc = mesh.MeshUtil.MeanValueSmooth;
            else if (SmoothType == SmoothTypes.Cotan)
                smoothFunc = mesh.MeshUtil.CotanSmooth;

            Action<int> smooth = (i) => {
                int vID = Vertices[i];
                SmoothedPostions[i] = smoothFunc(Mesh, vID, a);
            };
            Action<int> project = (i) => {
                Vector3D pos = SmoothedPostions[i];
                SmoothedPostions[i] = ProjectF(pos, Vector3F.AxisY, Vertices[i]);
            };

            IndexRangeEnumerator indices = new IndexRangeEnumerator(0, NV);

            for (int round = 0; round < num_rounds; ++round) {

                gParallel.ForEach<int>(indices, smooth);
                if ( ProjectF != null )
                    gParallel.ForEach<int>(indices, project);

                // bake
                for (int i = 0; i < NV; ++i)
                    Mesh.SetVertex(Vertices[i], SmoothedPostions[i]);
            }

            return true;
        }
    }




}
