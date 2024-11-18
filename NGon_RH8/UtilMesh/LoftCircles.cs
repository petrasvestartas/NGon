using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace NGon_RH8.UtilMesh {
    public class LoftCircles : GH_Component {

        public LoftCircles()
          : base("LoftCircles", "LoftCircles",
              "LoftCircles",
                "NGon", "Utilities Mesh") {
        }

 
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radii", "R", "Radii of Circles", GH_ParamAccess.list);

        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Circles", "C", "Circles", GH_ParamAccess.list);
            pManager.AddBrepParameter("Loft", "B", "Lofted Circles", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Brep brep = null;
            DA.GetData(0, ref brep);
            //List<Curve> C = new List<Curve>(2);
            //DA.GetDataList(0, C);
            List<double> R = new List<double>(2);
            
          
            List<Curve> C = NGonCore.BrepUtil.Get2ValenceCurves(brep);
            DA.GetDataList(1, R);

 
                if (C.Count == 2) {

                    ////////////////////////////////////////////////
                    //Cast curves to circles
                    ////////////////////////////////////////////////
                    Circle c0 = Circle.Unset;
                    Circle c1 = Circle.Unset;
                    C[0].TryGetCircle(out c0);
                    C[1].TryGetCircle(out c1);


                    ////////////////////////////////////////////////
                    //Project first circle to the second cicle end
                    ////////////////////////////////////////////////
                    Rhino.Geometry.Transform projection = Rhino.Geometry.Transform.PlanarProjection(c1.Plane);
                    c1 = c0;
                    c1.Transform(projection);


                    ////////////////////////////////////////////////
                    //Change radii of Circles
                    ////////////////////////////////////////////////

                    if (R.Count > 0) {

                        List<double> radii = new List<double>(2);

                        if (R.Count != 2 && R.Count == 1) {
                            radii.Add(R[0]);
                            radii.Add(R[0]);
                        } else {
                            radii.Add(R[0]);
                            radii.Add(R[1]);
                        }

                        c0 = new Circle(c0.Plane, radii[0]);
                        c1 = new Circle(c1.Plane, radii[1]);
                    }

                    DA.SetDataList(0, new Circle[] { c0, c1 });
                DA.SetData(1, NGonCore.BrepUtil.Loft(c0.ToNurbsCurve(), c1.ToNurbsCurve(), true));
                }
            }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.CircleLoft;
            }
        }

   
        public override Guid ComponentGuid {
            get { return new Guid("3bedff79-370e-4d00-895a-86cbaf7b0440"); }
        }
    }
}