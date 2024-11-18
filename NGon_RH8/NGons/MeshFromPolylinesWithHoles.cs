using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.NGons {
    public class MeshFromPolylinesWithHoles : GH_Component_NGon {
        public MeshFromPolylinesWithHoles()
          : base("NgonH", "NgonH",
              "Ngon with holes, delaunay with holes",
               "NGons") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Polylines", "P", "Polylines", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            try {

                List<Curve> curves = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Polylines");

                double len = -1;
                int id = -1;

                int counter = 0;
                foreach (Curve c in curves) {

                    double tempLen = c.GetBoundingBox(false).Diagonal.Length;
                    if (tempLen > len) {
                        len = tempLen;
                        id = counter;
                    }
                    counter++;
                }

                Polyline pL = curves[id].ToPolylineFromCP();

                List<Curve> crv = new List<Curve>();
                for (int i = 0; i < curves.Count; i++) {
                    if (i != id) {
                        crv.Add(curves[i]);
                    }
                }

                Mesh mesh = Rhino.Geometry.Mesh.CreatePatch(pL, 0.01, null, crv, null, null, true, 1);

                DA.SetData(0, mesh);
                this.PreparePreview(mesh, DA.Iteration, mesh.GetFacePolylines());



            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }



        }




        protected override System.Drawing.Bitmap Icon => Properties.Resources.delaunayHoles;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b278aa048d14}");
    }
}