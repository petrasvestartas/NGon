using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using NGonsCore;
using NGonsCore.Clipper;
using NGonsCore.SpecificGeometry;
using Rhino;
using Rhino.Geometry;

namespace NGonGh.Utils {
    public class RecipricalThickness : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalThickness()
          : base("RecipricalThickness", "RecipricalThickness",
              "Rotate mesh edge by average normal and give thickness",
              "NGon", "Reciprocal") {
       
            }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", 0);
            pManager.AddNumberParameter("Angle", "A", "Angle", 0, 10);
            pManager.AddNumberParameter("Distance", "D", "Distance", 0, 0.24);
            pManager.AddNumberParameter("Width", "W", "Width", 0, 0.1);
            pManager[1].Optional=(true);
            pManager[2].Optional = (true);
            pManager[3].Optional = (true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();
            DA.GetData<Mesh>(0, ref mesh);
            double num = 10;
            double num1 = 0.2;
            double num2 = 0.1;
            DA.GetData<double>(1, ref num);
            DA.GetData<double>(2, ref num1);
            DA.GetData<double>(3, ref num2);
            if (mesh.IsValid) {
                if (mesh.Ngons.Count == 0) {
                    mesh = MeshCreate.MeshFromPolylines(mesh.GetFacePolylines(), RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                }
                DataTree<Polyline> dataTree = mesh.ReciprocalFrame(num, num1, num2);
                //this.PreparePreview(mesh, DA.Iteration, dataTree.AllData(), false, null, null, 255, 0, 0, null, 255, 255, 255, 1);
                DA.SetDataTree(0, dataTree);
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.ReciprocalThickness;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-4568a5ee1f08"); }
        }
    }
}