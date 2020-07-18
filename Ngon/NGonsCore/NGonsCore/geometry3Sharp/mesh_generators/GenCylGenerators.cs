﻿using System.Collections.Generic;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.curve;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh_generators
{
    public class TubeGenerator : MeshGenerator
    {
        public List<Vector3D> Vertices;
        public Polygon2d Polygon;
        public bool Capped = true;

        // [TODO] Frame3d ??
        public Frame3f Frame = Frame3f.Identity;

        // set to true if you are going to texture this or want sharp edges
        public bool NoSharedVertices = true;

        public int startCapCenterIndex = -1;
        public int endCapCenterIndex = -1;

        override public void Generate()
        {
            if (Polygon == null)
                Polygon = Polygon2d.MakeCircle(1.0f, 8);

            int Slices = Polygon.VertexCount;
            int nRings = Vertices.Count;
            int nRingSize = (NoSharedVertices) ? Slices + 1 : Slices;
            int nCapVertices = (NoSharedVertices) ? Slices + 1 : 1;
            if (Capped == false)
                nCapVertices = 0;

            vertices = new VectorArray3d(nRings * nRingSize + 2 * nCapVertices);
            uv = new VectorArray2f(vertices.Count);
            normals = new VectorArray3f(vertices.Count);

            int nSpanTris = (Vertices.Count - 1) * (2 * Slices);
            int nCapTris = (Capped) ? 2 * Slices : 0;
            triangles = new IndexArray3i(nSpanTris + nCapTris);

            Frame3f fCur = new Frame3f(Frame);
            Vector3D dv = CurveUtils.GetTangent(Vertices, 0); ;
            fCur.Origin = (Vector3F)Vertices[0];
            fCur.AlignAxis(2, (Vector3F)dv);
            Frame3f fStart = new Frame3f(fCur);

            // generate tube
            for (int ri = 0; ri < nRings; ++ri) {

                // propagate frame
                if (ri != 0) {
                    Vector3D tan = CurveUtils.GetTangent(Vertices, ri);
                    fCur.Origin = (Vector3F)Vertices[ri];
                    if (ri == 11)
                        dv = tan;
                    fCur.AlignAxis(2, (Vector3F)tan);
                }

                float uv_along = (float)ri / (float)(nRings - 1);

                // generate vertices
                int nStartR = ri * nRingSize;
                for (int j = 0; j < nRingSize; ++j) {
                    float uv_around = (float)j / (float)(nRings);

                    int k = nStartR + j;
                    Vector2D pv = Polygon.Vertices[j % Slices];
                    Vector3D v = fCur.FromFrameP((Vector2F)pv, 2);
                    vertices[k] = v;
                    uv[k] = new Vector2F(uv_along, uv_around);
                    Vector3F n = (Vector3F)(v - fCur.Origin).Normalized;
                    normals[k] = n;
                }
            }


            // generate triangles
            int ti = 0;
            for (int ri = 0; ri < nRings-1; ++ri) {
                int r0 = ri * nRingSize;
                int r1 = r0 + nRingSize;
                for (int k = 0; k < nRingSize - 1; ++k) {
                    triangles.Set(ti++, r0 + k, r0 + k + 1, r1 + k + 1, Clockwise);
                    triangles.Set(ti++, r0 + k, r1 + k + 1, r1 + k, Clockwise);
                }
                if (NoSharedVertices == false) {      // close disc if we went all the way
                    triangles.Set(ti++, r1 - 1, r0, r1, Clockwise);
                    triangles.Set(ti++, r1 - 1, r1, r1 + nRingSize - 1, Clockwise);
                }
            }

            if (Capped) {
                // add endcap verts
                int nBottomC = nRings * nRingSize;
                vertices[nBottomC] = fStart.Origin;
                uv[nBottomC] = new Vector2F(0.5f, 0.5f);
                normals[nBottomC] = -fStart.Z;
                startCapCenterIndex = nBottomC;

                int nTopC = nBottomC + 1;
                vertices[nTopC] = fCur.Origin;
                uv[nTopC] = new Vector2F(0.5f, 0.5f);
                normals[nTopC] = fCur.Z;
                endCapCenterIndex = nTopC;

                if (NoSharedVertices) {
                    // duplicate first loop and make a fan w/ bottom-center
                    int nExistingB = 0;
                    int nStartB = nTopC + 1;
                    for (int k = 0; k < Slices; ++k) {
                        vertices[nStartB + k] = vertices[nExistingB + k];
                        uv[nStartB + k] = (Vector2F)Polygon.Vertices[k].Normalized;
                        normals[nStartB + k] = normals[nBottomC];
                    }
                    append_disc(Slices, nBottomC, nStartB, true, Clockwise, ref ti);

                    // duplicate second loop and make fan
                    int nExistingT = nRingSize * (nRings - 1);
                    int nStartT = nStartB + Slices;
                    for (int k = 0; k < Slices; ++k) {
                        vertices[nStartT + k] = vertices[nExistingT + k];
                        uv[nStartT + k] = (Vector2F)Polygon.Vertices[k].Normalized;
                        normals[nStartT + k] = normals[nTopC];
                    }
                    append_disc(Slices, nTopC, nStartT, true, !Clockwise, ref ti);

                } else {
                    append_disc(Slices, nBottomC, 0, true, Clockwise, ref ti);
                    append_disc(Slices, nTopC, nRingSize * (nRings-1), true, !Clockwise, ref ti);
                }
            }

        }
    }
}
