using System;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.SubD {
    public class DiamondDivide : GH_Component_NGon {

        public DiamondDivide()
            : base("DiamondDivide", "DiamondDivide",
                "Divide nurbs surface into diamond shapes",
                 "Subdivide") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("U", "U", "Divide in first direction", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V", "V", "Divide in second direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Boundary", "B", "Boundary Ngons", GH_ParamAccess.item, -1);
            pManager.AddBooleanParameter("Swap", "S", "Swap direction of the surface from UV to VU, RhinoCommon Transpose", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Flip", "F", "Flip Triangulation", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {

                //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Surface> stree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Surface>();
                //DA.GetDataTree<Grasshopper.Kernel.Types.GH_Surface>(0, out stree);


                double U = 10;
                int V = 10;

                DA.GetData(1, ref U);
                bool flag2 = DA.GetData(2, ref V);

                int T = -1;
                bool F = false;
                bool swap = true;

                DA.GetData(3, ref T);
                DA.GetData(4, ref swap);
                DA.GetData(5, ref F);



                Brep brep = new Brep();

                if (DA.GetData(0, ref brep)) {


                    if (brep.Faces.Count == 1 && !brep.Faces[0].IsClosed(0) && !brep.Faces[0].IsClosed(1)) {
                        bool flag = true;
                        Surface S = brep.Faces[0].ToNurbsSurface();


  

                       


                        if (flag) {
                            Surface tempS = S;

                            if (swap) {
                                S = tempS.Transpose(true);

                            } else {
                                S = tempS.Reverse(0, true);
                            }


                            if (U <= 0)
                                U = 1;

                            if (V <= 0)
                                V = 1;

                            S.SetDomain(0, new Interval(0, 1));
                            S.SetDomain(1, new Interval(0, 1));

                            var mesh = NGonsCore.MeshCreate.DiamondMesh(S, Math.Max(2,(int)U), V, T, F);

                            DA.SetData(0, mesh);
                            this.PreparePreview(mesh, DA.Iteration);
                            this.Message = "Division UV ";
                        }
                    } else {

                        if (!explode) {// && !flag2

                            int[][] feDivisions = null;
                            if (U == 0)
                                U = 1000;

                            if (!flag2 && foo) {
                                this.Message = "Division U Dist";
                                int[] edgeDivisions = brep.EdgeDivisions(U, ref feDivisions);
                            } else {
                                this.Message = "Division UV Explode";
                            }

                            System.Collections.Generic.List<Polyline> plinesClothed = new System.Collections.Generic.List<Polyline>();
                            System.Collections.Generic.List<Polyline> plinesNaked = new System.Collections.Generic.List<Polyline>();
                            NGonsCore.MeshCreate.HexOrDiamondGrid(brep, Math.Max(2, (int)U), (int)Math.Max(2, V), ref plinesClothed, ref plinesNaked, 0.5, false, feDivisions);
                            plinesClothed.AddRange(plinesNaked);
                            Mesh mesh = NGonsCore.MeshCreate.MeshFromPolylines(plinesClothed, 0);
                            this.PreparePreview(mesh, DA.Iteration);
                            DA.SetData(0, mesh);

                    

                        } else {

                            Mesh mesh = new Mesh();
                            for (int i = 0; i < brep.Faces.Count; i++) {


                                //Reparametricize
                                Surface S = brep.Faces[i].ToNurbsSurface();
                                S.SetDomain(0, new Interval(0, 1));
                                S.SetDomain(1, new Interval(0, 1));
                                var m = NGonsCore.MeshCreate.DiamondMesh(S, Math.Max(2, (int)U), V, T, F);
                                mesh.Append(m);
                            }
                            this.PreparePreview(mesh, DA.Iteration);
                            DA.SetData(0, mesh);
                            this.Message = "Division UV Explode";

                        }

                    }
                }
           
                
            }catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }


        bool foo = true;
        bool explode = false;

        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean("Divisions", this.foo);
            writer.SetBoolean("Explode", this.explode);
            return base.Write(writer);

        }

        public override bool Read(GH_IReader reader) {
            this.foo = false;
            this.explode = true;
            reader.TryGetBoolean("Divisions", ref this.foo);
            reader.TryGetBoolean("Explode", ref this.explode);
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);

            var fooToggle = Menu_AppendItem(menu, "Divisions", FooHandler, true, this.foo);
            fooToggle.ToolTipText = "Divisions";


            var explodeToggle = Menu_AppendItem(menu, "Explode", ExplodeHandler, true, this.explode);
            explodeToggle.ToolTipText = "Explode";
        }

        protected void FooHandler(object sender, EventArgs e) {
            this.foo = !this.foo;

            this.ExpireSolution(true);
        }

        protected void ExplodeHandler(object sender, EventArgs e) {
            this.explode = !this.explode;

            this.ExpireSolution(true);
        }

        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.diamond;

            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b558c9-c00b-4bf1-b2b4-8110a5ee1f02"); }
        }
    }
}