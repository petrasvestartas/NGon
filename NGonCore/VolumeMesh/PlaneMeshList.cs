using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace NGonCore
{
    public class PlaneMeshList : IEnumerable<PlaneMesh>
    {
        private readonly VolumeMesh _owner;
        internal List<PlaneMesh> _planeMeshes;
        
        internal PlaneMeshList(VolumeMesh owner)
        {
            this._planeMeshes = new List<PlaneMesh>();
            this._owner = owner;
        }


        public int Count
        {
            get
            {
                return this._planeMeshes.Count;
            }
        }

        public void Add(PlaneMesh planeMesh)
        {
            this._planeMeshes.Add(planeMesh);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<PlaneMesh> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _planeMeshes[i];
        }

    }
}
