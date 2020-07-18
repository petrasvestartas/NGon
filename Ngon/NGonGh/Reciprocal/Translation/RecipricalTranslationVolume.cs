using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using NGonsCore.Clipper;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace NGonGh.Utils {
    public class RecipricalTranslationVolume : GH_Component {

        public RecipricalTranslationVolume()
          : base("ReciMoveVol", "ReciMoveVol",
              "Adding volumetric elements to translation reciprocal",
              "NGon", "Reciprocal") {
       
            }

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddGenericParameter("Nexors0", "Nexors0", "Nexors0", GH_ParamAccess.item);
            pManager.AddGenericParameter("MeshProp0", "Prop0", "Mesh Properties0", GH_ParamAccess.item);

            pManager.AddGenericParameter("Nexors1", "Nexors1", "Nexors1", GH_ParamAccess.item);
            pManager.AddGenericParameter("MeshProp1", "Prop1", "Mesh Properties1", GH_ParamAccess.item);


            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Beams0", "Beams0", "Beams0", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Panels0", "Panels0", "Panels0", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RoundBeams0M", "RoundBeams0M", "RoundBeams0M", GH_ParamAccess.tree);
            pManager.AddCurveParameter("RoundBeams0C", "RoundBeams0C", "RoundBeams0C", GH_ParamAccess.tree);
            pManager.AddCurveParameter("RoundBeams0C", "RoundBeams0C", "RoundBeams0C", GH_ParamAccess.tree);



            pManager.AddCurveParameter("Beams1", "Beams1", "Beams1", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Panels1", "Panels1", "Panels1", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RoundBeams1M", "RoundBeams1M", "RoundBeams1M", GH_ParamAccess.tree);
            pManager.AddCurveParameter("RoundBeams1C", "RoundBeams1C", "RoundBeams1C", GH_ParamAccess.tree);
            pManager.AddCurveParameter("RoundBeams1C", "RoundBeams1C", "RoundBeams1C", GH_ParamAccess.tree);


            pManager.AddLineParameter("Dowels0", "Dowels0", "Dowels0", GH_ParamAccess.tree);
            pManager.AddLineParameter("Dowels1", "Dowels1", "Dowels1", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            MeshProps meshProps0 = DA.Fetch<MeshProps>("MeshProp0");
            NGonsCore.Nexorades.Nexors nexors0 = DA.Fetch<NGonsCore.Nexorades.Nexors>("Nexors0");

            MeshProps meshProps1 = DA.Fetch<MeshProps>("MeshProp1");
            NGonsCore.Nexorades.Nexors nexors1 = DA.Fetch<NGonsCore.Nexorades.Nexors>("Nexors1");

            try
            {


                DA.SetDataTree(0, nexors0.Beams2(meshProps0));
                DA.SetDataTree(1, nexors0.Panels(meshProps0));
                nexors0.RoundBeams(meshProps0, 0.06);
                DA.SetDataTree(2, nexors0.GetNexorPipesBreps());
                DA.SetDataTree(3, nexors0.Cuts());
                DA.SetDataTree(4, nexors0.GetCNC_Cuts());
               

                DA.SetDataTree(5, nexors1.Beams2(meshProps1));
                DA.SetDataTree(6, nexors1.Panels(meshProps1));
                nexors1.RoundBeams(meshProps1, 0.06);
                DA.SetDataTree(7, nexors1.GetNexorPipesBreps());
                DA.SetDataTree(8, nexors1.Cuts());
                DA.SetDataTree(9, nexors1.GetCNC_Cuts());

                DA.SetDataTree(10,nexors0.GetDowels(0.03,10));
                DA.SetDataTree(11,nexors1.GetDowels(0.03, 10));


            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.ReciTranslation2;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c40b-4bf1-b2b4-1414a5ee1f04"); }
        }
    }
}