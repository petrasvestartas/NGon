using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Modifications
{
    public class OffsetMeshVDA : GH_Component_NGon
    {
        public OffsetMeshVDA()
          : base("Offset Projected", "Offset Projected",
              "Offset mesh using mesh offset or mesh plane intersections",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset", "O", "Offset dist", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Type", "T", "Type of offset, 0 - plane intersection 1 - mesh offset and project", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Dist2", "W", "For plate thickness works only with type 0", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Tol", "T", "Angle Tolerance", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Custom", "C", "Custom", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.list);
            pManager.AddCurveParameter("ProjectedPolylines", "C", "Mesh to offset", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Thickness", "C", "Mesh to offset", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("EdgePlanes", "P", "Mesh to offset", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            try {
                Mesh M = DA.Fetch<Mesh>("Mesh");
                double D = DA.Fetch<double>("Offset");
                int T = DA.Fetch<int>("Type");
                double W = DA.Fetch<double>("Dist2");
                double Tol = DA.Fetch<double>("Tol");
                var mid = DA.FetchList<int>("Custom");


                bool P = true;
                Polyline[] p0;
                Polyline[] p1;
                DataTree<Polyline> dt = new DataTree<Polyline>();
                DataTree<Polyline> dtThickness = new DataTree<Polyline>();
                if (D == 0) {



                    switch (T) {

                        case (1):

                        base.Message = "No Offset \n Projection";
                        //output
                        dt = new DataTree<Polyline>();
                        var polygons = M.ProjectedPolylinesToAveragePlane(P);
                        for (int i = 0; i < M.Ngons.Count; i++) {
                            dt.Add(polygons[i], new GH_Path(DA.Iteration, i));
                        }

                        break;

                        default:

                        base.Message = "No Offset \n FaceEdgeBisector";

                        //Get edge planes and corner bisectors *----*----*
                        Plane[][] bisePlanes;
                        Plane[][] edgePlanes;
                        M.GetEdgeAndBisectorPlanes(out bisePlanes, out edgePlanes);
                        dt = new DataTree<Polyline>();


                        //Get Outlines
                        p0 = M.GetPolylines();
                        for (int i = 0; i < M.Ngons.Count; i++) {
                            Plane plane = p0[i].GetPlane(P);
                            dt.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));

                            if (W > 0) {
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(W);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                            }

                        }

                        break;
                    }
                    DA.SetDataList(0, new Mesh[] { M });
                } else {

                    //Offset two meshes in both sides
                    Mesh m0 = M.DuplicateMesh();
                    Mesh m1 = M.DuplicateMesh();

                    m0 = m0.Offset(D * 0.5);
                    m1 = m1.Offset(-D * 0.5);


                    switch (T) {

                        case (1):
                        //case (2):

                        base.Message = "Offset \n Projection";



                        //output
                        Mesh mA = M.DuplicateMesh();
                        Mesh mB = M.DuplicateMesh();
                        Mesh mC = M.DuplicateMesh();
                        Mesh mD = M.DuplicateMesh();
                        Mesh mA_ = M.DuplicateMesh();
                        Mesh mB_ = M.DuplicateMesh();

                        Mesh mE = M.DuplicateMesh();
                        Mesh mF = M.DuplicateMesh();

                        mA = mA.Offset(-D * 0.5 + W * 0.5);
                        mB = mB.Offset(-D * 0.5 - W * 0.5);
                        mC = mC.Offset(D * 0.5 + W * 0.5);
                        mD = mD.Offset(D * 0.5 - W * 0.5);
                        mA_ = mA_.Offset(-D * 0.5);
                        mB_ = mB_.Offset(D * 0.5);
                        mE = mE.Offset(W * 0.5);
                        mF = mF.Offset(-W * 0.5);


                        dtThickness = new DataTree<Polyline>();
                        dt = new DataTree<Polyline>();

                        //Rhino.RhinoApp.WriteLine("Hi");

                        var a = mA.ProjectedPolylinesToAveragePlane(P);
                        var b = mB.ProjectedPolylinesToAveragePlane(P);
                        var c = mC.ProjectedPolylinesToAveragePlane(P);
                        var d = mD.ProjectedPolylinesToAveragePlane(P);
                        var a_ = mA_.ProjectedPolylinesToAveragePlane(P);
                        var b_ = mB_.ProjectedPolylinesToAveragePlane(P);
                        var e = mE.ProjectedPolylinesToAveragePlane(P);
                        var f = mF.ProjectedPolylinesToAveragePlane(P);

                        for (int i = 0; i < a.Length; i++) {
                            dtThickness.Add(a[i], new GH_Path(DA.Iteration, i));
                            dtThickness.Add(b[i], new GH_Path(DA.Iteration, i));

                            if (mid.Contains(i)) {
                                dtThickness.Add(e[i], new GH_Path(DA.Iteration, i));
                                dtThickness.Add(f[i], new GH_Path(DA.Iteration, i));
                            }
                            dtThickness.Add(c[i], new GH_Path(DA.Iteration, i));
                            dtThickness.Add(d[i], new GH_Path(DA.Iteration, i));


                            dt.Add(a_[i], new GH_Path(DA.Iteration, i));
                            dt.Add(b_[i], new GH_Path(DA.Iteration, i));
                        }

                        break;

                        default:

                        base.Message = "Offset \n FaceEdgeBisector";

                        //Get edge planes and corner bisectors *----*----*
                        Plane[][] bisePlanes;
                        Plane[][] edgePlanes;
                        M.GetEdgeAndBisectorPlanes(out bisePlanes, out edgePlanes);

                        DataTree<Plane> pls = new DataTree<Plane>();

                        for (int i = 0; i < bisePlanes.Length; i++) {
                            pls.AddRange(bisePlanes[i], new GH_Path(DA.Iteration, 0, i));
                            pls.AddRange(edgePlanes[i], new GH_Path(DA.Iteration, 1, i));
                        }
                        //EdgePlanes = pls;
                        DA.SetDataTree(3, pls);

                        dt = new DataTree<Polyline>();
                        dtThickness = new DataTree<Polyline>();

                        //Get Outlines
                        p0 = M.GetPolylines();
                        p1 = M.GetPolylines();
                        for (int i = 0; i < M.Ngons.Count; i++) {

                            Plane plane = p0[i].GetPlane(P);

                            plane = plane.MovePlanebyAxis(D * 0.5);
                            dt.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));

                            if (W > 0) {
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(W);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                            }



                            plane = plane.MovePlanebyAxis(-D);
                            dt.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));

                            if (W > 0) {
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(W);
                                dtThickness.Add(PolylineUtil.OutlineFromFaceEdgeCorner(plane, edgePlanes[i], bisePlanes[i], T, Tol), new GH_Path(DA.Iteration, i));
                                plane = plane.MovePlanebyAxis(-W * 0.5);
                            }

                        }

                        break;


                    }

                    //Output
                    //OffsetMesh = new Mesh[] { m0, m1 };
                    this.PreparePreview(m0, DA.Iteration, dt.AllData(), false);
                    DA.SetDataList(0, new Mesh[] { m0, m1 });

              
            
        }
   
            DA.SetDataTree(1, dt);
            DA.SetDataTree(2, dtThickness);
            //ProjectedPolylines = dt;
            //Thickness = dtThickness;

        }catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }


    protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MeshOffsetVDA;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("b30e676d-d93d-466a-85b5-89958c6e1478"); }
        }
    }
}