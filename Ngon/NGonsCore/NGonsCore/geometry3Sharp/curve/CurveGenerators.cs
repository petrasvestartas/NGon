using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{
    abstract public class CurveGenerator
    {
        public VectorArray3d vertices;
        public bool closed = false;

        abstract public void Generate();

        public void Make(DCurve3 c) {
            int nV = vertices.Count;
            for (int i = 0; i < nV; ++i) {
                c.AppendVertex(vertices[i]);
            }
            c.Closed = closed;
        }
    }



    public class LineGenerator : CurveGenerator
    {
        public Vector3D Start = Vector3D.Zero;
        public Vector3D End = Vector3D.AxisX;

        // must set one of these, otherwise we use default
        public int Subdivisions = -1;
        public double StepSize = 0.0;


        public override void Generate()
        {
            double fLen = (Start - End).Length;

            int nSteps = 10;
            if (Subdivisions > 0)
                nSteps = Subdivisions;
            else if (StepSize > 0)
                nSteps = (int)math.MathUtil.Clamp( (int)(fLen / StepSize), 2, 10000);

            vertices = new VectorArray3d(nSteps+1);

            for ( int i = 0; i < nSteps; ++i ) {
                double t = (double)i / (double)nSteps;
                Vector3D v = (1.0 - t) * Start + (t) * End;
                vertices[i] = (v);
            }
            vertices[nSteps] = End;

            closed = false;
        }

    }

}