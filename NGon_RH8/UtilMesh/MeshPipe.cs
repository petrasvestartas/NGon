using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Utils {
    public class MeshPipe : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public MeshPipe()
          : base("MeshPipe", "MeshPipe",
              "Create mesh pipe from curves",
             "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius", "R", "Pipe Radius", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Sides", "S", "Number of sides", GH_ParamAccess.item,6);
            pManager.AddIntegerParameter("Tolerance", "T", "Tolerance", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Ending", "E", "0-None, 1-Box, 2-Dome, 3-Flat", GH_ParamAccess.item,0);
            // pManager.AddBooleanParameter("Ending", "F", "", GH_ParamAccess.item);
            pManager[0].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            // private void RunScript(Curve C, double R, int S, int t, int E, bool F, List<Interval> I, ref object A) {


            Curve C = null;
            double R = 0.1;
            int S = 6;
            int t = 10;
            int E = 0;

            DA.GetData<Curve>(0, ref C);
            DA.GetData(1, ref R);
            DA.GetData(2, ref S);
            DA.GetData(3, ref t);
            DA.GetData(4, ref E);



            var capType = MeshPipeCapStyle.Box;
                switch (E) {
                    case (1):
                    capType = MeshPipeCapStyle.Box;
                    break;
                    case (2):
                    capType = MeshPipeCapStyle.Dome;
                    break;
                    case (3):
                    capType = MeshPipeCapStyle.Flat;
                    break;
                    default:
                    capType = MeshPipeCapStyle.None;
                    break;


                }

            if (C != null) {
                if (C.IsValid) {

                    Mesh mesh = Mesh.CreateFromCurvePipe(C, R, S, t, capType, false);

                    PreparePreview(mesh, DA.Iteration);

                    DA.SetData(0, mesh);
                }
            }


            }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.Pipe;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-89ad2b9e9288"); }
        }
    }
}