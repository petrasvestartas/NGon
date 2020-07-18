using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Utils {
    public class NGonClear : GH_Component {

        public NGonClear()
          : base("Clear", "Clear",
              "Clear ngons",
              "NGon", "NGons") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            Mesh mm = m.DuplicateMesh();
            mm.Ngons.Clear();

      

            DA.SetData(0, mm);

        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Clear;

            }
        }

        public override GH_Exposure Exposure => GH_Exposure.quarternary;
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("880f76ee-c4f8-42a7-bc81-c60cfa64bfbd"); }
        }
    }
}