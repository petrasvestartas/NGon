using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NGonGh.Graphs {
    public class MeshCurve : GH_Component {

        public MeshCurve()
          : base("MeshCurve", "MeshCurve",
              "MeshCurve",
              "NGon", "Graph") {
        }

          protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "C", "Polyline List", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "r", "Radius of each node", GH_ParamAccess.list);
            pManager.AddCircleParameter("Circles", "C", "Circles for the scale", GH_ParamAccess.list);


            pManager.AddIntegerParameter("U", "U", "division in one direction", GH_ParamAccess.item,4);
            pManager.AddIntegerParameter("V", "V", "division in second direction", GH_ParamAccess.item,4);
            pManager.AddNumberParameter("VDist", "VD", "division edge by length", GH_ParamAccess.item,0);

            for (int i = 1; i < pManager.ParamCount; i++) {
                pManager[i].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Anchors0", "P0", "Points that are at the end of each line", GH_ParamAccess.tree);
            pManager.AddPointParameter("Anchors1", "P1", "Points that are at the end of each line and on Arc", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Outlines", "C","Outlines of each quad",GH_ParamAccess.tree);
            pManager.AddCurveParameter("Nodes", "N", "Nodes of the graph to display radius", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA) {


            List<Curve> curves = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curve");

            List<Line> RhinoLines = new List<Line>();

            for(int i = 0; i < curves.Count; i++) {

                Polyline polyline = new Polyline();
                curves[i].TryGetPolyline(out polyline);
                RhinoLines.AddRange(polyline.GetSegments());
            }


            //List<Grasshopper.Kernel.Types.GH_Line> lines = new List<Grasshopper.Kernel.Types.GH_Line>();
            //if (!DA.GetDataList<Grasshopper.Kernel.Types.GH_Line>(0, lines)) return;

            //List<Line> RhinoLines = new List<Line>();
            //foreach (GH_Line l in lines) {
            //    RhinoLines.Add(l.Value);
            //}

            List<double> r = NGonsCore.Clipper.DataAccessHelper.FetchList<double>(DA, "Radius");
            double VDist = NGonsCore.Clipper.DataAccessHelper.Fetch<double>(DA, "VDist");
            int U = NGonsCore.Clipper.DataAccessHelper.Fetch<int>(DA, "U");
            int V = NGonsCore.Clipper.DataAccessHelper.Fetch<int>(DA, "V");
            List<Circle> circlesRadius = NGonsCore.Clipper.DataAccessHelper.FetchList<Circle>(DA, "Circles");




            DataTree<Point3d> anchors0 = new DataTree<Point3d>();
            DataTree<Point3d> anchors1 = new DataTree<Point3d>();
            DataTree<Polyline> outlines = new DataTree<Polyline>();
            Circle[] circles = new Circle[0];

           Mesh mesh =  NGonsCore.Graphs.LineGraph.CurveMesh(RhinoLines, r, VDist, U, V, ref anchors0, ref anchors1, ref outlines,ref circles,circlesRadius);

            DA.SetData(0, mesh);
            DA.SetDataTree(1, anchors0);
            DA.SetDataTree(2, anchors1);
            DA.SetDataTree(3, outlines);
            DA.SetDataList(4, circles);

      

        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.CurveMesh;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("7a8e9987-f8cb-41f9-979f-183cf393ecdc"); }
        }



    }
}