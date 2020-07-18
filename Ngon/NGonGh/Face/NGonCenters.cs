using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;

namespace NGonGh.Face
{
    public class NGonCenters : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the NGonCenters class.
        /// </summary>
        public NGonCenters()
          : base("NGon Centers", "NGonCenters",
              "GetNGonCenter",
              "Face")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Centers", "C", "NGon Centers", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normals", "N", "Average normal at ngon center", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "P", "Average planes at ngon center", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            var pt = mesh.GetNGonCenters();

            DA.SetDataList(0, pt);
            DA.SetDataList(1, mesh.GetNgonNormals());
            DA.SetDataList(2, mesh.GetNgonPlanes());

            this.PreparePreview(mesh, DA.Iteration, null, false, pt.ToList());


        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_Center;

        public override Guid ComponentGuid => new Guid("{1d349937-cc38-43c2-b67b-e20ec180925a}");
    }
}