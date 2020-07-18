using NGonsCore.geometry3Sharp.curve;

namespace NGonsCore.geometry3Sharp.math
{
	public struct Segment3d : IParametricCurve3d
    {
        // Center-direction-extent representation.
        // Extent is half length of segment
        public Vector3D Center;
        public Vector3D Direction;
        public double Extent;

        public Segment3d(Vector3D p0, Vector3D p1) {
            //update_from_endpoints(p0, p1);
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5 * Direction.Normalize();
        }
        public Segment3d(Vector3D center, Vector3D direction, double extent) {
            Center = center; Direction = direction; Extent = extent;
        }

        public void SetEndpoints(Vector3D p0, Vector3D p1) {
            update_from_endpoints(p0, p1);
        }


        public Vector3D P0 {
            get { return Center - Extent * Direction; }
            set { update_from_endpoints(value, P1); }
        }
        public Vector3D P1 {
            get { return Center + Extent * Direction; }
            set { update_from_endpoints(P0, value); }
        }
        public double Length {
            get { return 2 * Extent; }
        }

        // parameter is signed distance from center in direction
        public Vector3D PointAt(double d) {
            return Center + d * Direction;
        }

        // t ranges from [0,1] over [P0,P1]
        public Vector3D PointBetween(double t) {
            return Center + (2 * t - 1) * Extent * Direction;
        }


		public double DistanceSquared(Vector3D p)
		{
			double t = (p - Center).Dot(Direction);
			if ( t >= Extent )
				return P1.DistanceSquared(p);
			else if ( t <= -Extent )
				return P0.DistanceSquared(p);
			Vector3D proj = Center + t * Direction;
			return (proj - p).LengthSquared;
		}

        public Vector3D NearestPoint(Vector3D p)
        {
			double t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1;
            if (t <= -Extent)
                return P0;
			return Center + t * Direction;
        }

        public double Project(Vector3D p)
        {
            return (p - Center).Dot(Direction);
        }


        void update_from_endpoints(Vector3D p0, Vector3D p1) {
            Center = 0.5 * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5* Direction.Normalize();
        }


        // conversion operators
        public static implicit operator Segment3d(Segment3f v)
        {
            return new Segment3d(v.Center, v.Direction, v.Extent);
        }
        public static explicit operator Segment3f(Segment3d v)
        {
            return new Segment3f((Vector3F)v.Center, (Vector3F)v.Direction, (float)v.Extent);
        }


		// IParametricCurve3d interface

		public bool IsClosed { get { return false; } }

		public double ParamLength { get { return 1.0f; } }

		// t in range[0,1] spans arc
		public Vector3D SampleT(double t) {
			return Center + (2 * t - 1) * Extent * Direction;
		}

		public Vector3D TangentT(double t) {
			return Direction;
		}

		public bool HasArcLength { get { return true; } }
		public double ArcLength { get { return 2*Extent; } }

		public Vector3D SampleArcLength(double a) {
			return P0 + a * Direction;
		}

		public void Reverse() {
			update_from_endpoints(P1,P0);
		}

		public IParametricCurve3d Clone() {
			return new Segment3d(this.Center, this.Direction, this.Extent);
		}


    }



    public struct Segment3f
    {
        // Center-direction-extent representation.
        // Extent is half length of segment
        public Vector3F Center;
        public Vector3F Direction;
        public float Extent;

        public Segment3f(Vector3F p0, Vector3F p1)
        {
            //update_from_endpoints(p0, p1);
            Center = 0.5f * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5f * Direction.Normalize();
        }
        public Segment3f(Vector3F center, Vector3F direction, float extent)
        {
            Center = center; Direction = direction; Extent = extent;
        }


        public void SetEndpoints(Vector3F p0, Vector3F p1) {
            update_from_endpoints(p0, p1);
        }


        public Vector3F P0
        {
            get { return Center - Extent * Direction; }
            set { update_from_endpoints(value, P1); }
        }
        public Vector3F P1
        {
            get { return Center + Extent * Direction; }
            set { update_from_endpoints(P0, value); }
        }
        public float Length {
            get { return 2 * Extent; }
        }

        // parameter is signed distance from center in direction
        public Vector3F PointAt(float d) {
            return Center + d * Direction;
        }


        // t ranges from [0,1] over [P0,P1]
        public Vector3F PointBetween(float t) {
            return Center + (2 * t - 1) * Extent * Direction;
        }


		public float DistanceSquared(Vector3F p)
		{
			float t = (p - Center).Dot(Direction);
			if ( t >= Extent )
				return P1.DistanceSquared(p);
			else if ( t <= -Extent )
				return P0.DistanceSquared(p);
			Vector3F proj = Center + t * Direction;
			return (proj - p).LengthSquared;
		}

        public Vector3F NearestPoint(Vector3F p)
        {
			float t = (p - Center).Dot(Direction);
            if (t >= Extent)
                return P1;
            if (t <= -Extent)
                return P0;
			return Center + t * Direction;
        }


        public float Project(Vector3F p)
        {
            return (p - Center).Dot(Direction);
        }




        void update_from_endpoints(Vector3F p0, Vector3F p1)
        {
            Center = 0.5f * (p0 + p1);
            Direction = p1 - p0;
            Extent = 0.5f * Direction.Normalize();
        }
    }

}
