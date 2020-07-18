using System;

namespace NGonsCore.geometry3Sharp.math
{
    public struct Vector2D : IComparable<Vector2D>, IEquatable<Vector2D>
    {
        public double x;
        public double y;

        public Vector2D(double f) { x = y = f; }
        public Vector2D(double x, double y) { this.x = x; this.y = y; }
        public Vector2D(double[] v2) { x = v2[0]; y = v2[1]; }
        public Vector2D(float f) { x = y = f; }
        public Vector2D(float x, float y) { this.x = x; this.y = y; }
        public Vector2D(float[] v2) { x = v2[0]; y = v2[1]; }
        public Vector2D(Vector2D copy) { x = copy.x; y = copy.y; }
        public Vector2D(Vector2F copy) { x = copy.x; y = copy.y; }


        static public readonly Vector2D Zero = new Vector2D(0.0f, 0.0f);
        static public readonly Vector2D One = new Vector2D(1.0f, 1.0f);
        static public readonly Vector2D AxisX = new Vector2D(1.0f, 0.0f);
        static public readonly Vector2D AxisY = new Vector2D(0.0f, 1.0f);
		static public readonly Vector2D MaxValue = new Vector2D(double.MaxValue,double.MaxValue);
		static public readonly Vector2D MinValue = new Vector2D(double.MinValue,double.MinValue);


        public double this[int key]
        {
            get { return (key == 0) ? x : y; }
            set { if (key == 0) x = value; else y = value; }
        }


        public double LengthSquared
        {
            get { return x * x + y * y; }
        }
        public double Length
        {
            get { return (double)Math.Sqrt(LengthSquared); }
        }

        public double Normalize(double epsilon = MathUtil.Epsilon)
        {
            double length = Length;
            if (length > epsilon) {
                double invLength = 1.0 / length;
                x *= invLength;
                y *= invLength;
            } else {
                length = 0;
                x = y = 0;
            }
            return length;
        }
        public Vector2D Normalized
        {
            get {
                double length = Length;
                if (length > MathUtil.Epsilon) {
                    double invLength = 1 / length;
                    return new Vector2D(x * invLength, y * invLength);
                } else
                    return Vector2D.Zero;
            }
        }

		public bool IsNormalized {
			get { return Math.Abs( (x * x + y * y) - 1) < MathUtil.ZeroTolerance; }
		}

        public bool IsFinite
        {
            get { double f = x + y; return double.IsNaN(f) == false && double.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals) {
            x = Math.Round(x, nDecimals);
            y = Math.Round(y, nDecimals);
        }


        public double Dot(Vector2D v2)
        {
            return x * v2.x + y * v2.y;
        }


        public double Cross(Vector2D v2) {
            return y * v2.y - y * v2.x;
        }


		public Vector2D Perp {
			get { return new Vector2D(y, -x); }
		}
		public Vector2D UnitPerp {
			get { return new Vector2D(y, -x).Normalized; }
		}
		public double DotPerp(Vector2D v2) {
			return x*v2.y - y*v2.x;
		}


        public double AngleD(Vector2D v2) {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot) * MathUtil.Rad2Deg;
        }
        public static double AngleD(Vector2D v1, Vector2D v2) {
            return v1.AngleD(v2);
        }
        public double AngleR(Vector2D v2) {
            double fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return Math.Acos(fDot);
        }
        public static double AngleR(Vector2D v1, Vector2D v2) {
            return v1.AngleR(v2);
        }



		public double DistanceSquared(Vector2D v2) {
			double dx = v2.x-x, dy = v2.y-y;
			return dx*dx + dy*dy;
		}
        public double Distance(Vector2D v2) {
            double dx = v2.x-x, dy = v2.y-y;
			return Math.Sqrt(dx*dx + dy*dy);
		}


        public void Set(Vector2D o) {
            x = o.x; y = o.y;
        }
        public void Set(double fX, double fY) {
            x = fX; y = fY;
        }
        public void Add(Vector2D o) {
            x += o.x; y += o.y;
        }
        public void Subtract(Vector2D o) {
            x -= o.x; y -= o.y;
        }



		public static Vector2D operator -(Vector2D v) {
			return new Vector2D(-v.x, -v.y);
		}

        public static Vector2D operator+( Vector2D a, Vector2D o ) {
            return new Vector2D(a.x + o.x, a.y + o.y); 
        }
        public static Vector2D operator +(Vector2D a, double f) {
            return new Vector2D(a.x + f, a.y + f);
        }

        public static Vector2D operator-(Vector2D a, Vector2D o) {
            return new Vector2D(a.x - o.x, a.y - o.y);
        }
        public static Vector2D operator -(Vector2D a, double f) {
            return new Vector2D(a.x - f, a.y - f);
        }

        public static Vector2D operator *(Vector2D a, double f) {
            return new Vector2D(a.x * f, a.y * f);
        }
        public static Vector2D operator *(double f, Vector2D a) {
            return new Vector2D(a.x * f, a.y * f);
        }
        public static Vector2D operator /(Vector2D v, double f)
        {
            return new Vector2D(v.x / f, v.y / f);
        }
        public static Vector2D operator /(double f, Vector2D v)
        {
            return new Vector2D(f / v.x, f / v.y);
        }


		public static Vector2D operator *(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x * b.x, a.y * b.y);
		}
		public static Vector2D operator /(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x / b.x, a.y / b.y);
		}


        public static bool operator ==(Vector2D a, Vector2D b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return (a.x != b.x || a.y != b.y);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector2D)obj;
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
        public int CompareTo(Vector2D other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector2D other)
        {
            return (x == other.x && y == other.y);
        }


        public bool EpsilonEqual(Vector2D v2, double epsilon) {
            return Math.Abs(x - v2.x) < epsilon && 
                   Math.Abs(y - v2.y) < epsilon;
        }
        public bool PrecisionEqual(Vector2D v2, int nDigits)
        {
            return Math.Round(x, nDigits) == Math.Round(v2.x, nDigits) &&
                   Math.Round(y, nDigits) == Math.Round(v2.y, nDigits);
        }


        public static Vector2D Lerp(Vector2D a, Vector2D b, double t)
        {
            double s = 1 - t;
            return new Vector2D(s * a.x + t * b.x, s * a.y + t * b.y);
        }


        public override string ToString() {
            return string.Format("{0:F8} {1:F8}", x, y);
        }


        public static implicit operator Vector2D(Vector2F v)
        {
            return new Vector2D(v.x, v.y);
        }
        public static explicit operator Vector2F(Vector2D v)
        {
            return new Vector2F((float)v.x, (float)v.y);
        }

    }
}
