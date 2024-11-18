using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonCore;
using NGonCore.Clipper;
using NGonCore.Geometry;
using NGonCore.Text;
using Rhino.Geometry;


namespace SubD.Modifications {

    public static class Helpers {


        public static DataTree<Polyline> ToPolylineDT(this GH_Structure<GH_Curve> Curves) {

            DataTree<Polyline> C = new DataTree<Polyline>();

            for (int i = 0; i < Curves.Branches.Count; i++) {
                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {
                    Polyline polyline;
                    Curves.get_DataItem(Curves.Paths[i], j).Value.TryGetPolyline(out polyline);
                    C.Add(polyline, Curves.Paths[i]);
                }
            }

            return C;

        }

        public static DataTree<Vector3d> ToVectorDT(this GH_Structure<GH_Vector> Curves) {

            DataTree<Vector3d> C = new DataTree<Vector3d>();

            for (int i = 0; i < Curves.Branches.Count; i++) {
                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {

                    C.Add(Curves.get_DataItem(Curves.Paths[i], j).Value, Curves.Paths[i]);
                }
            }

            return C;

        }

        public static DataTree<int> ToIntDT(this GH_Structure<GH_Integer> Curves) {

            DataTree<int> C = new DataTree<int>();

            for (int i = 0; i < Curves.Branches.Count; i++) {
                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {

                    C.Add(Curves.get_DataItem(Curves.Paths[i], j).Value, Curves.Paths[i]);
                }
            }

            return C;

        }

        public static DataTree<Panel> ToPanelsDT(this GH_Structure<GH_Curve> Curves) {

            DataTree<Panel> panelGroups = new DataTree<Panel>();

            for (int i = 0; i < Curves.Branches.Count; i++) {

                List<Polyline> outlines = new List<Polyline>();

                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {
                    Polyline polyline;
                    Curves.get_DataItem(Curves.Paths[i], j).Value.TryGetPolyline(out polyline);
                    outlines.Add(polyline);

                    if (outlines.Count==2) {
                        panelGroups.Add(new Panel(outlines[0], outlines[1]), Curves.Paths[i]);
                        outlines.Clear();
                    }


                }

            }


            return panelGroups;
        }

        public static DataTree<Vector3d> ToDT(this GH_Structure<GH_Vector> edgeVectors) {
            DataTree<Vector3d> EVec = new DataTree<Vector3d>();
            for (int i = 0; i < edgeVectors.Branches.Count; i++) {
                for (int j = 0; j < edgeVectors.get_Branch(i).Count; j++) {
                    EVec.Add(edgeVectors.get_DataItem(edgeVectors.Paths[i], j).Value, edgeVectors.Paths[i]);
                }

            }
            return EVec;
        }

    }


    public class Panel {

        public Polyline[] contourNoJoints = new Polyline[2];
        public Polyline[] contour = new Polyline[2];

        public Plane plane = Plane.Unset;
        public Plane planeOffset0 = Plane.Unset;
        public Plane planeOffset1 = Plane.Unset;

        public Plane planeRot = Plane.Unset;
        public Plane planeRotOffset0 = Plane.Unset;
        public Plane planeRotOffset1 = Plane.Unset;
        public Plane planeEdge = Plane.Unset;

        public Plane planeF0 = Plane.Unset;
        public Plane planeF1 = Plane.Unset;

        Mesh mesh = new Mesh();


        public List<Plane> edgePlanes = new List<Plane>();
        public string id;
        public List<Curve> text = new List<Curve>();


        public List<Polyline> cuts = new List<Polyline>();



        public Panel(Plane plane) {
            this.plane = plane;
            this.contourNoJoints = new Polyline[] { new Polyline(), new Polyline() };
            this.contour = new Polyline[] { new Polyline(), new Polyline() };
        }

        public Panel(Polyline p0, Polyline p1) {
            this.contourNoJoints = new Polyline[] { new Polyline(p0), new Polyline(p1) };
            this.contour = new Polyline[] { new Polyline(p0), new Polyline(p1) };

            mesh = Mesh.CreateFromClosedPolyline(PolylineUtil.tweenPolylines(new Polyline(p0), new Polyline(p1)));

            this.planeOffset0 = p0.plane();
            this.planeOffset1 = p1.plane();
            this.plane = new Plane((planeOffset0.Origin + planeOffset1.Origin) * 0.5, planeOffset0.XAxis, planeOffset0.YAxis);
        }

        public Polyline MidContour() {
            return PolylineUtil.tweenPolylines(contour[0], contour[1]);
        }

