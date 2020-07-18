using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Modifications
{
    public class OffsetMesh : GH_Component_NGon
    {
        public OffsetMesh()
          : base("OffsetMesh", "OffsetMesh",
              "Offset mesh",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D", "Offset dist", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Solid", "S", "Solid", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double d = 1;
            DA.GetData(1, ref d);

            bool s = false;
            DA.GetData(2, ref s);

            Mesh mesh = d == 0 ? m : m.Offset(d,s);
            if (d == 0) {
                DA.SetData(0, m);
                this.PreparePreview(m, DA.Iteration);
            }
            {
                this.PreparePreview(mesh, DA.Iteration);
                DA.SetData(0, mesh);

            }

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.OFFSET;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("b30e676d-d93d-466a-85b5-89958c6e9578"); }
        }
    }
}