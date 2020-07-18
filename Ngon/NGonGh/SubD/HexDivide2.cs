using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace SubD.SubD {
    public class HexQuadDivide : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public HexQuadDivide()
          : base("HexQuadDivide", "HexQuadDivide",
              "Divide nurbs surface into hexagons based on quad tiles",
               "Sub") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddSurfaceParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U", "U", "Divide in first direction", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V", "V", "Divide in second direction", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Parameter", "T", "Scale hexagon", GH_ParamAccess.item, 0.25);

            pManager.AddBooleanParameter("Swap", "S", "Swap direction of the surface from UV to VU, RhinoCommon Transpose", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Rebuild", "R", "Rebuild Surface", GH_ParamAccess.item, 9);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

            pManager[4].Optional = true;
            pManager[5].Optional = true;
 

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Surface S = null;
            DA.GetData(0, ref S);

            int U = 10;
            int V = 10;
            int T = 0;
            double a = 0;
            int r = 9;
            bool q = true;

            bool swap = true;
            DA.GetData(1, ref U);
            DA.GetData(2, ref V);
            DA.GetData(3, ref a);
        
            DA.GetData(4, ref swap);
            DA.GetData(5, ref r);


            Surface tempS = S;

            if (swap) {
                S = tempS.Transpose(true);
                
            } else {
                S = tempS.Reverse(0, true);
            }

            if (r > 0)
                S = S.Rebuild(3, 3, r, r);


            if (U <= 0)
                U = 1;

            if (V <= 0)
                V = 1;

            S.SetDomain(0, new Interval(0, 1));
            S.SetDomain(1, new Interval(0, 1));


            bool f = false;

            if (T == 0 || T == 1)
                f = true;


            if (T != 0 && T != 1) {
                T = 0;
            }

   

            Mesh mesh = MeshCreate.HexMesh2(S, U, V, a);


            DA.SetData(0, mesh);
            this.PreparePreview(mesh, DA.Iteration);
  
        }




        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.hex2;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b558c9-c00b-4bf1-b2b4-8110a5ee1f08"); }
        }
    }
}