﻿using System;
using System.Collections.Generic;
using System.Linq;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.core
{
    //
    // convenience functions for setting values in an array in sets of 2/3
    //   (eg for arrays that are actually a list of vectors)
    //
    public class BufferUtil
    {
        static public void SetVertex3(double[] v, int i, double x, double y, double z) {
            v[3 * i] = x;
            v[3 * i + 1] = y;
            v[3 * i + 2] = z;
        }
        static public void SetVertex3(float[] v, int i, float x, float y, float z)
        {
            v[3 * i] = x;
            v[3 * i + 1] = y;
            v[3 * i + 2] = z;
        }

        static public void SetVertex2(double[] v, int i, double x, double y)
        {
            v[2 * i] = x;
            v[2 * i + 1] = y;
        }
        static public void SetVertex2(float[] v, int i, float x, float y)
        {
            v[2 * i] = x;
            v[2 * i + 1] = y;
        }

        static public void SetTriangle(int[] v, int i, int a, int b, int c)
        {
            v[3 * i] = a;
            v[3 * i + 1] = b;
            v[3 * i + 2] = c;
        }


        static public double Dot(double[] a, double [] b)
        {
            double dot = 0;
            for (int i = 0; i < a.Length; ++i)
                dot += a[i] * b[i];
            return dot;
        }

        static public void MultiplyAdd(double[] dest, double multiply, double[] add)
        {
            for (int i = 0; i < dest.Length; ++i)
                dest[i] += multiply * add[i];
        }

        static public double DistanceSquared(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; ++i)
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            return sum;
        }

		/// <summary>
		/// Count number of elements in array (or up to max_i) that pass FilterF test
		/// </summary>
		static public int CountValid<T>(T[] data, Func<T, bool> FilterF, int max_i = -1) {
			int n = (max_i == -1) ? data.Length : max_i;
			int valid = 0;
			for (int i = 0; i < n; ++i) {
				if (FilterF(data[i]))
					valid++;
			}
			return valid;
		}

		/// <summary>
		/// shifts elements of array (or up to max_i) that pass FilterF to front of list,
		/// and returns number that passed
		/// </summary>
		static public int FilterInPlace<T>(T[] data, Func<T,bool> FilterF, int max_i = -1) {
			int N = (max_i == -1) ? data.Length : max_i;
			int k = 0;
			for (int i = 0; i < N; ++i) {
				if (FilterF(data[i]))
					data[k++] = data[i];
			}
			return k;
		}

		/// <summary>
		/// return a new array containing only elements (or up to max_i) that pass FilterF test
		/// </summary>
		static public T[] Filter<T>(T[] data, Func<T, bool> FilterF, int max_i = -1) {
			int n = (max_i == -1) ? data.Length : max_i;
			int valid = CountValid(data, FilterF);
			if (valid == 0)
				return null;
			T[] result = new T[valid];
			int k = 0;
			for (int i = 0; i < n; ++i) {
				if (FilterF(data[i]))
					result[k++] = data[i];
			}
			return result;
		}




        /// <summary>
        /// convert input set into Vector3d.
        /// Supports packed list of float/double tuples, list of Vector3f/Vector3d
        /// </summary>
        static public Vector3D[] ToVector3d<T>(IEnumerable<T> values) {
            Vector3D[] result = null;

            int N = values.Count();
            int k = 0; int j = 0;

            Type t = typeof(T);
            if (t == typeof(float)) {
                N /= 3;
                result = new Vector3D[N];
                IEnumerable<float> valuesf = values as IEnumerable<float>;
                foreach (float f in valuesf) {
                    result[k][j++] = f;
                    if (j == 3) {
                        j = 0; k++;
                    }
                }
            } else if (t == typeof(double)) {
                N /= 3;
                result = new Vector3D[N];
                IEnumerable<double> valuesd = values as IEnumerable<double>;
                foreach (double f in valuesd) {
                    result[k][j++] = f;
                    if (j == 3) {
                        j = 0; k++;
                    }
                }
            } else if (t == typeof(Vector3F)) {
                result = new Vector3D[N];
                IEnumerable<Vector3F> valuesvf = values as IEnumerable<Vector3F>;
                foreach (Vector3F v in valuesvf)
                    result[k++] = v;

            } else if (t == typeof(Vector3D)) {
                result = new Vector3D[N];
                IEnumerable<Vector3D> valuesvd = values as IEnumerable<Vector3D>;
                foreach (Vector3D v in valuesvd)
                    result[k++] = v;

            } else
                throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());

            return result;
        }



        /// <summary>
        /// convert input set into Vector3f.
        /// Supports packed list of float/double tuples, list of Vector3f/Vector3d
        /// </summary>
        static public Vector3F[] ToVector3f<T>(IEnumerable<T> values) {
            Vector3F[] result = null;

            int N = values.Count();
            int k = 0; int j = 0;

            Type t = typeof(T);
            if ( t == typeof(float) ) {
                N /= 3;
                result = new Vector3F[N];
                IEnumerable<float> valuesf = values as IEnumerable<float>;
                foreach ( float f in valuesf ) {
                    result[k][j++] = f;
                    if ( j == 3 ) {
                        j = 0; k++;
                    }
                }
            } else if ( t == typeof(double) ) {
                N /= 3;
                result = new Vector3F[N];
                IEnumerable<double> valuesd = values as IEnumerable<double>;
                foreach ( double f in valuesd ) {
                    result[k][j++] = (float)f;
                    if ( j == 3 ) {
                        j = 0; k++;
                    }
                }
            } else if ( t == typeof(Vector3F) ) {
                result = new Vector3F[N];
                IEnumerable<Vector3F> valuesvf = values as IEnumerable<Vector3F>;
                foreach (Vector3F v in valuesvf)
                    result[k++] = v;

            } else if ( t == typeof(Vector3D) ) {
                result = new Vector3F[N];
                IEnumerable<Vector3D> valuesvd = values as IEnumerable<Vector3D>;
                foreach (Vector3D v in valuesvd)
                    result[k++] = (Vector3F)v;

            } else
                throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());

            return result;
        }




        /// <summary>
        /// convert input set into Index3i.
        /// Supports packed list of int tuples, list of Vector3i/Index3i
        /// </summary>
        static public Index3i[] ToIndex3i<T>(IEnumerable<T> values) {
            Index3i[] result = null;

            int N = values.Count();
            int k = 0; int j = 0;

            Type t = typeof(T);
            if (t == typeof(int)) {
                N /= 3;
                result = new Index3i[N];
                IEnumerable<int> valuesi = values as IEnumerable<int>;
                foreach (int i in valuesi) {
                    result[k][j++] = i;
                    if (j == 3) {
                        j = 0; k++;
                    }
                }
            } else if (t == typeof(Index3i)) {
                result = new Index3i[N];
                IEnumerable<Index3i> valuesvi = values as IEnumerable<Index3i>;
                foreach (Index3i v in valuesvi)
                    result[k++] = v;

            } else if (t == typeof(Vector3i)) {
                result = new Index3i[N];
                IEnumerable<Vector3i> valuesvi = values as IEnumerable<Vector3i>;
                foreach (Vector3i v in valuesvi)
                    result[k++] = v;

            } else
                throw new NotSupportedException("ToVector3d: unknown type " + t.ToString());

            return result;
        }



    }




    // utility class for porting C++ code that uses this kind of idiom:
    //    T * ptr = &array[i];
    //    ptr[k] = value
    public struct ArrayAlias<T>
    {
        public T[] Source;
        public int Index;

        public ArrayAlias(T[] source, int i)
        {
            Source = source;
            this.Index = i;
        }

        public T this[int i]
        {
            get { return Source[Index + i]; }
            set { Source[Index + i] = value; }
        }
    }


}
