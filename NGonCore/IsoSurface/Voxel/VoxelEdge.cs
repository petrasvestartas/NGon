using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.IsoSurface.Voxel
{
    public class VoxelEdge
    {
        public VoxelEdgePoint Xpoint;

        public int id;

        public VoxelVertex[] V = new VoxelVertex[2];

        public Vector Axis
        {
            get
            {
                return this.V[1].P - this.V[0].P;
            }
        }

        public bool HasIntersection
        {
            get
            {
                return this.Xpoint != null;
            }
            set
            {
                if (value)
                {
                    if (this.Xpoint == null)
                    {
                        this.Xpoint = new VoxelEdgePoint();
                        return;
                    }
                }
                else
                {
                    this.Xpoint = null;
                }
            }
        }
    }
}
