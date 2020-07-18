﻿using System;


#if G3_USING_UNITY
using UnityEngine;
#endif

namespace NGonsCore.geometry3Sharp.math
{
    public struct Vector3D : IComparable<Vector3D>, IEquatable<Vector3D>
    {
        public double x;
        public double y;
        public double z; 

        public Vector3D(double f) { x = y = z = f; }
        public Vector3D(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
        public Vector3D(double[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
        public Vector3D(Vector3D copy) { x = copy.x; y = copy.y; z = copy.z; }
        public Vector3D(Vector3F copy) { x = copy.x; y = copy.y; z = copy.z; }

        static public readonly Vector3D Zero = new Vector3D(0.0f, 0.0f, 0.0f);
        static public readonly Vector3D One = new Vector3D(1.0f, 1.0f, 1.0f);
        static public readonly Vector3D AxisX = new Vector3D(1.0f, 0.0f, 0.0f);
        static public readonly Vector3D AxisY = new Vector3D(0.0f, 1.0f, 0.0f);
        static public readonly Vector3D AxisZ = new Vector3D(0.0f, 0.0f, 1.0f);
		static public readonly Vector3D MaxValue = new Vector3D(double.MaxValue,double.MaxValue,double.MaxValue);
		static public readonly Vector3D MinValue = new Vector3D(double.MinValue,double.MinValue,double.MinValue);

        public double this[int key]
        {
            get { return (key == 0) ? x : (key == 1) ? y : z; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else z = value; }
        }

        public Vector2D xy {
            get { return new Vector2D(x, y); }
            set { x = xy.x; y = xy.y; }
        }
        public Vector2D xz {
            get { return new Vector2D(x, z); }
            set { x = xy.x; z = xy.y; }
        }
        public Vector2D yz {
            get { return new Vector2D(y, z); }
            set { y = xy.x; z = xy.y; }
        }

        public double LengthSquared
        {
            get { return x * x + y * y + z * z; }
        }
        public double Length
        {
            get { return Math.Sqrt(LengthSquared); }
        }

        public double LengthL1
        {
            get { return Math.Abs(x) + Math.Abs(y) + Math.Abs(z); }
        }


        public double Normalize(double epsilon = MathUtil.Epsilon)
        {
            double length = Length;
            if (length > epsilon) {
                double invLength = 1.0 / length;
                x *= invLength;
                y *= invLength;
                z *= invLength;
            } else {
                length = 0;
                x = y = z = 0;
            }
            return length;
        }
        public Vector3D Normalized
        {
            get {
                double length = Length;
                if (length > MathUtil.Epsilon) {
                    double invLength = 1.0 / length;
                    return new Vector3D(x * invLength, y * invLength, z * invLength);
                } else
                    return Vector3D.Zero;
            }
        }

		public bool IsNormalized {
			get { return Math.Abs( (x * x + y * y + z * z) - 1) < MathUtil.ZeroTolerance; }
		}

        public bool IsFinite
        {
            get { double f = x + y + z; return double.IsNaN(f) == false && double.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals) {
            x = Math.Round(x, nDecimals);
            y = Math.Round(y, nDecimals);
            z = Math.Round(z, nDecimals);
        }


        public double Dot(Vector3D v2)
        {
            return x * v2.x + y * v2.y + z * v2.z;
        }
        public static double Dot(Vector3D v1, Vector3D v2)
        {
            return v1.Dot(v2);
        }

        public Vector3D Cross(Vector3D v2)
        {
            return new Vector3D(
                y * v2.z - z * v2.y,
                z * v2.x - x * v2.z,
                x * v2.y - y * v2.x);
        }
        public static Vector3D Cross(Vector3D v1, Vector3D v2)
        {
            return v1.Cross(v2);
        }

        public Vector3D UnitCross(Vector3D v2)
        {
            Vector3D n = new Vector3D(
                y * v2.z - z * v2.y,
                z * v2.x - x * v2.z,
                x * v2.y - y * v2.x);
            n.Normalize();
            return n;
        }

        public double AngleD(Vector3D v2)
        {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot) * MathUtil.Rad2Deg;
        }
        public static double AngleD(Vector3D v1, Vector3D v2)
        {
            return v1.AngleD(v2);
        }
        public double AngleR(Vector3D v2)
        {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot);
        }
        public static double AngleR(Vector3D v1, Vector3D v2)
        {
            return v1.AngleR(v2);
        }

		public double DistanceSquared(Vector3D v2) {
			double dx = v2.x-x, dy = v2.y-y, dz = v2.z-z;
			return dx*dx + dy*dy + dz*dz;
		}
        public double Distance(Vector3D v2) {
            double dx = v2.x-x, dy = v2.y-y, dz = v2.z-z;
			return Math.Sqrt(dx*dx + dy*dy + dz*dz);
		}

        public void Set(Vector3D o)
        {
            x = o.x; y = o.y; z = o.z;
        }
        public void Set(double fX, double fY, double fZ)
        {
            x = fX; y = fY; z = fZ;
        }
        public void Add(Vector3D o)
        {
            x += o.x; y += o.y; z += o.z;
        }
        public void Subtract(Vector3D o)
        {
            x -= o.x; y -= o.y; z -= o.z;
        }



        public static Vector3D operator -(Vector3D v)
        {
            return new Vector3D(-v.x, -v.y, -v.z);
        }

        public static Vector3D operator *(double f, Vector3D v)
        {
            return new Vector3D(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3D operator *(Vector3D v, double f)
        {
            return new Vector3D(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3D operator /(Vector3D v, double f)
        {
            return new Vector3D(v.x / f, v.y / f, v.z / f);
        }
        public static Vector3D operator /(double f, Vector3D v)
        {
            return new Vector3D(f / v.x, f / v.y, f / v.z);
        }

        public static Vector3D operator *(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3D operator /(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.x / b.x, a.y / b.y, a.z / b.z);
        }


        public static Vector3D operator +(Vector3D v0, Vector3D v1)
        {
            return new Vector3D(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
        }
        public static Vector3D operator +(Vector3D v0, double f)
        {
            return new Vector3D(v0.x + f, v0.y + f, v0.z + f);
        }

        public static Vector3D operator -(Vector3D v0, Vector3D v1)
        {
            return new Vector3D(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
        }
        public static Vector3D operator -(Vector3D v0, double f)
        {
            return new Vector3D(v0.x - f, v0.y - f, v0.z - f);
        }



        public static bool operator ==(Vector3D a, Vector3D b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Vector3D a, Vector3D b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3D)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int) 2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector3D other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            else if (z != other.z)
                return z < other.z ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector3D other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }


        public bool EpsilonEqual(Vector3D v2, double epsilon) {
            return Math.Abs(x - v2.x) < epsilon && 
                   Math.Abs(y - v2.y) < epsilon &&
                   Math.Abs(z - v2.z) < epsilon;
        }
        public bool PrecisionEqual(Vector3D v2, int nDigits)
        {
            return Math.Round(x, nDigits) == Math.Round(v2.x, nDigits) &&
                   Math.Round(y, nDigits) == Math.Round(v2.y, nDigits) &&
                   Math.Round(z, nDigits) == Math.Round(v2.z, nDigits);
        }


        public static Vector3D Lerp(Vector3D a, Vector3D b, double t)
        {
            double s = 1 - t;
            return new Vector3D(s * a.x + t * b.x, s * a.y + t * b.y, s * a.z + t * b.z);
        }



        public override string ToString() {
            return string.Format("{0:F8} {1:F8} {2:F8}", x, y, z);
        }
        public string ToString(string fmt) {
            return string.Format("{0} {1} {2}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt));
        }



        public static implicit operator Vector3D(Vector3F v)
        {
            return new Vector3D(v.x, v.y, v.z);
        }
        public static explicit operator Vector3F(Vector3D v)
        {
            return new Vector3F((float)v.x, (float)v.y, (float)v.z);
        }


#if G3_USING_UNITY
        public static implicit operator Vector3d(UnityEngine.Vector3 v)
        {
            return new Vector3d(v.x, v.y, v.z);
        }
        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
#endif




        // complicated functions go down here...


        // [RMS] this is from WildMagic5, but I added returning the minLength value
        //   from GTEngine, because I use this in place of GTEngine's Orthonormalize in
        //   ComputeOrthogonalComplement below
        public static double Orthonormalize(ref Vector3D u, ref Vector3D v, ref Vector3D w)
        {
            // If the input vectors are v0, v1, and v2, then the Gram-Schmidt
            // orthonormalization produces vectors u0, u1, and u2 as follows,
            //
            //   u0 = v0/|v0|
            //   u1 = (v1-(u0*v1)u0)/|v1-(u0*v1)u0|
            //   u2 = (v2-(u0*v2)u0-(u1*v2)u1)/|v2-(u0*v2)u0-(u1*v2)u1|
            //
            // where |A| indicates length of vector A and A*B indicates dot
            // product of vectors A and B.

            // compute u0
            double minLength = u.Normalize();

            // compute u1
            double dot0 = u.Dot(v);
            v -= dot0 * u;
            double l = v.Normalize();
            if (l < minLength)
                minLength = l;

            // compute u2
            double dot1 = v.Dot(w);
            dot0 = u.Dot(w);
            w -= dot0 * u + dot1 * v;
            l = w.Normalize();
            if (l < minLength)
                minLength = l;

            return minLength;
        }


        // Input W must be a unit-length vector.  The output vectors {U,V} are
        // unit length and mutually perpendicular, and {U,V,W} is an orthonormal basis.
        // ported from WildMagic5
        public static void GenerateComplementBasis(ref Vector3D u, ref Vector3D v, Vector3D w)
        {
            double invLength;

            if (Math.Abs(w.x) >= Math.Abs(w.y)) {
                // W.x or W.z is the largest magnitude component, swap them
                invLength = MathUtil.InvSqrt(w.x * w.x + w.z * w.z);
                u.x = -w.z * invLength;
                u.y = 0;
                u.z = +w.x * invLength;
                v.x = w.y * u.z;
                v.y = w.z * u.x - w.x * u.z;
                v.z = -w.y * u.x;
            } else {
                // W.y or W.z is the largest magnitude component, swap them
                invLength = MathUtil.InvSqrt(w.y * w.y + w.z * w.z);
                u.x = 0;
                u.y = +w.z * invLength;
                u.z = -w.y * invLength;
                v.x = w.y * u.z - w.z * u.y;
                v.y = -w.x * u.z;
                v.z = w.x * u.y;
            }
        }

        // this function is from GTEngine
        // Compute a right-handed orthonormal basis for the orthogonal complement
        // of the input vectors.  The function returns the smallest length of the
        // unnormalized vectors computed during the process.  If this value is nearly
        // zero, it is possible that the inputs are linearly dependent (within
        // numerical round-off errors).  On input, numInputs must be 1 or 2 and
        // v0 through v(numInputs-1) must be initialized.  On output, the
        // vectors v0 through v2 form an orthonormal set.
        public static double ComputeOrthogonalComplement(int numInputs, Vector3D v0, ref Vector3D v1, ref Vector3D v2 /*, bool robust = false*/)
        {
            if (numInputs == 1) {
                if (Math.Abs(v0[0]) > Math.Abs(v0[1])) {
                    v1 = new Vector3D( -v0[2], 0.0, +v0[0] );
                }
                else
                {
                    v1 = new Vector3D(0.0, +v0[2], -v0[1]);
                }
                numInputs = 2;
            }

            if (numInputs == 2) {
                v2 = Vector3D.Cross(v0, v1);
                return Vector3D.Orthonormalize(ref v0, ref v1, ref v2);
                //return Orthonormalize<3, Real>(3, v, robust);
            }

            return 0;
        }

    }
}
