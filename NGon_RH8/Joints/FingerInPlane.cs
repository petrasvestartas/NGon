using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace NGon_RH8.Transform {
    public class FingerInPlane : GH_Component {
 
        public FingerInPlane()
          : base("Finger", "Finger",
              "Finger in plane",
              "NGon", "Joint") {
        }

    
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "A", "Offset Distance", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Dist", "B", "Offset Distance", GH_ParamAccess.item, 0.25);
            pManager.AddNumberParameter("Scale","S","Scale from corners",GH_ParamAccess.item,0.9);
            pManager.AddCurveParameter("Plines", "P", "Additional polylines", GH_ParamAccess.tree);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        
        }

   
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.tree);
            pManager.AddCurveParameter("AddPolylines", "A", "Addtional polylines used to create offset plate like contouring", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh mesh = new Mesh();
            double d = 1;
            double step = 0.25;
            double scale = 0.9;
            var additionalPolylines = new GH_Structure<GH_Curve>();

            if (DA.GetData(0,ref mesh)) {

                DA.GetData(1, ref d);
                DA.GetData(2, ref step);
                DA.GetData(3, ref scale);
                DA.GetDataTree(4, out additionalPolylines);

                var additionalPolylinesCopy = new GH_Structure<GH_Curve>(additionalPolylines,false);

                bool flag = true;
                if (additionalPolylines.Branches.Count != 2)
                    flag = false;
                else {
                    if (additionalPolylines[0].Count != mesh.Ngons.Count || additionalPolylines[1].Count != mesh.Ngons.Count)
                        flag = false;

                }
                if (!flag) {
                    var plines = Joinery.Miter(mesh, d, step, scale, ref additionalPolylinesCopy);

                    DA.SetDataTree(1, GrasshopperUtil.ListOfListsToTree(plines, DA.Iteration));
                    DA.SetDataTree(2, additionalPolylinesCopy);
                } else {
                    //public static List<List<Polyline>> Miter(Mesh M,  double D,double JointStep, double scale, ref GH_Structure<GH_Curve> additionalPlines)
                    var plines = Joinery.Miter(mesh, d, step, scale,  additionalPolylinesCopy);
                    DA.SetDataTree(2, GrasshopperUtil.ListOfListsToTree(plines, DA.Iteration));
                    //DA.SetDataTree(2, additionalPolylinesCopy);
                }


            }
        }

 
        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.Miter;
            }
        }

  
        public override Guid ComponentGuid {
            get { return new Guid("23b32811-86d4-4830-a6d0-c6754861423c"); }
        }
    }
}