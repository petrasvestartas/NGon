using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.Modifications {
    public class Flip : GH_Component_NGon {

        public Flip()
          : base("Flip", "Flip",
              "Flip",
              "Transform") {
        }

  
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh with reversed ngons", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "M", "Mesh with reversed normals", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                Mesh mesh = null;
                DA.GetData(0, ref mesh);

                if (mesh != null) {

                    Polyline[] plines = mesh.GetPolylines();
                    for(int i = 0; i < plines.Length; i++) {
                        plines[i] = new Polyline(plines[i].Flip());
                    }

                    Mesh meshReversed = MeshCreate.MeshFromPolylines(plines, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    Mesh meshFlipped = mesh.DuplicateMesh();
                    meshFlipped.Flip(true, true, true);

                    DA.SetData(0, meshReversed);

                    PreparePreview(meshReversed, DA.Iteration);
                    DA.SetData(1, meshFlipped);

                }
            }catch(Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Flip;// Properties.Resources.Dual;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f2f8ba8b-822c-4e11-9ece-40b7aca1298f"); }
        }
    }
}