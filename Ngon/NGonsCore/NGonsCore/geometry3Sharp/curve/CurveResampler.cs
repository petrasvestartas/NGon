using System;
using System.Collections.Generic;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{
    public class CurveResampler
    {

        double[] lengths;

        // will return null if no edges need to be split!
        public List<Vector3D> SplitResample(ISampledCurve3d curve, double fMaxEdgeLen)
        {
            double fMaxSqr = fMaxEdgeLen * fMaxEdgeLen;

            int N = curve.VertexCount;
            int Nstop = (curve.Closed) ? N + 1 : N;
            if (lengths == null || lengths.Length < Nstop )
                lengths = new double[Nstop];
            bool bFoundSplit = false;
            for (int i = 0; i < Nstop; ++i) {
                lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % N));
                if (lengths[i] > fMaxSqr)
                    bFoundSplit = true;
            }
            if (!bFoundSplit)
                return null;


            List<Vector3D> vNew = new List<Vector3D>();
            Vector3D prev = curve.GetVertex(0);
            vNew.Add(prev);
            for ( int i = 0; i < Nstop-1; ++i ) {
                Vector3D next = curve.GetVertex((i + 1) % N);

                if (lengths[i] > fMaxSqr) {
                    double fLen = Math.Sqrt(lengths[i]);
                    int nSteps = (int)(fLen / fMaxEdgeLen) + 1;
                    for ( int k = 1; k < nSteps; ++k ) {
                        double t = (double)k / (double)nSteps;
                        Vector3D mid = Vector3D.Lerp(prev, next, t);
                        vNew.Add(mid);
                    }
                }
                vNew.Add(next);
                prev = next;
            }

            return vNew;
        }




        // will return null if no edges need to be split!
        public List<Vector3D> SplitCollapseResample(ISampledCurve3d curve, double fMaxEdgeLen, double fMinEdgeLen)
        {
            double fMaxSqr = fMaxEdgeLen * fMaxEdgeLen;
            double fMinSqr = fMinEdgeLen * fMinEdgeLen;

            int N = curve.VertexCount;
            int Nstop = (curve.Closed) ? N + 1 : N;
            if (lengths == null || lengths.Length < Nstop)
                lengths = new double[Nstop];
            bool bFoundSplit = false;
            bool bFoundCollapse = false;
            for (int i = 0; i < Nstop - 1; ++i) {
                lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % N));
                if (lengths[i] > fMaxSqr)
                    bFoundSplit = true;
                else if (lengths[i] < fMinSqr)
                    bFoundCollapse = true;
            }
            if (bFoundSplit == false && bFoundCollapse == false)
                return null;


            List<Vector3D> vNew = new List<Vector3D>();
            Vector3D prev = curve.GetVertex(0);
            vNew.Add(prev);
            double collapse_accum = 0;
            for (int i = 0; i < Nstop - 1; ++i) {
                Vector3D next = curve.GetVertex((i + 1) % N);

                // accumulate collapsed edges. if we accumulate past min-edge length,
                // then need to drop a vertex
                if (lengths[i] < fMinSqr) {   
                    collapse_accum += Math.Sqrt(lengths[i]);
                    if ( collapse_accum > fMinEdgeLen ) {
                        collapse_accum = 0;
                        vNew.Add(next);
                    }
                    prev = next;
                    continue;
                }

                // if we have been accumulating collapses, then we need to
                // drop a new vertex  (todo: is this right? shouldn't we just
                //   continue from previous?)
                if ( collapse_accum > 0 ) {
                    vNew.Add(prev);
                    collapse_accum = 0;
                }

                // split edge if it is too long
                if (lengths[i] > fMaxSqr) {
                    double fLen = Math.Sqrt(lengths[i]);
                    int nSteps = (int)(fLen / fMaxEdgeLen) + 1;
                    for (int k = 1; k < nSteps; ++k) {
                        double t = (double)k / (double)nSteps;
                        Vector3D mid = Vector3D.Lerp(prev, next, t);
                        vNew.Add(mid);
                    }
                }
                vNew.Add(next);
                prev = next;
            }

            return vNew;
        }



    }
}
