using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;


namespace NGon_RH8.Modifications
{



    public class ProjectMesh : GH_Component_NGon
    {


        /// <summary>
        /// Initializes a new instance of the Planarize class.
        /// </summary>
        public ProjectMesh()
            : base("ProjectPairs", "ProjectPairs",
                "ProjectPairs",
                "Planarize")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh ", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "For animations, add slider 0.00 to 1.00 to interpolate", GH_ParamAccess.item, 0);
            //pManager.AddIntegerParameter("Type", "T", "0 - separate planes; 1 - separate boxes", GH_ParamAccess.item, 0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh ", GH_ParamAccess.list);
            pManager.AddCurveParameter("Plines","P","Plines",GH_ParamAccess.tree);
            //  pManager.AddCurveParameter("Polylines", "P", "Projected NGons to average planes", GH_ParamAccess.list);
            // pManager.AddIntegerParameter("Id", "I", "Vertex Id in original mesh", GH_ParamAccess.tree);
        }




        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            Mesh mesh_ = new Mesh();
            DA.GetData(0, ref mesh);

            double scale = 1;
            DA.GetData(1, ref scale);

            //int t = 0;
            //DA.GetData(2, ref t);

            Polyline[][] pairplines = null;
            var m = mesh.ProjectPairsMesh(scale, ref pairplines);
            foreach (var mm in m) {
                
                mm.Clean();
                mesh_.Append(mm);

            }
            this.PreparePreview(mesh_, DA.Iteration);


            DA.SetDataList(0,m);
            DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(pairplines, 0));
        }






        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.ProjectPairsMesh;

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf4093");
    }
}