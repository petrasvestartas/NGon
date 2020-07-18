﻿using System;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve {

	public class Arc2d : IParametricCurve2d
	{
		public Vector2D Center;
		public double Radius;
		public double AngleStartDeg;
		public double AngleEndDeg;
		public bool IsReversed;		// use ccw orientation instead of cw
		

		public Arc2d(Vector2D center, double radius, double startDeg, double endDeg)
		{
			IsReversed = false;
			Center = center;
			Radius = radius;
			AngleStartDeg = startDeg;
			AngleEndDeg = endDeg;
			if ( AngleEndDeg < AngleStartDeg )
				AngleEndDeg += 360;

			// [TODO] handle full arcs, which should be circles?
		}


		public Vector2D P0 {
			get { return SampleT(0.0); }
		}
		public Vector2D P1 {
			get { return SampleT(1.0); }
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
			get { return false; }
		}


		public double ParamLength {
			get { return 1.0f; }
		}


		// t in range[0,1] spans arc
		public Vector2D SampleT(double t) {
			double theta = (IsReversed) ?
				(1-t)*AngleEndDeg + (t)*AngleStartDeg : 
				(1-t)*AngleStartDeg + (t)*AngleEndDeg;
			theta = theta * math.MathUtil.Deg2Rad;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
		}


        public Vector2D TangentT(double t)
        {
			double theta = (IsReversed) ?
				(1-t)*AngleEndDeg + (t)*AngleStartDeg : 
				(1-t)*AngleStartDeg + (t)*AngleEndDeg;
			theta = theta * math.MathUtil.Deg2Rad;
            Vector2D tangent = new Vector2D(-Math.Sin(theta), Math.Cos(theta));
            if (IsReversed)
                tangent = -tangent;
            tangent.Normalize();
            return tangent;
        }


		public bool HasArcLength { get {return true;} }

		public double ArcLength {
			get {
				return (AngleEndDeg-AngleStartDeg) * math.MathUtil.Deg2Rad * Radius;
			}
		}

		public Vector2D SampleArcLength(double a) {
            if (ArcLength < math.MathUtil.Epsilon)
                return (a < 0.5) ? SampleT(0) : SampleT(1);
			double t = a / ArcLength;
			double theta = (IsReversed) ?
				(1-t)*AngleEndDeg + (t)*AngleStartDeg : 
				(1-t)*AngleStartDeg + (t)*AngleEndDeg;
			theta = theta * math.MathUtil.Deg2Rad;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2D(Center.x + Radius*c, Center.y + Radius*s);
		}

		public void Reverse() {
			IsReversed = ! IsReversed;
		}

        public IParametricCurve2d Clone() {
            return new Arc2d(this.Center, this.Radius, this.AngleStartDeg, this.AngleEndDeg) 
                { IsReversed = this.IsReversed };
        }



        public double Distance(Vector2D point)
        {
            Vector2D PmC = point - Center;
            double lengthPmC = PmC.Length;
            if (lengthPmC > math.MathUtil.Epsilon) {
                Vector2D dv = PmC / lengthPmC;
				double theta = Math.Atan2(dv.y, dv.x) * math.MathUtil.Rad2Deg;
				if ( ! (theta >= AngleStartDeg && theta <= AngleEndDeg) ) {
					double ctheta = math.MathUtil.ClampAngleDeg(theta, AngleStartDeg, AngleEndDeg);
                    double radians = ctheta * math.MathUtil.Deg2Rad;
					double c = Math.Cos(radians), s = Math.Sin(radians);
                    Vector2D pos = new Vector2D(Center.x + Radius * c, Center.y + Radius * s);
					return pos.Distance(point);
                } else {
					return Math.Abs(lengthPmC - Radius);
                }
            } else {
                return Radius;
            }
        }


        public Vector2D NearestPoint(Vector2D point)
        {
            Vector2D PmC = point - Center;
            double lengthPmC = PmC.Length;
            if (lengthPmC > math.MathUtil.Epsilon) {
                Vector2D dv = PmC / lengthPmC;
                double theta = Math.Atan2(dv.y, dv.x);
                theta *= math.MathUtil.Rad2Deg;
                theta = math.MathUtil.ClampAngleDeg(theta, AngleStartDeg, AngleEndDeg);
                theta = math.MathUtil.Deg2Rad * theta;
                double c = Math.Cos(theta), s = Math.Sin(theta);
                return new Vector2D(Center.x + Radius * c, Center.y + Radius * s);
            } else 
                return SampleT(0.5);        // all points equidistant
        }


	}
}
