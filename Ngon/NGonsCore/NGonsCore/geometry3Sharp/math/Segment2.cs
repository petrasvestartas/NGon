using NGonsCore.geometry3Sharp.curve;

namespace NGonsCore.geometry3Sharp.math
{
	public struct Segment2d : IParametricCurve2d
    {
        // Center-direction-extent representation.
        public Vector2D Center;
        public Vector2D Direction;
        public double Extent;

        public Segment2d(Vector2D p0, Vector2D p1)
        {
            //update_from_endpoints(p0, p1);
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5 * Direction.Normalize();
        }
        public Segment2d(Vector2D center, Vector2D direction, double extent)
        {
            Center = center; Direction = direction; Extent = extent;
        }

        public Vector2D P0
        {
            get { return Center - Extent * Direction; }
            set { update_from_endpoints(value, P1); }
        }
        public Vector2D P1
        {
            get { return Center + Extent * Direction; }
            set { update_from_endpoints(P0, value); }
        }
        public double Length {
            get { return 2 * Extent; }
        }

		public Vector2D Endpoint(int i) {
			return (i == 0) ? (Center - Extent * Direction) : (Center + Extent * Direction);
		}

		// parameter is signed distance from center in direction
		public Vector2D PointAt(double d) {
			return Center + d * Direction;
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector2D PointBetween(double t) {
			return Center + (2 * t - 1) * Extent * Direction;
		}

		public double DistanceSquared(Vector2D p)
		{
			double t = (p - Center).Dot(Direction);
			if ( t >= Extent )
				return P1.DistanceSquared(p);
			else if ( t <= -Extent )
				return P0.DistanceSquared(p);
			Vector2D proj = Center + t * Direction;
			return (proj - p).LengthSquared;
		}

        public Vector2D NearestPoint(Vector2D p)
        {
			double t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1;
            if (t <= -Extent)
                return P0;
			return Center + t * Direction;
        }

        public double Project(Vector2D p)
        {
            return (p - Center).Dot(Direction);
        }

        void update_from_endpoints(Vector2D p0, Vector2D p1)
        {
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5 * Direction.Normalize();
        }


		// IParametricCurve2d interface

		public bool IsClosed { get { return false; } }

		public double ParamLength { get { return 1.0f; } }

		// t in range[0,1] spans arc
		public Vector2D SampleT(double t) {
			return Center + (2 * t - 1) * Extent * Direction;
		}

		public Vector2D TangentT(double t) {
            return Direction;
		}

		public bool HasArcLength { get { return true; } }
		public double ArcLength { get { return 2*Extent; } }

		public Vector2D SampleArcLength(double a) {
			return P0 + a * Direction;
		}

		public void Reverse() {
			update_from_endpoints(P1,P0);
		}

        public IParametricCurve2d Clone() {
            return new Segment2d(this.Center, this.Direction, this.Extent);
        }
    }



    public struct Segment2f
    {
        // Center-direction-extent representation.
        public Vector2F Center;
        public Vector2F Direction;
        public float Extent;

        public Segment2f(Vector2F p0, Vector2F p1)
        {
            //update_from_endpoints(p0, p1);
            Center = 0.5f * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5f * Direction.Normalize();
        }
        public Segment2f(Vector2F center, Vector2F direction, float extent)
        {
            Center = center; Direction = direction; Extent = extent;
        }

        public Vector2F P0
        {
            get { return Center - Extent * Direction; }
            set { update_from_endpoints(value, P1); }
        }
        public Vector2F P1
        {
            get { return Center + Extent * Direction; }
            set { update_from_endpoints(P0, value); }
        }
        public float Length {
            get { return 2 * Extent; }
        }


		// parameter is signed distance from center in direction
		public Vector2F PointAt(float d) {
			return Center + d * Direction;
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector2F PointBetween(float t) {
			return Center + (2.0f * t - 1.0f) * Extent * Direction;
		}

		public float DistanceSquared(Vector2F p)
		{
			float t = (p - Center).Dot(Direction);
			if ( t >= Extent )
				return P1.DistanceSquared(p);
			else if ( t <= -Extent )
				return P0.DistanceSquared(p);
			Vector2F proj = Center + t * Direction;
			return (proj - p).LengthSquared;
		}

        public Vector2F NearestPoint(Vector2F p)
        {
			float t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1;
            if (t <= -Extent)
                return P0;
			return Center + t * Direction;
        }

        public float Project(Vector2F p)
        {
            return (p - Center).Dot(Direction);
        }



        void update_from_endpoints(Vector2F p0, Vector2F p1)
        {
            Center = 0.5f * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5f * Direction.Normalize();
        }
    }

}
