using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore.IsoSurface.Voxel
{
    public class VoxelCube
    {
        public VoxelVertex[] V = new VoxelVertex[8];

        public VoxelEdge[] Edges = new VoxelEdge[12];

        public object ExtraData;

        public Vector Centre
        {
            get
            {
                return Vector.MidPoint(this.V[0].P, this.V[6].P);
            }
        }
    }
}
