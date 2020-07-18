using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.NGons {
    public class MeshFromPolylinesWithHoles : GH_Component_NGon {
        public MeshFromPolylinesWithHoles()
          : base("Delaunay", "Delaunay",
              "Delaunay with holes",
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

                List<Curve> curves = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Polylines");

                double len = -1;
                int id = -1;

                int counter = 0;
                foreach (Curve c in curves) {

                    double tempLen = c.GetBoundingBox(false).Diagonal.Length;
                    if (tempLen > len) {
                        len = tempLen;
                        id = counter++;
                    }

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

                /*
                GH_Structure<GH_Curve> curves = new GH_Structure<GH_Curve>();
                bool flag = DA.GetDataTree(0, out curves);

                double len = -1;
                int id = -1;

                int counter = 0;
                foreach(GH_Curve c in curves) {
                    
                    double tempLen = c.Value.GetBoundingBox(false).Diagonal.Length;
                    if (tempLen > len) {
                        len = tempLen;
                        id = counter++;
                    }
                    
                }

                GH_Structure<GH_Curve> curves_ = new GH_Structure<GH_Curve>();
                curves_.AppendRange(curves[id]);

                //for(int i = 0; i < curves.PathCount)


                double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

                //reserve one processor for GUI
                int totalMaxConcurrancy = System.Environment.ProcessorCount - 1;
                base.Message = totalMaxConcurrancy + " threads";

                //create a dictionary that works in parallel
                var mPatch = new System.Collections.Concurrent.ConcurrentDictionary<GH_Path, Rhino.Geometry.Mesh>();

                //Multi-threading the loop
                System.Threading.Tasks.Parallel.ForEach(curves.Paths,
                  new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = totalMaxConcurrancy },
                  pth => {
                     
                  //convert first curve in branch to polyline
                  //don't know why the boundary parameter can't be a Curve if the holes are allowed to be Curves
                  Polyline pL = null;
                      var tempCurve = curves.get_DataItem(pth,0).Value;
                      tempCurve.TryGetPolyline(out pL);

                  Curve[] crv = new Curve[curves.get_Branch(pth).Count];

                      int i = 0;
                      foreach (GH_Curve ghc in curves)
                          crv[i++] = ghc.Value;
                      
                  //check validity of pL
                  if (!pL.IsValid) {
                          base.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Curve could not be converted to polyline or is invalid");
                      }

                  //the magic found here:
                  //https://discourse.mcneel.com/t/mesh-with-holes-from-polylines-in-rhinowip-to-c/45589
                  mPatch[pth] = Rhino.Geometry.Mesh.CreatePatch(pL, tol, null, crv, null, null, true, 1);
                
                  });
                //End of multi-threaded loop


                //convert dictionary to regular old data tree
                GH_Structure<GH_Mesh> mTree = new GH_Structure<GH_Mesh>();
                Mesh allMesh = new Mesh();
                foreach (KeyValuePair<GH_Path, Rhino.Geometry.Mesh> m in mPatch) {
                    mTree.Append(new GH_Mesh(m.Value), m.Key);
                    allMesh.Append(m.Value);
                }


                DA.SetDataTree(0,mTree);




                // Polyline[] p = curves.ToPolylines();


                // Mesh mymesh = MeshCreate.MeshFromClosedPolylineWithHoles(p);
                // DA.SetData(0, mymesh);
                */
                //this.PreparePreview(allMesh, DA.Iteration,allMesh.GetFacePolylines());

            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }



        }




        protected override System.Drawing.Bitmap Icon => Properties.Resources.delaunayHoles;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b278aa048d14}");
    }
}