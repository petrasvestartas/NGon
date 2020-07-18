using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonsCore
{
    public class PlaneMesh
    {

        Mesh _mesh;
        Plane[] _facePlanes;
        int _ID;

        int[] _curIDmf;
        int[] _adjIDm;
        int[] _adjIDmf;
  

        public PlaneMesh(Mesh mesh, Plane[] facePlanes, int ID = -1, int[] curIDmf = null, int[] adjIDm = null, int[] adjIDmf = null )
        {
            this._mesh = mesh;
            this._facePlanes = facePlanes;
            this._ID = ID;

            if ((curIDmf.Length == adjIDm.Length) && (curIDmf.Length == adjIDmf.Length))
            {
                this._curIDmf = curIDmf;
                this._adjIDm = adjIDm;
                this._adjIDmf = adjIDmf;
            }

        }

    }
}
