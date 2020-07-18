namespace NGonsCore.geometry3Sharp.math
{
    //3D plane, based on WildMagic5 Wm5Plane3 class

    public struct Plane3d
    {
        public Vector3D Normal;
        public double Constant;

        public Plane3d(Vector3D normal, double constant)
        {
            Normal = normal;
            Constant = constant;
        }

         // N is specified, c = Dot(N,P) where P is a point on the plane.
        public Plane3d(Vector3D normal, Vector3D point)
        {
            Normal = normal;
            Constant = Normal.Dot(point);
        }

        // N = Cross(P1-P0,P2-P0)/Length(Cross(P1-P0,P2-P0)), c = Dot(N,P0) where
        // P0, P1, P2 are points on the plane.
        public Plane3d(Vector3D p0, Vector3D p1, Vector3D p2)
        {
            Vector3D edge1 = p1 - p0;
            Vector3D edge2 = p2 - p0;
            Normal = edge1.UnitCross(edge2);
            Constant = Normal.Dot(p0);
        }


        // Compute d = Dot(N,P)-c where N is the plane normal and c is the plane
        // constant.  This is a signed distance.  The sign of the return value is
        // positive if the point is on the positive side of the plane, negative if
        // the point is on the negative side, and zero if the point is on the
        // plane.
        public double DistanceTo(Vector3D p)
        {
            return Normal.Dot(p) - Constant;
        }

        // The "positive side" of the plane is the half space to which the plane
        // normal points.  The "negative side" is the other half space.  The
        // function returns +1 when P is on the positive side, -1 when P is on the
        // the negative side, or 0 when P is on the plane.
        public int WhichSide (Vector3D p)
        {
            double distance = DistanceTo(p);
            if (distance < 0)
                return -1;
            else if (distance > 0)
                return +1;
            else
                return 0;
        }

    }




    public struct Plane3f
    {
        public Vector3F Normal;
        public float Constant;

        public Plane3f(Vector3F normal, float constant)
        {
            Normal = normal;
            Constant = constant;
        }

         // N is specified, c = Dot(N,P) where P is a point on the plane.
        public Plane3f(Vector3F normal, Vector3F point)
        {
            Normal = normal;
            Constant = Normal.Dot(point);
        }

        // N = Cross(P1-P0,P2-P0)/Length(Cross(P1-P0,P2-P0)), c = Dot(N,P0) where
        // P0, P1, P2 are points on the plane.
        public Plane3f(Vector3F p0, Vector3F p1, Vector3F p2)
        {
            Vector3F edge1 = p1 - p0;
            Vector3F edge2 = p2 - p0;
            Normal = edge1.UnitCross(edge2);
            Constant = Normal.Dot(p0);
        }


        // Compute d = Dot(N,P)-c where N is the plane normal and c is the plane
        // constant.  This is a signed distance.  The sign of the return value is
        // positive if the point is on the positive side of the plane, negative if
        // the point is on the negative side, and zero if the point is on the
        // plane.
        public float DistanceTo(Vector3F p)
        {
            return Normal.Dot(p) - Constant;
        }

        // The "positive side" of the plane is the half space to which the plane
        // normal points.  The "negative side" is the other half space.  The
        // function returns +1 when P is on the positive side, -1 when P is on the
        // the negative side, or 0 when P is on the plane.
        public int WhichSide (Vector3F p)
        {
            float distance = DistanceTo(p);
            if (distance < 0)
                return -1;
            else if (distance > 0)
                return +1;
            else
                return 0;
        }

    }


}