        public void ChangeJoint(double extend = 10, int type = 0, double cutExtend = 5, double addExtend = 0) {



            switch (type) {

                case (1):

                if (contour[0].Count == 25) {

                    for (int i = 0; i < contour.Length; i++) {

                        Polyline newContour = new Polyline();

                        for (int j = 4; j < 24; j++) {





                            if (j == 4) {
                                Vector3d v0 = contour[i][j] - contour[i][j + 1];
                                v0.Unitize();
                                v0 *= cutExtend;
                                newContour.Add(contour[i][j + 1] + v0);

                                v0 = contour[i][j + 1] - contour[i][j + 2];
                                v0.Unitize();
                                v0 *= -extend;
                                newContour.Insert(0, newContour[0] + v0);

                            } else if (j == 23) {
                                Vector3d v0 = contour[i][j] - contour[i][j - 1];
                                v0.Unitize();
                                v0 *= cutExtend;
                                newContour.Add(contour[i][j - 1] + v0);

                                v0 = contour[i][j - 2] - contour[i][j - 1];
                                v0.Unitize();
                                v0 *= -extend;
                                newContour.Add(newContour.Last - v0);
                            } else {
                                newContour.Add(contour[i][j]);
                            }
                        }


                        newContour.Close();
                        contour[i] = newContour;



                        for (int j = 0; j < contour[i].Count; j++) {

                            if (addExtend > 0 && ((j == 16) || (j == 12) || (j == 4) || (j == 8))) {
                                Vector3d v0 = (contour[i][j] - contour[i][j - 1]).UnitVector() * addExtend;
                                contour[i][j] += v0;
                            } else if (addExtend > 0 && ((j == 17) || (j == 13) || (j == 9) || (j == 5))) {
                                Vector3d v0 = (contour[i][j] - contour[i][j + 1]).UnitVector() * addExtend;
                                contour[i][j] += v0;
                            }
                        }

                    }
                }

                if (contour[0].Count == 29) {


                    for (int i = 0; i < contour.Length; i++) {

                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(contour[i]);
                        Polyline newContour = new Polyline();

                        for (int j = 4; j < 28; j++) {





                            if (j == 4) {
                                Vector3d v0 = contour[i][j] - contour[i][j + 1];
                                v0.Unitize();
                                v0 *= cutExtend;
                                newContour.Add(contour[i][j + 1] + v0);

                                v0 = contour[i][j + 1] - contour[i][j + 2];
                                v0.Unitize();
                                v0 *= -extend;
                                newContour.Insert(0, newContour[0] + v0);

                            } else if (j == 27) {
                                Vector3d v0 = contour[i][j] - contour[i][j - 1];
                                v0.Unitize();
                                v0 *= cutExtend;
                                newContour.Add(contour[i][j - 1] + v0);

                                v0 = contour[i][j - 2] - contour[i][j - 1];
                                v0.Unitize();
                                v0 *= -extend;
                                newContour.Add(newContour.Last - v0);
                            } else {
                                newContour.Add(contour[i][j]);
                            }
                        }


                        newContour.Close();
                        contour[i] = newContour;



                        for (int j = 0; j < contour[i].Count; j++) {

                            if (addExtend > 0 && ((j == 16) || (j == 12) || (j == 4) || (j == 8) || (j == 20))) {
                                Vector3d v0 = (contour[i][j] - contour[i][j - 1]).UnitVector() * addExtend;
                                contour[i][j] += v0;
                            } else if (addExtend > 0 && ((j == 17) || (j == 13) || (j == 9) || (j == 5) || (j == 21))) {
                                Vector3d v0 = (contour[i][j] - contour[i][j + 1]).UnitVector() * addExtend;
                                contour[i][j] += v0;
                            }
                        }
                    }



                }

                break;



                default:
                if (contour[0].Count == 25) {

                    for (int i = 0; i < contour.Length; i++) {

                        Polyline newContour = new Polyline();

                        for (int j = 5; j < 23; j++) {
                            newContour.Add(contour[i][j]);
                        }



                        //extend top part
                        newContour[0] += (newContour[0] - newContour[1]).UnitVector() * extend;
                        newContour[newContour.Count - 1] += (newContour[newContour.Count - 1] - newContour[newContour.Count - 2]).UnitVector() * extend;
                        newContour.Close();

                        contour[i] = newContour;
                    }
                }
                break;


            }



        }

