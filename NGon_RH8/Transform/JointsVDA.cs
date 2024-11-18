using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonCore;
using NGonCore.Clipper;
using NGonCore.Geometry;
using Rhino.Geometry;


namespace NGon_RH8.Modifications
{
    public class JointsVDA : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AdjustMeshVertices class.
        /// </summary>
        public JointsVDA()
          : base("JointsVDA", "JointsVDA",
              "JointsVDA","NGon",
                "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh needed for topology", GH_ParamAccess.item);
            pManager.AddCurveParameter("Panels", "Panels", "Panel outlines as polylines", GH_ParamAccess.tree);
            pManager.AddVectorParameter("EdgeVectors", "EdgeVectors", "EdgeVectors are used for joint direction", GH_ParamAccess.tree);

            pManager.AddIntegerParameter("JointDiv", "JointDiv", "Number of joints on each edge", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("JointLen", "JointLen", "If number is negative the edge is scaled, if positive exact length is used", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("JointHei", "JointHei", "Joint height", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("JointThi", "JointThi", "Joint Thickiness must be the same as mesh offset W parameter", GH_ParamAccess.item,3);

            pManager.AddBooleanParameter("Center", "Center", "Orient joint to the centre", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Finger", "Finger", "Create finger joint", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Custom", "Custom", "Create half joint if two layers exists",GH_ParamAccess.item,-1);
            pManager.AddCurveParameter("PanelsChamfer", "PanelsChamfer", "Additional Polylines in case offset does not work properly", GH_ParamAccess.tree);

            for (int i = 2; i < pManager.ParamCount; i++) {
                pManager[i].Optional = true;
            }


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddCurveParameter("Panels", "Panels", "Panels Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Joints", "Joints", "Joints Polylines", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = DA.Fetch<Mesh>("Mesh");
            GH_Structure<GH_Curve> Curves =  DA.FetchTree<GH_Curve>("Panels");
            DataTree<Polyline> C = new DataTree<Polyline>();
            GH_Structure<GH_Vector> edgeVectors = DA.FetchTree<GH_Vector>("EdgeVectors");
            DataTree<Vector3d> EVec = new DataTree<Vector3d>();
            int D = DA.Fetch<int>("JointDiv");
            double L = DA.Fetch<double>("JointLen");
            double H = DA.Fetch<double>("JointHei");
            double W = DA.Fetch<double>("JointThi");
            bool Center = DA.Fetch<bool>("Center");
            bool Finger = DA.Fetch<bool>("Finger");
            double Custom = DA.Fetch<double>("Custom");
            GH_Structure<GH_Curve> CurvesChamfer = DA.FetchTree<GH_Curve>("PanelsChamfer");
            DataTree<Polyline> CChamfer = new DataTree<Polyline>();


            for (int i = 0; i < Curves.Branches.Count; i++) {
                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {
                    Polyline polyline;
                    Curves.get_DataItem(Curves.Paths[i], j).Value.TryGetPolyline(out polyline);
                    C.Add(polyline, Curves.Paths[i]);
                }
            }

            if (CurvesChamfer.DataCount == Curves.DataCount) {
                for (int i = 0; i < Curves.Branches.Count; i++) {
                    for (int j = 0; j < Curves.get_Branch(i).Count; j++) {
                        Polyline polyline;
                        CurvesChamfer.get_DataItem(Curves.Paths[i], j).Value.TryGetPolyline(out polyline);
                        CChamfer.Add(polyline, Curves.Paths[i]);
                    }
                }
            }

            for (int i = 0; i < edgeVectors.Branches.Count; i++) {
                for (int j = 0; j < edgeVectors.get_Branch(i).Count; j++) {
                    EVec.Add(edgeVectors.get_DataItem(edgeVectors.Paths[i], j).Value, edgeVectors.Paths[i]);
                }
            }

            //Solution

            DataTree<Polyline> diagonalConnections = new DataTree<Polyline>();




            int divisions = Math.Max(1, D);
            double length = L;//Math.Max(0.1, L);
            double height = Math.Max(0.1, H);
            double width = Math.Max(0.1, W);


            int[][] tv = M.GetNGonsTopoBoundaries();
            int[][] fe = M.GetNGonFacesEdges(tv);
            HashSet<int> e = M.GetAllNGonEdges(tv);
            Dictionary<int, int[]> efDict = M.GetFE(e, false);
            Point3d[] centers = M.GetNGonCenters();


            DataTree<Polyline> Ccut = new DataTree<Polyline>();
            for (int i = 0; i < C.BranchCount; i++) {
                for (int j = 0; j < C.Branch(i).Count; j++) {
                    if (CurvesChamfer.DataCount == Curves.DataCount) {
                        Ccut.Add(new Polyline(CChamfer.Branch(i)[j]), new GH_Path(CChamfer.Path(i)));
                    } else {
                        Ccut.Add(new Polyline(C.Branch(i)[j]), new GH_Path(C.Path(i)));
                        //Rhino.RhinoApp.WriteLine(CChamfer.DataCount.ToString() + " " + Curves.DataCount.ToString());
                    }
                }
            }



            DataTree<Vector3d> EV = new DataTree<Vector3d>();
            if (EVec.DataCount == 0) {
                foreach (int ee in e) {
                    if (M.TopologyEdges.GetConnectedFaces(ee).Length != 1) {
                        if (Center) {
                            int[] facePair = efDict[ee];
                            Vector3d insertionVec = centers[facePair[0]] - centers[facePair[1]];
                            insertionVec.Unitize();
                            EV.Add(insertionVec, new GH_Path(ee));
                            //Line li = new Line(centers[facePair[0]] , centers[facePair[1]]);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(li);
                        } else {
                            EV.Add(NGonCore.VectorUtil.BisectorVector(M.TopologyEdges.EdgeLine(ee), M.GetNGonNormal(efDict[ee][0])), new GH_Path(ee));
                        }
                    }
                }
            } else {
                EV = EVec;
            }



            DataTree<Polyline> recJoints = new DataTree<Polyline>();
            for (int i = 0; i < EV.BranchCount; i++) {

                GH_Path p = EV.Path(i);
                int ecur = p.Indices[0];
                int f0 = efDict[ecur][0];
                int f1 = efDict[ecur][1];

                //Divide line into points
                Line line = M.TopologyEdges.EdgeLine(ecur);
                Point3d[] pts = NGonCore.PointUtil.InterpolatePoints(line.From, line.To, divisions, false);
                Point3d[] pts0 = new Point3d[pts.Length];
                Point3d[] pts1 = new Point3d[pts.Length];

                //Get average normal between faces
                Vector3d edgeNormal = M.GetNGonNormal(f0) + M.GetNGonNormal(f1);
                edgeNormal.Unitize();
                Vector3d cross = Vector3d.CrossProduct(edgeNormal, EV.Branch(p)[0]);
                cross.Unitize();

                //Get line planes
                Plane[] edgePlanes = new Plane[pts.Length];
                Plane edgePlane90 = new Plane(line.PointAt(0.5), cross, edgeNormal);

                for (int j = 0; j < pts.Length; j++) {
                    Vector3d v = EV.Branch(p)[0];
                    v.Unitize();
                    edgePlanes[j] = new Plane(pts[j], v, edgeNormal);
                }




                Point3d[][] edgePoints = new Point3d[edgePlanes.Length][];







                for (int j = 0; j < edgePlanes.Length; j++) {//C.Branch(f0).Count


                    Polyline[] rectangleWithoutJoints = new Polyline[] { new Polyline(), new Polyline() };
                    List<Polyline>[] rectangleJoints = new List<Polyline>[] { new List<Polyline>(), new List<Polyline>() };

                    foreach (int fn in efDict[ecur]) {
                        int e0 = Array.IndexOf(fe[fn], ecur);

                        edgePoints[j] = new Point3d[C.Branch(fn).Count];
                        Polyline[] perpJoint = new Polyline[C.Branch(fn).Count];

                        Polyline recJoint0 = new Polyline();
                        Polyline recJoint1 = new Polyline();

                        for (int k = 0; k < C.Branch(fn).Count; k++) {

                            if (k % 2 == 0 && k != 0) {
                                //rectangleJoints[0].Add(recJoint0);
                                //rectangleJoints[1].Add(recJoint1);
                                recJoint0.Clear();
                                recJoint1.Clear();
                            }


                            Line segment = C.Branch(fn)[k].SegmentAt(e0);
                            Plane planeOff0 = edgePlanes[j].MovePlanebyAxis(W * 0.5);
                            Plane planeOff1 = edgePlanes[j].MovePlanebyAxis(-W * 0.5);

                            Plane planeFace = C.Branch(fn)[k].plane();
                            Plane plane3 = new Plane(planeFace.Origin, edgePlanes[j].Normal, planeFace.Normal);


                            //Face0 edge points
                            Point3d edgePoint0 = PlaneUtil.LinePlane(segment, planeOff0);
                            Point3d edgePoint1 = PlaneUtil.LinePlane(segment, planeOff1);
                            //Inner Points
                            Vector3d v0 = PlaneUtil.PlanePlaneVec(planeFace, planeOff0);
                            Vector3d v1 = PlaneUtil.PlanePlaneVec(planeFace, planeOff1);

                            bool moveFlag = (segment.PointAt(0.5) + v0).DistanceTo(planeFace.Origin) < (segment.PointAt(0.5) - v0).DistanceTo(planeFace.Origin);
                            bool flagMod = j % 2 == 0;
                            bool flagFace = efDict[ecur][0] == fn;
                            flagMod = false;

                            //double scalarLength = (centers[fn] - edgePlanes[j].Origin).Length*(double)Math.Abs(length);
                            if (L < 0) {
                                length = (centers[fn] - edgePlanes[j].Origin).Length * (double)Math.Abs(L);
                            }
                            //length = (centers[fn] - edgePlanes[j].Origin).Length*1;

                            Point3d innerPoint0 = (moveFlag) ? edgePoint0 + (v0 * length) : edgePoint0 - (v0 * length);
                            Point3d innerPoint1 = (moveFlag) ? edgePoint1 + (v1 * length) : edgePoint1 - (v1 * length);

                            if (Finger) {

                                innerPoint0 = (!moveFlag && !Finger) ? edgePoint0 + (v0 * length) : edgePoint0 - (v0 * length);
                                innerPoint1 = (!moveFlag && !Finger) ? edgePoint1 + (v1 * length) : edgePoint1 - (v1 * length);
                            }

                            //Point3d innerPoint0 = edgePoint0 +(v0 * L);
                            //Point3d innerPoint1 = edgePoint1 +(v1 * L) ;

                            Polyline polylinep0 = new Polyline(new Point3d[] { edgePoint0, innerPoint0, innerPoint1, edgePoint1 });

                            // Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(polylinep0);



                            Line guideLine0 = new Line(edgePoint0, innerPoint0);
                            Line guideLine1 = new Line(edgePoint1, innerPoint1);
                            Point3d mid = (innerPoint1 + innerPoint0) * 0.5;
                            innerPoint0 = guideLine0.ClosestPoint(mid, false);
                            innerPoint1 = guideLine1.ClosestPoint(mid, false);

                            Point3d center = (edgePoint0 + innerPoint0 + innerPoint1 + edgePoint1) * 0.25;



                            Point3d center0 = guideLine0.ClosestPoint(center, false);
                            Point3d center1 = guideLine1.ClosestPoint(center, false);

                            perpJoint[k] = new Polyline(new Point3d[] { innerPoint0, center0, center1, innerPoint1 });
                            Polyline polyline0 = new Polyline(new Point3d[] { edgePoint0, center0, center1, edgePoint1 });

                            //finger joints or standard
                            if (Finger) {
                                if (efDict[ecur][0] == fn) {
                                    Ccut.Branch(efDict[ecur][0])[k].InsertPolyline(polyline0);//offset needed
                                } else {
                                    Ccut.Branch(efDict[ecur][1])[k].InsertPolyline(polyline0);//offset needed
                                }
                            } else {
                                Ccut.Branch(fn)[k].InsertPolyline(polyline0);//offset needed
                            }


                            if (fn == efDict[ecur][0]) {
                                if (k == 1 || k == NGonCore.MathUtil.Wrap(C.Branch(fn).Count - 2, C.Branch(fn).Count)) {
                                    rectangleWithoutJoints[0].Add(innerPoint0);
                                    rectangleWithoutJoints[1].Add(innerPoint1);
                                }
                            } else {
                                if (k == 1) {
                                    rectangleWithoutJoints[0].Add(innerPoint0);
                                    rectangleWithoutJoints[1].Add(innerPoint1);
                                }

                                if (k == NGonCore.MathUtil.Wrap(C.Branch(fn).Count - 2, C.Branch(fn).Count)) {
                                    rectangleWithoutJoints[0].Insert(2, innerPoint0);
                                    rectangleWithoutJoints[1].Insert(2, innerPoint1);
                                }
                            }


                            // Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(innerPoint0);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(center0);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(innerPoint1);
                            // Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(recJoint0);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(recJoint1);

                            if (k % 2 == 0) {
                                recJoint0.Add(innerPoint0);
                                recJoint0.Add(center0);
                                recJoint1.Add(innerPoint1);
                                recJoint1.Add(center1);
                            } else {
                                recJoint0.Add(center0);
                                recJoint0.Add(innerPoint0);
                                recJoint1.Add(center1);
                                recJoint1.Add(innerPoint1);
                            }

                            if (k % 2 == 1) {
                                rectangleJoints[0].Add(new Polyline(recJoint0));
                                rectangleJoints[1].Add(new Polyline(recJoint1));
                            }
                            if (k % 2 == 1) {
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(recJoint0);
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(recJoint1);
                            }
                            //edgePoints[j][k] = edgePoint0;
                            //            Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(edgePoint0);
                            //            Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(edgePoint1);
                            //            Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(innerPoint0);
                            //            Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(innerPoint1);
                            //            Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(polyline0);
                        }



                    }
                    for (int m = 0; m < rectangleWithoutJoints.Length; m++) {
                        Line l0 = CurveUtil.ExtendLine(rectangleWithoutJoints[m].SegmentAt(0), height);
                        Line l1 = CurveUtil.ExtendLine(rectangleWithoutJoints[m].SegmentAt(2), height);
                        if (l0.From.DistanceTo(l1.From) < l0.From.DistanceTo(l1.To))
                            l1.Flip();
                        rectangleWithoutJoints[m] = new Polyline(new Point3d[] { l0.From, l0.To, l1.From, l1.To, l0.From });
                        // Line line
                    }

                    for (int m = 0; m < rectangleJoints[0].Count; m++) {
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(rectangleJoints[0][j]);
                        rectangleWithoutJoints[0].InsertPolyline(rectangleJoints[0][m]);
                        rectangleWithoutJoints[1].InsertPolyline(rectangleJoints[1][m]);

                    }

                    //try make special joint
                    if (Custom >= 0 && C.Branch(0).Count == 4) {
                        Polyline rebuild0 = new Polyline();
                        Polyline rebuild1 = new Polyline();
                        for (int m = 2; m < 18; m++) {
                            if (m == 2) {
                                Vector3d v0 = rectangleWithoutJoints[0][m] - rectangleWithoutJoints[0][m + 1];
                                v0.Unitize();
                                rebuild0.Add(rectangleWithoutJoints[0][m] + v0 * Custom);
                                rebuild1.Add(rectangleWithoutJoints[1][m] + v0 * Custom);
                            } else if (m == 17) {
                                Vector3d v0 = rectangleWithoutJoints[0][17] - rectangleWithoutJoints[0][16];
                                v0.Unitize();
                                rebuild0.Add(rectangleWithoutJoints[0][17] + v0 * Custom);
                                rebuild1.Add(rectangleWithoutJoints[1][17] + v0 * Custom);
                            } else {
                                rebuild0.Add(rectangleWithoutJoints[0][m]);
                                rebuild1.Add(rectangleWithoutJoints[1][m]);
                            }
                        }
                        rebuild0.Close();
                        rebuild1.Close();
                        rectangleWithoutJoints[0] = rebuild0;
                        rectangleWithoutJoints[1] = rebuild1;

                    }

                    recJoints.AddRange(rectangleWithoutJoints, new GH_Path(ecur));

                }




                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(rectangleWithoutJoints[0]);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(rectangleWithoutJoints[1]);
                //A = edgePlanes;


            }

            DA.SetDataTree(0, Ccut);
            DA.SetDataTree(1, recJoints);

            //DC = diagonalConnections;
            //Panels = Ccut;
            //Joints = recJoints;

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.vdajoints;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a8606789-1e62-44ac-803c-8cd799d9d11b"); }
        }
    }
}