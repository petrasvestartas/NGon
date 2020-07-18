namespace NGonsCore.geometry3Sharp.math
{
    // These are convenience classes used in place of local stack arrays
    // (which C# does not support, but is common in C++ code)


    public struct Vector3dTuple3
    {
        public Vector3D V0, V1, V2;

        public Vector3dTuple3(Vector3D v0, Vector3D v1, Vector3D v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector3D this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }
    }



    public struct Vector3fTuple3
    {
        public Vector3F V0, V1, V2;

        public Vector3fTuple3(Vector3F v0, Vector3F v1, Vector3F v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector3F this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }
    }




    public struct Vector2dTuple2
    {
        public Vector2D V0, V1;

        public Vector2dTuple2(Vector2D v0, Vector2D v1)
        {
            V0 = v0; V1 = v1;
        }

        public Vector2D this[int key]
        {
            get { return (key == 0) ? V0 : V1; }
            set { if (key == 0) V0 = value; else V1 = value; }
        }
    }


    public struct Vector2dTuple3
    {
        public Vector2D V0, V1, V2;

        public Vector2dTuple3(Vector2D v0, Vector2D v1, Vector2D v2)
        {
            V0 = v0; V1 = v1; V2 = v2;
        }

        public Vector2D this[int key]
        {
            get { return (key == 0) ? V0 : (key == 1) ? V1 : V2; }
            set { if (key == 0) V0 = value; else if (key == 1) V1 = value; else V2 = value; }
        }
    }

}
