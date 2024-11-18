using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace NGon_RH8.Edge
{
    public class ScaleNGonEdgesByFacePairs : GH_Component {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public ScaleNGonEdgesByFacePairs()
          : base("Scale NGon Face Edges by NGon Pairs", "SEdge",
              "Scale Get NGon Face Edges by NGon Pairs", "NGon",
              "Adjacency")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NGon Face Pair", "P", "NGon face pairs - DataTree", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scale", "S", "Scale mesh edges", GH_ParamAccess.item, 0.9);
            pManager[0].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Edges", "E", "Get Ngon Pair Edges (Lines) in Ngons Faces", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID", "I", "Get Ngon Edges", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Flag", "F", "Belongs to pair", GH_ParamAccess.list);
            pManager.AddLineParameter("Edges", "E", "Get Ngon Pair Edges (Lines) in Ngons Faces", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Polylines", "P", "Connections", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            GH_Structure<GH_Integer> pairs = new GH_Structure<GH_Integer>();
            DA.GetDataTree(1, out pairs);

            double scale = 0.9;
            DA.GetData(2, ref scale);

            //Solution

            int[][] p = new int[pairs.PathCount][];

            //Grasshopper datatree to int[] {a,b}
            for (int i = 0; i < pairs.PathCount; i++)
                p[i] = (new int[] { pairs[i][0].Value, pairs[i][1].Value });

           
            var edges = mesh.FindMeshEdgeByNGonPair(p);

            //Offset all lines by dist
            int[][] NGFaceEdgesID = edges.Item4;
            Line[][] NGFacesEdges = new Line[edges.Item4.Length][];

            //Get face centers
            Point3d[] centers = mesh.GetNGonCenters();

            for (int i = 0; i < edges.Item4.Length; i++)
            {
                NGFacesEdges[i] = new Line[edges.Item4[i].Length];
                for (int j = 0; j < edges.Item4[i].Length; j++)
                {
                    Line l = mesh.TopologyEdges.EdgeLine(NGFaceEdgesID[i][j]);
                    l.Transform(Rhino.Geometry.Transform.Scale(centers[i], scale));
                    NGFacesEdges[i][j] = l;

                    if (edges.Item6.ContainsKey(NGFaceEdgesID[i][j]))
                    {
                        edges.Item5[NGFaceEdgesID[i][j]].Add(l.From);
                        edges.Item5[NGFaceEdgesID[i][j]].Add(l.To);
                    }
                }
            }



            //Draw line between face centers
            //pairs gives a and b face
            //item1 gives flag
            //item3 gives edges id

            //Create dictionary with KEY - edge id VALUE - flag
            //Then dictionary with KEY - edge id VALUE - end points


            Polyline[] plines = new Polyline[edges.Item6.Count];
            int id = 0;
            foreach (var i in edges.Item5)
            {
                plines[id] = new Polyline(new Point3d[] { i.Value[0], i.Value[1], i.Value[3], i.Value[2], i.Value[0] });
                id++;
            }
           //Output

            DA.SetDataList(0, edges.Item2);
            DA.SetDataList(1, edges.Item3);
            DA.SetDataList(2, edges.Item1);
            DA.SetDataTree(3, GrasshopperUtil.JaggedArraysToTree(NGFacesEdges, 0));
          DA.SetDataList(4, plines);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.mergebypair;
            }
        }

        public override Guid ComponentGuid => new Guid("{b0e10148-f1a2-430b-aa49-a28385982df0}");
    }
}