using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.IsoSurface.Voxel
{
    public struct Vector
    {
        private const double ONETHIRD = 0.33333333333333331;

        public double x;

        public double y;

        public double z;

        public static Vector Origin = default(Vector);

        public static Vector Xaxis = new Vector(1.0, 0.0, 0.0);

        public static Vector Yaxis = new Vector(0.0, 1.0, 0.0);

        public static Vector Zaxis = new Vector(0.0, 0.0, 1.0);

        public double this[int i]
        {
            get
            {
                if (i == 0)
                {
                    return this.x;
                }
                if (i == 1)
                {
                    return this.y;
                }
                return this.z;
            }
            set
            {
                if (i == 0)
                {
                    this.x = value;
                    return;
                }
                if (i == 1)
                {
                    this.y = value;
                    return;
                }
                this.z = value;
            }
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
            set
            {
                double num = Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
                if (num == 0.0)
                {
                    return;
                }
                num = value / num;
                this.x *= num;
                this.y *= num;
                this.z *= num;
            }
        }

        public double LengthSquared
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public Vector Normalized
        {
            get
            {
                Vector result = new Vector(this.x, this.y, this.z);
                result.Normalize();
                return result;
            }
        }

        public Vector(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public void Zero()
        {
            this.x = 0.0;
            this.y = 0.0;
            this.z = 0.0;
        }

        public void eX()
        {
            this.x = 1.0;
            this.y = 0.0;
            this.z = 0.0;
        }

        public void eY()
        {
            this.x = 0.0;
            this.y = 1.0;
            this.z = 0.0;
        }

        public void eZ()
        {
            this.x = 0.0;
            this.y = 0.0;
            this.z = 1.0;
        }


        public void FindMin(Vector v)
        {
            if (this.x > v.x)
            {
                this.x = v.x;
            }
            if (this.y > v.y)
            {
                this.y = v.y;
            }
            if (this.z > v.z)
            {
                this.z = v.z;
            }
        }

        public void FindMax(Vector v)
        {
            if (this.x < v.x)
            {
                this.x = v.x;
            }
            if (this.y < v.y)
            {
                this.y = v.y;
            }
            if (this.z < v.z)
            {
                this.z = v.z;
            }
        }

        public void FindMin(double _x, double _y, double _z)
        {
            if (this.x > _x)
            {
                this.x = _x;
            }
            if (this.y > _y)
            {
                this.y = _y;
            }
            if (this.z > _z)
            {
                this.z = _z;
            }
        }

        public void FindMax(double _x, double _y, double _z)
        {
            if (this.x < _x)
            {
                this.x = _x;
            }
            if (this.y < _y)
            {
                this.y = _y;
            }
            if (this.z < _z)
            {
                this.z = _z;
            }
        }

        public void Set(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public void Set(Vector v, double scale)
        {
            this.x = v.x * scale;
            this.y = v.y * scale;
            this.z = v.z * scale;
        }

        public void Set(Vector v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public void Add(Vector v)
        {
            this.x += v.x;
            this.y += v.y;
            this.z += v.z;
        }

        public void Add(double _x, double _y, double _z)
        {
            this.x += _x;
            this.y += _y;
            this.z += _z;
        }

        public void Add(Vector v, double scale)
        {
            this.x += v.x * scale;
            this.y += v.y * scale;
            this.z += v.z * scale;
        }

        public void Sum(Vector v1, Vector v2)
        {
            this.x = v1.x + v2.x;
            this.y = v1.y + v2.y;
            this.z = v1.z + v2.z;
        }

        public void Sum(Vector v1, double m1, Vector v2, double m2)
        {
            this.x = v1.x * m1 + v2.x * m2;
            this.y = v1.y * m1 + v2.y * m2;
            this.z = v1.z * m1 + v2.z * m2;
        }

        public void Sum(Vector v1, Vector v2, double m2)
        {
            this.x = v1.x + v2.x * m2;
            this.y = v1.y + v2.y * m2;
            this.z = v1.z + v2.z * m2;
        }

        public void Difference(Vector v1, Vector v2)
        {
            this.x = v1.x - v2.x;
            this.y = v1.y - v2.y;
            this.z = v1.z - v2.z;
        }

        public void Subtract(Vector v)
        {
            this.x -= v.x;
            this.y -= v.y;
            this.z -= v.z;
        }

        public void Cross(Vector v1, Vector v2)
        {
            this.x = v1.y * v2.z - v1.z * v2.y;
            this.y = v1.z * v2.x - v1.x * v2.z;
            this.z = v1.x * v2.y - v1.y * v2.x;
        }

        public static Vector CrossProduct(Vector v1, Vector v2)
        {
            return new Vector(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }

        public double Dot(Vector v)
        {
            return this.x * v.x + this.y * v.y + this.z * v.z;
        }

        public static double operator *(Vector v1, Vector v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static Vector operator *(Vector v, double m)
        {
            return new Vector(v.x * m, v.y * m, v.z * m);
        }

        public static Vector operator *(double m, Vector v)
        {
            return new Vector(v.x * m, v.y * m, v.z * m);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector operator %(Vector v1, Vector v2)
        {
            return new Vector(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }

        public void Scale(double scalefactor)
        {
            this.x *= scalefactor;
            this.y *= scalefactor;
            this.z *= scalefactor;
        }

        public void Multiply(Vector v)
        {
            this.x *= v.x;
            this.y *= v.y;
            this.z *= v.z;
        }

        public void Reverse()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        public void ReverseOf(Vector v)
        {
            this.x = -v.x;
            this.y = -v.y;
            this.z = -v.z;
        }

        public void Restrict(Vector _min, Vector _max)
        {
            if (this.x < _min.x)
            {
                this.x = _min.x;
            }
            else if (this.x > _max.x)
            {
                this.x = _max.x;
            }
            if (this.y < _min.y)
            {
                this.y = _min.y;
            }
            else if (this.y > _max.y)
            {
                this.y = _max.y;
            }
            if (this.z < _min.z)
            {
                this.z = _min.z;
                return;
            }
            if (this.z > _max.z)
            {
                this.z = _max.z;
            }
        }

        public double Normalize()
        {
            double length = this.Length;
            if (length == 0.0)
            {
                return 0.0;
            }
            double num = 1.0 / length;
            this.x *= num;
            this.y *= num;
            this.z *= num;
            return length;
        }

        public double Normalize(double scale)
        {
            double length = this.Length;
            if (length == 0.0)
            {
                return 0.0;
            }
            double num = scale / length;
            this.x *= num;
            this.y *= num;
            this.z *= num;
            return length;
        }

        public void Abs()
        {
            this.x = Math.Abs(this.x);
            this.y = Math.Abs(this.y);
            this.z = Math.Abs(this.z);
        }

        public static double TriangleArea(Vector p0, Vector p1, Vector p2)
        {
            return Vector.CrossProduct(p1 - p0, p2 - p0).Length * 0.5;
        }

        public double DistanceTo(Vector v)
        {
            Vector c_vector = new Vector(this.x - v.x, this.y - v.y, this.z - v.z);
            return c_vector.Length;
        }

        public double DistanceToXY(Vector v)
        {
            return Math.Sqrt((this.x - v.x) * (this.x - v.x) + (this.y - v.y) * (this.y - v.y));
        }

        public double DistanceToSqr(Vector v)
        {
            Vector c_vector = new Vector(this.x - v.x, this.y - v.y, this.z - v.z);
            return c_vector.LengthSquared;
        }

        public bool IsEqual(Vector v, double tol = 0.0001)
        {
            return Math.Abs(this.x - v.x) < tol && Math.Abs(this.y - v.y) < tol && Math.Abs(this.z - v.z) < tol;
        }

        public void MatrixTransform(double[] ma, out Vector res)
        {
            res = default(Vector);
            res.x = this.x * ma[0] + this.y * ma[4] + this.z * ma[8] + ma[12];
            res.y = this.x * ma[1] + this.y * ma[5] + this.z * ma[9] + ma[13];
            res.z = this.x * ma[2] + this.y * ma[6] + this.z * ma[10] + ma[14];
        }

        public void MatrixTransform2D(double[] ma, ref Vector res)
        {
            res.x = this.x * ma[0] + this.y * ma[4] + this.z * ma[8] + ma[12];
            res.y = this.x * ma[1] + this.y * ma[5] + this.z * ma[9] + ma[13];
        }

        public void MatrixTransformP(double[] ma, out Vector res)
        {
            res = default(Vector);
            double num = 1.0 / ma[15];
            res.x = (this.x * ma[0] + this.y * ma[4] + this.z * ma[8] + ma[12]) * num;
            res.y = (this.x * ma[1] + this.y * ma[5] + this.z * ma[9] + ma[13]) * num;
            res.z = (this.x * ma[2] + this.y * ma[6] + this.z * ma[10] + ma[14]) * num;
        }

        public Vector Mid(Vector v)
        {
            return new Vector((this.x + v.x) * 0.5, (this.y + v.y) * 0.5, (this.z + v.z) * 0.5);
        }

        public void Mid(Vector va, Vector vb)
        {
            this.x = (va.x + vb.x) * 0.5;
            this.y = (va.y + vb.y) * 0.5;
            this.z = (va.z + vb.z) * 0.5;
        }

        public static Vector MidPoint(Vector va, Vector vb)
        {
            return new Vector((va.x + vb.x) * 0.5, (va.y + vb.y) * 0.5, (va.z + vb.z) * 0.5);
        }

        public void Normal3P(Vector va, Vector vb, Vector vc)
        {
            this.Cross(vb - va, vc - va);
        }

        public bool IsInsideOrderedPair(Vector va, Vector vb)
        {
            return this.x >= va.x && this.x <= vb.x && this.y >= va.y && this.y <= vb.y && this.z >= va.z && this.z <= vb.z;
        }

        public static Vector Centroid(Vector va, Vector vb, Vector vc)
        {
            return new Vector((vc.x + va.x + vb.x) * 0.33333333333333331, (vc.y + va.y + vb.y) * 0.33333333333333331, (vc.z + va.z + vb.z) * 0.33333333333333331);
        }

        private void MakeCentroid(Vector va, Vector vb, Vector vc)
        {
            this.x = (vc.x + va.x + vb.x) * 0.33333333333333331;
            this.y = (vc.y + va.y + vb.y) * 0.33333333333333331;
            this.z = (vc.z + va.z + vb.z) * 0.33333333333333331;
        }

        public Vector ProjectionPoint(Vector va, Vector vb)
        {
            Vector c_vector = vb - va;
            c_vector.Normalize();
            c_vector *= (this - va) * c_vector;
            return c_vector + va;
        }

        private void Interpolation(Vector iv0, Vector iv1, double s0, double s1)
        {
            this.x = iv0.x * s0 + iv1.x * s1;
            this.y = iv0.y * s0 + iv1.y * s1;
            this.z = iv0.z * s0 + iv1.z * s1;
        }

        private void AddBestDirection(Vector _v0, Vector _v1)
        {
            double num = this.Dot(_v0);
            double num2 = this.Dot(_v1);
            if (Math.Abs(num) > Math.Abs(num2))
            {
                if (num > 0.0)
                {
                    this.Add(_v0);
                    return;
                }
                this.Subtract(_v0);
                return;
            }
            else
            {
                if (num2 > 0.0)
                {
                    this.Add(_v1);
                    return;
                }
                this.Subtract(_v1);
                return;
            }
        }

        private void SetToBestDirection(Vector _v0, Vector _v1)
        {
            double num = this.Dot(_v0);
            double num2 = this.Dot(_v1);
            if (Math.Abs(num) > Math.Abs(num2))
            {
                if (num > 0.0)
                {
                    this.Set(_v0.x, _v0.y, _v0.z);
                    return;
                }
                this.Set(-_v0.x, -_v0.y, -_v0.z);
                return;
            }
            else
            {
                if (num2 > 0.0)
                {
                    this.Set(_v1.x, _v1.y, _v1.z);
                    return;
                }
                this.Set(-_v1.x, -_v1.y, -_v1.z);
                return;
            }
        }

        private void Reflect(Vector _axis)
        {
            Vector v = this;
            double num = v * _axis;
            this.x = 2.0 * num * _axis.x - v.x;
            this.y = 2.0 * num * _axis.y - v.y;
            this.z = 2.0 * num * _axis.z - v.z;
        }

        public static implicit operator string(Vector v)
        {
            return string.Concat(new string[]
            {
                "[",
                v.x.ToString(),
                ",",
                v.y.ToString(),
                ",",
                v.z.ToString(),
                "]"
            });
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "[",
                this.x.ToString(),
                ",",
                this.y.ToString(),
                ",",
                this.z.ToString(),
                "]"
            });
        }
    }
}
