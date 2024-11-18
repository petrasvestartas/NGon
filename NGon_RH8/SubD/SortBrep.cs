using System;
using System.Collections.Generic;
using NGonCore;
using NGonCore.Graphs;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.SubD {
    public class SortBrep : GH_Component {
        /// <summary>
        /// Initializes a new instance of the SortBrep class.
        /// </summary>
        public SortBrep()
          : base("SortBrep", "SortBrep",
              "Sort Brep Faces if possible",
              "NGon", "Subdivide") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Brep", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Brep oriented if possible", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Brep b = null;
            DA.GetData(0, ref b);
            Brep bOriented = b.OrientBrep();
            DA.SetData(0, bOriented);
        }
        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.OrientBrep;

            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("b2012910-e351-4dd9-8eba-ee8f30812bf3"); }
        }
    }
}