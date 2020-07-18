using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonsCore;
using System.Linq;

namespace NGonGh.Vertex
{
    public class NGonVertices : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the NGonVertices class.
        /// </summary>
        public NGonVertices()
          : base("NGon Vertices", "Vertices",
              "Get All Ngon vertices and topology vertices",
              " Vertex")
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

            pManager.AddIntegerParameter("ID", "I", "Get Mesh Vertices in Ngons (HashSet)", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Naked", "N", "Naked vertices", GH_ParamAccess.tree);

            pManager.AddIntegerParameter("V", "V", "V", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("E0", "E0", "E0", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("E1", "E1", "E1", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            int iteration = DA.Iteration;

            HashSet<int> v = mesh.GetAllNGonsVertices();
            HashSet<int> tv = mesh.GetAllNGonsTopoVertices();




            bool[]  nakedVertices = mesh.GetNakedNGonPointStatus(v);

            var pt = mesh.GetAllNGonsVerticesPoint3F(v);

            List<Point3d> points = new List<Point3d>();
            foreach (var p in pt)
                    points.Add(p);

            this.PreparePreview(mesh, DA.Iteration, null, false, points);


            DA.SetDataTree(0, new DataTree<Point3f>(pt, new GH_Path(iteration)));
           // DA.SetDataTree(1, new DataTree<Point3f>(mesh.GetAllNGonsTopoVerticesPoint3F(tv)));
            DA.SetDataTree(1, new DataTree<int>(v, new GH_Path(iteration)));
            DA.SetDataTree(2, new DataTree<bool>(nakedVertices, new GH_Path(iteration)));
            //DA.SetDataTree(3, new DataTree<int>(tv, new GH_Path(iteration)));


            /////////////Graph
            //Edges start and end point
            int[] nodes = tv.ToArray();
            List<int> edges0 = new List<int>();
            List<int> edges1 = new List<int>();


            foreach (int tv_Current in tv) {
                int[] tv_Neigbours = mesh.TopologyVertices.ConnectedTopologyVertices(tv_Current, true);
                foreach (int tv_Neighbour in tv_Neigbours) {
                    if (tv.Contains(tv_Neighbour)) {
                        edges0.Add(tv_Current);
                        edges1.Add(tv_Neighbour);
                    }
                }
            }

            DataTree<int> dt0 = new DataTree<int>();
            DataTree<int> dt1 = new DataTree<int>();
            DataTree<int> dt2 = new DataTree<int>();
            dt0.AddRange(nodes, new Grasshopper.Kernel.Data.GH_Path(iteration));
            dt1.AddRange(edges0, new Grasshopper.Kernel.Data.GH_Path(iteration));
            dt2.AddRange(edges1, new Grasshopper.Kernel.Data.GH_Path(iteration));

            DA.SetDataTree(3, dt0);
            DA.SetDataTree(4, dt1);
            DA.SetDataTree(5, dt2);


        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_AllV;

        public override Guid ComponentGuid => new Guid("{f6fe728a-5e0d-4224-a12d-92d80840df7f}");
    }
}