﻿using System;
using System.Collections.Generic;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.io;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    public class DMesh3Builder : IMeshBuilder
    {
        public List<NGonsCore.geometry3Sharp.mesh.DMesh3> Meshes;
        public List<GenericMaterial> Materials;

		// this is a map from index into Meshes to index into Materials (-1 if no material)
		//  (so, currently we can only have 1 material per mesh!)
        public List<int> MaterialAssignment;

        public List<Dictionary<string, object> > Metadata;

        int nActiveMesh;

        public DMesh3Builder()
        {
            Meshes = new List<NGonsCore.geometry3Sharp.mesh.DMesh3>();
            Materials = new List<GenericMaterial>();
            MaterialAssignment = new List<int>();
            Metadata = new List<Dictionary<string, object>>();
            nActiveMesh = -1;
        }

        public int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups)
        {
            int index = Meshes.Count;
			NGonsCore.geometry3Sharp.mesh.DMesh3 m = new NGonsCore.geometry3Sharp.mesh.DMesh3(bHaveVtxNormals, bHaveVtxColors, bHaveVtxUVs, bHaveFaceGroups);
            Meshes.Add(m);
            MaterialAssignment.Add(-1);     // no material is known
            Metadata.Add(new Dictionary<string, object>());
            nActiveMesh = index;
            return index;
        }

        public void SetActiveMesh(int id)
        {
            if (id >= 0 && id < Meshes.Count)
                nActiveMesh = id;
            else
                throw new ArgumentOutOfRangeException("active mesh id is out of range");
        }

        public int AppendTriangle(int i, int j, int k)
        {
            return Meshes[nActiveMesh].AppendTriangle(i, j, k);
        }

        public int AppendTriangle(int i, int j, int k, int g)
        {
            return Meshes[nActiveMesh].AppendTriangle(i, j, k, g);
        }

        public int AppendVertex(double x, double y, double z)
        {
            return Meshes[nActiveMesh].AppendVertex(new Vector3D(x, y, z));
        }
        public int AppendVertex(NewVertexInfo info)
        {
            return Meshes[nActiveMesh].AppendVertex(info);
        }

        public bool SupportsMetaData { get { return true; } }
        public void AppendMetaData(string identifier, object data)
        {
            Metadata[nActiveMesh].Add(identifier, data);
        }


        // just store GenericMaterial object, we can't use it here
        public int BuildMaterial(GenericMaterial m)
        {
            int id = Materials.Count;
            Materials.Add(m);
            return id;
        }

        // do material assignment to mesh
        public void AssignMaterial(int materialID, int meshID)
        {
            if (meshID >= MaterialAssignment.Count || materialID >= Materials.Count)
                throw new ArgumentOutOfRangeException("[SimpleMeshBuilder::AssignMaterial] meshID or materialID are out-of-range");
            MaterialAssignment[meshID] = materialID;
        }





        //
        // DMesh3 construction utilities
        //

        /// <summary>
        /// ultimate generic mesh-builder, pass it arrays of floats/doubles, or lists
        /// of Vector3d, or anything in-between. Will figure out how to interpret
        /// </summary>
        public static NGonsCore.geometry3Sharp.mesh.DMesh3 Build<VType,TType,NType>(IEnumerable<VType> Vertices,  
                                                      IEnumerable<TType> Triangles, 
                                                      IEnumerable<NType> Normals = null,
                                                      IEnumerable<int> TriGroups = null)
        {
            NGonsCore.geometry3Sharp.mesh.DMesh3 mesh = new NGonsCore.geometry3Sharp.mesh.DMesh3(Normals != null, false, false, TriGroups != null);

            Vector3D[] v = BufferUtil.ToVector3d(Vertices);
            for (int i = 0; i < v.Length; ++i)
                mesh.AppendVertex(v[i]);

            if ( Normals != null ) {
                Vector3F[] n = BufferUtil.ToVector3f(Normals);
                if ( n.Length != v.Length )
                    throw new Exception("DMesh3Builder.Build: incorrect number of normals provided");
                for (int i = 0; i < n.Length; ++i)
                    mesh.SetVertexNormal(i, n[i]);
            }

            Index3i[] t = BufferUtil.ToIndex3i(Triangles);
            for (int i = 0; i < t.Length; ++i)
                mesh.AppendTriangle(t[i]);

            if ( TriGroups != null ) {
                List<int> groups = new List<int>(TriGroups);
                if (groups.Count != t.Length)
                    throw new Exception("DMesh3Builder.Build: incorect number of triangle groups");
                for (int i = 0; i < t.Length; ++i)
                    mesh.SetTriangleGroup(i, groups[i]);
            }

            return mesh;
        }

         


    }





}
