using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using NGonsCore;
using NGonsCore.Clipper;
using NGonsCore.Geometry;
using NGonsCore.Text;
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

        public static DataTree<Panel> ToPanelsDT(this GH_Structure<GH_Curve> Curves) {

            DataTree<Panel> panelGroups = new DataTree<Panel>();

            for (int i = 0; i < Curves.Branches.Count; i++) {

                List<Polyline> outlines = new List<Polyline>();

                for (int j = 0; j < Curves.get_Branch(i).Count; j++) {
                    Polyline polyline;
                    Curves.get_DataItem(Curves.Paths[i], j).Value.TryGetPolyline(out polyline);
                    outlines.Add(polyline);

                    if (j % 2 == 1) {
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

        public void ChangeJoint(double extend = 10, int type = 0, double cutExtend = 5) {

            switch(type){

                case(1):
                if (contour[0].Count > 23) {

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
    
                    }
                }
                break;

                default:
                if (contour[0].Count > 23) {

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
            Vector3d v00 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane0);
            Vector3d v01 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane1);
            Vector3d v10 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane0);
            Vector3d v11 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane1);


            //Really fucked up way of checking which direction plane must be offseted for correct cut direction
            Point3d p0 = mesh.ClosestPoint((segment0.PointAt(0.5) + v00 * 10));
            Point3d p1 = mesh.ClosestPoint((segment0.PointAt(0.5) - v00 * 10));
            //bool moveFlag = (segment0.PointAt(0.5) + v00*10).DistanceTo(planeOffset0.Origin) < (segment0.PointAt(0.5) - v00*10).DistanceTo(planeOffset0.Origin);
            bool moveFlag = p0.DistanceToSquared((segment0.PointAt(0.5) + v00 * 10)) < p1.DistanceToSquared((segment0.PointAt(0.5) - v00 * 10));


            if (bake) {

              
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(planeOffset0.Origin);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(segment0.PointAt(0.5) + v00*100);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(segment0.PointAt(0.5) - v00*100);

            }

            //Moved Points
            Point3d innerPoint00 = (moveFlag) ? edgePoint00 + (v00 * length) : edgePoint00 - (v00 * length);
            Point3d innerPoint01 = (moveFlag) ? edgePoint01 + (v01 * length) : edgePoint01 - (v01 * length);
            Point3d innerPoint10 = (moveFlag) ? edgePoint10 + (v10 * length) : edgePoint10 - (v10 * length);
            Point3d innerPoint11 = (moveFlag) ? edgePoint11 + (v11 * length) : edgePoint11 - (v11 * length);
            Point3d innerPointCenter = (innerPoint00 + innerPoint01 + innerPoint10 + innerPoint11) * 0.25;
            Plane perpPlane = new Plane(innerPointCenter, jointPlane0.Normal, Vector3d.CrossProduct(planeOffset0.Normal, v00));



            innerPoint00 = perpPlane.RayPlane(edgePoint00, v00);
            innerPoint01 = perpPlane.RayPlane(edgePoint01, v01);
            innerPoint10 = perpPlane.RayPlane(edgePoint10, v10);
            innerPoint11 = perpPlane.RayPlane(edgePoint11, v11);

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(perpPlane, 50, 50));
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(jointPlane0, 50, 50));


            //Middle points and projection to plane
            Point3d innerPointMid00 = (moveFlag) ? edgePoint00 + (v00 * length * 0.5) : edgePoint00 - (v00 * length * 0.5);
            Point3d innerPointMid01 = (moveFlag) ? edgePoint01 + (v01 * length * 0.5) : edgePoint01 - (v01 * length * 0.5);
            Point3d innerPointMid10 = (moveFlag) ? edgePoint10 + (v10 * length * 0.5) : edgePoint10 - (v10 * length * 0.5);
            Point3d innerPointMid11 = (moveFlag) ? edgePoint11 + (v11 * length * 0.5) : edgePoint11 - (v11 * length * 0.5);
            Point3d innerPointMid = (innerPointMid00 + innerPointMid01 + innerPointMid10 + innerPointMid11) * 0.25;
            Plane perpPlaneMid = new Plane(innerPointMid, jointPlane0.Normal, perpPlane.YAxis);

            innerPointMid00 = perpPlaneMid.RayPlane(edgePoint00, v00);
            innerPointMid01 = perpPlaneMid.RayPlane(edgePoint01, v01);
            innerPointMid10 = perpPlaneMid.RayPlane(edgePoint10, v10);
            innerPointMid11 = perpPlaneMid.RayPlane(edgePoint11, v11);


            //It is not closest point because the connection is not perpendicular to two adjacent plates
            //It might be close to perpendicular but not possible

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

            jointPanel.contour[0].InsertPolyline(cutNei0);
            jointPanel.contour[1].InsertPolyline(cutNei1);

            if (bake) {
                Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut0);
                Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut1);
            }
          
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
                "Transform") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh needed for topology", GH_ParamAccess.item);
            pManager.AddCurveParameter("Panels", "Panels", "Panel outlines as polylines", GH_ParamAccess.tree);
            pManager.AddVectorParameter("EdgeVectors", "EdgeVectors", "EdgeVectors are used for joint direction", GH_ParamAccess.tree);

            pManager.AddIntegerParameter("JointDiv", "JointDiv", "Number of joints on each edge", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("JointLen", "JointLen", "If number is negative the edge is scaled, if positive exact length is used", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("JointHei", "JointHei", "Joint height", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("JointThi", "JointThi", "Joint Thickiness must be the same as mesh offset W parameter", GH_ParamAccess.item, 3);

            pManager.AddBooleanParameter("Center", "Center", "Orient joint to the centre", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Finger", "Finger", "Create finger joint", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Custom", "Custom", "Create half joint if two layers exists", GH_ParamAccess.item, -1);
            //pManager.AddCurveParameter("PanelsChamfer", "PanelsChamfer", "Additional Polylines in case offset does not work properly", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Sequence", "Sequence", "Sequence", GH_ParamAccess.list, 1);

            pManager.AddNumberParameter("TextScale", "TextScale", "0 - Panel Size, 1 - Joint Size, 2 - Adj Size, 3 - Adj Pos, 4 - Adj Scale Center,  5 - Joint Pos, 6 - last elements, 7 - extend" +
                "", GH_ParamAccess.list);
            for (int i = 2; i < pManager.ParamCount; i++) {
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

        protected override void SolveInstance(IGH_DataAccess DA) {

            try {

                ////////////////////////////////////////////////////////////////////
                //Inputs Grasshopper
                Mesh M = DA.Fetch<Mesh>("Mesh");
                DataTree<Polyline> PanelOutlines = DA.FetchTree<GH_Curve>("Panels").ToPolylineDT();
                DataTree<Vector3d> EVec = DA.FetchTree<GH_Vector>("EdgeVectors").ToDT();
                int D = DA.Fetch<int>("JointDiv");
                double L = DA.Fetch<double>("JointLen");
                double H = DA.Fetch<double>("JointHei");
                double W = DA.Fetch<double>("JointThi");
                bool Center = DA.Fetch<bool>("Center");
                bool Finger = DA.Fetch<bool>("Finger");
                double Custom = DA.Fetch<double>("Custom");
                DataTree<Polyline> CChamfer = new DataTree<Polyline>();
                int iterations = DA.Fetch<int>("Iterations");
                List<int> sequence = DA.FetchList<int>("Sequence");
                List<double> textSize = DA.FetchList<double>("TextScale");

                if (textSize.Count < 6)
                    textSize = new List<double> { 20, 10,10,0.5,0.75,10 };

                DataTree<Panel> PanelGroups = DA.FetchTree<GH_Curve>("Panels").ToPanelsDT();
                DataTree<Panel> JointGroups = new DataTree<Panel>();
                ////////////////////////////////////////////////////////////////////


                ////////////////////////////////////////////////////////////////////
                //Inputs Local
                int divisions = Math.Max(1, D);
                double jointLength = L;//Math.Max(0.1, L);
                double height = Math.Max(0.1, H);
                double width = Math.Max(0.1, W);

                int[][] tv = M.GetNGonsTopoBoundaries();
                int[][] fe = M.GetNGonFacesEdges(tv);
                HashSet<int> e = M.GetAllNGonEdges(tv);
                Dictionary<int, int[]> efDict = M.GetFE(e, false);
                Point3d[] centers = M.GetNGonCenters();
                ////////////////////////////////////////////////////////////////////

                ////////////////////////////////////////////////////////////////////
                //      Insertion vectors
                DataTree<Vector3d> EV = M.insertionVectors(Center, EVec);
                ////////////////////////////////////////////////////////////////////

                DataTree<Polyline> diagonalConnections = new DataTree<Polyline>();
                //DataTree<Polyline> recJoints = new DataTree<Polyline>();

                //Iterate insertion edges
                Dictionary<int, int> meshEdgeDict = new Dictionary<int, int>();
                for (int i = 0; i < EV.BranchCount; i++) {// EV.BranchCount


                    int meshEdge = EV.Path(i).Indices[0];//mesh edge is used as dataTree branch

                    meshEdgeDict.Add(meshEdge,i);

                    if (efDict[meshEdge].Length != 2) continue;
                    int f0 = efDict[meshEdge][0];
                    int f1 = efDict[meshEdge][1];

                    //Divide line into points and create a planes on these point, following insertion direction and average face normal
                    Point3d[] pts = M.TopologyEdges.EdgeLine(meshEdge).InterpolateLine(divisions, false);

                    for (int j = 0; j < pts.Length; j++)

                        JointGroups.Add(new Panel(new Plane(pts[j], EV.Branch(EV.Path(i))[0].UnitVector(), M.GetMeshEdgePerpDir(meshEdge))), EV.Path(i));


                    //Construct joint outlines from planes
                    //Iterate number of joints per edge
                    for (int j = 0; j < pts.Length; j++) {

                        JointGroups.Branch(EV.Path(i))[j].planeOffset0 = JointGroups.Branch(EV.Path(i))[j].plane.MovePlanebyAxis(W * 0.5); //offset planes
                        JointGroups.Branch(EV.Path(i))[j].planeOffset1 = JointGroups.Branch(EV.Path(i))[j].plane.MovePlanebyAxis(-W * 0.5);//offset planes
                        JointGroups.Branch(EV.Path(i))[j].planeRot = new Plane(pts[j], Vector3d.CrossProduct(EV.Branch(EV.Path(i))[0].UnitVector(), M.GetMeshEdgePerpDir(meshEdge)), M.GetMeshEdgePerpDir(meshEdge));
                        JointGroups.Branch(EV.Path(i))[j].planeRotOffset0 = JointGroups.Branch(EV.Path(i))[j].planeRot.MovePlanebyAxis(jointLength); //offset planes
                        JointGroups.Branch(EV.Path(i))[j].planeRotOffset1 = JointGroups.Branch(EV.Path(i))[j].planeRot.MovePlanebyAxis(-jointLength); //offset plane
                        JointGroups.Branch(EV.Path(i))[j].planeEdge = new Plane(pts[j], M.TopologyEdges.EdgeLine(meshEdge).Direction, M.GetMeshEdgePerpDir(meshEdge));

                        List<Plane> planesF0 = new List<Plane>();
                        List<Plane> planesF1 = new List<Plane>();
                        for (int k = 0; k < PanelGroups.Branch(f0).Count; k++) {
                            planesF0.Add(PanelGroups.Branch(f0)[k].plane);
                            planesF1.Add(PanelGroups.Branch(f1)[k].plane);
                        }

                  

                        JointGroups.Branch(EV.Path(i))[j].planeF0 = PlaneUtil.AveragePlaneOrigin(planesF0);
                        JointGroups.Branch(EV.Path(i))[j].planeF1 = PlaneUtil.AveragePlaneOrigin(planesF1);

                        List<Plane> jointPlaneLoop = new List<Plane> {
                            JointGroups.Branch(EV.Path(i))[j].planeRotOffset0,
                            JointGroups.Branch(EV.Path(i))[j].planeF0.MovePlanebyAxis(height),
                            JointGroups.Branch(EV.Path(i))[j].planeEdge,
                            JointGroups.Branch(EV.Path(i))[j].planeF1.MovePlanebyAxis(height),//3
                            JointGroups.Branch(EV.Path(i))[j].planeRotOffset1,
                            JointGroups.Branch(EV.Path(i))[j].planeF1.MovePlanebyAxis(-height),//5
                            JointGroups.Branch(EV.Path(i))[j].planeEdge,
                            JointGroups.Branch(EV.Path(i))[j].planeF0.MovePlanebyAxis(-height),

                        };

                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(JointGroups.Branch(EV.Path(i))[j].planeF1, new Interval(-20, 20), new Interval(-20, 20)));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(JointGroups.Branch(EV.Path(i))[j].planeF1, new Interval(-20, 20), new Interval(-20, 20)));

                        JointGroups.Branch(EV.Path(i))[j].contourNoJoints[0] = PolylineUtil.PolylineFromPlanes(JointGroups.Branch(EV.Path(i))[j].planeOffset0, jointPlaneLoop);
                        JointGroups.Branch(EV.Path(i))[j].contourNoJoints[1] = PolylineUtil.PolylineFromPlanes(JointGroups.Branch(EV.Path(i))[j].planeOffset1, jointPlaneLoop);
                        JointGroups.Branch(EV.Path(i))[j].contour[0] = new Polyline(JointGroups.Branch(EV.Path(i))[j].contourNoJoints[0]);
                        JointGroups.Branch(EV.Path(i))[j].contour[1] = new Polyline(JointGroups.Branch(EV.Path(i))[j].contourNoJoints[1]);

 
                    }


                    //Construct Cuts
                    //Iterate number of joints per edge
                    for (int j = 0; j < pts.Length; j++) {

                        int localMeshEdgeF0 = Array.IndexOf(fe[f0], meshEdge);
                        int localMeshEdgeF1 = Array.IndexOf(fe[f1], meshEdge);

                        

                        //Iterate number of panels and create cuts
                        for (int k = 0; k < PanelGroups.Branch(f0).Count; k++) {

                            Panel jointPanel = JointGroups.Branch(EV.Path(i))[j];

                            jointPanel.id = f0.ToString() + "-" + f1.ToString() ;

                            //if(f0==f1)  
                              //  Rhino.RhinoApp.WriteLine(jointPanel.id);

                            if (pts.Length > 1)
                                jointPanel.id += "-" + j.ToString();

                            bool flag = f0 == 165 && f1 == 166;
                            PanelGroups.Branch(f0)[k].CreateCut(localMeshEdgeF0, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, jointLength, ref jointPanel, flag);//, ref neiPanel, ref jointPanel);
                            PanelGroups.Branch(f1)[k].CreateCut(localMeshEdgeF1, JointGroups.Branch(EV.Path(i))[j].planeOffset0, JointGroups.Branch(EV.Path(i))[j].planeOffset1, jointLength, ref jointPanel, flag);//, ref neiPanel, ref jointPanel);
                          
                            JointGroups.Branch(EV.Path(i))[j] = jointPanel;
                        }


                    }


                }


                for (int i = 0; i < JointGroups.BranchCount; i++) {
                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {
                        if(Custom>0)
                            JointGroups.Branch(i)[j].ChangeJoint(Custom,0);
                        else if (Custom < 0) {
                            JointGroups.Branch(i)[j].ChangeJoint(Custom, 1,textSize[7]);
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

                HashSet<int> jointSequence = new HashSet<int>();

                HashSet<int> jointSequenceLast = new HashSet<int>();
                int last = Math.Min(iterations, PanelGroups.BranchCount);
                int prev = Math.Max(0,last - (int)textSize[6]);


                for (int i = 0; i < last; i++) {//Math.Min(iterations, sequence.Count) PanelGroups.BranchCount

                    for (int j = 0; j < fe[i].Length; j++) {
                        bool seq = jointSequence.Add(fe[i][j]);

                        if (i >= prev) {
                            if (seq)
                                jointSequenceLast.Add(fe[i][j]);
                        }

                    }

                    for (int j = 0; j < PanelGroups.Branch(i).Count; j++) {
                   
                        dtPlates.Add(PanelGroups.Branch(i)[j].contour[0], new GH_Path(i, j));
                        dtPlates.Add(PanelGroups.Branch(i)[j].contour[1], new GH_Path(i, j));
                        dtPlatesMid.Add(PanelGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));

                        if(i >= prev) {
                            dtPlatesLast.Add(PanelGroups.Branch(i)[j].contour[0], new GH_Path(i, j));
                            dtPlatesLast.Add(PanelGroups.Branch(i)[j].contour[1], new GH_Path(i, j));
                            dtPlatesMidLast.Add(PanelGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));
                        }

            

                        Plane textPlane = PanelGroups.Branch(i)[j].planeOffset0;
                        textPlane.Flip();

                        if(j == 1)
                            textPlane = PanelGroups.Branch(i)[j].planeOffset1;



                        dtPlatesPlanes.Add(textPlane, new GH_Path(i, j));

                        if (i >= prev) {
                            dtPlatesPlanesLast.Add(textPlane, new GH_Path(i, j));
                        }


                            string text = i.ToString() + "-" + j.ToString();
                        var txtCrv = Typewriter.Regular.Write(text, textPlane, textSize[0]);
                        dtPlatesTxt.AddRange(txtCrv, new GH_Path(i, j));

                        if (i >= prev) {
                            dtPlatesTxtLast.AddRange(txtCrv, new GH_Path(i, j));
                        }


                        //for(int k = 0; k < PanelGroups.Branch(i)[j].contourNoJoints.Length; k++) {


                        Line[] segments= PanelGroups.Branch(i)[j].contourNoJoints[j].GetSegments();

                            int counter = 0;
                            foreach(Line l in segments) {

                                int meshEdge = fe[i][counter];

                                int neiF = M.GetOppositeNgon(meshEdge,i);
                               

                                //Adjacent face plane
                                Point3d origin = l.PointAt(textSize[3]);
                                Vector3d xaxis = l.Direction;
                                Vector3d yaxis = l.Direction;
                                origin.Transform(Transform.Scale(textPlane.Origin, textSize[4]));
                                yaxis.Rotate(Math.PI * 0.5, textPlane.ZAxis);
                                Plane ePlane = new Plane(origin, xaxis, yaxis);

                                var txtCrvF = Typewriter.Regular.Write(neiF.ToString(), ePlane, textSize[2]);
                                dtPlatesTxt.AddRange(txtCrvF, new GH_Path(i, j));

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF, new GH_Path(i, j));
                            }



                            //Mesh edge direction
                            Line meshEdgeLine = M.TopologyEdges.EdgeLine(meshEdge);
                            meshEdgeLine.Transform(Transform.Scale(meshEdgeLine.PointAt(0.5), textSize[4]));
                            meshEdgeLine.Transform(Transform.Scale(textPlane.Origin, textSize[4]));
                            //meshEdgeLine.Extend(-textSize[4], -textSize[4]);
                                

                                Plane e0Plane = new Plane(ePlane.ClosestPoint(meshEdgeLine.From), xaxis, yaxis);
                                Plane e1Plane = new Plane(ePlane.ClosestPoint(meshEdgeLine.To), xaxis, yaxis);
                               
                                var txtCrvF0 = Typewriter.Regular.Write("I", e0Plane, textSize[2]);
                                dtPlatesTxt.AddRange(txtCrvF0, new GH_Path(i, j));

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF0, new GH_Path(i, j));
                            }

                            var txtCrvF1 = Typewriter.Regular.Write("II", e1Plane, textSize[2]);
                                dtPlatesTxt.AddRange(txtCrvF1, new GH_Path(i, j));

                            if (i >= prev) {
                                dtPlatesTxtLast.AddRange(txtCrvF1, new GH_Path(i, j));
                            }


                            counter++;
                            //
                        }

                        


                    }

                }

                foreach (int meshEdge in jointSequence) {
                    //for (int i = 0; i < Math.Min(iterations, sequence.Count); i++) {//JointGroups.BranchCount
                    if (!meshEdgeDict.ContainsKey(meshEdge)) continue;
                    int i = meshEdgeDict[meshEdge];

                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {
                        dtJoints.Add(JointGroups.Branch(i)[j].contour[0], new GH_Path(meshEdge, j));
                        dtJoints.Add(JointGroups.Branch(i)[j].contour[1], new GH_Path(meshEdge, j));
                        dtJointsMid.Add(JointGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));

                        dtJointsPlanes.Add(JointGroups.Branch(i)[j].planeOffset0, new GH_Path(meshEdge, j));

                        Plane planet = new Plane(JointGroups.Branch(i)[j].planeOffset0.Origin + JointGroups.Branch(i)[j].planeOffset0.YAxis * textSize[5], JointGroups.Branch(i)[j].planeOffset0.XAxis, JointGroups.Branch(i)[j].planeOffset0.YAxis);


                        string text = JointGroups.Branch(i)[j].id;
                        var txtCrv = Typewriter.Regular.Write(text, planet, textSize[1]);
                        dtJointsTxt.AddRange(txtCrv, new GH_Path(meshEdge, j));

                    }

                }

                foreach (int meshEdge in jointSequenceLast) {
                    //for (int i = 0; i < Math.Min(iterations, sequence.Count); i++) {//JointGroups.BranchCount
                    if (!meshEdgeDict.ContainsKey(meshEdge)) continue;
                    int i = meshEdgeDict[meshEdge];

                    for (int j = 0; j < JointGroups.Branch(i).Count; j++) {
                        dtJointsLast.Add(JointGroups.Branch(i)[j].contour[0], new GH_Path(meshEdge, j));
                        dtJointsLast.Add(JointGroups.Branch(i)[j].contour[1], new GH_Path(meshEdge, j));
                        dtJointsMidLast.Add(JointGroups.Branch(i)[j].MidContour(), new GH_Path(i, j));

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







            } catch(Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            
        }



        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.vdajoints;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("a8606789-1e62-44ac-803c-8cd799d9d48b"); }
        }
    }
}