using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGon_RH8.Modifications
{
    public class NGonFrame : GH_Component_NGon
    {

        public NGonFrame()
          : base("NGonFrame", "NGonFrame",
              "NGonFrame",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D", "Distance", GH_ParamAccess.item,0.5);

            pManager[0].Optional = true;
       }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.list);

            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            double dist =0.5;
            DA.GetData(1, ref dist);

            var o = mesh.OffsetNgon(dist);


            Polyline[] polylines = mesh.OffsetNgonAndMerge(dist);
            this.PreparePreview(mesh, DA.Iteration, polylines.ToList(), false);

            DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(o.Item1, 0));

            DA.SetDataList(1, o.Item2);
            DA.SetDataList(2, polylines);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.FRAME;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("ba91b8ac-5dd3-412a-995b-80a5f994b72d"); }
        }
    }
}