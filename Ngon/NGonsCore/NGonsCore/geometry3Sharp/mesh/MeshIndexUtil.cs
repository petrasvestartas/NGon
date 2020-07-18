﻿using System.Collections.Generic;
using System.Diagnostics;
using NGonsCore.geometry3Sharp.core;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh
{
    /// <summary>
    /// Utility functions for manipulating sets/lists of mesh indices
    /// </summary>
    public static class MeshIndexUtil
    {

        /// <summary>
        /// given list of edges of MeshA, and vertex map from A to B, map to list of edges on B
        /// </summary>
        public static List<int> MapEdgesViaVertexMap(IIndexMap AtoBV, NGonsCore.geometry3Sharp.mesh.DMesh3 MeshA, NGonsCore.geometry3Sharp.mesh.DMesh3 MeshB, List<int> edges)
        {
            int N = edges.Count;
            List<int> result = new List<int>(N);
            for ( int i = 0; i < N; ++i ) {
                int eid_a = edges[i];
                Index2i aev = MeshA.GetEdgeV(eid_a);
                int bev0 = AtoBV[aev.a];
                int bev1 = AtoBV[aev.b];
                int eid_b = MeshB.FindEdge(bev0, bev0);
                Debug.Assert(eid_b != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID);
                result.Add(eid_b);
            }
            return result;
        }

    }
}
