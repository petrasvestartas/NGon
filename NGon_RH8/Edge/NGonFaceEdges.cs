using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Edge
{
    public class NGonFaceEdges : GH_Component {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public NGonFaceEdges()
          : base("NGon Face Edges", "Face Edges",
              "Get Ngon faces edges id and lines",
              "NGon", "Adjacency")
        {
        }


        List<Rhino.Display.Text3d> texts = new List<Rhino.Display.Text3d>();
        BoundingBox bbox = new BoundingBox();

        public override void DrawViewportWires(IGH_PreviewArgs args) {
            foreach (var t in texts) {
                //args.Display.Draw3dText(t.Text, System.Drawing.Color.Black, t.TextPlane, t.Height, null, false, false, Rhino.DocObjects.TextHorizontalAlignment.Center, Rhino.DocObjects.TextVerticalAlignment.Middle);
                args.Display.Draw3dText(t.Text, System.Drawing.Color.Black, t.TextPlane, t.Height, "RhSS");
            }
                //base.DrawViewportWires(args);
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("TextSize", "T", "Size of display text", GH_ParamAccess.item, 10);

            pManager.AddNumberParameter("Edge Scale", "S", "Edge Scale", GH_ParamAccess.item, 0.75);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Edges", "E", "Get Mesh Edges in Ngons Faces", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ID", "I", "Get Mesh Edges (Lines) in Ngons Faces", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("FID", "F", "Neighbour Faces", GH_ParamAccess.tree);
            //
        }

        protected override void BeforeSolveInstance() {
            texts.Clear();
            bbox = new BoundingBox();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            double size = 10;
            DA.GetData(1, ref size);

            double textScale = 0.75;
            DA.GetData(2, ref textScale);
            int iteration = DA.Iteration;

            bbox.Union(mesh.GetBoundingBox(false));


            int[][] tv = mesh.GetNGonsTopoBoundaries();
            int[][] fe = mesh.GetNGonFacesEdges(tv);
            var l = mesh.GetNGonFacesEdgesLines(fe);
            DataTree<int> dt2 = new DataTree<int>();
            HashSet<int> allE = mesh.GetAllNGonEdges(tv);
            int[] allEArray = allE.ToArray();
           int[][] ef = mesh.GetNgonsConnectedToNGonsEdges(allE, true);

            for(int i = 0; i < mesh.Ngons.Count; i++) {
                for(int j = 0; j < fe[i].Length; j++) {
                    int elocal = Array.IndexOf(allEArray, fe[i][j]);
                    int neiF = (ef[elocal][0] == i) ? ef[elocal][1] : ef[elocal][0];
                    dt2.Add(neiF,new Grasshopper.Kernel.Data.GH_Path(i,DA.Iteration));
                }
            }

            List< Line > ld = new List<Line>();
            foreach (var ll in l)
                ld.AddRange(ll);

            //this.PreparePreview(mesh, DA.Iteration, null, false,null,ld);


            Plane[] planes = mesh.GetNgonPlanes();

            for(int i = 0; i < planes.Length; i++) {
                if(faces)
                    this.texts.Add(new Rhino.Display.Text3d("F"+i.ToString(), planes[i],size));
            }

            for (int i = 0; i < planes.Length; i++) {
                
              for(int j = 0; j < fe[i].Length; j++) {
                     Point3d p = l[i][j].PointAt(0.5);
                    p.Transform(Rhino.Geometry.Transform.Scale(planes[i].Origin, textScale));
                    Plane edgePlane = new Plane(p, l[i][j].Direction, Vector3d.CrossProduct( planes[i].ZAxis, l[i][j].Direction));
                    if(edgesE)
                        this.texts.Add(new Rhino.Display.Text3d("E" + fe[i][j].ToString(), edgePlane, size * 0.75));
                    if (edgesF)
                        this.texts.Add(new Rhino.Display.Text3d("F" + dt2.Branch(i)[j].ToString(), edgePlane, size * 0.75));
                }
               
            }


            DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(l, iteration));
            DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(fe, iteration));
            DA.SetDataTree(2, dt2);

        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_EdgeFaces;

        public override Guid ComponentGuid => new Guid("{b0e10148-f1a2-430b-aa49-a28385512df0}");




        bool faces = false;
        bool edgesE = false;
        bool edgesF = false;


        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean("Display Faces", this.faces);
            writer.SetBoolean("Display Edges", this.edgesE);
            writer.SetBoolean("Display Edge Faces", this.edgesF);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {

            faces = false;
            edgesE = false;
            edgesF = false;

            reader.TryGetBoolean("Display Faces", ref this.faces);
            reader.TryGetBoolean("Display Edges", ref this.edgesE);
            reader.TryGetBoolean("Display Edge Faces", ref this.edgesF);

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);

            var FacesToggle = Menu_AppendItem(menu, "Display Faces", FacesHandler, true, this.faces);
            FacesToggle.ToolTipText = "Display Faces";


            var EdgesEToggle = Menu_AppendItem(menu, "Display Edges", EdgesEHandler, true, this.edgesE);
            EdgesEToggle.ToolTipText = "Display Edges";


            var EdgesFToggle = Menu_AppendItem(menu, "Display Edge Faces", EdgesFHandler, true, this.edgesF);
            EdgesFToggle.ToolTipText = "Display Edge Faces";
        }

        protected void FacesHandler(object sender, EventArgs e) {
            this.faces = !this.faces;
            this.ExpireSolution(true);
        }

        protected void EdgesEHandler(object sender, EventArgs e) {
            this.edgesE = !this.edgesE;
            this.ExpireSolution(true);
        }

        protected void EdgesFHandler(object sender, EventArgs e) {
            this.edgesF = !this.edgesF;
            this.ExpireSolution(true);
        }



    }
}