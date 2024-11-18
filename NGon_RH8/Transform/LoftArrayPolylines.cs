using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using NGonCore;
using System.Linq;
using NGon_RH8;

namespace NGon_RH8.Polylines
{
    public class LoftArrayPolylines : GH_Component_NGon
    {
        public LoftArrayPolylines()
          : base("Loft Polylines", "Loft ",
              "Loft two polylines, must contain equal number of points",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Polylines to loft - data tree with two polylines", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polylines", "P", "Polylines to loft - data tree with two polylines", GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Curve> c0 = new List<Curve>();
            List<Curve> c1 = new List<Curve>();
            bool flag0 = DA.GetDataList(0, c0);
            bool flag1 = DA.GetDataList(1, c1);
            try
            {

                if (flag0 == true && flag1 == false && c0.Count == 2)
                {
                    //Rhino.RhinoApp.WriteLine("Hi");
                    var p = c0.ToPolylines();
                    Mesh mesh_ = NGonCore.MeshCreate.LoftPolylineWithHoles( new List<Polyline>{ p[0] }, new List<Polyline> { p[1] });
                    mesh_.UnifyNormals();
                    mesh_.FaceNormals.UnitizeFaceNormals();
                    mesh_.Unweld(0.001, true);
                    //if (mesh_.SolidOrientation() == -1)
                    //mesh_.Flip(true, true, true);
                    DA.SetData(0, mesh_);
                    this.PreparePreview(mesh_, DA.Iteration, p.ToList());
                }
                else if (c0.Count == c1.Count)
                {




                    List<Polyline> p = new List<Polyline>();
                    var p0 = c0.ToPolylines(false);
                    var p1 = c1.ToPolylines(false);
                    /*
                    for(int i = 0; i < p0.Length; i++)
                    {
                        p0[i].Transform(Transform.Scale(Point3d.Origin, 100000));
                       p1[i].Transform(Transform.Scale(Point3d.Origin, 100000));
                    }
                    */

                    p.AddRange(p0);
                    p.AddRange(p1);
                    //Rhino.RhinoApp.WriteLine(p0.Length.ToString());
                    Mesh mesh_ = NGonCore.MeshCreate.LoftPolylineWithHoles(p0.ToList(), p1.ToList());

                    //mesh_.Transform(Transform.Scale(Point3d.Origin, 1 /10));
                    //mesh_.Clean();
                    // mesh_.UnifyNormals();
                    //mesh_.Unweld(0.001, true);
                    //if (mesh_.SolidOrientation() == -1)
                    //mesh_.Flip(true, true, true);
                    DA.SetData(0, mesh_);
                    this.PreparePreview(mesh_, DA.Iteration, p);
                }


            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.LoftHoles;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6a08c2db-ed0e-4b4c-82c7-3343a1a4223c"); }
        }
    }
}