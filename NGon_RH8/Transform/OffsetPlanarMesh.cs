using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;

namespace NGon_RH8.Modifications
{
    public class OffsetPlanarMesh : GH_Component_NGon
    {
        public OffsetPlanarMesh()
          : base("OffsetPlanar", "OffsetPlanar",
              "Offset planar mesh, Planar Offset works with 3 valuence closed meshes only For n > 3 valence experimental component",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D", "Offset dist", GH_ParamAccess.item,1);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Outlines", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double d = 1;
            DA.GetData(1, ref d);

            Polyline[] polylinesOriginal = m.GetPolylines();
            Polyline[] polylines = m.PlanarOffset(d);
            //Polyline[] polylines = m.PlanarBisectorOffset(d);

            DataTree<Polyline> dt = new DataTree<Polyline>();
            for (int i = 0; i < polylines.Length; i++)
                dt.AddRange(new[] { polylinesOriginal[i], polylines[i] }, new Grasshopper.Kernel.Data.GH_Path(i));

            this.PreparePreview(m, DA.Iteration, dt.AllData(),false);

            DA.SetDataTree(0, dt);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.offsetPlanar;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("b30e676d-d93d-466a-85b5-89958c6e5698"); }
        }
    }
}