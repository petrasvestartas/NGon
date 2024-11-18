
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NGon_RH8.Polygons {
    public class PlaneDisplay : GH_Component {



        string w = "0";
        public override bool Write(GH_IWriter writer) {
            writer.SetString("Thickness", this.thk.ToString());
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {
            string thk_ = thk.ToString();
            reader.TryGetString("Thickness", ref thk_);
            this.thk = thk_ == "" || thk_ == "0" ? 1 : Convert.ToInt32(this.w);
            return base.Read(reader);
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);
     
            var setInt = Menu_AppendItem(menu, "Thickness");
            var footNumber0 = GH_DocumentObject.Menu_AppendTextItem(setInt.DropDown, this.thk.ToString(), null, TextChanged, true);
        }



        private void TextChanged(Grasshopper.GUI.GH_MenuTextBox sender, string newText) {
            this.thk = newText == ""? 1 : Convert.ToInt32(newText);
            ExpireSolution(true);
        }



        public PlaneDisplay()
          : base("PlaneDisplay", "PD",
              "PlaneDisplay",
                "NGon", "Polygon") {
        }

        public override BoundingBox ClippingBox => bb;

        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (planes == null) return;
            if (base.Hidden|| planes.Count == 0) return;

            //Color grid = base.Attributes.Selected ? gridColorSelected : gridColor;
            Color grid = System.Drawing.Color.Black;
            double r = Grasshopper.CentralSettings.PreviewPlaneRadius;
            double step = 2;
            foreach (Plane pln in planes) {
                      //Grid
                      for(double i = -1.0; i <= 1.0; i += step){
                        Point3d p0 = pln.PointAt(i * r, -r);
                        Point3d p1 = pln.PointAt(i * r, r);
                        args.Display.DrawLine(p0, p1, grid, thk);
                        p0 = pln.PointAt(-r, i * r);
                        p1 = pln.PointAt(r, i * r);
                        args.Display.DrawLine(p0, p1, grid, thk);
                      }

                args.Display.DrawLine(pln.Origin, pln.Origin + pln.XAxis * r, xColor, thk);
                args.Display.DrawLine(pln.Origin, pln.Origin + pln.YAxis * r, yColor, thk);
                
                if (zColor.A != 0) args.Display.DrawLine(pln.Origin, pln.Origin + pln.ZAxis * r*1.0, zColor, thk);
            }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddPlaneParameter("Planes", "P","Planes",GH_ParamAccess.tree);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }


        List<Plane> planes;
        BoundingBox bb;
        Color xColor;
        Color yColor;
        Color zColor;
        Color gridColor;
        Color gridColorSelected;
        int thk;
        protected override void SolveInstance(IGH_DataAccess DA) {

            GH_Structure<GH_Plane> tree = new GH_Structure<GH_Plane>();
            DA.GetDataTree(0, out tree);
         

            if (DA.Iteration == 0) {
                planes = new List<Plane>();
                bb = BoundingBox.Empty;
                xColor = System.Drawing.Color.FromArgb(255,0,0);
                yColor = System.Drawing.Color.FromArgb(162,255, 0);
                zColor = System.Drawing.Color.FromArgb(0,100, 255);
                //thk = 3;
                gridColor = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.IndianRed);
                gridColorSelected = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColourSelected", Color.YellowGreen);
            }

            double r = Grasshopper.CentralSettings.PreviewPlaneRadius;
            Interval ri = new Interval(-r, r);

            foreach (GH_Plane p in tree.AllData(true))
                if (p.Value != null) {
                    planes.Add(p.Value);
                    bb = BoundingBox.Union(bb, new Box(p.Value, ri, ri, ri).BoundingBox);
                }
   
         
        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.planeDisplay;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f9cd7308-8dc6-49e7-894c-3b0d73527996"); }
        }
    }
}