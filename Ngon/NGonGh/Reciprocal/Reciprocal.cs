using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore.SpecificGeometry;

namespace SubD.SubD {
    public class Reciprocal : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public Reciprocal()
          : base("Reciprocal", "Reciprocal",
              "Turn NGon Mesh to reciprocal frame",
               "Reci") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);

            pManager.AddNumberParameter("Angle", "A", "Angle", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Distance", "D", "Distance", GH_ParamAccess.item, 0.24);
            pManager.AddNumberParameter("Width", "W", "Width", GH_ParamAccess.item, 0.1);


            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;


        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.tree);

        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh = new Mesh();
            DA.GetData(0,ref mesh);

            double angle = 10;
            double dist = 0.2;
            double width = 0.1;

            DA.GetData(1, ref angle);
            DA.GetData(2, ref dist);
            DA.GetData(3, ref width);

            if (mesh.IsValid) {

                if (mesh.Ngons.Count == 0) 
                    mesh = MeshCreate.MeshFromPolylines(mesh.GetFacePolylines(),Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                var dt = mesh.ReciprocalFrame( angle, dist, width);
                this.PreparePreview(mesh, DA.Iteration, dt.AllData(), false);
                DA.SetDataTree(0,dt);
            }
        }

     

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.Reciprocal2;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-8140a5ee1f08"); }
        }
    }
}