using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace NGon_RH8.Vertex
{
    public class NGonFaceVertices : GH_Component_NGon
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public NGonFaceVertices()
          : base("NGon Face Vertices", "NGFaceVertices",
              "Get Ngon vertices and topology vertices",
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
            pManager.AddPointParameter("Vertices", "V", "Get Mesh Vertices in Ngons", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("ID", "I", "Get Mesh Vertices in Ngons", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("TopoID", "IT", "Get Mesh Topology Vertices in Ngons", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            uint[][] v = mesh.GetNGonsBoundaries();
            int[][] tv = mesh.GetNGonsTopoBoundaries();



            var pt = mesh.GetNGonsBoundariesPoint3F(v);

            List<Point3d> points = new List<Point3d>();
            List<Line[]> lineArray = new List<Line[]>();

            foreach (var p in pt) {

                Point3d center = new Point3d();
                foreach (var pp in p) {
                    center += pp;
                    points.Add(pp);
                }
                center /= p.Length;

                Point3d[] p_Copy = new Point3d[p.Length];
                int i = 0;
                foreach (var pp in p) {
                    Point3d pp_Copy = pp;
                    pp_Copy.Transform(Rhino.Geometry.Transform.Scale(center,0.95));
                    p_Copy[i++] = pp_Copy;
                }
                lineArray.Add(p_Copy.ToLineArray(true));


            }

            this.PreparePreview(mesh, iteration, null, false, points,null,255,0,0, lineArray);



            DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(pt, iteration));
           // DA.SetDataTree(0, Util.JaggedArraysToTree(MeshUtil.GetNGonsTopoBoundariesPoint3F(mesh, tv), iteration));
            DA.SetDataTree(1, GrasshopperUtil.JaggedArraysToTree(v, iteration));
            DA.SetDataTree(2, GrasshopperUtil.JaggedArraysToTree(tv, iteration));
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_FV;

        public override Guid ComponentGuid => new Guid("{1a878d29-580f-4d7e-b93c-9927ef0a2504}");
    }
}