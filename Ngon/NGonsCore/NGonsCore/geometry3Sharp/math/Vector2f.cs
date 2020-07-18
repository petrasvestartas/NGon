using System;


#if G3_USING_UNITY
using UnityEngine;
#endif

namespace NGonsCore.geometry3Sharp.math
{
    public struct Vector2F : IComparable<Vector2F>, IEquatable<Vector2F>
    {
        public float x;
        public float y;

        public Vector2F(float f) { x = y = f; }
        public Vector2F(float x, float y) { this.x = x; this.y = y; }
        public Vector2F(float[] v2) { x = v2[0]; y = v2[1]; }
        public Vector2F(double f) { x = y = (float)f; }
        public Vector2F(double x, double y) { this.x = (float)x; this.y = (float)y; }
        public Vector2F(double[] v2) { x = (float)v2[0]; y = (float)v2[1]; }
        public Vector2F(Vector2F copy) { x = copy[0]; y = copy[1]; }
        public Vector2F(Vector2D copy) { x = (float)copy[0]; y = (float)copy[1]; }


        static public readonly Vector2F Zero = new Vector2F(0.0f, 0.0f);
        static public readonly Vector2F One = new Vector2F(1.0f, 1.0f);
        static public readonly Vector2F AxisX = new Vector2F(1.0f, 0.0f);
        static public readonly Vector2F AxisY = new Vector2F(0.0f, 1.0f);
		static public readonly Vector2F MaxValue = new Vector2F(float.MaxValue,float.MaxValue);
		static public readonly Vector2F MinValue = new Vector2F(float.MinValue,float.MinValue);

        public float this[int key]
        {
            get { return (key == 0) ? x : y; }
            set { if (key == 0) x = value; else y = value; }
        }


        public float LengthSquared
        {
            get { return x * x + y * y; }
        }
        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        public float Normalize(float epsilon = MathUtil.Epsilonf)
        {
            float length = Length;
            if (length > epsilon) {
                float invLength = 1.0f / length;
                x *= invLength;
                y *= invLength;
            } else {
                length = 0;
                x = y = 0;
            }
            return length;
        }
        public Vector2F Normalized
        {
            get {
                float length = Length;
                if (length > MathUtil.Epsilonf) {
                    float invLength = 1 / length;
                    return new Vector2F(x * invLength, y * invLength);
                } else
                    return Vector2F.Zero;
            }
        }

		public bool IsNormalized {
			get { return Math.Abs( (x * x + y * y) - 1) < MathUtil.ZeroTolerancef; }
		}

        public bool IsFinite
        {
            get { float f = x + y; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals) {
            x = (float)Math.Round(x, nDecimals);
            y = (float)Math.Round(y, nDecimals);
        }

        public float Dot(Vector2F v2)
        {
            return x * v2.x + y * v2.y;
        }


        public float Cross(Vector2F v2) {
            return y * v2.y - y * v2.x;
        }


		public Vector2F Perp {
			get { return new Vector2F(y, -x); }
		}
		public Vector2F UnitPerp {
			get { return new Vector2F(y, -x).Normalized; }
		}
		public float DotPerp(Vector2F v2) {
			return x*v2.y - y*v2.x;
		}


        public float AngleD(Vector2F v2) {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot) * MathUtil.Rad2Deg);
        }
        public static float AngleD(Vector2F v1, Vector2F v2) {
            return v1.AngleD(v2);
        }
        public float AngleR(Vector2F v2) {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot));
        }
        public static float AngleR(Vector2F v1, Vector2F v2) {
            return v1.AngleR(v2);
        }



		public float DistanceSquared(Vector2F v2) {
			float dx = v2.x-x, dy = v2.y-y;
			return dx*dx + dy*dy;
		}
        public float Distance(Vector2F v2) {
            float dx = v2.x-x, dy = v2.y-y;
			return (float)Math.Sqrt(dx*dx + dy*dy);
		}


        public void Set(Vector2F o) {
            x = o.x; y = o.y;
        }
        public void Set(float fX, float fY) {
            x = fX; y = fY;
        }
        public void Add(Vector2F o) {
            x += o.x; y += o.y;
        }
        public void Subtract(Vector2F o) {
            x -= o.x; y -= o.y;
        }


		public static Vector2F operator -(Vector2F v) {
			return new Vector2F(-v.x, -v.y);
		}

        public static Vector2F operator+( Vector2F a, Vector2F o ) {
            return new Vector2F(a.x + o.x, a.y + o.y); 
        }
        public static Vector2F operator +(Vector2F a, float f) {
            return new Vector2F(a.x + f, a.y + f);
        }

        public static Vector2F operator-(Vector2F a, Vector2F o) {
            return new Vector2F(a.x - o.x, a.y - o.y);
        }
        public static Vector2F operator -(Vector2F a, float f) {
            return new Vector2F(a.x - f, a.y - f);
        }

        public static Vector2F operator *(Vector2F a, float f) {
            return new Vector2F(a.x * f, a.y * f);
        }
        public static Vector2F operator *(float f, Vector2F a) {
            return new Vector2F(a.x * f, a.y * f);
        }
        public static Vector2F operator /(Vector2F v, float f)
        {
            return new Vector2F(v.x / f, v.y / f);
        }
        public static Vector2F operator /(float f, Vector2F v)
        {
            return new Vector2F(f / v.x, f / v.y);
        }

		public static Vector2F operator *(Vector2F a, Vector2F b)
		{
			return new Vector2F(a.x * b.x, a.y * b.y);
		}
		public static Vector2F operator /(Vector2F a, Vector2F b)
		{
			return new Vector2F(a.x / b.x, a.y / b.y);
		}


        public static bool operator ==(Vector2F a, Vector2F b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(Vector2F a, Vector2F b)
        {
            return (a.x != b.x || a.y != b.y);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector2F)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int) 2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector2F other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector2F other)
        {
            return (x == other.x && y == other.y);
        }


        public bool EpsilonEqual(Vector2F v2, float epsilon) {
            return (float)Math.Abs(x - v2.x) < epsilon && 
                   (float)Math.Abs(y - v2.y) < epsilon;
        }
        public bool PrecisionEqual(Vector2F v2, int nDigits)
        {
            return Math.Round(x, nDigits) == Math.Round(v2.x, nDigits) &&
                   Math.Round(y, nDigits) == Math.Round(v2.y, nDigits);
        }


        public static Vector2F Lerp(Vector2F a, Vector2F b, float t)
        {
            float s = 1 - t;
            return new Vector2F(s * a.x + t * b.x, s * a.y + t * b.y);
        }


        public override string ToString() {
            return string.Format("{0:F8} {1:F8}", x, y);
        }




#if G3_USING_UNITY
        public static implicit operator Vector2f(UnityEngine.Vector2 v)
        {
            return new Vector2f(v.x, v.y);
        }
        public static implicit operator Vector2(Vector2f v)
        {
            return new Vector2(v.x, v.y);
        }
#endif

    }
}
