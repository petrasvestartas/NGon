using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Planarize {

    public class PlanarizeNeighbourFacesNGons : GH_Component_NGon {


        public PlanarizeNeighbourFacesNGons() : base("Planar Faces", "Planar", "Planarize NeighbourFaces NGons ", "Planarize") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            //pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Tolerance", "T", "Planarity tolerance", 0, 0.0);
           // pManager.AddIntegerParameter("Fix Naked", "N", "0:None 1:Fixed 2:Follow normal 3:Positive normal 4:Negative normal 5:Negotiate", 0, 0);
           // pManager.AddIntegerParameter("Iterations", "I", "Iterations", 0, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddCurveParameter("Lines", "L", "Lines from plane intersection, for debugging", GH_ParamAccess.tree);
            //pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            //pManager.AddPlaneParameter("P", "P", "P", GH_ParamAccess.list);
            //pManager.AddPointParameter("P", "P", "P", GH_ParamAccess.list);
            // pManager.AddIntegerParameter("Iterations", "I", "Number of exectuted iterations", 0);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = new Mesh();
          if(  DA.GetData(0, ref mesh)) {
                var dtLines = new DataTree<Line>();
                Polyline[] plines = PlanarizeNeighbourFaces.Run(mesh,ref dtLines);
                DA.SetDataList(0, plines);
                DA.SetDataTree(1, dtLines);
                this.PreparePreview(mesh, DA.Iteration, plines.ToList(),false);
            }

        }



        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.TangentPlane; } }

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf1258");
    }
}

