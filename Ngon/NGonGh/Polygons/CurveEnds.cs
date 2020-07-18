using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGonGh.SubD {
    public class CurveEnds : GH_Component {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public CurveEnds()
          : base("CurveEnds", "CurveEnds",
              "Fix Curve Ends",
              "NGon", "Polygon") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points to fix points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "D", "Distance where points will affect the curve", GH_ParamAccess.item,0.01);
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            List<Curve> C_ = NGonsCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curves");
           
            double T = NGonsCore.Clipper.DataAccessHelper.Fetch<double>(DA, "Tolerance");
            List<Point3d> P = NGonsCore.Clipper.DataAccessHelper.FetchList<Point3d>(DA, "Points");

            List<Curve> curves = new List<Curve>();

            if (P.Count > 0) {
                for (int i = 0; i < C_.Count; i++) {
                    Curve C = C_[i].DuplicateCurve();
                    PointCloud cloud = new PointCloud(P);

                    double tol = (T == 0.0) ? C.GetLength() * 0.5 : T;

                    Point3d p0 = cloud.PointAt(cloud.ClosestPoint(C.PointAtStart));
                    Point3d p1 = cloud.PointAt(cloud.ClosestPoint(C.PointAtEnd));

                    if (p0.DistanceTo(C.PointAtStart) < tol) {
                        C.SetStartPoint(p0);
                    }

                    if (p1.DistanceTo(C.PointAtEnd) < tol) {
                        C.SetEndPoint(p1);
                    }
                    curves.Add(C);
                }
            } else {

                
                PointCloud cloud = new PointCloud();
                double tol = (T == 0.0) ? 0.01 : T;
                base.Message = tol.ToString();

                //Add start and end point to pointcloud with incrementing nornal if point already exists
                for (int i = 0; i < C_.Count; i++) {

                    if (i == 0) {
                        cloud.Add(C_[i].PointAtStart, new Vector3d(0, 0, 1));
                        cloud.Add(C_[i].PointAtEnd, new Vector3d(0, 0, 1));
                    } else {
                        int cp0 = cloud.ClosestPoint(C_[i].PointAtStart);
                        bool flag0 = cloud.PointAt(cp0).DistanceToSquared(C_[i].PointAtStart) < tol;
                        if (flag0) {
                            cloud[cp0].Normal = new Vector3d(0, 0, cloud[cp0].Normal.Z + 1);
                        } else {
                            cloud.Add(C_[i].PointAtStart, new Vector3d(0, 0, 1));
                        }

                        int cp1 = cloud.ClosestPoint(C_[i].PointAtEnd);
                        bool flag1 = cloud.PointAt(cp1).DistanceToSquared(C_[i].PointAtEnd) < tol;
                        if (flag1) {
                            cloud[cp1].Normal = new Vector3d(0, 0, cloud[cp1].Normal.Z + 1);
                        } else {
                            cloud.Add(C_[i].PointAtEnd, new Vector3d(0, 0, 1));
                        }
                    }

                }//for

                //List<Curve> curves = new List<Curve>();
                for (int i = 0; i < C_.Count; i++) {
                    //Print(cloud[cloud.ClosestPoint(C[i].PointAtStart)].Normal.Z.ToString() + " " + cloud[cloud.ClosestPoint(C[i].PointAtEnd)].Normal.Z.ToString());
                    int a = cloud.ClosestPoint(C_[i].PointAtStart);
                    int b = cloud.ClosestPoint(C_[i].PointAtEnd);
                    bool flag0 = cloud[a].Normal.Z != 1;
                    bool flag1 = cloud[b].Normal.Z != 1;

                    Curve c = C_[i].DuplicateCurve();
                    if (flag0) {
                        c.SetStartPoint(cloud[a].Location);
                    }
                    if (flag1) {
                        c.SetEndPoint(cloud[b].Location);
                    }
                    curves.Add(c);

                }


            }

            
            DA.SetDataList(0, curves);


         
        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.ChangeEndPoints;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-1458a5ee1f08"); }
        }
    }
}