using System;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.mesh_ops
{
    public class MeshLoopSmooth
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;
        public EdgeLoop Loop;

        public double Alpha = 0.25f;
        public int Rounds = 10;

        // reproject smoothed position to new location
        public Func<Vector3D, int, Vector3D> ProjectF;


        Vector3D[] SmoothedPostions;

        public MeshLoopSmooth(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, EdgeLoop loop)
        {
            Mesh = mesh;
            Loop = loop;

            SmoothedPostions = new Vector3D[Loop.Vertices.Length];

            ProjectF = null;
        }


        public virtual ValidationStatus Validate()
        {
            ValidationStatus loopStatus = MeshValidation.IsEdgeLoop(Mesh, Loop);
            return loopStatus;
        }


        public virtual bool Smooth()
        {
            int NV = Loop.Vertices.Length;

            double a = math.MathUtil.Clamp(Alpha, 0, 1);
            double num_rounds = math.MathUtil.Clamp(Rounds, 0, 10000);

            for (int round = 0; round < num_rounds; ++round) {

                // compute
                gParallel.ForEach(Interval1i.Range(NV), (i) => {
                    int vid = Loop.Vertices[(i + 1) % NV];
                    Vector3D prev = Mesh.GetVertex(Loop.Vertices[i]);
                    Vector3D cur = Mesh.GetVertex(vid);
                    Vector3D next = Mesh.GetVertex(Loop.Vertices[(i + 2) % NV]);

                    Vector3D centroid = (prev + next) * 0.5;
                    SmoothedPostions[i] = (1 - a) * cur + (a) * centroid;
                });

                // bake
                gParallel.ForEach(Interval1i.Range(NV), (i) => {
                    int vid = Loop.Vertices[(i + 1) % NV];
                    Vector3D pos = SmoothedPostions[i];

                    if (ProjectF != null)
                        pos = ProjectF(pos, vid);

                    Mesh.SetVertex(vid, pos);
                });
            }

            return true;
        }

    }
}
