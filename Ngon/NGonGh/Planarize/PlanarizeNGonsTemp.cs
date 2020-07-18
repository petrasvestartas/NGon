﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace SubD.Planarize {

    public class PlanarizeNGonsTemp : GH_Component {
        public struct Prop {
            public Mesh P;
            public double tolerance;
            public bool[] byrefplan;
            public double[] byrefdev;
            public Point3d[] cen;
            public Plane[] pl;
        }


        public PlanarizeNGonsTemp() : base("Planarize", "Planarize", "Planarize ", "SubD", "Planarize") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            //pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "Planarity tolerance", 0, 0.0);
            pManager.AddIntegerParameter("Fix Naked", "N", "0:None 1:Fixed 2:Follow normal 3:Positive normal 4:Negative normal 5:Negotiate", 0, 0);
            pManager.AddIntegerParameter("Iterations", "I", "Iterations", 0, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            //pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddPlaneParameter("P", "P", "P", GH_ParamAccess.list);
            pManager.AddPointParameter("P", "P", "P", GH_ParamAccess.list);
            // pManager.AddIntegerParameter("Iterations", "I", "Number of exectuted iterations", 0);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            //List<Curve> curves = new List<Curve>();

            double tolerance = 0;
            int type = 0;
            int iterations = 0;
            Mesh mesh2 = new Mesh();
            if (DA.GetData(0, ref mesh2) && DA.GetData<double>(1, ref tolerance) && DA.GetData<int>(2, ref type) && DA.GetData<int>(3, ref iterations))
            {

                Mesh mesh = mesh2.DuplicateMesh();

                if (iterations < 0)
                    iterations = 0;


                //Solution
                bool[] planar = new bool[mesh.Ngons.Count];//mesh faces
                double deviation = 1.7976931348623157E+308;
                Plane[] NGonPlanes = this.GetPlanes(mesh, tolerance, ref deviation, ref planar);

                //Vertex - NGons
                //int[][] faceMap = mesh.FaceMap();
                HashSet<int> allNGonV = mesh.GetAllNGonsVertices();
                int[][] faceMap = mesh.GetNGonsConnectedToNGonVertices(allNGonV);

                bool[] nakedStatus = mesh.GetNakedNGonPointStatus(allNGonV);
                Vector3f[] vertexNormals = mesh.GetNgonNormals3f();
                Rhino.RhinoApp.ClearCommandHistoryWindow();
                List<Point3d> pts = new List<Point3d>();

                for (int i = 0; i < iterations; i++) {
                  
                    //Point3d[] array3 = null;
                    //Point3d[] array4 = null;

                    //if (type == 5) {
                    //    array3 = new Point3d[allNGonV.Count];
                    //    array4 = new Point3d[allNGonV.Count];
                    //    Array.Copy(allNGonV.ToArray(), array3, allNGonV.Count);
                    //    Array.Copy(allNGonV.ToArray(), array4, allNGonV.Count);
                    //}
                 
                    for (int j = 0; j < allNGonV.Count; j++) {
             
                        if (!(nakedStatus[j] && type == 1)) {
       

                            int num8 = 0;

                            for (int k = 0; k < faceMap[j].Length; k++) {
                                
                                if (planar[faceMap[j][k]])
                                    num8++;
                            }

                    
                            
                            if (num8 != faceMap[j].Length) {

                                Point3d[] avPoints = new Point3d[faceMap[j].Length];

                                //Loop through all ngon vertices
                                //and connected planes
                                for (int l = 0; l < faceMap[j].Length; l++)
                                {
                                    avPoints[l] = NGonPlanes[faceMap[j][l]].ClosestPoint(mesh.Vertices[allNGonV.ElementAt(j)]);
                                    //GrasshopperUtil.Debug(faceMap[j][l]);
                                }
                               // Rhino.RhinoApp.WriteLine();

                                Point3d point3d = NGonsCore.PointUtil.AveragePoint(avPoints);




                                //if (nakedStatus[j] & num2 > 1) {
                                //    switch (num2) {
                                //        case 2: {
                                //            Point3d point3d2 = mesh.Vertices[j];
                                //            Line line = new Line(point3d2, vertexNormals[j], 1.0);
                                //            mesh.Vertices[j] = (Point3f)line.ClosestPoint(point3d, false);
                                //            break;
                                //        }
                                //        case 3: {
                                //            Point3d point3d3 = mesh.Vertices[j];
                                //            Line line2 = new Line(point3d3, vertexNormals[j], 1.0);
                                //            if (line2.ClosestParameter(point3d) > 0.0)
                                //                mesh.Vertices[j] = (Point3f)line2.ClosestPoint(point3d, false);
                                //            break;
                                //        }
                                //        case 4: {
                                //            Point3d point3d4 = mesh.Vertices[j];
                                //            Line line3 = new Line(point3d4, vertexNormals[j], 1.0);
                                //            if (line3.ClosestParameter(point3d) < 0.0)
                                //                mesh.Vertices[j] = (Point3f)line3.ClosestPoint(point3d, false);
                                //            break;
                                //        }
                                //        case 5: {
                                //            Point3d point3d5 = mesh.Vertices[j];
                                //            Line line4 = new Line(point3d5, vertexNormals[j], 1.0);
                                //            if (line4.ClosestParameter(point3d) < 0.0)
                                //                array4[j] = line4.ClosestPoint(point3d, false);
                                //            else
                                //                array3[j] = line4.ClosestPoint(point3d, false);
                                //            break;
                                //        }
                                //    }

                                //} else {
                                    //if (type == 5) {
                                    //    array4[j] = point3d;
                                    //    array3[j] = point3d;
                                    //}
                                    mesh.Vertices[allNGonV.ElementAt(j)] = (Point3f)point3d;
                               // }
                            }//if
                        }
                    }

                    //if (type == 5) {
                    //    Mesh mesh2 = new Mesh();
                    //    double num11 = 0.0;
                    //    double num12 = 0.0;

                    //    for (int m = 0; m < array3.Length; m++) {
                    //        num11 += array4[m].DistanceTo(mesh.Vertices[m]);
                    //        num12 += array3[m].DistanceTo(mesh.Vertices[m]);
                    //    }

                    //    if (num11 < num12)
                    //        mesh2.Vertices.AddVertices(array4);
                    //    else
                    //        mesh2.Vertices.AddVertices(array3);


                    //    for (int n = 0; n < mesh.Faces.Count; n++)
                    //        if (mesh.Faces[n].IsQuad)
                    //            mesh2.Faces.AddFace(mesh.Faces[n].A, mesh.Faces[n].B, mesh.Faces[n].C, mesh.Faces[n].D);
                    //        else
                    //            mesh2.Faces.AddFace(mesh.Faces[n].A, mesh.Faces[n].B, mesh.Faces[n].C);



                    //    mesh = mesh2;
                    //}

                    Plane[] array5 = this.GetPlanes(mesh, tolerance, ref deviation, ref planar);
                    this.AssignOrigins(NGonPlanes, ref array5);
                    NGonPlanes = array5;
                    array5 = null;

                    if (deviation <= tolerance)
                        break;

                }
                DA.SetData(0, mesh);
                DA.SetDataList(1, NGonPlanes);

            }
        }

        private bool AssignOrigins(Plane[] Source, ref Plane[] Target) {
            for (int i = 0; i < Target.Length; i++)
                Target[i].Origin = (Source[i].Origin);

            return true;
        }

        private Plane[] GetPlanes(Mesh P, double tolerance, ref double deviation, ref bool[] planar) {
            Prop closure = new Prop();
            closure.P = P;
            closure.tolerance = tolerance;
            deviation = 0.0;
   
            closure.pl = new Plane[closure.P.Ngons.Count];
            closure.cen = closure.P.GetNGonCenters();
            closure.byrefplan = new bool[planar.Length];
            closure.byrefdev = new double[planar.Length];

            //Parallel.For(0, closure.P.Faces.Count, delegate (int i) {



            for (int i = 0; i < closure.P.Ngons.Count; i++) {

                Point3d[] array = closure.P.Ngons.NgonBoundaryVertexList(closure.P.Ngons[i], false);


                Plane plane = Plane.Unset;
                double num3 = 0.0;
                Plane.FitPlaneToPoints(array, out plane, out num3);

                if (num3 <= closure.tolerance)
                    closure.byrefplan[i] = true;
                else
                    closure.byrefplan[i] = false;

                closure.byrefdev[i] = num3;
                plane.Origin = (closure.cen[i]);
                closure.pl[i] = plane;

            }
            //closure.pl = closure.P.GetNgonPlanes();


            ////});


            planar = closure.byrefplan;

                for (int j = 0; j < closure.byrefdev.Length; j++)
                    deviation = Math.Max(deviation, closure.byrefdev[j]);

                return closure.pl;
            
        }

        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.planarize;

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf4092");
    }
}