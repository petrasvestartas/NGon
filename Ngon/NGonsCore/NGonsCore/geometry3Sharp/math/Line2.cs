namespace NGonsCore.geometry3Sharp.math
{
    public struct Line2d
    {
        public Vector2D Origin;
        public Vector2D Direction;

        public Line2d(Vector2D origin, Vector2D direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        // parameter is distance along Line
        public Vector2D PointAt(double d) {
            return Origin + d * Direction;
        }

        public double Project(Vector2D p)
        {
            return (p - Origin).Dot(Direction);
        }

        public double DistanceSquared(Vector2D p)
        {
            double t = (p - Origin).Dot(Direction);
            Vector2D proj = Origin + t * Direction;
            return (proj - p).LengthSquared;
        }

        // conversion operators
        public static implicit operator Line2d(Line2f v)
        {
            return new Line2d(v.Origin, v.Direction);
        }
        public static explicit operator Line2f(Line2d v)
        {
            return new Line2f((Vector2F)v.Origin, (Vector2F)v.Direction);
        }


    }


    public struct Line2f
    {
        public Vector2F Origin;
        public Vector2F Direction;

        public Line2f(Vector2F origin, Vector2F direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        // parameter is distance along Line
        public Vector2F PointAt(float d)
        {
            return Origin + d * Direction;
        }

        public float Project(Vector2F p)
        {
            return (p - Origin).Dot(Direction);
        }

        public float DistanceSquared(Vector2F p)
        {
            float t = (p - Origin).Dot(Direction);
            Vector2F proj = Origin + t * Direction;
            return (proj - p).LengthSquared;
        }
    }
}
