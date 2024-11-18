using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonCore;
using System.Linq;

namespace NGon_RH8.Vertex
{
    public class NGonVertexVertices : GH_Component_NGon
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public NGonVertexVertices()
          : base("NGon Vertices", "Vertices",
              "Get All Ngon vertices and their connected vertices in ngons",
              "Adjacency")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "V", "Get Mesh Vertices in Ngons (HashSet)", GH_ParamAccess.tree);
            pManager.AddPointParameter("Vertices", "N", "Connected Vertices to  Mesh Vertices in Ngons (HashSet)", GH_ParamAccess.tree);

            pManager.AddIntegerParameter("ID", "I", "Get Mesh Vertices in Ngons (HashSet)", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("NID", "I", "Connected Vertices ID to  Mesh Vertices in Ngons (HashSet)", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            //Inputs
            int[][] tv = mesh.GetNGonsTopoBoundaries();
            HashSet<int> tvAll = mesh.GetAllNGonsTopoVertices();
            HashSet<int> e = mesh.GetAllNGonEdges(tv);
            int[] allEArray = e.ToArray();
            int[] allvArray = tvAll.ToArray();

            //Outputs
            Point3f[] allVPt = mesh.GetAllNGonsTopoVerticesPoint3F(tvAll);
            List<int>[] conV = mesh.GetConnectedNgonVertices( allEArray, allvArray);
            List<Point3d>[] conVP = mesh.GetConnectedNgonPoints( conV);

            List<Line> lines = new List<Line>();

               for (int i = 0; i < conVP.Length; i++)
            {
                foreach (Point3d j in conVP[i])
                {
                    Point3d p = new Point3d(allVPt[i]);
                    Line l = new Line(p, j);
                    l.Transform(Rhino.Geometry.Transform.Scale(p,0.25));
                    lines.Add(l);

                }
            }




            this.PreparePreview(mesh, DA.Iteration, null, false, null,lines);


            DA.SetDataTree(0, new DataTree<Point3f>(allVPt, new GH_Path(iteration)));
           DA.SetDataTree(1, GrasshopperUtil.ArrayOfListsToTree(conVP, iteration));
            DA.SetDataTree(2, new DataTree<int>(tvAll, new GH_Path(iteration)));
            DA.SetDataTree(3, GrasshopperUtil.ArrayOfListsToTree(conV, iteration));
      


        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.ngonvtongonvertices;

        public override Guid ComponentGuid => new Guid("{1a169d29-580f-4d7e-b93c-9927ef0a2504}");
    }
}