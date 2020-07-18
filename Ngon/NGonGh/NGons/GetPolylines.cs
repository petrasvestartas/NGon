using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGonGh.NGons
{
    public class GetPolylines : GH_Component_NGon
    {


        public GetPolylines()
          : base("Get Polylines", "ToP",  "Checks if there are ngons in a mesh and extracts ngons",   "NGons")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);


            Polyline[] po = null;

            if (mesh.Ngons.Count > 0)
                po = mesh.GetPolylines();
            else
                po = mesh.GetFacePolylinesArray();
            this.PreparePreview(mesh, DA.Iteration,po.ToList(),false);
            DA.SetDataList(0, po);

          

        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_GetPolylines;

        public override Guid ComponentGuid => new Guid("{059739fa-d10c-44ba-a2ad-9c7cc0522fde}");
    }
}