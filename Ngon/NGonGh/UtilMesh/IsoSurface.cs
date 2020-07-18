using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonsCore.IsoSurface.Voxel;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;

namespace NGonGh {
    public class IsoSurfaceComponent : GH_Component
    {

        public IsoSurfaceComponent()
          : base("IsoSurface", "IsoSurface",
              "IsoSurface",
"NGon", "Utilities Mesh") {
         
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.isofield;
            }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Box", "B", "Box", GH_ParamAccess.item, new Box(new BoundingBox(-1.0, -1.0, -1.0, 1.0, 1.0, 1.0)));
            pManager.AddNumberParameter("Values", "V", "Values", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("XYZ resolution", "XYZ", "XYZ resolution", GH_ParamAccess.list, new List<int> { 20, 20, 20 });
            pManager.AddIntegerParameter("X resolution", "X", "X resolution", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Y resolution", "Y", "Y resolution", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Z resolution", "Z", "Z resolution", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("IsoValue", "I", "IsoValue", GH_ParamAccess.item, 0.15);
            pManager.AddBooleanParameter("Merge Vertices", "M", "Merge Vertices", 0, false);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", 0);
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Box box = default(Box);
            System.Collections.Generic.List<double> list = new System.Collections.Generic.List<double>();
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            double iso = 0.0;
            bool flag = false;
            if (!DA.GetData<Box>(0, ref box))
            {
                return;
            }
            if (!DA.GetDataList<double>(1, list))
            {
                return;
            }

            DA.GetData(2, ref num);
            DA.GetData(3, ref num2);
            DA.GetData(4, ref num3);



            if (!DA.GetData<double>(5, ref iso)) return;
            if (!DA.GetData<bool>(6, ref flag))  return;
 

            double num4 = 1.0 / ((double)num - 1.0);
            double num5 = 1.0 / ((double)num2 - 1.0);
            double num6 = 1.0 / ((double)num3 - 1.0);
            Mesh mesh = new Mesh();
            double[] array = new double[8];
            Vector[] array2 = new Vector[8];
            Vector[] array3 = new Vector[15];
            int num7 = num2 * num;
            int num8 = 0;
            array2[0] = new Vector(0.0, 0.0, 0.0);
            array2[1] = new Vector(num4, 0.0, 0.0);
            array2[2] = new Vector(num4, num5, 0.0);
            array2[3] = new Vector(0.0, num5, 0.0);
            array2[4] = new Vector(0.0, 0.0, num6);
            array2[5] = new Vector(num4, 0.0, num6);
            array2[6] = new Vector(num4, num5, num6);
            array2[7] = new Vector(0.0, num5, num6);
            for (int i = 0; i < 8; i++)
            {
                Point3d point3d = box.PointAt(array2[i].x, array2[i].y, array2[i].z);
                point3d -= (Vector3d)box.PointAt(0.0, 0.0, 0.0);
                array2[i].Set(point3d.X, point3d.Y, point3d.Z);
            }
            for (int i = 1; i < num3; i++)
            {
                for (int j = 1; j < num2; j++)
                {
                    for (int k = 1; k < num; k++)
                    {
                        int num9 = (i - 1) * num7 + (j - 1) * num + (k - 1);
                        array[0] = list[num9];
                        array[1] = list[num9 + 1];
                        array[2] = list[num9 + 1 + num];
                        array[3] = list[num9 + num];
                        array[4] = list[num9 + num7];
                        array[5] = list[num9 + 1 + num7];
                        array[6] = list[num9 + 1 + num + num7];
                        array[7] = list[num9 + num + num7];
                        Point3d point3d2 = box.PointAt((double)(k - 1) * num4, (double)(j - 1) * num5, (double)(i - 1) * num6);
                        int faces = VoxelBox.GetFaces(array2, array3, array, iso);
                        if (faces > 0)
                        {
                            for (int l = 0; l < faces; l += 3)
                            {
                                mesh.Vertices.Add(array3[l].x + point3d2.X, array3[l].y + point3d2.Y, array3[l].z + point3d2.Z);
                                mesh.Vertices.Add(array3[l + 1].x + point3d2.X, array3[l + 1].y + point3d2.Y, array3[l + 1].z + point3d2.Z);
                                mesh.Vertices.Add(array3[l + 2].x + point3d2.X, array3[l + 2].y + point3d2.Y, array3[l + 2].z + point3d2.Z);
                                mesh.Faces.AddFace(num8, num8 + 1, num8 + 2);
                                num8 += 3;
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                mesh.Vertices.CombineIdentical(true, true);
            }
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            DA.SetData(0, mesh);
        }

        public override GH_Exposure Exposure {
            get { return GH_Exposure.secondary; }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("{59c11f13-1331-40b8-9fe7-986bbe7f2e64}"); }
        }

    }
}
