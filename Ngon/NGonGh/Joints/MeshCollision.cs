using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore;

namespace NGonGh.Joints {
    public class MeshCollision : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshCollision class.
        /// </summary>
        public MeshCollision()
          : base("MeshCollision", "MC",
              "MeshCollision",
 "NGon", "Joint") {
        }

    
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("M", "M", "M",GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddIntegerParameter("I", "I", "I", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            List<Mesh> mesh = new List<Mesh>();
            if (DA.GetDataList(0, mesh)){

                List<int>[] mc = MeshUtilSimple.MeshCollisions(mesh);
                DA.SetDataTree(0,GrasshopperUtil.ArrayOfListsToTree(mc));

                //Next check overlapping faces only if ngons exists

                List<Polyline[]>[] mcPlines = new List<Polyline[]>[mc.Length];

                for (int i = 0; i < mc.Length; i++) {

                    mcPlines[i] = new List<Polyline[]>();

                    for (int j = 0; j < mc[i].Count; j++) {

                        //Do it with ngons
                        MeshUtil.GetOverlap(mesh[i], mesh[mc[i][j]], 0.1);

                    }

                }

            }//if


        }

   
        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.MeshCollision;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("87c984dd-a4e6-48dc-89df-71b17e765158"); }
        }
    }
}