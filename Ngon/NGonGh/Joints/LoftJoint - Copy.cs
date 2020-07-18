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
              "NGon", "Joints") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddCurveParameter("Curve 0", "C0", "First Curve, if no curves are supplied, then mesh is used as input", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Curve 1", "C1", "Second Curve, if no curves are supplied, then mesh is used as input", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh 0", "M0", "Mesh0", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh 1", "M1", "Mesh1", GH_ParamAccess.item);

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

            //Retrieve parameters

            GH_Structure<GH_Curve> c0 = new GH_Structure<GH_Curve>();
            GH_Structure<GH_Curve> c1 = new GH_Structure<GH_Curve>();
            Mesh mesh0 = new Mesh();
            Mesh mesh1 = new Mesh();
            double planeWidth = 0.01;
            int divisions = 2;
            bool cornerType = true;
            double parallel = 0.0;
            bool screws = true;

            DA.GetDataTree<GH_Curve>(0, out c0);
            DA.GetDataTree<GH_Curve>(1, out c1);
            DA.GetData(2, ref mesh0);
            DA.GetData(3, ref mesh1);
            DA.GetData(4, ref planeWidth);
            DA.GetData(5, ref divisions);
            DA.GetData(6, ref cornerType);
            DA.GetData(7, ref parallel);
            DA.GetData(8, ref screws);


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

                for (int i = 0; i < n; i++) {
                    boxes[i] = new TheBox(outlines0[i], outlines1[i], planeWidth, new List<double> { 0.5 }, cornerType, parallel, divisions, i);

                    boxes[i].ConstructPlanes();
                    sidePlanes[i] = boxes[i].sidePlanes;
                    topPlanes[i] = boxes[i].topPlanes;

                    boxes[i].ConstructOutlines();
                    sideOutlines0[i] = boxes[i].sideOutlines0;
                    midOutlines0[i] = boxes[i].midOutlines0;
                }

                DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IE2(sidePlanes));
                DA.SetDataTree(2, NGonsCore.GrasshopperUtil.IE2(topPlanes));
                DA.SetDataTree(3, NGonsCore.GrasshopperUtil.IE3(sideOutlines0));
                DA.SetDataTree(4, NGonsCore.GrasshopperUtil.IE3(midOutlines0));


                //Use meshes as input with its adjacency
            } else {

                //mesh0.Append(mesh1);

                Mesh[] meshBoxes = mesh0.ProjectPairsMesh(mesh1, 1);
                DA.SetDataList(0,meshBoxes);

                int n = meshBoxes.Length;


            }



        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return null;
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