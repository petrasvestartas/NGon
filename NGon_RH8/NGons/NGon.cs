using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.NGons {
    public class NGon : GH_Component_NGon {



        public NGon()
          : base("NGonPreview", "NGonPreview", "NGonPreview",     "NGons") {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.tree);
            pManager.AddColourParameter("Front", "F", "Front Color", GH_ParamAccess.item);
            pManager.AddColourParameter("Back", "B", "Back Color", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("LineWeight", "W", "LineWeight", GH_ParamAccess.item, 1);
            pManager.AddColourParameter("LineColor", "C", "LineColor", GH_ParamAccess.item, System.Drawing.Color.Black);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            //pManager[4].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            //pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            System.Drawing.Color f = System.Drawing.Color.White;
            System.Drawing.Color b = System.Drawing.Color.Red;
            System.Drawing.Color lc = System.Drawing.Color.Black;
            int w = 1;
            DA.GetData(1, ref f);
            DA.GetData(2, ref b);
            //DA.GetData(3, ref w);
            DA.GetData(3, ref lc);


            Grasshopper.Kernel.Data.GH_Structure<GH_Mesh> tree = new Grasshopper.Kernel.Data.GH_Structure<GH_Mesh>();
            DA.GetDataTree(0, out tree);

            Mesh mesh = new Mesh();
            foreach (GH_Mesh m in tree) {
                mesh.Append(m.Value);
            }

            this.PreparePreview(mesh,DA.Iteration,null,true,null,null,b.R,b.G,b.B,null,f.R,f.G,f.B,w,true, lc.R, lc.G,lc.B);

           // DA.SetDataTree(0, tree);

        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.preview;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("47c23c43-efa7-4d73-ac4b-2e729e76ecb7"); }
        }

    }
}