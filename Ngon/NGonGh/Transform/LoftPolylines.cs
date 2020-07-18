using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using NGonsCore;
namespace NGonGh.Polylines
{
    public class LoftPolylines : GH_Component_NGon
    {
        public LoftPolylines()
          : base("Loft Polylines", "Loft",
              "Loft two polylines, must contain equal number of points",
               "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddCurveParameter("Curve0", "C0", "Curve to loft - data tree with two polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Curve1", "C1", "Curve to loft - data tree with two polylines", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Fast", "F", "Fast", GH_ParamAccess.item, true);
           

            //pManager.AddCurveParameter("Curve0", "C0", "Curve to loft - data tree with two polylines", GH_ParamAccess.item);
            //pManager.AddCurveParameter("Curve1", "C1", "Curve to loft - data tree with two polylines", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("Divisions", "N", "Curves divisions if input is not polyline", GH_ParamAccess.item, 2);
            //pManager.AddBooleanParameter("Cap", "C", "Cap", GH_ParamAccess.item, true);


            pManager[1].Optional = true;
            //pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.tree);
           // pManager.AddCurveParameter("Polylines", "P", "Lofted polylines", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Grasshopper.DataTree<Polyline> P0 = new Grasshopper.DataTree<Polyline>();
            Grasshopper.DataTree<Polyline> P1 = new Grasshopper.DataTree<Polyline>();

            GH_Structure<GH_Curve> C0 = new GH_Structure<GH_Curve>();
            GH_Structure<GH_Curve> C1 = new GH_Structure<GH_Curve>();
            bool b = true;

            DA.GetDataTree(0, out C0);
            DA.GetDataTree(1, out C1);
            DA.GetData(2,ref b);

            if (C0.DataCount == C1.DataCount)
            {
                for (int i = 0; i < C0.Branches.Count; i++)
                {
                    if (C0.Branches[i].Count == 1 && C1.Branches[i].Count == 1)
                    {
                        Polyline poly0 = new Polyline();
                        Polyline poly1 = new Polyline();
                        bool f0 = C0.Branches[i][0].Value.TryGetPolyline(out poly0);
                        bool f1 = C1.Branches[i][0].Value.TryGetPolyline(out poly1);



                        if (poly0.Count == poly1.Count)
                        {

                            P0.Add(poly0, C0.Paths[i]);
                            P1.Add(poly1, C1.Paths[i]);
                        }
                    }

                }

                if (C0 != null && C1 != null)
                {
                    Grasshopper.DataTree<Mesh> mesh = MeshUtil.LoftMeshFast(P0, P1, b);
                    Mesh meshDispay = new Mesh();
                    foreach (Mesh mm in mesh.AllData())
                        meshDispay.Append(mm);

                    PreparePreview(meshDispay, DA.Iteration);
                    DA.SetDataTree(0, mesh);
                }

            }
            else if (C0.DataCount > 0 && C1.DataCount == 0)
            {

                for (int i = 0; i < C0.Branches.Count; i++)
                {

                    if(C0.Branches[i].Count == 2)
                    {
                        Polyline poly0 = new Polyline();
                        Polyline poly1 = new Polyline();
                        bool f0 = C0.Branches[i][0].Value.TryGetPolyline(out poly0);
                        bool f1 = C0.Branches[i][1].Value.TryGetPolyline(out poly1);


                        if (poly0.Count == poly1.Count)
                        {

                            P0.Add(poly0, C0.Paths[i]);
                            P1.Add(poly1, C0.Paths[i]);
                        }
                    }else if(C0.Branches[i].Count% 2 == 0)
                    {

                        for(int j = 0; j < C0.Branches[i].Count; j+=2)
                        {
                            Polyline poly0 = new Polyline();
                            Polyline poly1 = new Polyline();
                            bool f0 = C0.Branches[i][j].Value.TryGetPolyline(out poly0);
                            bool f1 = C0.Branches[i][j+1].Value.TryGetPolyline(out poly1);


                            if (poly0.Count == poly1.Count)
                            {

                                P0.Add(poly0, new GH_Path(i,(int)(j*0.5)));
                                P1.Add(poly1, new GH_Path(i, (int)(j * 0.5)));
                            }



                        }
                    }

                }


                Grasshopper.DataTree<Mesh> mesh = MeshUtil.LoftMeshFast(P0, P1, b);
                Mesh meshDispay = new Mesh();
                foreach (Mesh mm in mesh.AllData())
                    meshDispay.Append(mm);

                PreparePreview(meshDispay, DA.Iteration);
                DA.SetDataTree(0, mesh);

            }

            //Curve C0 = null;
            //Curve C1 = null;
            //DA.GetData(0, ref C0);
            //DA.GetData(1, ref C1);

            //int N= 2;
            //DA.GetData(2, ref N);

            //bool E = true;
            //DA.GetData(3, ref E);



            //if(C0 != null && C1 != null) {
            //   Mesh mesh = PolylineUtil.LoftTwoCurves(C0, C1, N, E);
            //    PreparePreview(mesh, DA.Iteration);
            //    DA.SetData(0, mesh);
            //}

        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.loft;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6a08c2db-ed0e-4b8c-82c7-3343a1a4223c"); }
        }
    }
}