using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using NGonsCore.Clipper;
using System.Windows.Forms;
using GH_IO.Serialization;

namespace NGonGh.Utils
{
    public class WeldMesh : GH_Component_NGon
    {

        public WeldMesh()
          : base("WeldMesh", "Weld",
              "Description",
             "Utilities Mesh")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh to weld", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius","R","Radius for welding", GH_ParamAccess.item,0.001);
            pManager[0].Optional = true;
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh to weld", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> m_ = DA.FetchList<Mesh>("Mesh");
            double radius = DA.Fetch<double>("Radius");

            Mesh[] m = new Mesh[m_.Count];
            Mesh mj = new Mesh();

            for(int i = 0; i < m_.Count; i++) {
                m[i] = m_[i].DuplicateMesh();
                mj.Append(m_[i]);
            }





            if (radius == -2) {
                for (int i = 0; i < m_.Count; i++) {
                    if (m[i].SolidOrientation() == -1)
                        m[i].Flip(true, true, true);
                    m[i].Unweld(0, true);

                    mj.Append(m[i]);
                }

                DA.SetDataList(0,m);

                this.PreparePreview(mj, DA.Iteration);

            }
            if (radius == -3) {

                for (int i = 0; i < m_.Count; i++) {
                    if (m[i].SolidOrientation() == -1)
                        m[i].Flip(true, true, true);
                    m[i]=m[i].WeldUsingRTree(0.001, foo);
                    m[i].Unweld(0, true);
                    m[i].UnifyNormals();

                    mj.Append(m[i]);
                }
                DA.SetDataList(0, m);

               
                this.PreparePreview(mj, DA.Iteration);
            }


            //m.WeldUsingRTree(radius);
            else if(radius>0){

                //Rhino.RhinoApp.WriteLine("hi");

                mj=mj.WeldUsingRTree(radius,foo);


                mj.Compact();
                mj.Vertices.CombineIdentical(true, true);
                mj.Vertices.CullUnused();

                if (mj.Ngons.Count > 0)
                    mj.UnifyNormalsNGons();
                else
                    mj.UnifyNormals();


                mj.Weld(3.14159265358979);
                mj.FaceNormals.ComputeFaceNormals();
                mj.Normals.ComputeNormals();
                


                if (mj.SolidOrientation() == -1)
                    mj.Flip(true, true, true);

                this.PreparePreview(mj, DA.Iteration);



                DA.SetDataList(0, new Mesh[] {mj });

                //m.WeldFull(radius);

            }
         
            //DA.SetData(0, m);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.WELD;
            }
        }

        bool foo = false;
        bool align = false;

        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean("Use RTree", this.foo);
            writer.SetBoolean("Align", this.align);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {
            this.foo = false;
            this.align = false;
            reader.TryGetBoolean("Use RTree", ref this.foo);
            reader.TryGetBoolean("Align", ref this.align);
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);

            var fooToggle = Menu_AppendItem(menu, "Use RTree", FooHandler, true, this.foo);
            fooToggle.ToolTipText = "Seach points using RTree, if nothing works...";


            var alignToggle = Menu_AppendItem(menu, "Align", AlignHandler, true, this.align);
            alignToggle.ToolTipText = "Heal Naked Edges";
        }

        protected void FooHandler(object sender, EventArgs e) {
            this.foo = !this.foo;
            this.ExpireSolution(true);
        }

        protected void AlignHandler(object sender, EventArgs e) {
             this.align = !this.align;
            this.ExpireSolution(true);
        }



        public override Guid ComponentGuid
        {
            get { return new Guid("55f1321a-d5e1-4c3f-aedb-bd27ce77a583"); }
        }
    }
}