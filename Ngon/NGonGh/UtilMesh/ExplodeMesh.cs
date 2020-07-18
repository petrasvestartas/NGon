using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGonGh.Utils
{
    public class ExplodeMesh : GH_Component
    {

        public ExplodeMesh()
          : base("ExplodeMesh", "ExplodeMesh",
              "Explodes mesh and assigns average color to each vertex of a face","NGon",
             "Utilities Mesh")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Explode","E","Explode mesh by faces", GH_ParamAccess.item,false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            bool e = false;
            DA.GetData(1,ref e);


            DA.SetDataList(0, m.ExplodeMesh(e));

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Explode;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("55f1321a-d5e1-4c3f-aedb-bd27ce58a583"); }
        }
    }
}