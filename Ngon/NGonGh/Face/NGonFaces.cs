using System;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;
using System.Collections.Generic;

namespace NGonGh.Face
{
    public class NGonFaces : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the NGonFaces class.
        /// </summary>
        public NGonFaces()
          : base("NGon Faces", "NGonFaces",
              "Get Mesh Faces in NGons",
              "Face")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Faces", "F", "Get Mesh faces in Ngons", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Faces", "I", "Get Mesh faces in Ngons", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            uint[][] fId = mesh.GetFacesInNGons();
            Polyline[][] fPolylines = mesh.GetFacePolylinesInNgons(fId);

            List <Polyline> display = new List<Polyline>();
            foreach (var d in fPolylines)
                foreach(var dd in d)
                    if(dd != null && dd.IsValid)
                        display.Add(dd);


            this.PreparePreview(mesh, DA.Iteration, display, false,null);
            var dtPlines = GrasshopperUtil.JaggedArraysToTree(fPolylines, iteration);
            DA.SetDataTree(0, dtPlines);
            DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(fId, iteration));
        }


        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.Icons_NGons_MeshFacesInNgon;


        public override Guid ComponentGuid => new Guid("{67ea4b9a-67a0-42c1-9634-95b3976892de}");
    }
}