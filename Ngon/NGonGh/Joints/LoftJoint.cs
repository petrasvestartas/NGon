using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using NGonsCore;

namespace SubD.Joints {
    public class LoftJoint : GH_Component {

        public LoftJoint()
          : base("LoftJoint", "LoftJoint",
              "Create Offset plates with joints from two closed polylines",
              "NGon", "Util") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddCurveParameter("Curve 0", "C0", "First Curve, if no curves are supplied, then mesh is used as input", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Curve 1", "C1", "Second Curve, if no curves are supplied, then mesh is used as input", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh 0", "M0", "Mesh0", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh 1", "M1", "Mesh1", GH_ParamAccess.item);

            pManager.AddIntegerParameter("SideDivisions", "SideDivisions", "SideDivisions", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Panel Width", "Panel Width", "Panel Width", GH_ParamAccess.item, 0.01);
            pManager.AddIntegerParameter("Divisions", "Divisions", "Divisions", GH_ParamAccess.item, 2);
            pManager.AddBooleanParameter("CornerType", "CornerType", "CornerType", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Parallel", "Parallel", "Parallel", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Screws", "Screws", "Screws", GH_ParamAccess.item, true);

            for (int i = 0; i < pManager.ParamCount; i++)
                pManager[i].Optional = true; 

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddGenericParameter("MeshBox", "MeshBox", "MeshBox", GH_ParamAccess.list);

            pManager.AddGenericParameter("Top Planes", "Top Planes", "Top Planes", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Side Planes", "Side Planes", "Side Planes", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Top Outlines", "Top Outlines", "Top Outlines", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Side Outlines", "MeshBox", "MeshBox", GH_ParamAccess.tree);

            pManager.AddGenericParameter("A", "A", "A", GH_ParamAccess.tree);
            pManager.AddGenericParameter("A", "A", "A", GH_ParamAccess.tree);
            pManager.AddGenericParameter("A", "A", "A", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                //Retrieve parameters

                GH_Structure<GH_Curve> c0 = new GH_Structure<GH_Curve>();
                GH_Structure<GH_Curve> c1 = new GH_Structure<GH_Curve>();
                Mesh mesh0 = new Mesh();
                Mesh mesh1 = new Mesh();
                int sideDivisions = 2;
                double planeWidth = 0.01;
                int divisions = 2;
                bool cornerType = true;
                double parallel = 0.0;
                bool screws = true;

                DA.GetDataTree<GH_Curve>(0, out c0);
                DA.GetDataTree<GH_Curve>(1, out c1);
                DA.GetData(2, ref mesh0);
                DA.GetData(3, ref mesh1);
                DA.GetData(4, ref sideDivisions);
                DA.GetData(5, ref planeWidth);
                DA.GetData(6, ref divisions);
                DA.GetData(7, ref cornerType);
                DA.GetData(8, ref parallel);
                DA.GetData(9, ref screws);


                //Use curves as input
                if (c0.DataCount > 0 && (c0.DataCount == c1.DataCount)) {

                    List<Polyline> outlines0 = new List<Polyline>();
                    List<Polyline> outlines1 = new List<Polyline>();

                    var c0Enumerator = c0.GetEnumerator();
                    var c1Enumerator = c1.GetEnumerator();

                    while (c0Enumerator.MoveNext() && c1Enumerator.MoveNext()) {
                        Object curve0 = c0Enumerator.Current;
                        Object curve1 = c1Enumerator.Current;
                    }


                    int n = c0.DataCount;


                    foreach (GH_Curve ghc in c0.AllData(true)) {
                        ghc.Value.TryGetPolyline(out Polyline p);
                        outlines0.Add(p);
                    }

                    foreach (GH_Curve ghc in c1.AllData(true)) {
                        ghc.Value.TryGetPolyline(out Polyline p);
                        outlines1.Add(p);
                    }


                    //2.0 Setup theBox class

                    TheBox[] boxes = new TheBox[n];

                    Plane[][] sidePlanes = new Plane[n][];
                    Plane[][] topPlanes = new Plane[n][];
                    Polyline[][][] midOutlines0 = new Polyline[n][][];
                    Polyline[][][] sideOutlines0 = new Polyline[n][][];

                    Polyline[][][] sideOutlines1 = new Polyline[n][][];
                    Plane[][][] edgePlanes = new Plane[n][][];

                    for (int i = 0; i < n; i++) {
                        boxes[i] = new TheBox(outlines0[i], outlines1[i], planeWidth, new List<double> { 0.25,0.75 }, cornerType, parallel, divisions, i);

                        boxes[i].ConstructPlanes();
                        sidePlanes[i] = boxes[i].sidePlanes;
                        topPlanes[i] = boxes[i].topPlanes;

                        boxes[i].ConstructOutlines();
                        sideOutlines0[i] = boxes[i].sideOutlines0;
                        sideOutlines1[i] = boxes[i].sideOutlines1;
                        midOutlines0[i] = boxes[i].midOutlines0;

                        if (sideDivisions > 1) {
                            boxes[i].ConstructJointsEdgeJoints(sideDivisions, -20);
                            edgePlanes[i] = boxes[i].sidePlanesR90SideJoints;
                        }
                    }

                    DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IE2(sidePlanes));
                    DA.SetDataTree(2, NGonsCore.GrasshopperUtil.IE2(topPlanes));
                    DA.SetDataTree(3, NGonsCore.GrasshopperUtil.IE3(sideOutlines0));
                    DA.SetDataTree(4, NGonsCore.GrasshopperUtil.IE3(sideOutlines1));
                    DA.SetDataTree(5, NGonsCore.GrasshopperUtil.IE3(midOutlines0));
                    DA.SetDataTree(6, NGonsCore.GrasshopperUtil.IE3(edgePlanes));


                    //Use meshes as input with its adjacency
                } else if (mesh0.Ngons.Count == mesh1.Ngons.Count) {

                    int n = mesh0.Ngons.Count;

                    Mesh[] meshBoxes = mesh0.ProjectPairsMesh(mesh1, 1);
                    DA.SetDataList(0, meshBoxes);

                    //1.0 Get top and side polylines from boxes
                    Polyline[][] meshBoxesOutlinesTop = new Polyline[n][];
                    Polyline[][] meshBoxesOutlinesSide = new Polyline[n][];

                    for (int i = 0; i < n; i++) {
                        Polyline[] outlines = meshBoxes[i].GetPolylines();
                        meshBoxesOutlinesSide[i] = new Polyline[outlines.Length - 2];
                        Array.Copy(outlines, meshBoxesOutlinesSide[i], outlines.Length - 2);
                        meshBoxesOutlinesTop[i] = new Polyline[2] { outlines[outlines.Length - 1], outlines[outlines.Length - 2] };
                    }

                    //2.0 Setup theBox class

                    TheBox[] boxes = new TheBox[n];

                    Plane[][] sidePlanes = new Plane[n][];
                    Plane[][] topPlanes = new Plane[n][];

                    Polyline[][][] midOutlines0 = new Polyline[n][][];
                    Polyline[][][] sideOutlines0 = new Polyline[n][][];

                    Polyline[][][] misOutlinesJoints = new Polyline[n][][];//Repesent mid panels with finger joints
                    List<Polyline>[][] sideOutlinesJoints0 = new List<Polyline>[n][];//n - number of sides
                    List<Polyline>[][] sideOutlinesJoints1 = new List<Polyline>[n][];//n - number of sides

                    for (int i = 0; i < n; i++) {
                        boxes[i] = new TheBox(meshBoxesOutlinesTop[i][0], meshBoxesOutlinesTop[i][1], planeWidth, new List<double> { 0.5 }, cornerType, parallel, divisions + 1, i);

                        boxes[i].ConstructPlanes();
                        boxes[i].ConstructOutlines();
                        boxes[i].ConstructJoints();
                        boxes[i].ConstructJointsLines();

                        sidePlanes[i] = boxes[i].sidePlanes;
                        topPlanes[i] = boxes[i].topPlanes;

                        sideOutlines0[i] = boxes[i].sideOutlines0;
                        midOutlines0[i] = boxes[i].midOutlines0;

                        misOutlinesJoints[i] = boxes[i].misOutlinesJoints;
                        sideOutlinesJoints0[i] = boxes[i].sideOutlinesJoints0;
                        sideOutlinesJoints1[i] = boxes[i].sideOutlinesJoints1;
                    }

                    //3.0 Setup neighbours
                    if (screws) {
                        int[][] e = mesh0.GetNGonsTopoBoundaries();
                        HashSet<int> allE = mesh0.GetAllNGonEdges(e);
                        int[] allEArray = allE.ToArray();
                        int[][] eNgons = mesh0.GetNgonsConnectedToNGonsEdges(allE);
                        int[][] ne = mesh0.GetNGonFacesEdges(e);

                        List<Line>[][] screws_ = new List<Line>[n][];//n - number of sides

                        //Loop through ngons
                        for (int i = 0; i < n; i++) {

                            screws_[i] = new List<Line>[ne[i].Length];

                            //Loop through negon edges
                            for (int j = 0; j < ne[i].Length; j++) {

                                screws_[i][j] = new List<Line>();

                                //if not naked
                                if (mesh0.TopologyEdges.GetConnectedFaces(ne[i][j]).Length > 1) {

                                    //Get currnet ngon line

                                    // int prev = NGonsCore.MathUtil.Wrap(j-1,ne[i].Length);

                                    Line ln0 = boxes[i].jointLines[j][0];
                                    Line ln0_ = boxes[i].jointLines[j][1];

                                    ////Get neighbour ngon line
                                    //First get edge ngons
                                    int eID = Array.IndexOf(allEArray, ne[i][j]);
                                    int[] currentNGons = eNgons[eID];
                                    int neiN = (currentNGons[0] == i) ? currentNGons[1] : currentNGons[0];//neighbour ngon


                                    //Local edge id of neighbour polygon
                                    int neiEID = Array.IndexOf(ne[neiN], ne[i][j]);
                                    //int prevNei = NGonsCore.MathUtil.Wrap(j+1,ne[neiN].Length);
                                    Line ln1 = boxes[neiN].jointLines[neiEID][0];
                                    Line ln1_ = boxes[neiN].jointLines[neiEID][1];

                                    // Polyline polyline = new Polyline(){boxes[i].c0,ln0.PointAt(0.5),boxes[neiN].c0 };
                                    //Polyline polyline = new Polyline(){ln0.PointAt(0.5),0.5*(ln0.PointAt(0.5)+ln1.PointAt(0.5)) };

                                    Point3d[] intp0 = NGonsCore.PointUtil.InterpolatePoints(ln0.PointAt(0.2), ln0.PointAt(0.8), 0);
                                    Point3d[] intp1 = NGonsCore.PointUtil.InterpolatePoints(ln1.PointAt(0.8), ln1.PointAt(0.2), 0);
                                    Point3d[] intp0_ = NGonsCore.PointUtil.InterpolatePoints(ln0_.PointAt(0.2), ln0_.PointAt(0.8), 0);
                                    Point3d[] intp1_ = NGonsCore.PointUtil.InterpolatePoints(ln1_.PointAt(0.8), ln1_.PointAt(0.2), 0);


                                    for (int k = 0; k < intp0.Length; k++) {
                                        Vector3d vec = intp0[k] - intp1[k];
                                        vec.Unitize();
                                        vec *= 0.01;
                                        Line joint = new Line(intp0[k] + vec, 0.5 * (intp0[k] + intp1[k]));
                                        screws_[i][j].Add(joint);
                                    }

                                    for (int k = 0; k < intp0.Length; k++) {
                                        Vector3d vec = intp0_[k] - intp1_[k];
                                        vec.Unitize();
                                        vec *= 0.01;
                                        Line joint = new Line(intp0_[k] + vec, 0.5 * (intp0_[k] + intp1_[k]));
                                        screws_[i][j].Add(joint);
                                    }


                                }
                            }
                        }
                        DA.SetDataTree(6, NGonsCore.GrasshopperUtil.IE3(screws_, 0));
                    }//if screws

                    DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IE2(sidePlanes, 0));
                    DA.SetDataTree(2, NGonsCore.GrasshopperUtil.IE2(topPlanes, 0));
                    DA.SetDataTree(3, NGonsCore.GrasshopperUtil.IE3(misOutlinesJoints, 0));
                    DA.SetDataTree(4, NGonsCore.GrasshopperUtil.IE3(sideOutlinesJoints0, 0));
                    DA.SetDataTree(5, NGonsCore.GrasshopperUtil.IE3(sideOutlinesJoints1, 0));


                }
            }catch(Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }


        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return Properties.Resources.LoftJoints;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("f935073e-db7a-4acf-8a96-a70cbc7fef77"); }
        }
    }
}