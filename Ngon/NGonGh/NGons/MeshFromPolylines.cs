using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.NGons {
    public class MeshFromPolylines : GH_Component_NGon {
        public MeshFromPolylines()
          : base("From Polylines", "FromP",
              "Create Mesh from polylines and adds NGons properties",
              "NGons") {
        }

        bool Fan = false;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Polylines or Curves", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Weld", "W","Weld", GH_ParamAccess.item, 0.00001);

                //pManager[1].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            try {
                List<Curve> curves = new List<Curve>();
                bool flag = DA.GetDataList(0, curves);

                //double weld = -1;
                //DA.GetData(1, ref weld);

                //IEnumerable<Polyline> p = curves.ToPolylines();
                IEnumerable<Polyline> p = curves.ToPolylinesFromCP(Math.Max(0.00001,0.0001));


                Mesh mesh = MeshCreate.MeshFromPolylines(p, -1,Convert.ToInt32(!foo));

                this.PreparePreview(mesh, DA.Iteration);

                DA.SetData(0, mesh);

        } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());

            }

}




        bool foo = true;


        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean("Min Triangulation", this.foo);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {
            this.foo = false;
            reader.TryGetBoolean("Min Triangulation", ref this.foo);
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);

            var fooToggle = Menu_AppendItem(menu, "Min Triangulation", FooHandler, true, this.foo);
            fooToggle.ToolTipText = "Triangulation";
        }

        protected void FooHandler(object sender, EventArgs e) {
            this.foo = !this.foo;
            this.ExpireSolution(true);
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_FromPolylines;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b278aa048d59}");
    }
}