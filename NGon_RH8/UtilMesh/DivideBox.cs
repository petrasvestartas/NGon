using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonCore.IsoSurface.Voxel;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NGon_RH8 {
    public class DivideBox_Component : GH_Component
    {

        public DivideBox_Component()
          : base("DivideBox", "DivideBox",
              "DivideBox",
"NGon", "Utilities Mesh") 
            {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.dividebox;
            }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "Box", GH_ParamAccess.item, new Box(new BoundingBox(-1.0, -1.0, -1.0, 1.0, 1.0, 1.0)));
            pManager.AddIntegerParameter("X", "X", "X", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Y", "Y", "Y", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Z", "Z", "Z", GH_ParamAccess.item, 2);
            for (int i = 0; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("P", "P", "P",GH_ParamAccess.list);
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Box B = default(Box);
            DA.GetData<Box>(0, ref B);

            int x = 2;
            DA.GetData(1, ref x);
            int y = 2;
            DA.GetData(2, ref y);
            int z = 2;
            DA.GetData(3, ref z);

            //Solve

            x--;
            y--;
            z--;

            var num = new List<double>();
            Interval interval = new Interval(0, 1);


            Point3d[,,] points = new Point3d[z + 1, y + 1, x + 1];

            for (int i = 0; i <= x; i++)
                for (int j = 0; j <= y; j++)
                    for (int k = 0; k <= z; k++)
                    {
                        double a = interval.ParameterAt((double)i / (double)x);
                        double b = interval.ParameterAt((double)j / (double)y);
                        double c = interval.ParameterAt((double)k / (double)z);
                        points[k, j, i] = B.PointAt(a, b, c);
                    }

            //Output
            DA.SetDataList(0, points);

        }

        public override GH_Exposure Exposure {
            get { return GH_Exposure.secondary; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{59c11f13-1331-10b8-9fe7-891bbe7f2e54}"); }
        }

    }
}
