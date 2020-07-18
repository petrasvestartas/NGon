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
    public class RecipricalTranslation : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalTranslation()
          : base("ReciMove", "ReciMove",
              "Translation of Edges, forming nexorade",
              "NGon", "Reciprocal") {
       
            }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
         
            pManager.AddGenericParameter("MeshProp", "Prop", "Mesh Properties", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "Dist", "Distance", GH_ParamAccess.list);

            //pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddLineParameter("Lines","L","Line",GH_ParamAccess.tree);
            pManager.AddLineParameter("Ecc", "E", "Line Eccentricities", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Nexors", "Nexors", "Nexors", GH_ParamAccess.item);
            pManager.AddGenericParameter("MeshProp", "Prop", "Mesh Properties", GH_ParamAccess.item);
            pManager.AddLineParameter("LinesN", "LNaked", "Line", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

        
            MeshProps meshProps = DA.Fetch<MeshProps>("MeshProp");
            //GH_Structure<GH_Number> D = DA.FetchTree<GH_Number>("Dist");
            List<double> D = DA.FetchList<double>("Dist");

            try
            {
                double firstDist = (D.Count > 0) ? D[0] : 0.01;
                double[][] DArray = new double[][] { new double[1] { D[0] } };


                if (D.Count > 0)
                {
                     DArray = new double[meshProps.FEFlatten.Length][];
                    for (int i = 0; i < meshProps.FEFlatten.Length; i++)
                        DArray[i] = new double[meshProps.FEFlatten[i].Length];

                    for (int i = 0; i < meshProps._FlattenFE.Count; i++)
                        DArray[meshProps._FlattenFE[i][0]][meshProps._FlattenFE[i][1]] = D[i % D.Count];
                }


                //Compute translation
                //Create Nexors
                NGonsCore.Nexorades.Nexors nexors = new NGonsCore.Nexorades.Nexors();
                for (int i = 0; i < meshProps.M._countF(); i++)
                {
                    for (int j = 0; j < meshProps.M._countE(i); j++)
                    {
                        nexors.Add(meshProps.EFLines[i][j], i, j);
                        nexors[i, j].translation = DArray[i][j];
                    }
                }



                Line[][] translatedLines = meshProps.NexorTranslateLines(ref nexors, DArray);

           

                //Output
                DA.SetDataTree(0, nexors.GetNexorLines());
                DA.SetDataTree(1, nexors.GetNexorEccentricities());
                DA.SetData(2, nexors);
                DA.SetData(3, meshProps);
                DA.SetDataTree(4, nexors.GetNexorLines(-1));

            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.ReciTranslation1;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c40b-4bf1-b2b4-1423a5ee1f04"); }
        }
    }
}