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
    public class Coloring : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Coloring()
          : base("Coloring", "Coloring",
              "Coloring Edges by n colors",
              "NGon", "Graph") {
       
            }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
         
            pManager.AddMeshParameter("M", "M", "M", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n","n","n", GH_ParamAccess.item,2);
            pManager.AddBooleanParameter("EF", "EF", "Edges or Faces color", GH_ParamAccess.item, true);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Edges", "Edges", "Edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ID", "ID", "ID", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

        
            Mesh M = DA.Fetch<Mesh>("M");
            int n = 2;
            DA.GetData(1, ref n);

            bool ef = true;
            DA.GetData(2, ref ef);
            try
            {
                if (ef) {
              
                    Grasshopper.DataTree<int> Colors = Algo.Graph._GraphColorHalfEdges(M,n);
                    List<Polyline> EdgeOutlines = M._EFPolylinesAll();

                    DA.SetDataList(0, EdgeOutlines);
                    DA.SetDataTree(1, Colors);
                } else {
                    Grasshopper.DataTree<int> Colors = Algo.Graph._GraphColorFaces(M,n);
             

                    DA.SetDataList(0, M.GetPolylines());
                    DA.SetDataTree(1, Colors);
                }
               

            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.Coloring;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c40b-4bf1-b2b4-1145a5ee1f04"); }
        }
    }
}