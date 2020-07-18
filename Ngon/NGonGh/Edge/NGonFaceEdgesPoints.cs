using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace NGonGh.Edge
{
    public class NGonFaceEdgesPoints : GH_Component_NGon {

        public NGonFaceEdgesPoints()
          : base("NGon Face Edges Points", "Face Edges Points",
              "Get Ngon faces edges id and lines",
              "Edge")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D","Division distance",GH_ParamAccess.item, 10000);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddLineParameter("Edges", "E", "Get Mesh Edges in Ngons Faces", GH_ParamAccess.tree);
            //pManager.AddIntegerParameter("ID", "I", "Get Mesh Edges (Lines) in Ngons Faces", GH_ParamAccess.tree);
            //pManager.AddIntegerParameter("FID", "F", "Neighbour Faces", GH_ParamAccess.tree);
            pManager.AddPointParameter("P", "P", "EdgePoints", GH_ParamAccess.tree);
            //
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            double DistanceDivision = 10000;
            DA.GetData(1, ref DistanceDivision);
            int iteration = DA.Iteration;


            int[][] tv = mesh.GetNGonsTopoBoundaries();
             int[][] fe = mesh.GetNGonFacesEdges(tv);
            Point3d[][][] edgePoints = mesh.GetNGonFacesEdgesLinesPoints(fe, DistanceDivision);
            // var l = mesh.GetNGonFacesEdgesLines(fe);

            // DataTree<int> dt2 = new DataTree<int>();
            // HashSet<int> allE = mesh.GetAllNGonEdges(tv);
            // int[] allEArray = allE.ToArray();
            //int[][] ef = mesh.GetNgonsConnectedToNGonsEdges(allE, true);

            // for(int i = 0; i < mesh.Ngons.Count; i++) {
            //     for(int j = 0; j < fe[i].Length; j++) {
            //         int elocal = Array.IndexOf(allEArray, fe[i][j]);
            //         int neiF = (ef[elocal][0] == i) ? ef[elocal][1] : ef[elocal][0];
            //         dt2.Add(neiF,new Grasshopper.Kernel.Data.GH_Path(i,DA.Iteration));
            //     }
            // }

            // List< Line > ld = new List<Line>();
            // foreach (var ll in l)
            //     ld.AddRange(ll);

     



            // DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(l, iteration));
            // DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(fe, iteration));
            // DA.SetDataTree(2, dt2);
            var pts = GrasshopperUtil.TripleArraysToTree(edgePoints, iteration);
            DA.SetDataTree(0, pts);
            this.PreparePreview(mesh, DA.Iteration, null, false,pts.AllData());
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.EdgePoints;

        public override Guid ComponentGuid => new Guid("{b0e10785-f1a2-430b-aa49-a28385512df0}");
    }
}