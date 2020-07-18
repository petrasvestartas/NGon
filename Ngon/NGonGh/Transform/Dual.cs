using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Modifications {
    public class Dual : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the Dual class.
        /// </summary>
        public Dual()
          : base("Dual", "Dual",
              "Dual",
              "Transform") {
        }

  
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "T", "Type",GH_ParamAccess.item,0);
            pManager.AddPointParameter("P", "P", "Custom points instead of center, number of points must be equal to number of faces or ngons", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = null;
            DA.GetData(0, ref mesh);
            int type = 0;
            DA.GetData(1, ref type);


            List<Point3d> pts = new List<Point3d>();
            DA.GetDataList(2, pts);


            if (mesh != null) {
                mesh = mesh.Dual(type, null,pts);
                PreparePreview(mesh,DA.Iteration);
                DA.SetData(0, mesh);

            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Dual;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("f2f8ba8b-822c-4e11-9ece-60b7aca3142f"); }
        }
    }
}