        public void CreateCut(int segmentID, Plane jointPlane0, Plane jointPlane1, double length_, ref Panel jointPanel, bool bake = false) {//, ref Panel neiPanel,) {

           //jointPlane0.Bake(40);

            //jointPlane1.Bake(40);

            double e = 0;


            Polyline cut = new Polyline();

            Line segment0 = contourNoJoints[0].SegmentAt(segmentID);
            Line segment1 = contourNoJoints[1].SegmentAt(segmentID);

            double dist = this.plane.Origin.DistanceTo(segment0.PointAt(0.5));
            double length = length_;// * dist * 0.01;


            //Rhino.RhinoApp.WriteLine(dist.ToString());

            //Intersect plate edge line and joint plane offsets
            Point3d edgePoint00 = PlaneUtil.LinePlane(segment0, jointPlane0);
            Point3d edgePoint01 = PlaneUtil.LinePlane(segment0, jointPlane1);
            Point3d edgePoint10 = PlaneUtil.LinePlane(segment1, jointPlane0);
            Point3d edgePoint11 = PlaneUtil.LinePlane(segment1, jointPlane1);

            //Get direction of a cut
            Vector3d v00 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane0).Unit();
            Vector3d v01 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane1).Unit();
            Vector3d v10 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane0).Unit();
            Vector3d v11 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane1).Unit();


            //Really fucked up way of checking which direction plane must be offseted for correct cut direction
            Point3d p0 = mesh.ClosestPoint((segment0.PointAt(0.5) + v00 * 10));
            Point3d p1 = mesh.ClosestPoint((segment0.PointAt(0.5) - v00 * 10));
            //bool moveFlag = (segment0.PointAt(0.5) + v00*10).DistanceTo(planeOffset0.Origin) < (segment0.PointAt(0.5) - v00*10).DistanceTo(planeOffset0.Origin);
            bool moveFlag = p0.DistanceToSquared((segment0.PointAt(0.5) + v00 * 10)) < p1.DistanceToSquared((segment0.PointAt(0.5) - v00 * 10));


            //if (bake) {

              
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(planeOffset0.Origin);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(segment0.PointAt(0.5) + v00*100);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(segment0.PointAt(0.5) - v00*100);

            //}

            //Moved Points
            Point3d innerPoint00 = (moveFlag) ? edgePoint00 + (v00 * length) : edgePoint00 - (v00 * length);
            Point3d innerPoint01 = (moveFlag) ? edgePoint01 + (v01 * length) : edgePoint01 - (v01 * length);
            Point3d innerPoint10 = (moveFlag) ? edgePoint10 + (v10 * length) : edgePoint10 - (v10 * length);
            Point3d innerPoint11 = (moveFlag) ? edgePoint11 + (v11 * length) : edgePoint11 - (v11 * length);
            Point3d innerPointCenter = (innerPoint00 + innerPoint01 + innerPoint10 + innerPoint11) * 0.25;
            //Plane perpPlane = new Plane(innerPointCenter, jointPlane0.Normal, Vector3d.CrossProduct(planeOffset0.Normal, v00));
            Plane perpPlane = new Plane(innerPointCenter, jointPlane0.Normal, planeOffset0.Normal);
            //perpPlane = perpPlane.Switch("XZ");

            //if (planeOffset0.IsValid)
            //    planeOffset0.Bake(40);

            //if (jointPlane0.IsValid)
            //    jointPlane0.Bake(40);

            //if (perpPlane.IsValid)
            //    perpPlane.Bake(40);

            innerPoint00 = perpPlane.RayPlane(edgePoint00, v00);
            innerPoint01 = perpPlane.RayPlane(edgePoint01, v01);
            innerPoint10 = perpPlane.RayPlane(edgePoint10, v10);
            innerPoint11 = perpPlane.RayPlane(edgePoint11, v11);




            //Middle points and projection to plane
            Point3d innerPointMid00 = (moveFlag) ? edgePoint00 + (v00 * length * 0.5) : edgePoint00 - (v00 * length * 0.5);
            Point3d innerPointMid01 = (moveFlag) ? edgePoint01 + (v01 * length * 0.5) : edgePoint01 - (v01 * length * 0.5);
            Point3d innerPointMid10 = (moveFlag) ? edgePoint10 + (v10 * length * 0.5) : edgePoint10 - (v10 * length * 0.5);
            Point3d innerPointMid11 = (moveFlag) ? edgePoint11 + (v11 * length * 0.5) : edgePoint11 - (v11 * length * 0.5);
            Point3d innerPointMid = (innerPointMid00 + innerPointMid01 + innerPointMid10 + innerPointMid11) * 0.25;
            Plane perpPlaneMid = new Plane(innerPointMid, jointPlane0.Normal, perpPlane.YAxis);


            //if (planeOffset0.IsValid)
            //    planeOffset0.Bake(40);


            //if(perpPlaneMid.IsValid)
            //perpPlaneMid.Bake(40);

            innerPointMid00 = perpPlaneMid.RayPlane(edgePoint00, v00);
            innerPointMid01 = perpPlaneMid.RayPlane(edgePoint01, v01);
            innerPointMid10 = perpPlaneMid.RayPlane(edgePoint10, v10);
            innerPointMid11 = perpPlaneMid.RayPlane(edgePoint11, v11);



            //It is not closest point because the connection is not perpendicular to two adjacent plates
            //It might be close to perpendicular but not possible

            //edgePoint00.Bake();
            //innerPointMid00.Bake();
            //innerPointMid01.Bake();
            //edgePoint01.Bake();

            //edgePoint10.Bake();
            //innerPointMid10.Bake();
            //innerPointMid11.Bake();
            //edgePoint11.Bake();

            Polyline cut0 = new Polyline(new Point3d[] { edgePoint00, innerPointMid00 + v00 * e, innerPointMid01 + v01 * e, edgePoint01 });//perpPlane.ClosestPoint
            Polyline cut1 = new Polyline(new Point3d[] { edgePoint10, innerPointMid10 + v10 * e, innerPointMid11 + v11 * e, edgePoint11 });
            this.cuts.Add(cut0);
            this.cuts.Add(cut1);




            Polyline cutNei0 = new Polyline(new Point3d[] { innerPoint00, innerPointMid00 + v00 * -e, innerPointMid10 + v01 * -e, innerPoint10 });//perpPlane.ClosestPoint
            Polyline cutNei1 = new Polyline(new Point3d[] { innerPoint01, innerPointMid01 + v10 * -e, innerPointMid11 + v11 * -e, innerPoint11 });
            jointPanel.cuts.Add(cutNei0);
            jointPanel.cuts.Add(cutNei1);

     
            contour[0].InsertPolyline(cut0);
            contour[1].InsertPolyline(cut1);


            double additionalExtend = 20;

            //if (additionalExtend > 0) {
            //    Vector3d v0 = (cutNei0[0] - cutNei0[1]).UnitVector() * additionalExtend;
            //    Vector3d v1 = (cutNei0[3] - cutNei0[2]).UnitVector() * additionalExtend;
            //    cutNei0[0] += v0;
            //    cutNei0[0] += v1;

            //    v0 = (cutNei1[0] - cutNei1[1]).UnitVector() * additionalExtend;
            //    v1 = (cutNei1[3] - cutNei1[2]).UnitVector() * additionalExtend;
            //    cutNei1[0] += v0;
            //    cutNei1[0] += v1;
            //}


            cutNei0[0] = LineUtil.LineLine(cutNei0.SegmentAt(0), jointPanel.contour[0].SegmentAt((int)Math.Floor(jointPanel.contour[0].ClosestParameter(cutNei0[0]))));
            cutNei0[3] = LineUtil.LineLine(cutNei0.SegmentAt(2), jointPanel.contour[0].SegmentAt((int)Math.Floor(jointPanel.contour[0].ClosestParameter(cutNei0[3]))));

            cutNei1[0] = LineUtil.LineLine(cutNei1.SegmentAt(0), jointPanel.contour[1].SegmentAt((int)Math.Floor(jointPanel.contour[1].ClosestParameter(cutNei1[0]))));
            cutNei1[3] = LineUtil.LineLine(cutNei1.SegmentAt(2), jointPanel.contour[1].SegmentAt((int)Math.Floor(jointPanel.contour[1].ClosestParameter(cutNei1[3]))));
            jointPanel.contour[0].InsertPolyline(cutNei0);
            jointPanel.contour[1].InsertPolyline(cutNei1);

            //cutNei0.Bake();
            //cutNei1.Bake();

            //if (bake) {
            //    Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut0);
            //    Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut1);
            //}

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cutNei0);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cutNei1);

        }








    }

    public class PanelGroup {

        List<Panel> panels = new List<Panel>();

        public PanelGroup(List<Polyline> contourNoJoints) {

            for (int i = 0; i < contourNoJoints.Count; i += 2) {
                panels.Add(new Panel(contourNoJoints[i], contourNoJoints[i + 1]));
            }

        }

        public PanelGroup(Plane[] planes) {

            for (int i = 0; i < planes.Length; i++) {
                panels.Add(new Panel(planes[i]));
            }

        }
    }




    public class JointsVDANew : GH_Component {
        /// <summary>
        /// Initializes a new instance of the AdjustMeshVertices class.
        /// </summary>
        public JointsVDANew()
          : base("JointsVDA", "JointsVDA",
              "JointsVDA", "NGon",
                "Joint") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh needed for topology", GH_ParamAccess.item);
            pManager.AddVectorParameter("InsertionVec", "InsertionVec", "Insertion vectors", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Panels", "Panels", "Panel outlines as polylines", GH_ParamAccess.tree);


            //pManager.AddGenericParameter("Joints", "Joints", "Joint Parameters", GH_ParamAccess.tree);
          
            pManager.AddPointParameter("TwoJoints", "TwoJoints", "Two joint points markers", GH_ParamAccess.list);
            pManager.AddLineParameter("ExtendedJoints", "ExtendedJoints", "Extended joint as lines markers", GH_ParamAccess.list);
            pManager.AddLineParameter("DeeperCutsJoints", "DeeperCutsJoints", "DeeperCutsJoints as lines markers", GH_ParamAccess.list);


            pManager.AddBooleanParameter("Center", "Center", "Orient joint to the centre", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Finger", "Finger", "Create finger joint", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Sequence", "Sequence", "Sequence", GH_ParamAccess.list, 1);
            pManager.AddNumberParameter("TextScale", "TextScale", "0 - Panel Size, 1 - Joint Size, 2 - Adj Size, 3 - Adj Pos, 4 - Adj Scale Center,  5 - Joint Pos, 6 - last elements, 7 - extend" +   "", GH_ParamAccess.list, new List<double> { 30, 12, 15, 0.5, 0.6, 0, 1, 5 });


            for (int i = 0; i < pManager.ParamCount; i++) {
                pManager[i].Optional = true;
            }


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddCurveParameter("Panels", "Panels", "Panels Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Joints", "Joints", "Joints Polylines", GH_ParamAccess.tree);

            pManager.AddCurveParameter("PanelsMid", "PanelsMid", "Panels Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("JointsMid", "JointsMid", "Joints Polylines", GH_ParamAccess.tree);

            pManager.AddPlaneParameter("PanelsP", "PanelsP", "Panels Planes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("JointsP", "JointsP", "Joints Planes", GH_ParamAccess.tree);

            pManager.AddCurveParameter("PanelsTxt", "PanelsTxt", "Panels Text indexing", GH_ParamAccess.tree);
            pManager.AddCurveParameter("JointsTxt", "JointsTxt", "Joints Text indexing", GH_ParamAccess.tree);


            pManager.AddCurveParameter("PanelsLast", "PanelsLast", "Panels Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("JointsLast", "JointsLast", "Joints Polylines", GH_ParamAccess.tree);

            pManager.AddCurveParameter("PanelsMidLast", "PanelsMidLast", "Panels Polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("JointsMidLast", "JointsMidLast", "Joints Polylines", GH_ParamAccess.tree);

            pManager.AddPlaneParameter("PanelsPLast", "PanelsPLast", "Panels Planes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("JointsPLast", "JointsPLast", "Joints Planes", GH_ParamAccess.tree);

            pManager.AddCurveParameter("PanelsTxtLast", "PanelsTxtLast", "Panels Text indexing", GH_ParamAccess.tree);
            pManager.AddCurveParameter("JointsTxtLast", "JointsTxtLast", "Joints Text indexing", GH_ParamAccess.tree);
        }

        private List<JointsVDAInputs> GetJoints(DataTree<Vector3d> EV, Mesh M, List<Point3d> twoJointsP, List<Line> extendedJointsLn, List<Line> longerCutLn) {


            HashSet<int> twoJoints = M.SelectEdges(twoJointsP);
            HashSet<int> extendedJoints = M.SelectEdges(extendedJointsLn);
            HashSet<int> longerCut = M.SelectEdges(longerCutLn);



            List<JointsVDAInputs> joints = new List<JointsVDAInputs>();

            for (int i = 0; i < EV.BranchCount; i++) {
                int id = EV.Path(i).Indices[0];

                bool extendedJointsFlag = extendedJoints.Contains(id);
                bool longerCutFlag = longerCut.Contains(id);
                bool jointsCountFlag = twoJoints.Contains(id);


                //Gilesnii ipjovimai
                double length = (longerCutFlag) ? 100 : 60;//Cut length

                //Zali - be gilesnio ipjovimo

                double addExtend = (extendedJointsFlag && !longerCutFlag) ? 20 : 0;//Likusi dalis extend length
                double topExtend = (extendedJointsFlag) ? 25 : 10;//Pimpiukas extend length

                //Dvigubos jungtys
                int jointsCount = (jointsCountFlag) ? 2 : 1;//Number of joints

                double pimpiukasHeight = -25*0;//If not zero, custom joint is done
                double apaciosHeight = 95;
                double ipjovosStoris = 3.450;

                joints.Add(new JointsVDAInputs(id, jointsCount, length, apaciosHeight, ipjovosStoris, pimpiukasHeight, EV.Branch(EV.Path(i))[0], addExtend, topExtend));

            }

            return joints;
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            try {

                ////////////////////////////////////////////////////////////////////
                //Inputs Grasshopper
                Mesh M = DA.Fetch<Mesh>("Mesh");
                DataTree<Polyline> PanelOutlines = DA.FetchTree<GH_Curve>("Panels").ToPolylineDT();

                //Create joints
                DataTree<Vector3d> order = DA.FetchTree<GH_Vector>("InsertionVec").ToVectorDT();


                ////////////////////////////////////////////////////////////////////
                //      Insertion vectors

                //DataTree<Vector3d> EV = M.insertionVectors(Center, EVec);

                bool Center = DA.Fetch<bool>("Center");
                if (order.DataCount == 0)
                    order = M.insertionVectorsJoints(Center, null);//joints
                DataTree<Vector3d> EV = order;
                ////////////////////////////////////////////////////////////////////

                List<Point3d> twoJoints = DA.FetchList<Point3d>("TwoJoints");
                List<Line> extendedJoints = DA.FetchList<Line>("ExtendedJoints");
                List<Line> deeperCutsJoints = DA.FetchList<Line>("DeeperCutsJoints");

                List<JointsVDAInputs> joints = GetJoints(order, M, twoJoints, extendedJoints, deeperCutsJoints);

                if (order.DataCount != 0)
                    order = M.insertionVectorsJoints(Center, joints);//joints

                bool Finger = DA.Fetch<bool>("Finger");
                DataTree<Polyline> CChamfer = new DataTree<Polyline>();
                int iterations = DA.Fetch<int>("Iterations");
                List<int> sequence = DA.FetchList<int>("Sequence");
                List<double> textSize = DA.FetchList<double>("TextScale");


                if (textSize.Count != 8)
                    textSize = new List<double> { 30, 12, 15, 0.5, 0.6, 0, 1, 5 };

                DataTree<Panel> PanelGroups = DA.FetchTree<GH_Curve>("Panels").ToPanelsDT();
                DataTree<Panel> JointGroups = new DataTree<Panel>();

                ////////////////////////////////////////////////////////////////////


                ////////////////////////////////////////////////////////////////////

                int[][] tv = M.GetNGonsTopoBoundaries();
                int[][] fe = M.GetNGonFacesEdges(tv);
                HashSet<int> e = M.GetAllNGonEdges(tv);
                Dictionary<int, int[]> efDict = M.GetFE(e, false);
                Point3d[] centers = M.GetNGonCenters();

                Vector3d[] fn = M.GetNgonNormals();
                //int[][] ef = M.GetNgonsConnectedToNGonsEdges(e, true);
                ////////////////////////////////////////////////////////////////////


                DataTree<Polyline> diagonalConnections = new DataTree<Polyline>();
                //DataTree<Polyline> recJoints = new DataTree<Polyline>();

                //Iterate insertion edges
                Dictionary<int, int> meshEdgeDict = new Dictionary<int, int>();
                for (int i = 0; i < EV.BranchCount; i++) {// EV.BranchCount


                    int meshEdge = EV.Path(i).Indices[0];//mesh edge is used as dataTree branch

                    meshEdgeDict.Add(meshEdge, i);

                    if (efDict[meshEdge].Length != 2)
                        continue;
                    int f0 = efDict[meshEdge][0];
                    int f1 = efDict[meshEdge][1];

                    //Divide line into points and create a planes on these point, following insertion direction and average face normal
                    // Point3d[] pts = M.TopologyEdges.EdgeLine(meshEdge).InterpolateLine(divisions, false);
                    Point3d[] pts = M.TopologyEdges.EdgeLine(meshEdge).InterpolateLine(joints[i].divisions, false);

                    Vector3d avNormal = fn[f0] + fn[f1];
                    //Vector3d jointVector = Vector3d.CrossProduct(M.TopologyEdges.EdgeLine(meshEdge).Direction, avNormal);
                    //EV.Branch(EV.Path(i))[0] = jointVector;

                    for (int j = 0; j < pts.Length; j++) {
                        //(new Line(pts[j], pts[j]+ avNormal*40)).Bake();
                        //JointGroups.Add(new Panel(new Plane(pts[j], EV.Branch(EV.Path(i))[0].UnitVector(), M.GetMeshEdgePerpDir(meshEdge))), EV.Path(i));
                        Plane plane = new Plane(pts[j], avNormal, EV.Branch(EV.Path(i))[0].UnitVector());
                       // Plane plane = new Plane(pts[j],EV.Branch(EV.Path(i))[0].UnitVector(), M.GetMeshEdgePerpDir(meshEdge));

                        plane = plane.Switch("YX");
                        JointGroups.Add(new Panel(plane), EV.Path(i));
                        //plane.Bake(40);
                    }


                    //Construct joint outlines from planes
                    //Iterate number of joints per edge
                    for (int j = 0; j < pts.Length; j++) {

                        JointGroups.Branch(EV.Path(i))[j].planeOffset0 = JointGroups.Branch(EV.Path(i))[j].plane.MovePlanebyAxis(joints[i].thickness * 0.5); //offset planes

                        JointGroups.Branch(EV.Path(i))[j].planeOffset1 = JointGroups.Branch(EV.Path(i))[j].plane.MovePlanebyAxis(-joints[i].thickness * 0.5);//offset planes
                        JointGroups.Branch(EV.Path(i))[j].planeRot = new Plane(pts[j], Vector3d.CrossProduct(EV.Branch(EV.Path(i))[0].UnitVector(), M.GetMeshEdgePerpDir(meshEdge)), M.GetMeshEdgePerpDir(meshEdge));
                        int[] ngons = M.GetEdgeNgons(meshEdge);
                        Plane tempPlane = JointGroups.Branch(EV.Path(i))[j].planeRot.MovePlanebyAxis(joints[i].length);
                        int sign = tempPlane.Origin.DistanceToSquared(centers[ngons[0]]) < tempPlane.Origin.DistanceToSquared(centers[ngons[1]]) ? 1 : -1;

                        JointGroups.Branch(EV.Path(i))[j].planeRotOffset0 = JointGroups.Branch(EV.Path(i))[j].planeRot.MovePlanebyAxis(joints[i].length* sign); //offset planes
                        JointGroups.Branch(EV.Path(i))[j].planeRotOffset1 = JointGroups.Branch(EV.Path(i))[j].planeRot.MovePlanebyAxis(-joints[i].length* sign); //offset plane
                        JointGroups.Branch(EV.Path(i))[j].planeEdge = new Plane(pts[j], M.TopologyEdges.EdgeLine(meshEdge).Direction, M.GetMeshEdgePerpDir(meshEdge));

                        List<Plane> planesF0 = new List<Plane>();
                        List<Plane> planesF1 = new List<Plane>();
                        for (int k = 0; k < PanelGroups.Branch(f0).Count; k++) {
                            planesF0.Add(PanelGroups.Branch(f0)[k].plane);
                        }
                        for (int k = 0; k < PanelGroups.Branch(f1).Count; k++) {
                            planesF1.Add(PanelGroups.Branch(f1)[k].plane);
                        }



                        JointGroups.Branch(EV.Path(i))[j].planeF0 = PlaneUtil.AveragePlaneOrigin(planesF0);
                        JointGroups.Branch(EV.Path(i))[j].planeF1 = PlaneUtil.AveragePlaneOrigin(planesF1);

                        //JointGroups.Branch(EV.Path(i))[j].planeF0.MovePlanebyAxis(joints[i].height).Bake(40);
                        //JointGroups.Branch(EV.Path(i))[j].planeF1.MovePlanebyAxis(joints[i].height).Bake(40);

                        List<Plane> jointPlaneLoop = new List<Plane> {
                                                               JointGroups.Branch(EV.Path(i))[j].planeRotOffset0,
                                                               JointGroups.Branch(EV.Path(i))[j].planeF0.MovePlanebyAxis(joints[i].height),
                                                               JointGroups.Branch(EV.Path(i))[j].planeEdge,
                                                               JointGroups.Branch(EV.Path(i))[j].planeF1.MovePlanebyAxis(joints[i].height),//3
                                                               JointGroups.Branch(EV.Path(i))[j].planeRotOffset1,
                                                               JointGroups.Branch(EV.Path(i))[j].planeF1.MovePlanebyAxis(-joints[i].height),//5
                                                               JointGroups.Branch(EV.Path(i))[j].planeEdge,
                                                               JointGroups.Branch(EV.Path(i))[j].planeF0.MovePlanebyAxis(-joints[i].height),

                                                           };

                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(JointGroups.Branch(EV.Path(i))[j].planeF1, new Interval(-20, 20), new Interval(-20, 20)));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(JointGroups.Branch(EV.Path(i))[j].planeF1, new Interval(-20, 20), new Interval(-20, 20)));

                        JointGroups.Branch(EV.Path(i))[j].contourNoJoints[0] = PolylineUtil.PolylineFromPlanes(JointGroups.Branch(EV.Path(i))[j].planeOffset0, jointPlaneLoop);
                        JointGroups.Branch(EV.Path(i))[j].contourNoJoints[1] = PolylineUtil.PolylineFromPlanes(JointGroups.Branch(EV.Path(i))[j].planeOffset1, jointPlaneLoop);
                        JointGroups.Branch(EV.Path(i))[j].contour[0] = new Polyline(JointGroups.Branch(EV.Path(i))[j].contourNoJoints[0]);
                        JointGroups.Branch(EV.Path(i))[j].contour[1] = new Polyline(JointGroups.Branch(EV.Path(i))[j].contourNoJoints[1]);
                        //JointGroups.Branch(EV.Path(i))[j].contour[0].Bake();

                    }


                    //Construct Cuts
                    //Iterate number of joints per edge
                    for (int j = 0; j < pts.Length; j++) {

                        int localMeshEdgeF0 = Array.IndexOf(fe[f0], meshEdge);
                        int localMeshEdgeF1 = Array.IndexOf(fe[f1], meshEdge);



                        //Iterate number of panels and create cuts
                        for (int k = 0; k < PanelGroups.Branch(f0).Count; k++) {

                            Panel jointPanel = JointGroups.Branch(EV.Path(i))[j];
                            //Rhino.RhinoApp.WriteLine(jointPanel.contourNoJoints[0].Count.ToString());

                            jointPanel.id = f0.ToString() + "-" + f1.ToString();

                            if (pts.Length > 1)
                                jointPanel.id += "-" + j.ToString();

                            PanelGroups.Branch(f0)[k].CreateCut(localMeshEdgeF0, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, joints[i].length, ref jointPanel, false);//, ref neiPanel, ref jointPanel);
                            //PanelGroups.Branch(f1)[k].CreateCut(localMeshEdgeF1, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, joints[i].length, ref jointPanel, false);//, ref neiPanel, ref jointPanel);

                            JointGroups.Branch(EV.Path(i))[j] = jointPanel;
                        }


                        //Iterate number of panels and create cuts
                        for (int k = 0; k < PanelGroups.Branch(f1).Count; k++) {

                            Panel jointPanel = JointGroups.Branch(EV.Path(i))[j];

                            jointPanel.id = f0.ToString() + "-" + f1.ToString();

                            if (pts.Length > 1)
                                jointPanel.id += "-" + j.ToString();

                            //PanelGroups.Branch(f0)[k].CreateCut(localMeshEdgeF0, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, joints[i].length, ref jointPanel, false);//, ref neiPanel, ref jointPanel);
                            PanelGroups.Branch(f1)[k].CreateCut(localMeshEdgeF1, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, joints[i].length, ref jointPanel, false);//, ref neiPanel, ref jointPanel);

                            JointGroups.Branch(EV.Path(i))[j] = jointPanel;
                        }


                    }


                }


                for (int i = 0; i < JointGroups.BranchCount; i++) {
                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {
                        if (joints[i].custom == 0) {
                            if (joints[i].custom > 0)
                                JointGroups.Branch(i)[j].ChangeJoint(joints[i].custom, 0, joints[i].cutExtend, joints[i].addExtend);
                            else if (joints[i].custom < 0) {
                                JointGroups.Branch(i)[j].ChangeJoint(joints[i].custom, 1, joints[i].cutExtend, joints[i].addExtend);
                            }
                        }

                    }

                }














                ////////////////////////Output
                var dtPlates = new DataTree<Polyline>();
                var dtJoints = new DataTree<Polyline>();
                var dtPlatesMid = new DataTree<Polyline>();
                var dtJointsMid = new DataTree<Polyline>();
                var dtPlatesPlanes = new DataTree<Plane>();
                var dtJointsPlanes = new DataTree<Plane>();
                var dtPlatesTxt = new DataTree<Curve>();
                var dtJointsTxt = new DataTree<Curve>();

                var dtPlatesLast = new DataTree<Polyline>();
                var dtJointsLast = new DataTree<Polyline>();
                var dtPlatesMidLast = new DataTree<Polyline>();
                var dtJointsMidLast = new DataTree<Polyline>();
                var dtPlatesPlanesLast = new DataTree<Plane>();
                var dtJointsPlanesLast = new DataTree<Plane>();
                var dtPlatesTxtLast = new DataTree<Curve>();
                var dtJointsTxtLast = new DataTree<Curve>();

                HashSet<int> jointSequenceTemp = new HashSet<int>();
                HashSet<int> jointSequence = new HashSet<int>();

                HashSet<int> jointSequenceLast = new HashSet<int>();
                int last = Math.Min(iterations, PanelGroups.BranchCount);
                int prev = Math.Max(0, last - (int)textSize[6]);



                for (int i = 0; i < last; i++) {//Math.Min(iterations, sequence.Count) PanelGroups.BranchCount

                    for (int j = 0; j < fe[i].Length; j++) {

                        bool seq = jointSequenceTemp.Add(fe[i][j]);

                        if (i >= prev ) {
                           if (seq)
                                jointSequenceLast.Add(fe[i][j]);
                           else
                                jointSequence.Add(fe[i][j]);
                        } else
                            jointSequence.Add(fe[i][j]);

                    }

                    for (int j = 0; j < PanelGroups.Branch(i).Count; j++) {

             

                        if (i >= prev) {

                            dtPlatesLast.Add(PanelGroups.Branch(i)[j].contour[0], new GH_Path(i, j));
                            dtPlatesLast.Add(PanelGroups.Branch(i)[j].contour[1], new GH_Path(i, j));
                            dtPlatesMidLast.Add(PanelGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));
                        } else {
                            dtPlates.Add(PanelGroups.Branch(i)[j].contour[0], new GH_Path(i, j));
                            dtPlates.Add(PanelGroups.Branch(i)[j].contour[1], new GH_Path(i, j));
                            dtPlatesMid.Add(PanelGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));
                        }



                        Plane textPlane = PanelGroups.Branch(i)[j].planeOffset0;
                        textPlane.Flip();

                        if (j > 0)
                            textPlane = PanelGroups.Branch(i)[j].planeOffset1;



                       

                        if (i >= prev) {
                            dtPlatesPlanesLast.Add(textPlane, new GH_Path(i, j));
                        } else {
                            dtPlatesPlanes.Add(textPlane, new GH_Path(i, j));
                        }


                        string text = i.ToString() + "-" + j.ToString();
                        var txtCrv = Typewriter.Regular.Write(text, textPlane, textSize[0]);
                       

                        if (i >= prev) {
                            dtPlatesTxtLast.AddRange(txtCrv, new GH_Path(i, j));
                        } else {
                            dtPlatesTxt.AddRange(txtCrv, new GH_Path(i, j));
                        }


                        var a = PanelGroups.Branch(i)[j];

                        Line[] segments = PanelGroups.Branch(i)[j].contourNoJoints[Math.Min(1, j)].GetSegments();


                        int counter = 0;
                        foreach (Line l in segments) {

                            int meshEdge = fe[i][counter];

                            int neiF = M.GetOppositeNgon(meshEdge, i);


                            //Adjacent face plane
                            Point3d origin = l.PointAt(textSize[3]);
                            Vector3d xaxis = l.Direction;
                            Vector3d yaxis = l.Direction;
                            origin.Transform(Rhino.Geometry.Transform.Scale(textPlane.Origin, textSize[4]));
                            yaxis.Rotate(Math.PI * 0.5, textPlane.ZAxis);
                            Plane ePlane = new Plane(origin, xaxis, yaxis);

                            var txtCrvF = Typewriter.Regular.Write(neiF.ToString(), ePlane, textSize[2]);
                            

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF, new GH_Path(i, j));
                            } else {
                                dtPlatesTxt.AddRange(txtCrvF, new GH_Path(i, j));
                            }



                            //Mesh edge direction
                            Line meshEdgeLine = M.TopologyEdges.EdgeLine(meshEdge);
                            meshEdgeLine.Transform(Rhino.Geometry.Transform.Scale(meshEdgeLine.PointAt(0.5), textSize[4]));
                            meshEdgeLine.Transform(Rhino.Geometry.Transform.Scale(textPlane.Origin, textSize[4]));
                            //meshEdgeLine.Extend(-textSize[4], -textSize[4]);


                            Plane e0Plane = new Plane(ePlane.ClosestPoint(meshEdgeLine.From), xaxis, yaxis);
                            Plane e1Plane = new Plane(ePlane.ClosestPoint(meshEdgeLine.To), xaxis, yaxis);

                            var txtCrvF0 = Typewriter.Regular.Write("I", e0Plane, textSize[2]);
                         

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF0, new GH_Path(i, j));
                            } else {
                                dtPlatesTxt.AddRange(txtCrvF0, new GH_Path(i, j));
                            }

                            var txtCrvF1 = Typewriter.Regular.Write("II", e1Plane, textSize[2]);
                           

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF1, new GH_Path(i, j));
                            } else {
                                dtPlatesTxt.AddRange(txtCrvF1, new GH_Path(i, j));
                            }


                            counter++;
                            //
                        }




                    }

                }

                DataTree<Vector3d> insertionVectors = new DataTree<Vector3d>();


                foreach (int meshEdge in jointSequence) {

                    if (!meshEdgeDict.ContainsKey(meshEdge))
                        continue;
                    int i = meshEdgeDict[meshEdge];



                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {

                        GH_Path path = new GH_Path(meshEdge, j);

                        insertionVectors.Add(JointGroups.Branch(i)[j].planeOffset0.XAxis,path);


                        dtJoints.Add(JointGroups.Branch(i)[j].contour[0], path);
                        dtJoints.Add(JointGroups.Branch(i)[j].contour[1], path);
                        dtJointsMid.Add(JointGroups.Branch(i)[j].MidContour(), path);
                        dtJointsPlanes.Add(JointGroups.Branch(i)[j].planeOffset0, path);

                        Plane planet = new Plane(JointGroups.Branch(i)[j].planeOffset0.Origin + JointGroups.Branch(i)[j].planeOffset0.YAxis * textSize[5], JointGroups.Branch(i)[j].planeOffset0.XAxis, JointGroups.Branch(i)[j].planeOffset0.YAxis);





                        string text = JointGroups.Branch(i)[j].id;
                        var txtCrv = Typewriter.Regular.Write(text, planet, textSize[1]);
                        dtJointsTxt.AddRange(txtCrv, path);


                    }



                }








                foreach (int meshEdge in jointSequenceLast) {


                    if (!meshEdgeDict.ContainsKey(meshEdge))
                        continue;
                    int i = meshEdgeDict[meshEdge];



                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {




                        dtJointsLast.Add(JointGroups.Branch(i)[j].contour[0], new GH_Path(meshEdge, j));
                        dtJointsLast.Add(JointGroups.Branch(i)[j].contour[1], new GH_Path(meshEdge, j));
                        dtJointsMidLast.Add(JointGroups.Branch(i)[j].MidContour(), new GH_Path(meshEdge, j));

                        dtJointsPlanesLast.Add(JointGroups.Branch(i)[j].planeOffset0, new GH_Path(meshEdge, j));

                        Plane planet = new Plane(JointGroups.Branch(i)[j].planeOffset0.Origin + JointGroups.Branch(i)[j].planeOffset0.YAxis * textSize[5], JointGroups.Branch(i)[j].planeOffset0.XAxis, JointGroups.Branch(i)[j].planeOffset0.YAxis);


                        string text = JointGroups.Branch(i)[j].id;
                        var txtCrv = Typewriter.Regular.Write(text, planet, textSize[1]);
                        dtJointsTxtLast.AddRange(txtCrv, new GH_Path(meshEdge, j));

                    }

                }


                DA.SetDataTree(0, dtPlates);
                DA.SetDataTree(1, dtJoints);

                DA.SetDataTree(2, dtPlatesMid);
                DA.SetDataTree(3, dtJointsMid);

                DA.SetDataTree(4, dtPlatesPlanes);
                DA.SetDataTree(5, dtJointsPlanes);

                DA.SetDataTree(6, dtPlatesTxt);
                DA.SetDataTree(7, dtJointsTxt);




                DA.SetDataTree(8, dtPlatesLast);
                DA.SetDataTree(9, dtJointsLast);

                DA.SetDataTree(10, dtPlatesMidLast);
                DA.SetDataTree(11, dtJointsMidLast);

                DA.SetDataTree(12, dtPlatesPlanesLast);
                DA.SetDataTree(13, dtJointsPlanesLast);

                DA.SetDataTree(14, dtPlatesTxtLast);
                DA.SetDataTree(15, dtJointsTxtLast);






            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

        }



        protected override System.Drawing.Bitmap Icon {
            get {
                return NGon_RH8.Properties.Resources.vdajoints;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("a8606789-1e62-44ac-803c-8cd799d9d48b"); }
        }
    }
}