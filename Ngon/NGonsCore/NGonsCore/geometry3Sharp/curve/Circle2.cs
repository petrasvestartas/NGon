using System;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{
    public class Circle2d : IParametricCurve2d
    {
		public Vector2D Center;
		public double Radius;
		public bool IsReversed;		// use ccw orientation instead of cw

		public Circle2d(Vector2D center, double radius)
		{
			IsReversed = false;
			Center = center;
			Radius = radius;
		}


        public double Curvature
        {
            get { return 1.0 / Radius; }
        }
        public double SignedCurvature
        {
            get { return (IsReversed) ? (-1.0 / Radius) : (1.0 / Radius); }
        }


		public bool IsClosed {
			get { return true; }
		}

		public void Reverse() {
			IsReversed = ! IsReversed;
		}

        public IParametricCurve2d Clone() {
            return new Circle2d(this.Center, this.Radius) 
                { IsReversed = this.IsReversed };
        }

		// angle in range [0,360] (but works for any value, obviously)
        public Vector2D SampleDeg(double degrees)
        {
            double theta = degrees * math.MathUtil.Deg2Rad;
            double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
        }

		// angle in range [0,2pi] (but works for any value, obviously)
        public Vector2D SampleRad(double radians)
        {
            double c = Math.Cos(radians), s = Math.Sin(radians);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
        }


		public double ParamLength {
			get { return 1.0f; }
		}

		// t in range[0,1] spans circle [0,2pi]
		public Vector2D SampleT(double t) {
			double theta = (IsReversed) ? -t*math.MathUtil.TwoPI : t*math.MathUtil.TwoPI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
		}

        public Vector2D TangentT(double t)
        {
			double theta = (IsReversed) ? -t*math.MathUtil.TwoPI : t*math.MathUtil.TwoPI;
            Vector2D tangent = new Vector2D(-Math.Sin(theta), Math.Cos(theta));
            if (IsReversed)
                tangent = -tangent;
            tangent.Normalize();
            return tangent;
        }


		public bool HasArcLength { get {return true;} }

		public double ArcLength {
			get {
				return math.MathUtil.TwoPI * Radius;
			}
		}

		public Vector2D SampleArcLength(double a) {
			double t = a / ArcLength;
			double theta = (IsReversed) ? -t*math.MathUtil.TwoPI : t*math.MathUtil.TwoPI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
		}


        public bool Contains (Vector2D p ) {
            double d = Center.DistanceSquared(p);
            return d <= Radius * Radius;
        }


        public double Circumference {
			get { return math.MathUtil.TwoPI * Radius; }
            set { Radius = value / math.MathUtil.TwoPI; }
		}
        public double Diameter {
			get { return 2 * Radius; }
            set { Radius = value / 2; }
		}
        public double Area {
            get { return Math.PI * Radius * Radius; }
            set { Radius = Math.Sqrt(value / Math.PI); }
        }


        public double SignedDistance(Vector2D pt)
        {
            double d = Center.Distance(pt);
            return d - Radius;
        }
        public double Distance(Vector2D pt)
        {
            double d = Center.Distance(pt);
            return Math.Abs(d - Radius);
        }

    }
}
