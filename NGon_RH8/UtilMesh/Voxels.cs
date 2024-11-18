using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Utils {
    public class Voxels : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Voxels()
          : base("Voxels", "Voxels",
              "Voxels Mesh from Points",
             "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddPointParameter("Points", "Pt", "Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scale", "S", "S", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "Pt", "Points", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            List<Point3d> pts = new List<Point3d>();
            double scale = 0.01;
            if (scale == 0)
                scale = 0.01;

            DA.GetDataList(0,pts);
            DA.GetData(1,ref scale);

            NGonCore.Voxel.VoxelSpace<Point3d> v = new NGonCore.Voxel.VoxelSpace<Point3d>();

            for (int i = 0; i < pts.Count; i++) {
                v.Add(pts[i], new NGonCore.Voxel.VoxelAddress((int)(pts[i].X), (int)(pts[i].Y), (int)(pts[i].Z)) * scale);
            }

            List<Point3d> pointsOfVoxels = new List<Point3d>(v.AddressBook.Count);
            
            foreach(var p in v.AddressBook) {
               
                pointsOfVoxels.Add(new Point3d(p.X, p.Y, p.Z)/scale+new Point3d(new Point3d(0.5, 0.5, 0.5) / scale));
            }

            Mesh mesh = v.CreateMesh();
            mesh.Scale(1 / scale);
            NGonCore.MeshUtil.WeldUsingRTree(mesh,0.001);
            mesh.Unweld(0, true);
            PreparePreview(mesh, DA.Iteration,null,true,null,null, 255,255,255);

            DA.SetData(0,mesh);
            DA.SetDataList(1,pointsOfVoxels);

        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.Voxel;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-89ad2b9e9211"); }
        }
    }
}