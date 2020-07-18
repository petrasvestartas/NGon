using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;


namespace SubD.SubD {
    public class EqualDivide : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the EqualDivide class.
        /// </summary>
        public EqualDivide()
          : base("EqualDivide", "EqualDivide",
              "Divide nurbs surface into strips of similar length",
               "Sub") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddSurfaceParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddIntegerParameter("DivisionsU", "U", "Number of Strips", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("DivisionsU", "V", "Number of Strips", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Distance", "D", "Strips are divided by this number", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Lerp", "t", "Equalize distance between each line segment", GH_ParamAccess.item, 0.25);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polylines", "P", "Polylines before meshing", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {

            Surface s = null;
            int n = 10;
            int v = 10;
            double d = 10.0;
            double l = 0.00;

            DA.GetData(0, ref s);
            DA.GetData(1, ref n);
            DA.GetData(2,ref v);
            DA.GetData(3, ref d);
            DA.GetData(4, ref l);


            List<Polyline> polylines0 = new List<Polyline>();
            List<Polyline> polylines1 = new List<Polyline>();

            Mesh mesh = SurfaceUtil.SubdivideSurfaceEqualDist(s, n,v, d, l, ref polylines0, ref polylines1);

            polylines0.AddRange(polylines1);

            this.PreparePreview(mesh, DA.Iteration);
            DA.SetData(0, mesh);
            DA.SetDataList(1,polylines1);
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.EqualDivide;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("c3a8c1bf-a444-47df-a4bc-5cb43aadcbb8"); }
        }
    }
}