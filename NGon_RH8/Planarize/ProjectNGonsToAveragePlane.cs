using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGon_RH8.NGons
{
    public class ProjectNGonsToAveragePlane : GH_Component_NGon
    {

        public ProjectNGonsToAveragePlane()
          : base("Project NGons", "Project",
              "Project ngons to their average planes",
               "Planarize")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset","O","Offset ngon, ngon vertex normals are intersected with offset plane",GH_ParamAccess.item,1);
            pManager[0].Optional = true;
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Project Polylines to average planes", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double dist = 1.0;
            DA.GetData(1,ref dist);
            var polylines = m.ProjectNGonsToPlanes(dist);
            DA.SetDataList(0,polylines);

            this.PreparePreview(m, DA.Iteration,polylines.ToList(), false);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.project;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e58fcdc2-b9fe-4d2e-b3db-e1938910a207"); }
        }
    }
}