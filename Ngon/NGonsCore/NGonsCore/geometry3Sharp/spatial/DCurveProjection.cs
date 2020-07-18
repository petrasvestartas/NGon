using NGonsCore.geometry3Sharp.curve;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.spatial
{
    public class DCurveProjectionTarget : IProjectionTarget
    {
        public DCurve3 Curve;


        public DCurveProjectionTarget(DCurve3 curve)
        {
            this.Curve = curve;
        }

        public Vector3D Project(Vector3D vPoint, int identifier = -1)
        {
            Vector3D vNearest = Vector3D.Zero;
            double fNearestSqr = double.MaxValue;

            int N = (Curve.Closed) ? Curve.VertexCount : Curve.VertexCount - 1;
            for ( int i = 0; i < N; ++i ) {
                Segment3d seg = new Segment3d(Curve[i], Curve[(i + 1) % N]);
                Vector3D pt = seg.NearestPoint(vPoint);
                double dsqr = pt.DistanceSquared(vPoint);
                if (dsqr < fNearestSqr) {
                    fNearestSqr = dsqr;
                    vNearest = pt;
                }
            }

            return (fNearestSqr < double.MaxValue) ? vNearest : vPoint;
        }
    }
}
