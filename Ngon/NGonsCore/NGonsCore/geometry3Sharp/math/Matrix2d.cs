﻿using System;

namespace NGonsCore.geometry3Sharp.math
{
    // some functions ported from WildMagic5 Matrix2
    public class Matrix2d
    {
        public double m00, m01, m10, m11;


        public static readonly Matrix2d Identity = new Matrix2d(true);
        public static readonly Matrix2d Zero = new Matrix2d(false);
        public static readonly Matrix2d One = new Matrix2d(1, 1, 1, 1);


        public Matrix2d(bool bIdentity) {
            if (bIdentity) {
                m00 = m11 = 1;
                m01 = m10 = 0;
            } else
                m00 = m01 = m10 = m11 = 0;
        }
        public Matrix2d(double m00, double m01, double m10, double m11) {
            this.m00 = m00; this.m01 = m01; this.m10 = m10; this.m11 = m11;
        }
        public Matrix2d(double m00, double m11) {
            this.m00 = m00; this.m11 = m11; this.m01 = this.m10 = 0;
        }

        // Create a rotation matrix (positive angle -> counterclockwise).
        public Matrix2d(double radians) {
            SetToRotationRad(radians);
        }

        // Create matrices based on vector input.  The bool is interpreted as
        //   true: vectors are columns of the matrix
        //   false: vectors are rows of the matrix
        public Matrix2d(Vector2D u, Vector2D v, bool columns)
        {
            if (columns) {
                m00 = u.x; m01 = v.x; m10 = u.y; m11 = v.y;
            } else {
                m00 = u.x; m01 = u.y; m10 = v.x; m11 = v.y;
            }
        }

        // Create a tensor product U*V^T.
        public Matrix2d(Vector2D u, Vector2D v) {
            m00 = u.x * v.x;
            m01 = u.x * v.y;
            m10 = u.y * v.x;
            m11 = u.y * v.y;
        }


        public void SetToDiagonal(double m00, double m11) {
            this.m00 = m00; this.m11 = m11;
            this.m01 = this.m10 = 0;
        }

        public void SetToRotationRad(double angleRad) {
            m11 = m00 = Math.Cos(angleRad);
            m10 = Math.Sin(angleRad);
            m01 = -m10;
        }
        public void SetToRotationDeg(double angleDeg) {
            SetToRotationRad(MathUtil.Deg2Rad * angleDeg);
        }


        // u^T*M*v
        public double QForm(Vector2D u, Vector2D v) {
            return u.Dot(this * v);
        }


        public Matrix2d Transpose() {
            return new Matrix2d(m00, m10, m01, m11);
        }

        // Other operations.
        public Matrix2d Inverse(double epsilon = 0)
        {
            double det = m00 * m11 - m10 * m01;
            if (Math.Abs(det) > epsilon) {
                double invDet = 1.0 / det;
                return new Matrix2d(m11 * invDet, -m01 * invDet,
                                    -m10 * invDet, m00 * invDet);
            } else
                return Zero;
        }
        public Matrix2d Adjoint() {
            return new Matrix2d(m11, -m01, -m10, m00);
        }
        public double Determinant {
            get { return m00 * m11 - m01 * m10; }
        }


        public double ExtractAngle () {
            // assert:  'this' matrix represents a rotation
            return Math.Atan2(m10, m00);
        }


        public void Orthonormalize ()
        {
            // Algorithm uses Gram-Schmidt orthogonalization.  If 'this' matrix is
            // M = [m0|m1], then orthonormal output matrix is Q = [q0|q1],
            //
            //   q0 = m0/|m0|
            //   q1 = (m1-(q0*m1)q0)/|m1-(q0*m1)q0|
            //
            // where |V| indicates length of vector V and A*B indicates dot
            // product of vectors A and B.

            // Compute q0.
            double invLength = 1.0 / Math.Sqrt(m00 * m00 + m10 * m10);

            m00 *= invLength;
            m10 *= invLength;

            // Compute q1.
            double dot0 = m00 * m01 + m10 * m11;
            m01 -= dot0 * m00;
            m11 -= dot0 * m10;

            invLength = 1.0 / Math.Sqrt(m01 * m01 + m11 * m11);

            m01 *= invLength;
            m11 *= invLength;
        }


        public void EigenDecomposition (ref Matrix2d rot, ref Matrix2d diag)
        {
            double sum = Math.Abs(m00) + Math.Abs(m11);
            if (Math.Abs(m01) + sum == sum) {
                // The matrix M is diagonal (within numerical round-off).
                rot.m00 = (double)1;
                rot.m01 = (double)0;
                rot.m10 = (double)0;
                rot.m11 = (double)1;
                diag.m00 = m00;
                diag.m01 = (double)0;
                diag.m10 = (double)0;
                diag.m11 = m11;
                return;
            }

            double trace = m00 + m11;
            double diff = m00 - m11;
            double discr = Math.Sqrt(diff * diff + ((double)4) * m01 * m01);
            double eigVal0 = 0.5 * (trace - discr);
            double eigVal1 = 0.5 * (trace + discr);
            diag.SetToDiagonal(eigVal0, eigVal1);

            double cs, sn;
            if (diff >= 0.0) {
                cs = m01;
                sn = eigVal0 - m00;
            } else {
                cs = eigVal0 - m11;
                sn = m01;
            }
            double invLength = 1.0 / Math.Sqrt(cs * cs + sn * sn);
            cs *= invLength;
            sn *= invLength;

            rot.m00 = cs;
            rot.m01 = -sn;
            rot.m10 = sn;
            rot.m11 = cs;
        }




		public static Matrix2d operator -(Matrix2d v) {
            return new Matrix2d(-v.m00, -v.m01, -v.m10, -v.m11);
		}

        public static Matrix2d operator+( Matrix2d a, Matrix2d o ) {
            return new Matrix2d(a.m00 + o.m00, a.m01 + o.m01, a.m10 + o.m10, a.m11 + o.m11);
        }
        public static Matrix2d operator +(Matrix2d a, double f) {
            return new Matrix2d(a.m00 + f, a.m01 + f, a.m10 + f, a.m11 + f);
        }

        public static Matrix2d operator-(Matrix2d a, Matrix2d o) {
            return new Matrix2d(a.m00 - o.m00, a.m01 - o.m01, a.m10 - o.m10, a.m11 - o.m11);
        }
        public static Matrix2d operator -(Matrix2d a, double f) {
            return new Matrix2d(a.m00 - f, a.m01 - f, a.m10 - f, a.m11 - f);
        }

        public static Matrix2d operator *(Matrix2d a, double f) {
            return new Matrix2d(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
        }
        public static Matrix2d operator *(double f, Matrix2d a) {
            return new Matrix2d(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
        }
        public static Matrix2d operator /(Matrix2d a, double f)
        {
            return new Matrix2d(a.m00 / f, a.m01 / f, a.m10 / f, a.m11 / f);
        }


        // row*vector multiply
        public static Vector2D operator*(Matrix2d m, Vector2D v) {
            return new Vector2D( m.m00*v.x + m.m01*v.y,
                                 m.m10*v.x + m.m11*v.y );
        }

        // vector*column multiply
        public static Vector2D operator*(Vector2D v, Matrix2d m) {
            return new Vector2D( v.x * m.m00 + v.y * m.m10,
                                 v.x * m.m01 + v.y * m.m11 );
        }

    }
}
