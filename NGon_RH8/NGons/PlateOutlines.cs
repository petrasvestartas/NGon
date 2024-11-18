using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.NGons {
    public class PlateOutlines : GH_Component_NGon {
        public PlateOutlines()
            : base("AddNGons", "AddNGons",
                "Add NGons to Planar triangular faces",
                "NGons") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Pair", "P", "Find a Pair, 0 - all flat polygons, 1 - pair, 2 - sorted pair", GH_ParamAccess.item, 0);
            pManager[1].Optional = true;
            //pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            //pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("VertexID", "V", "Naked edge vertex id", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Polylines", "P", "Naked edge polylines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Polylines", "P", "Naked edge polylines", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            DA.GetData(0, ref m);
            int pair = 0;
            DA.GetData(1,ref pair);

            if (m.IsValid) {



                ////////////////////////////////
                //Add Mesh Ngons
                ////////////////////////////////
                Mesh mC = m.DuplicateMesh();
                //mC.Ngons.AddPlanarNgons(0.01, 0, 0, true);
                mC.Ngons.AddPlanarNgons(0.01, 3, 3, true);
                double[] areas = new double[mC.Ngons.Count];
                int[] id = Enumerable.Range(0, areas.Length).ToArray();


                //Get areas of NGons
                for (int i = 0; i < areas.Length; i++) {
                    areas[i] = 0;
                    uint[] faces = mC.Ngons[i].FaceIndexList();
                    for (int j = 0; j < mC.Ngons[i].FaceCount; j++) {
                        double area = NGonCore.MeshUtilSimple.FaceArea(mC.Vertices, mC.Faces[(int)faces[j]]);
                        areas[i] += area;
                    }
                }

                //Sort areas from largest
                Array.Sort(areas, id);
                Array.Reverse(areas);
                Array.Reverse(id);

                if (pair >0) {
                    //Sort Area so that they are oppossite to each other
                    //Get face adjacency
                    HashSet<int> NGonCurrentAndAdjacentFaces = new HashSet<int>();
                    foreach (int e in mC._fe(id[0]))
                    {
                        int[] faces = mC.TopologyEdges.GetConnectedFaces(e);
                      
                        foreach (int f in faces)
                        {
                            NGonCurrentAndAdjacentFaces.Add(f);
                            //Rhino.RhinoApp.WriteLine(f.ToString());
                        }
                    }

                   int[] id_ = new int[] { id[0], id[0] };
                    for (int i = 1; i < id.Length; i++)
                    {
                        bool isNeighbour = false;
                        foreach (var f in mC.Ngons[id[i]].FaceIndexList())
                            if (NGonCurrentAndAdjacentFaces.Contains((int)f))
                            {
                                isNeighbour = true;
                                break;
                            }

                        if (isNeighbour)
                            continue;
                        else
                        {
                            id_[1] = id[i];
                            break;
                        }

                    }
                    id = id_;
                }
            
                //Take outlines of ngons

                var dt = new DataTree<int>();
                var dtPlines = new DataTree<Polyline>();

                int c = 0;
                mC.FaceNormals.ComputeFaceNormals();
                Plane plane = Plane.Unset;
                Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Unset;
                Rhino.Geometry.Transform xformInv = Rhino.Geometry.Transform.Unset;


                //Get orientation plane
                if (c == 0 && pair > 1) {
                    int oneFace = (int)mC.Ngons[id[0]].FaceIndexList()[0];
                    plane = new Plane(mC.MeshFaceCenter(oneFace), -mC.FaceNormals[oneFace]);
                    xform = Rhino.Geometry.Transform.PlaneToPlane(plane, Plane.WorldXY);
                    xformInv = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, plane);
                    mC.Transform(xform);
                }

          



                foreach (int id_ in id) {

                    Rhino.Geometry.MeshNgon ngon = mC.Ngons[id_];

                    if (mC.Ngons.NgonHasHoles(id_)) {
                        //dt.AddRange(ids.Value, new GH_Path(c));
                        Mesh mCC = mC.DuplicateMesh();
              
                        uint[] faces = ngon.FaceIndexList();
                        Array.Sort(faces);



                        int faceCounter = 0;
                        foreach (int f in faces) {
                            mCC.Faces.RemoveAt(f - faceCounter);
                            faceCounter++;
                        }
                  


                        //Get Outlines
                        mCC = mCC.WeldUsingRTree(0.01);// weld else wont work with removed faces
                     
                        List<List<int>> nakedVertices = mCC.GetNakedVerticesID(1000);
                        List<Polyline> nakedPolylines = mCC.GetNakedPolylines(nakedVertices);
             
                        dtPlines.AddRange(nakedPolylines, new GH_Path(DA.Iteration,c));//c


                    } else {
                        uint[] vertices = ngon.BoundaryVertexIndexList();
                        Polyline pline = new Polyline();
                        for (int i = 0; i < vertices.Length; i++)
                            pline.Add(mC.Vertices[(int)vertices[i]]);

                        pline.Add(pline[0]);
                        dtPlines.Add(pline, new GH_Path(DA.Iteration, c));

                    }


                    if (c == 1 && pair>0)
                        break;
                    c++;


                }

                if (pair==2)
                {//&& false


                    for (int i = 0; i < dtPlines.BranchCount; i++) {
                        for (int j = 0; j < dtPlines.Branch(i).Count; j++) {
                            if (PolylineUtil.IsClockwiseClosedPolylineOnXYPlane(dtPlines.Branch(i)[j])) {
                                dtPlines.Branch(i)[j].Reverse();
                            }
                        }
                    }

                    var sortedPlines = PolylineUtil.FindPairsPlines(dtPlines.AllData(),true);
   
                    var dtPlines0 = new DataTree<Polyline>();
                    var dtPlines1 = new DataTree<Polyline>();
                    //Rhino.RhinoApp.WriteLine();
                    for (int i = 0; i < sortedPlines.Count; i++) {

                        //Rhino.RhinoApp.Write( ((int)sortedPlines[i][1].ClosestParameter(sortedPlines[i][0].PointAt(0))).ToString()+ "     ");

                        int[] sortValues = new int[sortedPlines[i][0].Count];
                        int[] sortValuesID = new int[sortedPlines[i][0].Count];
                        for (int j = 0; j < sortedPlines[i][0].Count; j++) {
                          
                            int cp = sortedPlines[i][1].ClosestIndex(sortedPlines[i][0].PointAt(j));
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(sortedPlines[i][1].PointAt(cp));
                            //int closesID = (int)Math.Round( Math.Abs(cp - j),0);
                            //Rhino.RhinoApp.Write(cp.ToString() + " " + j.ToString() + " "+MathUtil.Wrap(j-cp, sortedPlines[i][0].Count-1) + "     " 
                            int shiftValue = MathUtil.Wrap(j - cp, sortedPlines[i][0].Count - 1);
                            sortValues[shiftValue]++;
                            sortValuesID[j] = j;
                        }

                        //for (int j = 0; j < sortedPlines[i][0].Count; j++) {
                        //    Rhino.RhinoApp.WriteLine(sortValues[j].ToString());
                        //}
              
                        Array.Sort(sortValues, sortValuesID);
                        Array.Reverse(sortValues);
                        Array.Reverse(sortValuesID);
             

                        //Rhino.RhinoApp.WriteLine();
                        int cp0 = sortedPlines[i][1].ClosestIndex(sortedPlines[i][0].PointAt(0));
                        int shiftN = MathUtil.Wrap(0 - cp0, sortedPlines[i][0].Count - 1);
                        shiftN = sortValuesID[0];
                       sortedPlines[i][1].ShiftPolyline(shiftN);
                        //Rhino.RhinoApp.WriteLine(sortValuesID[0].ToString() + " " + shiftN.ToString());

                        sortedPlines[i][0].Transform(xformInv);
                        sortedPlines[i][1].Transform(xformInv);
                        dtPlines0.Add(sortedPlines[i][0], new GH_Path(DA.Iteration));
                        dtPlines1.Add(sortedPlines[i][1], new GH_Path(DA.Iteration));
                    }
                    mC.Transform(xformInv);

                    DA.SetDataTree(0, dtPlines0);
                    DA.SetDataTree(1, dtPlines1);
                    var plinesDisplay = new List<Polyline>();
                    plinesDisplay.AddRange(dtPlines0.AllData());
                    plinesDisplay.AddRange(dtPlines1.AllData());
                    this.PreparePreview(mC, DA.Iteration, plinesDisplay, false);
                }
                else{

                    DA.SetDataTree(0, dtPlines);
                    this.PreparePreview(mC, DA.Iteration, dtPlines.AllData(), true);
                }
                //mC.Ngons.AddPlanarNgons(0.01);
                DA.SetData(2, mC);

                //    A = areas;
                //    B = mC.Ngons[id[0]].BoundaryVertexIndexList();



            } else {
                //DA.SetData(0, m);
                DA.SetDataTree(0, new Grasshopper.DataTree<Polyline>());
            }

        }



        protected override System.Drawing.Bitmap Icon => Properties.Resources.PlanarNGons;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b111aa011d14}");



    }
}