using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonCore.IsoSurface.Voxel;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;

namespace NGon_RH8 {
    public class TriPeriodicSurfaces_Component : GH_Component
    {

        public TriPeriodicSurfaces_Component()
          : base("Tri-Periodic Surfaces", "Tri-Periodic Surfaces",
              "Tri-Periodic Surfaces",
"NGon", "Utilities Mesh") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "N", "0 = HybridPW, 1 = Diamond, 2 = Neovius, 3 = IWP, 4 = HybridPW", GH_ParamAccess.item, 0);
            for (int i = 0; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Values", "V", "Value of Mathematical Formulas", GH_ParamAccess.list);
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            List<Point3d> x = new List<Point3d>();
            bool flag = DA.GetDataList(0, x);

            int T = 0;
            DA.GetData(1, ref T);

            if (flag)
            {

                List<double> v = new List<double>();

                switch (T)
                {
                    case 0:
                        for(int i = 0; i < x.Count; i++)
                            v.Add(Gyroid(x[i].Z, x[i].Y, x[i].X));
                        break;
                    case 1:
                        for (int i = 0; i < x.Count; i++)
                            v.Add(Diamond(x[i].Z, x[i].Y, x[i].X));
                        break;
                    case 2:
                        for (int i = 0; i < x.Count; i++)
                            v.Add(Neovius(x[i].Z, x[i].Y, x[i].X));
                        break;
                    case 3:
                        for (int i = 0; i < x.Count; i++)
                            v.Add(IWP(x[i].Z, x[i].Y, x[i].X));
                        break;
                    default:
                        for (int i = 0; i < x.Count; i++)
                            v.Add(HybridPW(x[i].Z, x[i].Y, x[i].X));
                        break;
                }

                DA.SetDataList(0,v);
            }


        }

        public double Gyroid(double x, double y, double z)
        {
            return Math.Sin(x) * Math.Cos(y) + Math.Sin(y) * Math.Cos(z) + Math.Sin(z) * Math.Cos(x);
        }

        public double Diamond(double x, double y, double z)
        {
            double sx = Math.Sin(x);
            double sy = Math.Sin(y);
            double sz = Math.Sin(z);

            double cx = Math.Cos(x);
            double cy = Math.Cos(y);
            double cz = Math.Cos(z);

            return sx * sy * sz + sx * cy * cz + cx * sy * cz + cx * cy * sz;
        }


        public double Neovius(double x, double y, double z)
        {
            double cx = Math.Cos(x);
            double cy = Math.Cos(y);
            double cz = Math.Cos(z);

            return 3 * (cx + cy + cz) + 4 * cx * cy * cz;
        }


        public double IWP(double x, double y, double z)
        {
            double cx = Math.Cos(x);
            double cy = Math.Cos(y);
            double cz = Math.Cos(z);

            return cx * cy + cy * cz + cz * cx - cx * cy * cz + 0.25;
        }

        public double HybridPW(double x, double y, double z)
        {
            double cx = Math.Cos(x);
            double cy = Math.Cos(y);
            double cz = Math.Cos(z);

            return 4 * (cx * cy + cy * cz + cz * cx) -3 * cx * cy * cz + 2;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }



        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.isosurface;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("{59c11f13-1331-10b8-0fe7-874bbe7f2e52}"); }
        }

    }
}
