namespace NGonsCore.geometry3Sharp.math
{
    public struct Triangle3d
    {
        public Vector3D V0, V1, V2;

        public Triangle3d(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector3D this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }

        public Vector3D PointAt(double bary0, double bary1, double bary2)
        {
            return bary0 * V0 + bary1 * V1 + bary2 * V2;
        }
        public Vector3D PointAt(Vector3D bary)
        {
            return bary.x* V0 + bary.y* V1 + bary.z* V2;
        }

        // conversion operators
        public static implicit operator Triangle3d(Triangle3f v)
        {
            return new Triangle3d(v.V0, v.V1, v.V2);
        }
        public static explicit operator Triangle3f(Triangle3d v)
        {
            return new Triangle3f((Vector3F)v.V0, (Vector3F)v.V1, (Vector3F)v.V2);
        }
    }



    public struct Triangle3f
    {
        public Vector3F V0, V1, V2;

        public Triangle3f(Vector3F v0, Vector3F v1, Vector3F v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector3F this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }


        public Vector3F PointAt(float bary0, float bary1, float bary2)
        {
            return bary0 * V0 + bary1 * V1 + bary2 * V2;
        }
        public Vector3F PointAt(Vector3F bary)
        {
            return bary.x * V0 + bary.y * V1 + bary.z * V2;
        }
    }

}
