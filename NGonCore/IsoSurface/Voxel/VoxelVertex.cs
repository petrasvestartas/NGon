using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.IsoSurface.Voxel
{
    public class VoxelVertex
    {
        public Vector P = Vector.Origin;

        public Vector Gradient = Vector.Origin;

        public double Value;

        public double DValue;

        public object ExtraData;
    }
}
