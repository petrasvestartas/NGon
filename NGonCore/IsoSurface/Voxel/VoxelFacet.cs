using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.IsoSurface.Voxel
{
    public class VoxelFacet
    {
        public Vector Normal = Vector.Origin;

        public VoxelEdge[] Edges = new VoxelEdge[3];

        public Vector Point(int i)
        {
            return this.Edges[i].Xpoint.P;
        }
    }
}
