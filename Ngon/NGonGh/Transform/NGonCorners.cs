using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;

namespace NGonGh.Vertex
{
    public class NGonCorners : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the NGonVertices class.
        /// </summary>
        public NGonCorners()
          : base("Corners", "Corners",
              "Corners",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Angle in degrees", GH_ParamAccess.item,135);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddPointParameter("Corners", "C", "Vertices whose  both neighvours are connected to more than one ngon", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID", "I", "Mesh vertex id", GH_ParamAccess.list);


        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double angle = 135;
            DA.GetData(1, ref angle);

            List<Point3d> points = new List<Point3d>();
            List<int> mv = new List<int>();

            //Inputs
            int[][] tv = m.GetNGonsTopoBoundaries();
            HashSet<int> tvAll = m.GetAllNGonsTopoVertices();
            HashSet<int> e = m.GetAllNGonEdges(tv);
            int[] allEArray = e.ToArray();
            int[] allvArray = tvAll.ToArray();

            //Outputs
            Point3f[] allVPt = m.GetAllNGonsTopoVerticesPoint3F(tvAll);
            List<int>[] conV = m.GetConnectedNgonVertices(allEArray, allvArray);
            //List<Point3d>[] conVP = m.GetConnectedNgonPoints(conV);

            int[][] NGons = m.GetNGonsConnectedToNGonTopologyVertices(tvAll, false);
            bool[] flags = m.GetNakedNGonPointStatus(tvAll);

            Dictionary<int, int> dictV_NV = new Dictionary<int, int>();
            for (int i = 0; i < allvArray.Count(); i++)
                dictV_NV.Add(allvArray[i], i);



            for (int i = 0; i < flags.Length; i++) {

                if (flags[i]) {
                    List<int> v = conV[i];
                    //Rhino.RhinoApp.WriteLine(flags[i].ToString());
                    //Rhino.RhinoApp.WriteLine(v.Count.ToString());
                    if (v.Count == 2) {
                        int a = dictV_NV[v[0]];
                        int b = dictV_NV[v[1]];
                        int[] ngonsA = NGons[a];
                        int[] ngonsB = NGons[b];
                        //Rhino.RhinoApp.WriteLine(ngonsA.Length.ToString() + " " + ngonsB.Length.ToString());
                        //Rhino.RhinoApp.WriteLine(NGons.Length.ToString() );
                       
                        if (ngonsA.Length > 1 || ngonsB.Length > 1) {
                            Line l0 = new Line(m.Vertices[allvArray[i]], m.Vertices[allvArray[a]]);
                            Line l1 = new Line(m.Vertices[allvArray[i]], m.Vertices[allvArray[b]]);
                            double angle_ = Rhino.RhinoMath.ToDegrees( Vector3d.VectorAngle(l0.Direction, l1.Direction, Vector3d.CrossProduct(l0.Direction, l1.Direction)));
                            //Rhino.RhinoApp.WriteLine(angle.ToString());
                            if (angle_ < angle) {
                                points.Add(m.Vertices[allvArray[i]]);
                                mv.Add(allvArray[i]);
                            }
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(m.Vertices[allvArray[i]]);
                        }
                    }
                }
            }



            DA.SetDataList(0, points);
            DA.SetDataList(1, mv);

            base.PreparePreview(m, DA.Iteration, null, false, points);
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Corners;

        public override Guid ComponentGuid => new Guid("{f6fe728a-5e0d-4224-a12d-14d80150df7f}");
    }
}