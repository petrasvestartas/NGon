using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Utils {

    

    public class RecipricalVolume : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalVolume()
          : base("RecipricalVolume", "RecipricalVolume",
              "Volumetric elements",
              "NGon", "Reciprocal") {
       
            }

        


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "Angle", "Angle", GH_ParamAccess.item,0.7);
            pManager.AddNumberParameter("Scale", "Scale", "Scale", GH_ParamAccess.item,1.4);
            pManager.AddBooleanParameter("NormalType", "NormalType", "0 - normal is used as an average ngon plane, 1 - normal of adjacent triangle face",GH_ParamAccess.item,true);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item, 1);

            pManager.AddNumberParameter("Offset", "Offset", "Offset between layers", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Thickness", "Thickness", "Paper Thickness", GH_ParamAccess.item, 1);

            pManager.AddSurfaceParameter("RoofSurface", "RoofSurface", "RoofSurface",GH_ParamAccess.item);

            pManager.AddCurveParameter("JointMale", "JointMale", "JointMale", GH_ParamAccess.list);
            pManager.AddCurveParameter("JointFemale", "JointFemale", "JointFemale", GH_ParamAccess.list);
            pManager.AddNumberParameter("W1", "W1", "W1", GH_ParamAccess.item,1);
            pManager.AddNumberParameter("W2", "W2", "W2", GH_ParamAccess.item, 1);
            // pManager.AddBooleanParameter("Ending", "F", "", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            //pManager[6].Optional = true;
            //pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curve", "CM", "Curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C0", "Curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C1", "Curve", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "Plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("EndPlanes", "EP", "EndPlanes", GH_ParamAccess.list);

        
            //edges
            pManager.AddIntegerParameter("ID0", "I0", "First Line for collision", GH_ParamAccess.list);
            pManager.AddCurveParameter("FacePlanes", "FP", "Face Planes", GH_ParamAccess.list);
            pManager.AddCurveParameter("CutLines", "CL", "Face Cut Lines", GH_ParamAccess.list);
            //pManager.AddVectorParameter("Vectors", "V", "Rotation Vectors", GH_ParamAccess.list);

            pManager.AddCurveParameter("Structure", "Structure", "Face Cut Lines", GH_ParamAccess.tree);

            //pManager.AddPlaneParameter("EndPlane0", "EP0", "Plane0", GH_ParamAccess.list);
            //pManager.AddPlaneParameter("EndPlane1", "EP1", "Plane1", GH_ParamAccess.list);
            //pManager.AddPlaneParameter("TopPlane", "TP", "Plane0", GH_ParamAccess.list);
            //pManager.AddPlaneParameter("BottomPlane", "BP", "Plane1", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Tenon", "T", "Tenon", GH_ParamAccess.tree);
            //pManager.AddGenericParameter("Mortise", "M", "Mortise", GH_ParamAccess.tree);

        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                Mesh M = DA.Fetch<Mesh>("Mesh");
                double A = DA.Fetch<double>("Angle");
                double S = DA.Fetch<double>("Scale");
                bool N = DA.Fetch<bool>("NormalType");
                double D = DA.Fetch<double>("Height");
                double O = DA.Fetch<double>("Offset");
                double W1 = DA.Fetch<double>("W1");
                double W2 = DA.Fetch<double>("W2");
                double T = DA.Fetch<double>("Thickness");
                Surface RS = DA.Fetch<Surface>("RoofSurface");

                List<Curve> JointMale = DA.FetchList<Curve>("JointMale");
                List<Curve> JointFemale = DA.FetchList<Curve>("JointFemale");
                JointReciprocal jm = new JointReciprocal(JointMale);
                JointReciprocal jf = new JointReciprocal(JointFemale);

                int[][] tv = M.GetNGonsTopoBoundaries();
                HashSet<int> ehash = M.GetAllNGonEdges(tv);
                int[] e = ehash.ToArray();
                int[][] ef = M.GetNgonsConnectedToNGonsEdges(ehash, false);
                Plane[] planes = M.GetNgonPlanes();



                Dictionary<int, int> eDict = new Dictionary<int, int>();
                int[][] fe = M.GetNGonFacesEdges(tv);

                int i = 0;
                foreach (int meshedge in ehash) {
                    eDict.Add(meshedge, i++);
                }

                int[][] fe_ = new int[fe.Length][];
                int[][] fe_0 = new int[fe.Length][];


                for (i = 0; i < fe.Length; i++) {
                    fe_[i] = new int[fe[i].Length];
                    fe_0[i] = new int[fe[i].Length];

                    for (int j = 0; j < fe[i].Length; j++) {
                        fe_[i][j] = eDict[fe[i][MathUtil.Wrap((j - 1), fe[i].Length)]];
                    }

                    for (int j = 0; j < fe[i].Length; j++) {
                        fe_0[i][j] = eDict[fe[i][MathUtil.Wrap((j + 0), fe[i].Length)]];
                    }



                }



                List<Vector3d> vecs = new List<Vector3d>();

                i = 0;
                foreach (int n in e) {

                    int[] edgeFaces = ef[i];

                    Vector3d vec = Vector3d.Zero;

                    if (N) {
                        for (int j = 0; j < edgeFaces.Length; j++) {
                            vec += planes[edgeFaces[j]].ZAxis;
                        }
                        vec /= edgeFaces.Length;
                        base.Message = "Average";
                    } else {

                        int[] triangleFaces = M.TopologyEdges.GetConnectedFaces(n);

                        for (int j = 0; j < triangleFaces.Length; j++) {
                            vec += M.FaceNormals[triangleFaces[j]];
                        }
                        vec /= triangleFaces.Length;
                        base.Message = "Face";

                    }


                    vecs.Add(vec);
                    i++;
                }

                Line[] lines = M.GetAllNGonEdgesLines(ehash);
                //List<Line> ln = new List<Line>();
                var a = M._EF();

                for (int j = 0; j < lines.Length; j++) {
                    //Line l = lines[j];
                    if (a[j].Length == 2) {
                        lines[j].Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S));

                        lines[j].Transform(Rhino.Geometry.Transform.Rotation(A, vecs[j], lines[j].PointAt(0.5)));

                    } else {
                        lines[j].Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S));

                        lines[j].Transform(Rhino.Geometry.Transform.Rotation(A * 0.5, vecs[j], lines[j].PointAt(0.5)));
                        lines[j].Transform(Rhino.Geometry.Transform.Rotation(-A * 0.25, Vector3d.CrossProduct(vecs[j], lines[j].Direction), lines[j].PointAt(0.5)));
                    }
                }


                //2nd part extend

                Plane[] linePlanes = new Plane[lines.Length];
                for (i = 0; i < lines.Length; i++) {
                    linePlanes[i] = new Plane(lines[i].PointAt(0.5), lines[i].Direction, vecs[i]);
                }




                Plane[][] endPlanes = new Plane[0][];
                Line[] cutLines = new Line[0];
                int[][] neighbours = new int[0][];

                DA.SetDataList(1, GetLines(lines, linePlanes, fe_, out endPlanes, out neighbours, out cutLines, D));
                DA.SetDataList(2, GetLines(lines, linePlanes, fe_, out endPlanes, out neighbours, out cutLines, -D));
                DA.SetDataList(0, GetLines(lines, linePlanes, fe_, out endPlanes, out neighbours, out cutLines));
                DA.SetDataList(3, linePlanes);
                DA.SetDataTree(4, NGonCore.GrasshopperUtil.IE2(endPlanes));

                DA.SetDataTree(5, NGonCore.GrasshopperUtil.IE2(fe_));
                DA.SetDataTree(6, NGonCore.GrasshopperUtil.IE2(FaceOutlines(M, planes, linePlanes, fe_, fe, O, T, cutLines, eDict, RS)));
                DA.SetDataList(7, cutLines);
                //DA.SetDataTree(2, NGonCore.GrasshopperUtil.IE2(fe_0));
                //DA.SetDataList(3, vecs);

                //Get Support Structure

                //Polyline[][] supportStructure = GetSupportStructure(M, W1, W2, planes, linePlanes, endPlanes, fe_0, fe, RS, eDict, ref ep0, ref ep1, ref lp2, ref lp3);
                //DA.SetDataTree(8, NGonCore.GrasshopperUtil.IE2(supportStructure));
                //DA.SetDataList(9, ep0);
                //DA.SetDataList(10, ep1);
                //DA.SetDataList(11, lp2);
                //DA.SetDataList(12, lp3);




                ////Orient joints
                //var elements = new ElementReciprocal[e.Length];

                //for (i = 0; i < e.Length; i++) {
                //    if (supportStructure[i] == null) continue;
                //    elements[i] = new ElementReciprocal(supportStructure[i]);
                //    elements[i]._joints = new JointReciprocal[] { jm.OrientCopy(Plane.WorldXY, ep0[i]), jm.OrientCopy(Plane.WorldXY, ep1[i]) };
                //    elements[i]._jointsFemale = new JointReciprocal[] { jf.OrientCopy(Plane.WorldXY, ep0[i]), jf.OrientCopy(Plane.WorldXY, ep1[i]) };
                //}


                ////orient holes
                //for (i = 0; i < neighbours.Length; i++) {
                //    for (int j = 0; j < neighbours[i].Length; j++) {
                //        if (elements[neighbours[i][j]] == null) continue;
                //        if (elements[i] == null) continue;
                //        elements[neighbours[i][j]]._jointsNei.Add(elements[i]._jointsFemale[j]);
                //    }
                //}

                //DA.SetDataTree(13, NGonCore.GrasshopperUtil.IE3(elements.GetJointsGeo()));
                //DA.SetDataTree(12, NGonCore.GrasshopperUtil.IE2(jointsMortise));
            }catch(Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }



        public Polyline[][] GetSupportStructure(Mesh M, double W1, double W2, Plane[] facePlanes, Plane[] linePlanes, Plane[][] endPlanes, int[][] fe_, int[][] fe, Surface s, Dictionary<int,int> eDict, ref Plane[] ep0, ref Plane[] ep1, ref Plane[] lp2, ref Plane[] lp3) {

            //double thickness = 1.2;
            //double heightScale = 2;


            double thickness = W1;
            double heightScale = W2*2;


            double extend = 1;
            bool flip = true;

            var allNgonE = M.GetAllNGonEdges(M.GetNGonsTopoBoundaries()).ToArray();
          

            Polyline[][] support = new Polyline[linePlanes.Length][];

            ep0 = new Plane[linePlanes.Length];//Neighbour planes0
            ep1 = new Plane[linePlanes.Length];//Neighbour planes01
            Plane[] lp0 = new Plane[linePlanes.Length];//line planes0
            Plane[] lp1 = new Plane[linePlanes.Length];//line planes1 
            lp2 = new Plane[linePlanes.Length];//line planes2 rotated 90
            lp3 = new Plane[linePlanes.Length];//line planes3 rotated 90


            for (int i = 0; i < linePlanes.Length; i++) {

                Line projectionAxis = new Line(linePlanes[i].Origin, linePlanes[i].Origin + linePlanes[i].YAxis);
                Point3d projectedPt = NGonCore.MeshUtilSimple.SurfaceRay(s, projectionAxis);

                //Six sides of each beam
                lp0[i] = linePlanes[i].MovePlanebyAxis(thickness);
                lp1[i] = linePlanes[i].MovePlanebyAxis(-thickness);
                lp2[i] = (new Plane(projectedPt, linePlanes[i].XAxis, linePlanes[i].ZAxis)).MovePlanebyAxis(heightScale);
                lp3[i] = (new Plane(projectedPt, linePlanes[i].XAxis, linePlanes[i].ZAxis)).MovePlanebyAxis(0);

                ep0[i] = endPlanes[i][0].MovePlanebyAxis(thickness, linePlanes[i].Origin, 2, flip);
                ep1[i] = endPlanes[i][1].MovePlanebyAxis(thickness, linePlanes[i].Origin, 2, flip);


                Plane endPlane0_Offset = endPlanes[i][0].MovePlanebyAxis(thickness * extend, linePlanes[i].Origin, 2, !flip);
                Plane endPlane1_Offset = endPlanes[i][1].MovePlanebyAxis(thickness * extend, linePlanes[i].Origin, 2, !flip);

                //Rectangle3d r0 = new Rectangle3d(lp2[i],10,10);
                //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(r0);

                //First outlines
                List<Plane> sidePlanes = new List<Plane>() { lp0[i], lp2[i], lp1[i], lp3[i] };
                Polyline topOutline = PolylineUtil.PolylineFromPlanes(ep0[i], sidePlanes);
                Polyline botOutline = PolylineUtil.PolylineFromPlanes(ep1[i], sidePlanes);


                ep0[i] = new Plane(topOutline.CenterPoint(), topOutline.SegmentAt(0).Direction, topOutline.SegmentAt(1).Direction);
                ep1[i] = new Plane(botOutline.CenterPoint(), botOutline.SegmentAt(0).Direction, botOutline.SegmentAt(1).Direction);


                Point3d center = (ep0[i].Origin + ep1[i].Origin)*0.5;

                if (center.DistanceToSquared(ep0[i].Origin + ep0[i].ZAxis) < center.DistanceToSquared(ep0[i].Origin)) {
                    ep0[i].Flip();
                    ep0[i].Rotate(-Math.PI * 0.5, ep0[i].ZAxis);
                }
                if (center.DistanceToSquared(ep1[i].Origin + ep1[i].ZAxis) < center.DistanceToSquared(ep1[i].Origin)) {
                    ep1[i].Flip();
                    ep1[i].Rotate(-Math.PI * 0.5, ep1[i].ZAxis);
                }



                //

                ///if (i == 0)
                // Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(ep0[i],10,10));
                if (M._ef(allNgonE[i]).Length > 1) {//Skip naked
                    // if(M.TopologyEdges.GetConnectedFaces())
                    support[i] = new Polyline[] { topOutline, botOutline };
                }

            }

            //Loop each edge
            Tuple<int, int>[][] efe = M.GetEF_LocalID(M.GetAllNGonEdges(M.GetNGonsTopoBoundaries()));

            for (int i = 0; i < linePlanes.Length; i++) {

                //int meshE = eDict[i];
                Tuple<int, int>[] edgeFaces = efe[i];
                
                //if edge has two faces go inside their local edges
                if (edgeFaces.Length == 2) {




                    int e0 = fe_[edgeFaces[0].Item1].Next(edgeFaces[0].Item2);
                    int e1 = fe_[edgeFaces[0].Item1].Prev(edgeFaces[0].Item2);
                    int e2 = fe_[edgeFaces[1].Item1].Next(edgeFaces[1].Item2);
                    int e3 = fe_[edgeFaces[1].Item1].Prev(edgeFaces[1].Item2);



                    if (i == 11 && support[i] != null) {



                        Line[] lines = PolylineUtil.LoftLine(support[i][0], support[i][1]);
                        Point3d[] pInt = PlaneUtil.LinePlane(lines, linePlanes[e0]);


                        Rectangle3d rec = new Rectangle3d(linePlanes[i], 20, 20);
                        Rectangle3d rec0 = new Rectangle3d(linePlanes[e0], 10, 10);
                        Rectangle3d rec1 = new Rectangle3d(linePlanes[e2], 10, 10);
                        //rec.pl

                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec);
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec0);
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec1);

                        //Current stick intersection
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp0[e0]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp0[e1]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp0[e2]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp0[e3]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp1[e0]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp1[e1]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp1[e2]));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(PlaneUtil.LinePlane(lines, lp1[e3]));

                        //Compare with neighbour stick intersection - the 4 points
                    }



                }



            }




                return support;

        }

        public Polyline[][] FaceOutlines(Mesh M, Plane[] facePlanes, Plane[] linePlanes,  int[][] fe_,int[][] fe, double offset, double thickness, Line[] cutLines, Dictionary<int,int> e,Surface rs) {


            Mesh projectionMesh = Mesh.CreateFromBrep(rs.ToBrep())[0];

            Polyline[][] outlines = new Polyline[facePlanes.Length][];
            Polyline[][] outlinesCopy = new Polyline[facePlanes.Length][];

            //Draw perpendicular joint
            Tuple<int, int>[][] efe = M.GetEF_LocalID(M.GetAllNGonEdges(M.GetNGonsTopoBoundaries()));

            if (offset > 0) {



                for (int i = 0; i < facePlanes.Length; i++) {

                    //Base Planes
                    Plane basePlane0 = new Plane(facePlanes[i].MovePlanebyAxis(offset * 0.5 + thickness * 0.5));
                    Plane basePlane1 = new Plane(facePlanes[i].MovePlanebyAxis(offset * 0.5 + -thickness * 0.5));
                    Plane basePlane2 = new Plane(facePlanes[i].MovePlanebyAxis(offset * -0.5 + thickness * 0.5));
                    Plane basePlane3 = new Plane(facePlanes[i].MovePlanebyAxis(offset * -0.5 + -thickness * 0.5));


                    //Side Planes
                    List<Plane> sidePlanes = new List<Plane>();

                    for (int j = 0; j < fe_[i].Length; j++) {
                        sidePlanes.Add(linePlanes[fe_[i][MathUtil.Wrap(j - 1, fe_[i].Length)]]);
                    }

                    //Intersect base plane with side planes
                    Polyline outline0 = PolylineUtil.PolylineFromPlanes(basePlane0, sidePlanes);
                    Polyline outline1 = PolylineUtil.PolylineFromPlanes(basePlane1, sidePlanes);
                    Polyline outline2 = PolylineUtil.PolylineFromPlanes(basePlane2, sidePlanes);
                    Polyline outline3 = PolylineUtil.PolylineFromPlanes(basePlane3, sidePlanes);
                    outlines[i] = new Polyline[] { outline0, outline1, outline2, outline3 };
                }



                return outlines;
            } else {

                List<Plane>[] sidePlanes = new List<Plane>[facePlanes.Length];



                for (int i = 0; i < facePlanes.Length; i++) {

                    //Base Planes
                    Plane basePlane0 = new Plane(facePlanes[i].MovePlanebyAxis(thickness * 0.5));
                    Plane basePlane1 = new Plane(facePlanes[i].MovePlanebyAxis(-thickness * 0.5));

                    //Side Planes
                    sidePlanes[i] = new List<Plane>();

                    for (int j = 0; j < fe_[i].Length; j++) {
                        sidePlanes[i].Add(linePlanes[fe_[i][j]]);
                    }

                    //Intersect base plane with side planes
                    Polyline outline0 = PolylineUtil.PolylineFromPlanes(basePlane0, sidePlanes[i]);
                    Polyline outline1 = PolylineUtil.PolylineFromPlanes(basePlane1, sidePlanes[i]);

                    Polyline outline0Copy = new Polyline(outline0);
                    Polyline outline1Copy = new Polyline(outline1);

                    outlines[i] = new Polyline[] { outline0, outline1 };
                    outlinesCopy[i] = new Polyline[] { new Polyline(outline0), new Polyline(outline1) };
                }//for i



                for (int i = 0; i < facePlanes.Length; i++) {


                    //Loop segments and check intersection 
                    for (int j = 0; j < outlines[i][0].Count - 1; j++) {

                        //Get current segment
                        Line segment0 = outlines[i][0].SegmentAt(j); // current line
                        Line segment1 = outlines[i][1].SegmentAt(j); // current line

                        //Get connected face of that segment
                        int flattenID = e[fe[i][j]];
                        Tuple<int, int>[] edgeFacesAndEdge = efe[flattenID];


                        if (edgeFacesAndEdge.Length == 2) {

                            int neiFID = (edgeFacesAndEdge[0].Item1 == i) ? 1 : 0;
                            int neiF = edgeFacesAndEdge[neiFID].Item1;
                            int nextNei = edgeFacesAndEdge[neiFID].Item2;

                            Plane planeA = sidePlanes[neiF][MathUtil.Wrap(nextNei + 2, fe[neiF].Length)];
                            Plane planeB = sidePlanes[neiF][MathUtil.Wrap(nextNei + 0, fe[neiF].Length)];
                            Plane planeC = sidePlanes[neiF][MathUtil.Wrap(nextNei + 1, fe[neiF].Length)];
                            Plane planeD = sidePlanes[neiF][MathUtil.Wrap(nextNei + 3, fe[neiF].Length)];

                            //Rhino.RhinoApp.WriteLine(fe[neiF].Length.ToString() + " " + sidePlanes[neiF].Count.ToString() + " " + MathUtil.Wrap(nextNei - 1, fe[neiF].Length).ToString());
                            int nn = MathUtil.Wrap(nextNei - 1, fe[neiF].Length);

                            //nextNei = fe[neiF][MathUtil.Wrap(nextNei - 1, fe[neiF].Length)];

                            //for(int k = 0; k < fe[neiF].Length; k++) {
                            //int nextNei = k;
                            //Rhino.RhinoApp.wr

                            if (i == 11 && j == 3) {
                                Rectangle3d rec = new Rectangle3d(planeA, 10, 10);
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec);
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(outlinesCopy[i][0]);

                                rec = new Rectangle3d(planeB, 10, 10);
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec);


                                //rec = new Rectangle3d(planeC, 10, 10);
                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rec);
 


                            }
                        //Rhino.RhinoApp.WriteLine("ww");

                        bool flag = Rhino.Geometry.Intersect.Intersection.LinePlane(segment0, planeB, out double t);

                            if (t > 0 && t < 1) {

                                Point3d neiP0 = NGonCore.PlaneUtil.LinePlane(segment0, planeB);
                                outlinesCopy[i][0].Insert((int)Math.Ceiling(outlinesCopy[i][0].ClosestParameter(neiP0)), neiP0);

                                Point3d neiP1 = NGonCore.PlaneUtil.LinePlane(segment1, planeB);
                                outlinesCopy[i][1].Insert((int)Math.Ceiling(outlinesCopy[i][1].ClosestParameter(neiP1)), neiP1);

                               //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(neiP0);
                               //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(neiP1);
                            }





                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(neiP);


                            // }

                        }


                    }

                    //if (i == 11) {
                     //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(outlinesCopy[i][0]);
                    //}
                }

                for (int i = 0; i < facePlanes.Length; i++) {
                    outlinesCopy[i][0].RemoveAt(0);
                    outlinesCopy[i][1].RemoveAt(0);

                    for (int j = 0; j < outlinesCopy[i][0].Count; j++) {
                        Line line = new Line(outlinesCopy[i][0][j], outlinesCopy[i][1][j]);
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(line);
                       Point3d pt = MeshUtilSimple.MeshRay(projectionMesh, line );
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(pt);
                       outlinesCopy[i][0][j] = pt;
                    }

                    outlinesCopy[i][0].Close();
                    outlinesCopy[i][1].Close();
                }



                    //Edge vectors
                    Vector3d[] ev = new Vector3d[efe.Length];

            }




            return outlinesCopy;
        }

        public Line[] GetLines(Line[] lines, Plane[] linePlanes, int[][] fe_,  out Plane[][] endPlanes,out int[][] neighbours, out Line[] linesCut, double move = 0) {
            int i = 0;


            Line[] linesMoved = new Line[lines.Length];
            for (i = 0; i < lines.Length; i++) {
                linesMoved[i] = lines[i];
                linesMoved[i].Transform(Rhino.Geometry.Transform.Translation(linePlanes[i].YAxis * move));

            }

            List<Point3d>[] points = new List<Point3d>[lines.Length];


            List<int>[] planeIDS = new List<int>[lines.Length];

            for (i = 0; i < linesMoved.Length; i++) {
                points[i] = new List<Point3d>();
                planeIDS[i] = new List<int>();
            }

            for (i = 0; i < fe_.Length; i++) {

                for (int j = 0; j < fe_[i].Length; j++) {

                    int current = fe_[i][j];
                    int prev = fe_[i][NGonCore.MathUtil.Wrap(j - 1, fe_[i].Length)];
                    int next = fe_[i][NGonCore.MathUtil.Wrap(j + 1, fe_[i].Length)];

                    //Rhino.RhinoApp.WriteLine(current.ToString());

                    //Rhino.RhinoApp.WriteLine(prev.ToString() + " " + next.ToString() + " " + current.ToString());

                    Point3d p0 = NGonCore.PlaneUtil.LinePlane(linesMoved[current], linePlanes[prev]);
                    Point3d p1 = NGonCore.PlaneUtil.LinePlane(linesMoved[current], linePlanes[next]);
                    points[current].Add(p0);
                    points[current].Add(p1);
                    planeIDS[current].Add(prev);
                    planeIDS[current].Add(next);
                    //Line extendedLine = (linesMoved[current].From.DistanceToSquared(p0) < linesMoved[current].From.DistanceToSquared(p1)) ? new Line(p0,p1) : new Line(p1, p0);



                }

            }
        
            Line[] linesMid = new Line[linesMoved.Length];
           linesCut = new Line[linesMoved.Length];

            endPlanes = new Plane[linesMoved.Length][];
            neighbours = new int[linesMoved.Length][];

            for (i = 0; i < linesMoved.Length; i++) {
                Line line = linesMoved[i];

                double[] t = new double[points[i].Count];
                int[] id = new int[points[i].Count];

                for (int j = 0; j < points[i].Count; j++) {
                    t[j] = line.ClosestParameter(points[i][j]);
                    //Rhino.RhinoApp.WriteLine(t[j].ToString());
                    id[j] = j;
                }

                Array.Sort(t, id);
                int s = id[0];
                int e = id[id.Length - 1];

               

                if(id.Length == 4) {
                    linesCut[i] = new Line(points[i][id[1]], points[i][id[2]]);
                } else {
                    linesCut[i] = new Line(points[i][s], points[i][e]);
                }


                linesMid[i] = new Line(points[i][s], points[i][e]);

                if(id.Length != 2 && id.Length != 4)
                    Rhino.RhinoApp.WriteLine("cc" + id.Length.ToString());
                //Rhino.RhinoApp.WriteLine(s.ToString() + " " + e.ToString() + " " + planeIDS[i][s].ToString() + " " + e.ToString());
                //Rhino.RhinoApp.WriteLine(points[i].Count.ToString() + " " + planeIDS[i].Count.ToString() );


                Plane ps = new Plane(linePlanes[planeIDS[i][s]]);
                ps.Origin = points[i][s];
                Plane pe = new Plane(linePlanes[planeIDS[i][e]]);
                pe.Origin = points[i][e];
                endPlanes[i] = new Plane[] { ps,pe };
                neighbours[i] = new int[] { planeIDS[i][s], planeIDS[i][e] };

            }



                return linesMid;
        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.Reciprocal3;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-1489a5ee1f14"); }
        }
    }
}