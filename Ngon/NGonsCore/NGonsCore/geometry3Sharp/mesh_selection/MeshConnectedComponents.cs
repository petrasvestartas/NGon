﻿using System;
using System.Collections;
using System.Collections.Generic;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh_selection
{
    public class MeshConnectedComponents : IEnumerable<MeshConnectedComponents.Component>
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh;

        // if a Filter is set, only triangles contained in Filter are
        // considered. Both filters will be applied if available.
        public IEnumerable<int> FilterSet = null;
        public Func<int, bool> FilterF = null;

        // filter on seed values for region-growing. This can be useful
        // to restrict components to certain areas, when you don't want
        // (or don't know) a full-triangle-set filter
        public Func<int, bool> SeedFilterF = null;

        public struct Component
        {
            public int[] Indices;
        }

        public List<Component> Components;


        public MeshConnectedComponents(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh)
        {
            Mesh = mesh;
            Components = new List<Component>();
        }


        public int Count
        {
            get { return Components.Count; }
        }

        public Component this[int index] {
            get { return Components[index]; }
        }


        public IEnumerator<Component> GetEnumerator() {
            return Components.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return Components.GetEnumerator();
        }


        public int LargestByCount
        {
            get {
                int largest_i = 0;
                int largest_count = Components[largest_i].Indices.Length;
                for ( int i = 1; i < Components.Count; ++i ) {
                    if ( Components[i].Indices.Length > largest_count ) {
                        largest_count = Components[i].Indices.Length;
                        largest_i = i;
                    }
                }
                return largest_i;
            }
        }


        public void SortByCount(bool bIncreasing = true)
        {
            if ( bIncreasing ) 
                Components.Sort((x, y) => { return x.Indices.Length.CompareTo(y.Indices.Length); });
            else
                Components.Sort((x, y) => { return -x.Indices.Length.CompareTo(y.Indices.Length); });
        }



        public void FindConnectedT()
        {
            Components = new List<Component>();

            int NT = Mesh.MaxTriangleID;

            // [TODO] could use Euler formula to determine if mesh is closed genus-0...

            Func<int, bool> filter_func = (i) => { return Mesh.IsTriangle(i); };
            if (FilterF != null)
                filter_func = (i) => { return Mesh.IsTriangle(i) && FilterF(i); };



            // initial active set contains all valid triangles
            byte[] active = new byte[Mesh.MaxTriangleID];
            Interval1i activeRange = Interval1i.Empty;
            if (FilterSet != null) {
                for (int i = 0; i < NT; ++i)
                    active[i] = 255;
                foreach (int tid in FilterSet) {
                    bool bValid = filter_func(tid);
                    if (bValid) {
                        active[tid] = 0;
                        activeRange.Contain(tid);
                    }
                }
            } else {
                for (int i = 0; i < NT; ++i) {
                    bool bValid = filter_func(i);
                    if (bValid) {
                        active[i] = 0;
                        activeRange.Contain(i);
                    } else {
                        active[i] = 255;
                    }
                }
            }

            // temporary buffers
            List<int> queue = new List<int>(NT / 10);
            List<int> cur_comp = new List<int>(NT / 10);

            // keep finding valid seed triangles and growing connected components
            // until we are done
            IEnumerable<int> range = (FilterSet != null) ? FilterSet : activeRange;
            foreach ( int i in range ) { 
            //for ( int i = 0; i < NT; ++i ) { 
                if (active[i] == 255)
                    continue;

                int seed_t = i;
                if (SeedFilterF != null && SeedFilterF(seed_t) == false)
                    continue;

                queue.Add(seed_t);
                active[seed_t] = 1;      // in queue

                while ( queue.Count > 0 ) {
                    int cur_t = queue[queue.Count - 1];
                    queue.RemoveAt(queue.Count - 1);

                    active[cur_t] = 2;   // tri has been processed
                    cur_comp.Add(cur_t);

                    Index3i nbrs = Mesh.GetTriNeighbourTris(cur_t);
                    for ( int j = 0; j < 3; ++j ) {
                        int nbr_t = nbrs[j];
                        if ( nbr_t != NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID && active[nbr_t] == 0 ) {
                            queue.Add(nbr_t);
                            active[nbr_t] = 1;           // in queue
                        }
                    }
                }


                Component comp = new Component() {
                    Indices = cur_comp.ToArray()
                };
                Components.Add(comp);

                // remove tris in this component from active set
                for ( int j = 0; j < comp.Indices.Length; ++j) {
                    active[comp.Indices[j]] = 255;
                }

                cur_comp.Clear();
                queue.Clear();
            }


        }




    }
}
