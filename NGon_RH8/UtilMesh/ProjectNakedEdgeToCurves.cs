using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Utils {
    public class ProjectNakedEdgeToCurves : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public ProjectNakedEdgeToCurves()
          : base("ProjectNakedEdgeToCurves", "ProjectNaked",
              "Project Naked Edge To Curves",
             "Utilities Mesh") {
        }

      
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Iteration", "I", "Max Iterations", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Tolerance", "T", "Max Distance to Curve", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            List<Curve> curves = new List<Curve>();
            int maxIterations = 7;
            double maxDistance = 100;

            DA.GetDataList(1,  curves);
            DA.GetData(2, ref maxIterations);
            DA.GetData(3, ref maxDistance);

            if (DA.GetData(0, ref m)) {


     

                var mesh = m.ProjectNakedEdgeToCurves(curves, maxDistance, maxIterations);
                base.PreparePreview(mesh, DA.Iteration);
                DA.SetData(0, mesh);
            }


        }
          
            

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.ProjectNakedEdgeToCurve;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-58ad2b9e8965"); }
        }
    }
}