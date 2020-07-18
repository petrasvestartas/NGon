using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Transform {
    public class MeshFlap : GH_Component {
 
        public MeshFlap()
          : base("MeshFlap", "MeshFlap",
              "MeshFlap",
              "NGon", "Joint") {
        }

    
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist","D","Offset Distance",GH_ParamAccess.item,1);
            pManager.AddBooleanParameter("Flip", "F", "Flip", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Normals", "N", "Offset by face Normals", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Chamfer", "C", "Chamfer edge", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

   
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();
            double d = 1;
            bool f = false;
            bool n = false;
            bool c = false;
            if (DA.GetData(0,ref mesh)) {

                DA.GetData(1, ref d);
                DA.GetData(2, ref f);
                DA.GetData(3, ref n);
                DA.GetData(4, ref c);

                var meshes = new List<Mesh>();
                var plines = mesh.GetFlaps(d, f, n,c, ref meshes);
                DA.SetDataList(0, meshes);
                DA.SetDataTree(1,GrasshopperUtil.ListOfListsToTree(plines,DA.Iteration));

            }
        }

 
        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Flaps;
            }
        }

  
        public override Guid ComponentGuid {
            get { return new Guid("23b32811-86d4-4830-a6d0-c6754869573c"); }
        }
    }
}