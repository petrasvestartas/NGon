using System;
using System.Collections.Generic;
using NGonCore.ConvexHull;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGon_RH8.Graphs {
    public class ConvexHull : GH_Component {


        public ConvexHull()
          : base("ConvexHull", "ConvexHull",
              "ConvexHull from points",
              "NGon", "Graph") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polyline", "C", "Polyline", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Index", "I", "ID of points", GH_ParamAccess.list);
        }



        protected override void SolveInstance(IGH_DataAccess DA) {

            //Inputs
            List<Point3d> points = new List<Point3d>();
            DA.GetDataList(0, points);

            //Solution - Create hull


            //Fit planes to point to check if planes are co-planar
            Plane.FitPlaneToPoints(points, out Plane plane, out double t);
            plane.Origin=NGonCore.PointUtil.AveragePoint(points);
            Rhino.Geometry.Transform planeplane = Rhino.Geometry.Transform.PlaneToPlane(plane, Plane.WorldXY);
            foreach (Point3d pt in points)
                pt.Transform(planeplane);
  
            
            //Co-planar the convex hull will be an outline else 3d mesh

            if(t < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) {

                ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> hull = NGonCore.ConvexHull.ConvexHull.Create(points,true);

                Polyline polyline = new Polyline();
                foreach (NGonCore.ConvexHull.DefaultVertex pt in hull.Points) {
                    double[] pos = pt.Position;
                    polyline.Add(new Point3d(pos[0], pos[1], 0));
                }
                

                polyline.Add(polyline[0]);
                DA.SetData(1, polyline);

            } else {

                ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> hull = NGonCore.ConvexHull.ConvexHull.Create(points,false);

                Mesh mesh = new Mesh();
                var convexHullVertices = hull.Points.ToArray();
                foreach (NGonCore.ConvexHull.DefaultVertex pt in hull.Points) {
                    double[] pos = pt.Position;
                    mesh.Vertices.Add(new Point3d(pos[0], pos[1], pos[2]));
                }

                foreach (DefaultConvexFace<DefaultVertex> f in hull.Faces) {
                    int a = Array.IndexOf(convexHullVertices, f.Vertices[0]);
                    int b = Array.IndexOf(convexHullVertices, f.Vertices[1]);
                    int c = Array.IndexOf(convexHullVertices, f.Vertices[2]);
                    mesh.Faces.AddFace(a, b, c);
                }

                //Ouputs
                DA.SetData(0, mesh);


            }

                 
      





        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.hull;

        public override Guid ComponentGuid => new Guid("55229d4a-24b8-4b9b-a629-0e5f1b7faf8f");
    }
}