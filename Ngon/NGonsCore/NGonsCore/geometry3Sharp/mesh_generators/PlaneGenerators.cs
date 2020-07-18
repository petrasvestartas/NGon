﻿using System;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.curve;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh_generators
{
    // generate a two-triangle rect, centered at origin
    public class TrivialRectGenerator : MeshGenerator
    {
        public float Width = 1.0f;
        public float Height = 1.0f;

        public enum UVModes
        {
            FullUVSquare,
            CenteredUVRectangle,
            BottomCornerUVRectangle
        }
        public UVModes UVMode = UVModes.FullUVSquare;

        override public void Generate()
        {
            vertices = new VectorArray3d(4);
            uv = new VectorArray2f(4);
            normals = new VectorArray3f(4);
            triangles = new IndexArray3i(2);

            vertices[0] = new Vector3D(-Width / 2.0f, 0, -Height / 2.0f);
            vertices[1] = new Vector3D(Width / 2.0f, 0, -Height / 2.0f);
            vertices[2] = new Vector3D(Width / 2.0f, 0, Height / 2.0f);
            vertices[3] = new Vector3D(-Width / 2.0f, 0, Height / 2.0f);

            normals[0] = normals[1] = normals[2] = normals[3] = Vector3F.AxisY;

            float uvleft = 0.0f, uvright = 1.0f, uvbottom = 0.0f, uvtop = 1.0f;

            // if we want the UV subregion, we assume it is 
            if (UVMode != UVModes.FullUVSquare) {
                if (Width > Height) {
                    float a = Height / Width;
                    if (UVMode == UVModes.CenteredUVRectangle) {
                        uvbottom = 0.5f - a / 2.0f; uvtop = 0.5f + a / 2.0f;
                    } else {
                        uvtop = a;
                    }
                } else if (Height > Width) {
                    float a = Width / Height;
                    if (UVMode == UVModes.CenteredUVRectangle) {
                        uvleft = 0.5f - a / 2.0f; uvright = 0.5f + a / 2.0f;
                    } else {
                        uvright = a;
                    }
                }
            }

            uv[0] = new Vector2F(uvleft, uvbottom);
            uv[1] = new Vector2F(uvright, uvbottom);
            uv[2] = new Vector2F(uvright, uvtop);
            uv[3] = new Vector2F(uvleft, uvtop);

            if (Clockwise == true) {
                triangles.Set(0, 0, 1, 2);
                triangles.Set(1, 0, 2, 3);
            } else {
                triangles.Set(0, 0, 2, 1);
                triangles.Set(1, 0, 3, 2);
            }
        }
    }







    // Generate a rounded rect centered at origin.
    // Force individual corners to be sharp using the SharpCorners flags field.
    public class RoundRectGenerator : MeshGenerator
    {
        public float Width = 1.0f;
        public float Height = 1.0f;
        public float Radius = 0.1f;
        public int CornerSteps = 4;


        [Flags]
        public enum Corner
        {
            BottomLeft = 1,
            BottomRight = 2,
            TopRight = 4,
            TopLeft = 8
        }
        public Corner SharpCorners = 0;


        public enum UVModes
        {
            FullUVSquare,
            CenteredUVRectangle,
            BottomCornerUVRectangle
        }
        public UVModes UVMode = UVModes.FullUVSquare;

        // order is [inner_corner, outer_1, outer_2]
        static int[] corner_spans = new int[] { 0, 11, 4,   1, 5, 6,   2, 7, 8,   3, 9, 10 };

        override public void Generate()
        {
            int corner_v = 0, corner_t = 0;
            for (int k = 0; k < 4; ++k) {
                if (((int)SharpCorners & (1 << k)) != 0) {
                    corner_v += 1;
                    corner_t += 2;
                } else {
                    corner_v += CornerSteps;
                    corner_t += (CornerSteps + 1);
                }
            }

            vertices = new VectorArray3d(12 + corner_v);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);
            triangles = new IndexArray3i(10 + corner_t);

            float innerW = Width - 2 * Radius;
            float innerH = Height - 2 * Radius;

            // make vertices for inner "cross" (ie 5 squares)
            vertices[0] = new Vector3D(-innerW / 2.0f, 0, -innerH / 2.0f);
            vertices[1] = new Vector3D(innerW / 2.0f, 0, -innerH / 2.0f);
            vertices[2] = new Vector3D(innerW / 2.0f, 0, innerH / 2.0f);
            vertices[3] = new Vector3D(-innerW / 2.0f, 0, innerH / 2.0f);

            vertices[4] = new Vector3D(-innerW / 2, 0, -Height / 2);
            vertices[5] = new Vector3D(innerW / 2, 0, -Height / 2);

            vertices[6] = new Vector3D(Width / 2, 0, -innerH / 2);
            vertices[7] = new Vector3D(Width / 2, 0, innerH / 2);

            vertices[8] = new Vector3D(innerW / 2, 0, Height / 2);
            vertices[9] = new Vector3D(-innerW / 2, 0, Height / 2);

            vertices[10] = new Vector3D(-Width / 2, 0, innerH / 2);
            vertices[11] = new Vector3D(-Width / 2, 0, -innerH / 2);

            // make triangles for inner cross
            bool cycle = (Clockwise == false);
            int ti = 0;
            append_rectangle(0, 1, 2, 3, cycle, ref ti);
            append_rectangle(4,5,1,0, cycle, ref ti);
            append_rectangle(1,6,7,2, cycle, ref ti);
            append_rectangle(3,2,8,9, cycle, ref ti);
            append_rectangle(11,0,3,10, cycle, ref ti);

            int vi = 12;
            for ( int j = 0; j < 4; ++j ) {
                bool sharp = ((int)SharpCorners & (1 << j)) > 0;
                if (sharp) {
                    append_2d_disc_segment(corner_spans[3 * j], corner_spans[3 * j + 1], corner_spans[3 * j + 2], 1,
                        cycle, ref vi, ref ti, -1, math.MathUtil.SqrtTwo * Radius);
                } else {
                    append_2d_disc_segment(corner_spans[3 * j], corner_spans[3 * j + 1], corner_spans[3 * j + 2], CornerSteps,
                        cycle, ref vi, ref ti);
                }
            }


            for (int k = 0; k < vertices.Count; ++k)
                normals[k] = Vector3F.AxisY;

            float uvleft = 0.0f, uvright = 1.0f, uvbottom = 0.0f, uvtop = 1.0f;

            // if we want the UV subregion, we assume it is 
            if (UVMode != UVModes.FullUVSquare) {
                if (Width > Height) {
                    float a = Height / Width;
                    if (UVMode == UVModes.CenteredUVRectangle) {
                        uvbottom = 0.5f - a / 2.0f; uvtop = 0.5f + a / 2.0f;
                    } else {
                        uvtop = a;
                    }
                } else if (Height > Width) {
                    float a = Width / Height;
                    if (UVMode == UVModes.CenteredUVRectangle) {
                        uvleft = 0.5f - a / 2.0f; uvright = 0.5f + a / 2.0f;
                    } else {
                        uvright = a;
                    }
                }
            }

            Vector3D c = new Vector3D(-Width / 2, 0, -Height / 2);
            for ( int k = 0; k < vertices.Count; ++k ) {
                Vector3D v = vertices[k];
                double tx = (v.x - c.x) / Width;
                double ty = (v.y - c.y) / Height;
                uv[k] = new Vector2F( (1 - tx) * uvleft + (tx) * uvright, 
                                      (1 - ty) * uvbottom + (ty) * uvtop);
            }

        }



        static readonly float[] signx = new float[] { 1, 1, -1, -1 };
        static readonly float[] signy = new float[] { -1, 1, 1, -1 };
        static readonly float[] startangle = new float[] { 270, 0, 90, 180 };
        static readonly float[] endangle = new float[] { 360, 90, 180, 270 };

        /// <summary>
        /// This is a utility function that returns the set of border points, which
        /// is useful when we use a roundrect as a UI element and want the border
        /// </summary>
        public Vector3D[] GetBorderLoop()
        {
            int corner_v = 0;
            for (int k = 0; k < 4; ++k) {
                if (((int)SharpCorners & (1 << k)) != 0)
                    corner_v += 1;
                else 
                    corner_v += CornerSteps;
            }

            float innerW = Width - 2 * Radius;
            float innerH = Height - 2 * Radius;

            Vector3D[] vertices = new Vector3D[4 + corner_v];
            int vi = 0;

            for ( int i = 0; i < 4; ++i ) { 
                vertices[vi++] = new Vector3D(signx[i] * Width / 2, 0, signy[i] * Height / 2);

                bool sharp = ((int)SharpCorners & (1 << i)) > 0;
                Arc2d arc = new Arc2d( new Vector2D(signx[i] * innerW, signy[i] * innerH), 
                    (sharp) ? math.MathUtil.SqrtTwo * Radius : Radius,
                    startangle[i], endangle[i]);
                int use_steps = (sharp) ? 1 : CornerSteps;
                for (int k = 0; k < use_steps; ++k) {
                    double t = (double)(i + 1) / (double)(use_steps + 1);
                    Vector2D pos = arc.SampleT(t);
                    vertices[vi++] = new Vector3D(pos.x, 0, pos.y);
                }
            }

            return vertices;
        }


    }

}
