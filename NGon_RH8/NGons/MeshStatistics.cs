using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.NGons {
    public class MeshStatistics : GH_Component {
        public MeshStatistics()
          : base("MeshStatistics", "MeshStatistics",
              "MeshStatistics","NGon",
              "NGons") {
        }

     

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
          
            pManager.AddIntegerParameter("NGons", "F", "Number of NGons or faces", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Edges", "E", "Number of NGons or edges", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Vertices", "V", "Number of NGons or vertices", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            try {

                Mesh m = new Mesh();
                DA.GetData(0, ref m);

                int f = m.Ngons.Count == 0 ? m.Faces.Count : m.Ngons.Count;
                base.Message = "f " + f.ToString() + " \n" + "e " + m.TopologyEdges.Count.ToString() + "\n" + "v " + m.Vertices.Count.ToString();
                DA.SetData(0, f);
                DA.SetData(1, m.TopologyEdges.Count);
                DA.SetData(2, m.Vertices.Count);


            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());

            }

}






        protected override System.Drawing.Bitmap Icon => Properties.Resources.MeshStatistics;



        public override Guid ComponentGuid => new Guid("{92011ffc-0168-4ee5-9af7-b278aa048d59}");
    }
}