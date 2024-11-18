using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace NGon_RH8.Utils {
    public class RecipricalTranslationRAW : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalTranslationRAW()
          : base("ReciRaw", "ReciRaw",
              "ReciRaw",
              "NGon", "Reciprocal") {
       
            }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddGenericParameter("MeshProp", "Prop", "Mesh Properties", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "Dist", "Distance", GH_ParamAccess.list);
            pManager.AddNumberParameter("Offset", "Offset", "Offset", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Cut", "Cut", "Cut", GH_ParamAccess.list);
            pManager.AddNumberParameter("Extend", "Extend", "Extend", GH_ParamAccess.item);
            pManager.AddBoxParameter("Box", "Box", "Box", GH_ParamAccess.item);
            pManager.AddNumberParameter("BoxT", "BoxT", "BoxT", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            pManager.AddLineParameter("Lines", "L", "Line", GH_ParamAccess.tree);
            pManager.AddLineParameter("Ecc", "Ecc", "Line Eccentricities", GH_ParamAccess.tree);
            //pManager.AddGenericParameter("Nexors", "Nexors", "Nexors", GH_ParamAccess.item);
            //pManager.AddGenericParameter("MeshProp", "Prop", "Mesh Properties", GH_ParamAccess.item);
            pManager.AddLineParameter("LinesN", "LNaked", "Line", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Plines", "Plines", "Plines", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Profiles", "Profiles", "Profiles", GH_ParamAccess.tree);
         
            pManager.AddCurveParameter("Membrane", "Membrane", "Membrane", GH_ParamAccess.tree);
           pManager.AddPlaneParameter("Planes", "Planes", "Planes", GH_ParamAccess.tree);
            pManager.AddLineParameter("VerticalLines", "VerticalLines", "VerticalLines", GH_ParamAccess.tree);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {


            MeshProps meshProps = DA.Fetch<MeshProps>("MeshProp");
            List<double> D = DA.FetchList<double>("Dist");
            List<double> Offset = DA.FetchList<double>("Offset");
            List<Plane> planes = DA.FetchList<Plane>("Cut");
            double extend = DA.Fetch<double>("Extend");
            Box box = DA.Fetch<Box>("Box");
            double boxT = DA.Fetch<double>("BoxT");

            try {
                double firstDist = (D.Count > 0) ? D[0] : 0.01;
                double[][] DArray = new double[][] { new double[1] { D[0] } };


                if (D.Count > 0) {
                    DArray = new double[meshProps.FEFlatten.Length][];
                    for (int i = 0; i < meshProps.FEFlatten.Length; i++)
                        DArray[i] = new double[meshProps.FEFlatten[i].Length];

                    for (int i = 0; i < meshProps._FlattenFE.Count; i++)
                        DArray[meshProps._FlattenFE[i][0]][meshProps._FlattenFE[i][1]] = D[i % D.Count];
                }


                //Compute translation
                //Create Nexors
                NGonCore.Nexorades.Nexors nexors = new NGonCore.Nexorades.Nexors();
                for (int i = 0; i < meshProps.M._countF(); i++) {
                    //Rhino.RhinoApp.WriteLine(meshProps.faceColors[i].ToString());
                    for (int j = 0; j < meshProps.M._countE(i); j++) {
                        nexors.Add(meshProps.EFLines[i][j], i, j);
                        nexors[i, j].translation = DArray[i][j];

                        nexors[i, j].direction = meshProps.faceColors[i] == 0;

                    }
                }



                Line[][] translatedLines = meshProps.NexorTranslateLines(ref nexors, DArray);

                //nexors.Beams2(meshProps);
                //DA.SetDataTree(0, nexors0.Beams2(meshProps0));
                //DA.SetDataTree(1, nexors0.Panels(meshProps0));
                //nexors0.RoundBeams(meshProps0, 0.06);
                //DA.SetDataTree(2, nexors0.GetNexorPipesBreps());
                //DA.SetDataTree(3, nexors0.Cuts());
                //DA.SetDataTree(4, nexors0.GetCNC_Cuts());


               
                //Output
                DA.SetDataTree(0, nexors.GetNexorLines());
                DA.SetDataTree(1, nexors.GetNexorEccentricities());
                //DA.SetData(2, nexors);
                //DA.SetData(3, meshProps);
                DA.SetDataTree(2, nexors.GetNexorLines(-1));
                DA.SetDataTree(3, nexors.GetNexorPlines());


                var BeamsAndMemrane = nexors.BeamsFlatAndMembrane(meshProps, Offset);
                var axes = BeamsAndMemrane.Item1;
                var memebrane = BeamsAndMemrane.Item2;
                var axesPlanes = nexors.GetNexorCenterPlanes();

                for (int i = 0; i < axes.BranchCount; i++) {

                    if (axesPlanes.Branch(i).Count < 1) continue;
                    Plane plane = new Plane(axesPlanes.Branch(i)[0]);
                    if (!plane.IsValid) continue;

                    axesPlanes.Branch(i).Clear();
                    for (int j = 0; j < axes.Branch(i).Count; j++) {
                        Line l = axes.Branch(i)[j].ToLine();
                        if (l.IsValid && l != Line.Unset) {
                            //axesPlanes.Branch(i)[0] = axesPlanes.Branch(i)[0].ChangeOrigin(l.PointAt(0.5));
                            axesPlanes.Add(plane.ChangeOrigin(l.PointAt(0.5)),new GH_Path(axes.Paths[i]));
                          
                        }



                    }

                }






                //Extend
                for (int i = 0; i < axes.BranchCount; i++) {

                    for (int j = 0; j < axes.Branch(i).Count; j++) {
                        Line l = axes.Branch(i)[j].ToLine();
                        l.Extend(extend, extend);
                        axes.Branch(i)[j] = l.ToP();

                    }

                }

                //Cut by planes
                if (planes.Count > 0) {
                    Interval interval = new Interval(0, 1);
                    var axesCropped = new Grasshopper.DataTree<Line>();
                    foreach (Plane plane in planes) {
                        Point3d p0 = plane.Origin;
                        Point3d p1 = p0 + plane.ZAxis * 0.01;



                        for (int i = 0; i < axes.BranchCount; i++) {

                            for (int j = 0; j < axes.Branch(i).Count; j++) {

                                if (!axes.Branch(i)[j].IsValid && axes.Branch(i)[j].Length < 0.01) continue;

                                Polyline pline = new Polyline();

                                Line l = axes.Branch(i)[j].ToLine();

                                bool flag = Rhino.Geometry.Intersect.Intersection.LinePlane(l, plane, out double t);
                                Point3d p = l.PointAt(t);

                                if (flag && interval.IncludesParameter(t)) {

                                } else {
                                    flag = false;
                                }



                                if (p1.DistanceToSquared(l.From) < p0.DistanceToSquared(l.From))
                                    pline.Add(l.From);
                                if (flag)
                                    pline.Add(p);
                                //if (!flag) {
                                if (p1.DistanceToSquared(l.To) < p0.DistanceToSquared(l.To))
                                    pline.Add(l.To);
                                //}
                                axes.Branch(i)[j] = pline.Count > 1 ? new Polyline(new[] { pline[0], pline[pline.Count - 1] }) : new Polyline();//


                            }

                        }

                    }


                    //Extend boundary elements
                    if (box.IsValid) {
                        for (int i = 0; i < axes.BranchCount; i++) {

                            for (int j = 0; j < axes.Branch(i).Count; j++) {
                                if (axes.Branch(i)[j].Count<1) {
                                   axesPlanes.Branch(i)[j] = Plane.Unset;
                                   continue;

                                }
                                //if (l == null) {
                                //    //axesPlanes.Branch(i)[j] = Plane.Unset;
                                //    continue;
                                //}
                                Line l = axes.Branch(i)[j].ToLine();


                                Rhino.Geometry.Intersect.Intersection.LineBox(l, box, 0.01, out Interval d);
                                Point3d p0 = l.PointAt(d.Min);
                                Point3d p1 = l.PointAt(d.Max);
                               

                                if (l.From.DistanceTo(p0) < boxT)
                                    l.From = p0;

                                if (l.To.DistanceTo(p1) < boxT)
                                    l.To = p1;
                                axes.Branch(i)[j] = l.ToP();

                            }

                        }
                    }


                }


                    DA.SetDataTree(4, axes);
                    DA.SetDataTree(5, BeamsAndMemrane.Item2);
                DA.SetDataTree(6, axesPlanes);


            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }





        }

        public NGonCore.Nexorades.Nexors GetNexors(double[][]  DArray, MeshProps meshProps) {




            //Compute translation
            //Create Nexors
            NGonCore.Nexorades.Nexors nexors = new NGonCore.Nexorades.Nexors();
            for (int i = 0; i < meshProps.M._countF(); i++) {
    
                for (int j = 0; j < meshProps.M._countE(i); j++) {
                    nexors.Add(meshProps.EFLines[i][j], i, j);
                    nexors[i, j].translation = DArray[i][j];
                    nexors[i, j].direction = meshProps.faceColors[i] == 0;
              
                   //= DArray[i][j];
                }
            }


            return nexors;

        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return  Properties.Resources.ReciprocalRaw;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c40b-4bf1-b2b4-1423a5ee4f04"); }
        }
    }
}