using System;
using System.Collections.Generic;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve 
{
	public static class CurveUtils2 
	{


        public static IParametricCurve2d Convert(Polygon2d poly)
        {
            ParametricCurveSequence2 seq = new ParametricCurveSequence2();
            int N = poly.VertexCount;
            for (int i = 0; i < N; ++i)
                seq.Append(new Segment2d(poly[i], poly[(i + 1)%N]));
            seq.IsClosed = true;
            return seq;
        }


		// 2D curve utils?
		public static double SampledDistance(IParametricCurve2d c, Vector2D point, int N = 100)
		{
			double tMax = c.ParamLength;
			double min_dist = double.MaxValue;
			for ( int i = 0; i <= N; ++i ) {
				double fT = (double)i / (double)N;
				fT *= tMax;
				Vector2D p = c.SampleT(fT);
				double d = p.DistanceSquared(point);
				if ( d < min_dist )
					min_dist = d;
			}
			return Math.Sqrt(min_dist);
		}


        /// <summary>
        /// if the children of C are a tree, iterate through all the leaves
        /// </summary>
        public static IEnumerable<IParametricCurve2d> LeafCurvesIteration(IParametricCurve2d c)
        {
            if (c is IMultiCurve2d) {
                var multiC = c as IMultiCurve2d;
                foreach ( IParametricCurve2d c2 in multiC.Curves ) {
                    foreach (var c3 in LeafCurvesIteration(c2))
                        yield return c3;
                }
            } else
                yield return c;
        }


        public static List<IParametricCurve2d> Flatten(List<IParametricCurve2d> curves)
        {
            List<IParametricCurve2d> l = new List<IParametricCurve2d>();
            foreach ( IParametricCurve2d sourceC in curves) {
                foreach (var c in LeafCurvesIteration(sourceC))
                    l.Add(c);
            }
            return l;
        }



		// returns largest scalar coordinate value, useful for converting to integer coords
		public static Vector2D GetMaxOriginDistances(IEnumerable<Vector2D> vertices)
		{
			Vector2D max = Vector2D.Zero;
			foreach (Vector2D v in vertices) {
				double x = Math.Abs(v.x);
				if (x > max.x) max.x = x;
				double y = Math.Abs(v.y);
				if (y > max.y) max.y = y;
			}
			return max;
		}


		public static int FindNearestVertex(Vector2D pt, IEnumerable<Vector2D> vertices) {
			int i = 0;
			int iNearest = -1;
			double nearestSqr = double.MaxValue;
			foreach ( Vector2D v in vertices ) {
				double d = v.DistanceSquared(pt);
				if ( d < nearestSqr ) {
					nearestSqr = d;
					iNearest = i;
				}
				i++;
			}
			return iNearest;
		}


        public static Vector2D CentroidVtx(IEnumerable<Vector2D> vertices)
        {
            Vector2D c = Vector2D.Zero;
            int count = 0;
            foreach (Vector2D v in vertices) {
                c += v;
                count++;
            }
            if (count > 1)
                c /= (double)count;
            return c;
        }




        public static void LaplacianSmooth(IList<Vector2D> vertices, double alpha, int iterations, bool is_loop, bool in_place = false)
        {
            int N = vertices.Count;
            Vector2D[] temp = null;
            if (in_place == false)
                temp = new Vector2D[N];
            IList<Vector2D> set = (in_place) ? vertices : temp;

            double beta = 1.0 - alpha;
            for (int ii = 0; ii < iterations; ++ii) {
                if (is_loop) {
                    for (int i = 0; i < N; ++i) {
                        Vector2D c = (vertices[(i + N - 1) % N] + vertices[(i + 1) % N]) * 0.5;
                        set[i] = beta * vertices[i] + alpha * c;
                    }
                } else {
                    set[0] = vertices[0]; set[N - 1] = vertices[N - 1];
                    for (int i = 1; i < N-1; ++i) {
                        Vector2D c = (vertices[i-1] + vertices[i+1]) * 0.5;
                        set[i] = beta * vertices[i] + alpha * c;
                    }
                }

                if (in_place == false) {
                    for (int i = 0; i < N; ++i)
                        vertices[i] = set[i];
                }
            }
        }

	}
}
