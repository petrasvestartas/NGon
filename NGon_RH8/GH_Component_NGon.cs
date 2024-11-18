using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Xml.Linq;
using System.Linq;

namespace NGon_RH8
{

    public class GH_Component_NGon : GH_Component
    {


        public GH_Component_NGon(string Name, string Nick, string Desc, string Subcategory) : base(Name, Nick, Desc, "NGon", Subcategory) { }


        protected List<Mesh> Dmesh = new List<Mesh>();
        protected List<Line> Dlines = new List<Line>();
        protected List<Line> Dlines2 = new List<Line>();
        protected List<Polyline> Dpolylines = null;
        protected List<Point3d> Dpoints = null;
        protected List<Line[]> DLineArrays = null;
        protected bool displayMesh = false;


        public System.Drawing.Color baseC0 = System.Drawing.Color.White;
        public System.Drawing.Color baseC1 = System.Drawing.Color.Red;
        public double baset0 = 0;
        public double baset1 = 0;

        protected BoundingBox Dbbox;
        System.Drawing.Color col = System.Drawing.Color.Black;
        Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(System.Drawing.Color.White);
        int lineWeight = 1;
        public System.Drawing.Color lineColor = System.Drawing.Color.Black;

        bool changePreview = false;

        public override bool IsPreviewCapable => true;

        public override BoundingBox ClippingBox => Dbbox;

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {

            if (this.Hidden || this.Locked || !this.displayMesh) return;
            {
                if (changePreview)
                {
                    if (Attributes.Selected == false)
                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshShaded(m, mat);//mat
                    else
                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshShaded(m, mat);//mat


                }
                else
                {
                    if (Attributes.Selected == false)
                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshShaded(m, args.ShadeMaterial);//mat
                    else
                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshShaded(m, args.ShadeMaterial_Selected);//mat
                }
            }

        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {


            bool edgesOn = Grasshopper.CentralSettings.PreviewMeshEdges;

            col = Attributes.Selected ? args.WireColour_Selected : args.WireColour;
            this.lineWeight = args.DefaultCurveThickness;

            if (this.Hidden || this.Locked) return;



            if (Attributes.Selected == false)
            {

                if (edgesOn)
                {
                    if (changePreview)
                    {

                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshWires(m, this.lineColor, this.lineWeight);//mat

                    }
                    else
                    {

                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshWires(m, args.WireColour);//mat

                    }
                }

                if (this.displayMesh)
                    args.Display.DrawLines(Dlines, col, lineWeight);

                if (Dpoints != null)
                    args.Display.DrawPoints(Dpoints, Rhino.Display.PointStyle.Circle, 1f, col);

                if (Dlines2 != null)
                    args.Display.DrawLines(Dlines2, col, lineWeight);

                if (Dpolylines != null)
                {

                    foreach (Polyline polyline in Dpolylines)
                        args.Display.DrawPolyline(polyline, col, lineWeight);
                }

                if (DLineArrays != null)
                {
                    foreach (Line[] lines in DLineArrays)
                    {
                        if (lines.Length != 0)
                        {
                            args.Display.DrawArrows(lines, System.Drawing.Color.Black);
                            args.Display.DrawPoint(lines[0].From, Rhino.Display.PointStyle.Circle, 6, System.Drawing.Color.Black);
                        }
                    }
                }


            }
            else
            {
                if (edgesOn)
                {

                    if (changePreview)
                    {

                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshWires(m, this.lineColor, this.lineWeight + 1);//mat
                    }
                    else
                    {

                        foreach (Mesh m in Dmesh)
                            args.Display.DrawMeshWires(m, args.WireColour_Selected);//mat
                    }
                }





                if (Dpoints != null)
                    args.Display.DrawPoints(Dpoints, Rhino.Display.PointStyle.Circle, 3f, col);

                if (Dlines2 != null)
                    args.Display.DrawLines(Dlines2, col, lineWeight + 1);

                if (Dpolylines != null)
                {
                    //Rhino.RhinoApp.WriteLine(Dpolylines.Count.ToString());
                    foreach (Polyline polyline in Dpolylines)
                        args.Display.DrawPolyline(polyline, col, lineWeight + 1);
                }

                if (DLineArrays.Count != 0)
                    foreach (Line[] lines in DLineArrays)
                    {
                        if (lines != null)
                        {
                            args.Display.DrawArrows(lines, System.Drawing.Color.Black);
                            args.Display.DrawPoint(lines[0].From, Rhino.Display.PointStyle.Circle, 6, System.Drawing.Color.Black);
                        }
                    }



            }




        }

        public virtual void PreparePreview(Mesh msh, int DA_interation, List<Polyline> p = null, bool dMesh = true, List<Point3d> pt = null, List<Line> ln = null, int r = 255, int g = 0, int b = 0,
            List<Line[]> lineArrays = null, int fr = 255, int fg = 255, int fb = 255, int lineWeight = 1, bool changeDefaultPreview = false, int lr = 255, int lg = 0, int lb = 0)
        {

            if (changeDefaultPreview)
            {

                //this.PreparePreview(mesh, DA.Iteration, null, true, null, null, b.R, b.G, b.B, null, f.R, f.G, f.B, w);
                this.changePreview = changeDefaultPreview;
                this.mat.IsTwoSided = true;
                this.mat.Diffuse = System.Drawing.Color.FromArgb(fr, fg, fb);
                this.mat.BackDiffuse = System.Drawing.Color.FromArgb(r, g, b);
                this.lineColor = System.Drawing.Color.FromArgb(lr, lg, lb);
                this.lineWeight = lineWeight;
            }


            if (DA_interation == 0)
            {




            }








            this.lineWeight = lineWeight;
            this.displayMesh = dMesh;
            try
            {


                if (dMesh)
                    if (msh.IsValid)
                    {
                        Dmesh.Add(msh);
                        Dbbox.Union(msh.GetBoundingBox(false));
                    }

                if (p != null)
                {
                    Dpolylines.AddRange(p);
                }

                if (pt != null)
                    Dpoints.AddRange(pt);

                if (ln != null)
                    Dlines2.AddRange(ln);

                if (lineArrays != null)
                    DLineArrays.AddRange(lineArrays);



            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }





        }
        protected override void BeforeSolveInstance()
        {


            Dmesh.Clear();
            Dlines = new List<Line>();
            Dlines2 = new List<Line>();
            Dpolylines = new List<Polyline>();
            Dpoints = new List<Point3d>();
            DLineArrays = new List<Line[]>();
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            for (int i = 0; i < pManager.ParamCount; i++)
                pManager.HideParameter(i);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            for (int i = 0; i < pManager.ParamCount; i++)
                pManager.HideParameter(i);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {


        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e6eda5fb-6967-425f-8cee-10eade0f26f1"); }
        }
    }
}