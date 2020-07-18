namespace NGonsCore.geometry3Sharp.math
{
    public struct Triangle2d
    {
        public Vector2D V0, V1, V2;

        public Triangle2d(Vector2D v0, Vector2D v1, Vector2D v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector2D this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }

        public Vector2D PointAt(double bary0, double bary1, double bary2)
        {
            return bary0 * V0 + bary1 * V1 + bary2 * V2;
        }
        public Vector2D PointAt(Vector3D bary)
        {
            return bary.x* V0 + bary.y* V1 + bary.z* V2;
        }

        // conversion operators
        public static implicit operator Triangle2d(Triangle2f v)
        {
            return new Triangle2d(v.V0, v.V1, v.V2);
        }
        public static explicit operator Triangle2f(Triangle2d v)
        {
            return new Triangle2f((Vector2F)v.V0, (Vector2F)v.V1, (Vector2F)v.V2);
        }
    }



    public struct Triangle2f
    {
        public Vector2F V0, V1, V2;

        public Triangle2f(Vector2F v0, Vector2F v1, Vector2F v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector2F this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }


        public Vector2F PointAt(float bary0, float bary1, float bary2)
        {
            return bary0 * V0 + bary1 * V1 + bary2 * V2;
        }
        public Vector2F PointAt(Vector3F bary)
        {
            return bary.x * V0 + bary.y * V1 + bary.z * V2;
        }
    }

}
