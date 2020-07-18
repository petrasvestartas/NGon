namespace NGonsCore.geometry3Sharp.math
{
    public struct Line3d
    {
        public Vector3D Origin;
        public Vector3D Direction;

        public Line3d(Vector3D origin, Vector3D direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        // parameter is distance along Line
        public Vector3D PointAt(double d) {
            return Origin + d * Direction;
        }

        public double Project(Vector3D p)
        {
            return (p - Origin).Dot(Direction);
        }

        public double DistanceSquared(Vector3D p)
        {
            double t = (p - Origin).Dot(Direction);
            Vector3D proj = Origin + t * Direction;
            return (proj - p).LengthSquared;
        }

        public Vector3D ClosestPoint(Vector3D p)
        {
            double t = (p - Origin).Dot(Direction);
            return Origin + t * Direction;
        }

        // conversion operators
        public static implicit operator Line3d(Line3f v)
        {
            return new Line3d(v.Origin, v.Direction);
        }
        public static explicit operator Line3f(Line3d v)
        {
            return new Line3f((Vector3F)v.Origin, (Vector3F)v.Direction);
        }


    }


    public struct Line3f
    {
        public Vector3F Origin;
        public Vector3F Direction;

        public Line3f(Vector3F origin, Vector3F direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        // parameter is distance along Line
        public Vector3F PointAt(float d)
        {
            return Origin + d * Direction;
        }

        public float Project(Vector3F p)
        {
            return (p - Origin).Dot(Direction);
        }

        public float DistanceSquared(Vector3F p)
        {
            float t = (p - Origin).Dot(Direction);
            Vector3F proj = Origin + t * Direction;
            return (proj - p).LengthSquared;
        }

        public Vector3F ClosestPoint(Vector3F p)
        {
            float t = (p - Origin).Dot(Direction);
            return Origin + t * Direction;
        }
    }
}
