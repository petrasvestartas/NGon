using System;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public static class MeshTransforms
    {

        public static void Translate(IDeformableMesh mesh, Vector3D v) {
            Translate(mesh, v.x, v.y, v.z);
        }
        public static void Translate(IDeformableMesh mesh, double tx, double ty, double tz)
        {
            int NV = mesh.MaxVertexID;
            for ( int vid = 0; vid < NV; ++vid ) {
                if (mesh.IsVertex(vid)) {
                    Vector3D v = mesh.GetVertex(vid);
                    v.x += tx; v.y += ty; v.z += tz;
                    mesh.SetVertex(vid, v);
                }
            }
        }


        public static Vector3D Rotate(Vector3D pos, Vector3D origin, Quaternionf rotation)
        {
            Vector3D v = pos - origin;
            v = (Vector3D)(rotation * (Vector3F)v);
            v += origin;
            return v;
        }
        public static Frame3f Rotate(Frame3f f, Vector3D origin, Quaternionf rotation)
        {
            f.Origin -= (Vector3F)origin;
            f.Rotate(rotation);
            f.Origin += (Vector3F)origin;
            return f;
        }
        public static void Rotate(IDeformableMesh mesh, Vector3D origin, Quaternionf rotation)
        {
            int NV = mesh.MaxVertexID;
            for ( int vid = 0; vid < NV; ++vid ) {
                if (mesh.IsVertex(vid)) {
                    Vector3D v = mesh.GetVertex(vid);
                    v -= origin;
                    v = (Vector3D)(rotation * (Vector3F)v);
                    v += origin;
                    mesh.SetVertex(vid, v);
                }
            }
        }

        public static void Scale(IDeformableMesh mesh, double sx, double sy, double sz)
        {
            int NV = mesh.MaxVertexID;
            for ( int vid = 0; vid < NV; ++vid ) {
                if (mesh.IsVertex(vid)) {
                    Vector3D v = mesh.GetVertex(vid);
                    v.x *= sx; v.y *= sy; v.z *= sz;
                    mesh.SetVertex(vid, v);
                }
            }
        }
        public static void Scale(IDeformableMesh mesh, double s)
        {
            Scale(mesh, s, s, s);
        }


        public static void ToFrame(IDeformableMesh mesh, Frame3f f)
        {
            int NV = mesh.MaxVertexID;
            bool bHasNormals = mesh.HasVertexNormals;
            for ( int vid = 0; vid < NV; ++vid ) {
                if (mesh.IsVertex(vid)) {
                    Vector3D v = mesh.GetVertex(vid);
                    Vector3D vf = f.ToFrameP((Vector3F)v);
                    mesh.SetVertex(vid, vf);
                    if ( bHasNormals ) {
                        Vector3F n = mesh.GetVertexNormal(vid);
                        Vector3F nf = f.ToFrameV(n);
                        mesh.SetVertexNormal(vid, nf);
                    }
                }
            }
        }
        public static void FromFrame(IDeformableMesh mesh, Frame3f f)
        {
            int NV = mesh.MaxVertexID;
            bool bHasNormals = mesh.HasVertexNormals;
            for ( int vid = 0; vid < NV; ++vid ) {
                if (mesh.IsVertex(vid)) {
                    Vector3D vf = mesh.GetVertex(vid);
                    Vector3D v = f.FromFrameP((Vector3F)vf);
                    mesh.SetVertex(vid, v);
                    if ( bHasNormals ) {
                        Vector3F n = mesh.GetVertexNormal(vid);
                        Vector3F nf = f.FromFrameV(n);
                        mesh.SetVertexNormal(vid, nf);
                    }
                }
            }
        }


        public static Vector3D ConvertZUpToYUp(Vector3D v)
        {
            return new Vector3D(v.x, v.z, -v.y);
        }
        public static Vector3F ConvertZUpToYUp(Vector3F v)
        {
            return new Vector3F(v.x, v.z, -v.y);
        }
        public static Frame3f ConvertZUpToYUp(Frame3f f)
        {
            return new Frame3f(
                ConvertZUpToYUp(f.Origin),
                ConvertZUpToYUp(f.X),
                ConvertZUpToYUp(f.Y),
                ConvertZUpToYUp(f.Z));
        }
        public static void ConvertZUpToYUp(IDeformableMesh mesh)
        {
            int NV = mesh.MaxVertexID;
            bool bHasNormals = mesh.HasVertexNormals;
            for ( int vid = 0; vid < NV; ++vid ) {
                if ( mesh.IsVertex(vid) ) {
                    Vector3D v = mesh.GetVertex(vid);
                    mesh.SetVertex(vid, new Vector3D(v.x, v.z, -v.y));
                    if ( bHasNormals ) {
                        Vector3F n = mesh.GetVertexNormal(vid);
                        mesh.SetVertexNormal(vid, new Vector3F(n.x, n.z, -n.y));
                    }
                }
            }
        }

        public static Vector3D ConvertYUpToZUp(Vector3D v)
        {
            return new Vector3D(v.x, -v.z, v.y);
        }
        public static Vector3F ConvertYUpToZUp(Vector3F v)
        {
            return new Vector3F(v.x, -v.z, v.y);
        }
        public static Frame3f ConvertYUpToZUp(Frame3f f)
        {
            return new Frame3f(
                ConvertYUpToZUp(f.Origin),
                ConvertYUpToZUp(f.X),
                ConvertYUpToZUp(f.Y),
                ConvertYUpToZUp(f.Z));
        }
        public static void ConvertYUpToZUp(IDeformableMesh mesh)
        {
            int NV = mesh.MaxVertexID;
            bool bHasNormals = mesh.HasVertexNormals;
            for ( int vid = 0; vid < NV; ++vid ) {
                if ( mesh.IsVertex(vid) ) {
                    Vector3D v = mesh.GetVertex(vid);
                    mesh.SetVertex(vid, new Vector3D(v.x, -v.z, v.y));
                    if ( bHasNormals ) {
                        Vector3F n = mesh.GetVertexNormal(vid);
                        mesh.SetVertexNormal(vid, new Vector3F(n.x, -n.z, n.y));
                    }
                }
            }
        }


        public static Vector3D FlipLeftRightCoordSystems(Vector3D v)
        {
            return new Vector3D(v.x, v.y, -v.z);
        }
        public static Vector3F FlipLeftRightCoordSystems(Vector3F v)
        {
            return new Vector3F(v.x, v.y, -v.z);
        }
        public static Frame3f FlipLeftRightCoordSystems(Frame3f f)
        {
            throw new NotImplementedException("this doesn't work...frame becomes broken somehow?");
            //return new Frame3f(
            //    FlipLeftRightCoordSystems(f.Origin),
            //    f.X, f.Y, f.Z);
            //    //FlipLeftRightCoordSystems(f.X),
            //    //FlipLeftRightCoordSystems(f.Y),
            //    //FlipLeftRightCoordSystems(f.Z));
        }
        public static void FlipLeftRightCoordSystems(IDeformableMesh mesh)
        {
            int NV = mesh.MaxVertexID;
            for ( int vid = 0; vid < NV; ++vid ) {
                if ( mesh.IsVertex(vid) ) {
                    Vector3D v = mesh.GetVertex(vid);
                    v.z = -v.z;
                    mesh.SetVertex(vid, v);

                    if (mesh.HasVertexNormals) {
                        Vector3F n = mesh.GetVertexNormal(vid);
                        n.z = -n.z;
                        mesh.SetVertexNormal(vid, n);
                    }
                }
            }

            if ( mesh is NGonsCore.geometry3Sharp.mesh.DMesh3 ) {
                (mesh as NGonsCore.geometry3Sharp.mesh.DMesh3).ReverseOrientation(false);
            } else {
                throw new Exception("argh don't want this in IDeformableMesh...but then for SimpleMesh??");
            }

        }



        public static void PerVertexTransform(IDeformableMesh mesh, Func<Vector3D,Vector3D> TransformF )
        {
            int NV = mesh.MaxVertexID;
            for (int vid = 0; vid < NV; ++vid) {
                if (mesh.IsVertex(vid)) {
                    Vector3D newPos = TransformF(mesh.GetVertex(vid));
                    mesh.SetVertex(vid, newPos);
                }
            }
        }


    }
}
