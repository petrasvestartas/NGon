using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonCore;
using NGonCore.Geometry;
using Rhino.Geometry;

namespace NGon_RH8.UtilMesh {
    public class Brep2Edges : GH_Component {

        BoundingBox bbox = new BoundingBox();
        List<Brep> breps = new List<Brep>();
        Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(System.Drawing.Color.FromArgb(200, 200, 200));
        List<Curve> curves = new List<Curve>();
        int weight = 2;
        System.Drawing.Color color = System.Drawing.Color.FromArgb(200, 200, 200);
        List<double> materialParameters = new List<double>() { 0.904, 0.49, 0.0 };

        public override BoundingBox ClippingBox => bbox;
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (!base.Hidden && !base.Locked) {
                foreach (Curve c in curves)
                    args.Display.DrawCurve(c, System.Drawing.Color.Black, weight);
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            if (!base.Hidden && !base.Locked) {
                foreach (Brep b in breps)
                    args.Display.DrawBrepShaded(b, mat);
            }
        }

        public Brep2Edges()
          : base("BrepEdges", "BrepEdges",
              "Gets BRep edges that does not belong to the same surface",
              "NGon", "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Brep", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Weight", "W", "Curve weight", GH_ParamAccess.item, 2);
            pManager.AddColourParameter("Color", "C", "Color", GH_ParamAccess.item, System.Drawing.Color.FromArgb(200, 200, 200));
            pManager.AddNumberParameter("RefShTra", "P", "Parameters of material as a list, reflection - shiness - transparency", GH_ParamAccess.list, new List<double>() { 0.904, 0.49, 0.0 });

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            this.bbox = new BoundingBox();
            this.breps.Clear();
            this.curves.Clear();
            this.materialParameters.Clear();

            GH_Structure<GH_Brep> GH_Breps;
            DA.GetDataTree(0, out GH_Breps);

            DA.GetData(1, ref this.weight);
            DA.GetData(2, ref this.color);
            DA.GetDataList(3, this.materialParameters);

            GH_Structure<GH_Curve> curves = new GH_Structure<GH_Curve>();
            //Get Brep properties
            int i = 0;
            foreach (GH_Brep gh_brep in GH_Breps.AllData(true)) {
                Brep brep = gh_brep.Value;
                this.bbox.Union(brep.GetBoundingBox(true));
                this.breps.Add(brep);
                var crvs = BrepUtil.Get2ValenceCurves(brep);
                this.curves.AddRange(crvs);
                foreach (Curve cc in crvs)
                    curves.Append(new GH_Curve(cc), new GH_Path(i));
                i++;
            }

            //Create Material
            var c = (this.color == System.Drawing.Color.Empty) ? System.Drawing.Color.FromArgb(200, 200, 200) : this.color;
            double r = (this.materialParameters.Count != 3) ? 0.904 : this.materialParameters[0];
            double s = (this.materialParameters.Count != 3) ? 0.49 : this.materialParameters[1];
            double t = (this.materialParameters.Count != 3) ? 0.0 : this.materialParameters[2];

            //base.Message = r.ToString() + " " + s.ToString() + " " + t.ToString() + this.materialParameters.Count.ToString();
            base.Message = this.breps.Count.ToString();

            r = Math.Min(Math.Max(r, 0), 1);
            s = Math.Min(Math.Max(s, 0), 1);

            Rhino.DocObjects.Material material = new Rhino.DocObjects.Material();
            material.ReflectionColor = c;
            material.DiffuseColor = System.Drawing.Color.White;
            material.AmbientColor = System.Drawing.Color.Black;
            material.EmissionColor = System.Drawing.Color.Black;
            material.Reflectivity = r;
            material.ReflectionGlossiness = s;
            material.Transparency = t;

            mat = new Rhino.Display.DisplayMaterial(material);

            //mat.IsTwoSided = true;
            //mat.BackDiffuse = System.Drawing.Color.Red;

            DA.SetDataTree(0, curves);

            foreach (IGH_Param output in base.Params.Output) {
                if (output is Grasshopper.Kernel.IGH_PreviewObject) {
                    Grasshopper.Kernel.IGH_PreviewObject param = output as Grasshopper.Kernel.IGH_PreviewObject;
                    param.Hidden = true;
                }
            }

        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.PreviewBrepEdges;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("792744d9-278b-4916-aa11-5695336844ca"); }
        }
    }
}
