using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using NGonCore;
using System.Linq;

namespace NGon_RH8.Edge
{
    public class NGonsConnectedToNGonEdge : GH_Component {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public NGonsConnectedToNGonEdge()
          : base("NGons Connected To NGon Edge", "EdgeNGon",
              "Gets ngons connected to ngons edges, -1 is added to beggining of the list edge is naked (left / right property for clean meshes)",
               "NGon", "Adjacency")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Naked", "N", "Add -1 for naked edge", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("EF", "EF", "Edge Face Connectivity", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("EFPair", "EF", "Edge Face Connectivity - Two faces only", GH_ParamAccess.tree);
            pManager.AddNumberParameter("DihedralAngle", "A", "DihedralAngle", GH_ParamAccess.tree);
            pManager.AddLineParameter("DihedralAngleLocation", "AL", "DihedralAngle EdgeLocation", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);
            bool flag = true;
            DA.GetData(1, ref flag);

            int iteration = DA.Iteration;

            int[][] tv = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(tv);
            int[] allEArray = allE.ToArray();
            int[][] NGons = mesh.GetNgonsConnectedToNGonsEdges(allE, flag);
            //int[][] ef = mesh.GetNgonsConnectedToNGonsEdges(allE, false);

            Vector3d[] v = mesh.GetNgonNormals();
            Plane[] planes = mesh.GetNgonPlanes();

            int[][] fe = mesh.GetNGonFacesEdges(tv).ToArray();

            DataTree<int> dt = new DataTree<int>();
            DataTree<int> dt2 = new DataTree<int>();

            DataTree<double> dtAngle = new DataTree<double>();
            DataTree<Line> dtAngleLine = new DataTree<Line>();

         

            //Mesh edge
            for (int i = 0; i < NGons.Length; i++) {
                dt.AddRange(NGons[i], new GH_Path(i));//new GH_Path(allE.ElementAt(i)));

                //int nei = ()
                if (NGons[i].Count()==2) {
                   
                    //Get common edge vector
                    int f0 = NGons[i][0];
                    int f1 = NGons[i][1];

                    int meshEdge = fe[f0].Intersect(fe[f1]).ToArray()[0];
                    Line l = mesh.TopologyEdges.EdgeLine(meshEdge);
                    Point3d point3d = l.From;
                    Point3d point3d1 = l.To;


                    Vector3d item = v[f0];
                    Vector3d vector3d = v[f1];

                    double num = 180;
                    if (item.IsParallelTo(vector3d, Rhino.RhinoDoc.ActiveDoc.ModelAngleToleranceRadians) == 0) {


                        Vector3d vector3d1 = Vector3d.CrossProduct(item, point3d1 - point3d);
                        Vector3d vector3d2 = Vector3d.CrossProduct(vector3d, point3d - point3d1);
                        vector3d1.Unitize();
                        vector3d2.Unitize();




                        num = Math.PI - Vector3d.VectorAngle(item, vector3d, new Plane(l.Mid(), item, vector3d));

                        //Solve concave / convex cases
                        double angleBetweenMidPoints = Vector3d.VectorAngle(v[f0], planes[f1].Origin - planes[f0].Origin, new Plane(l.Mid(), v[f0], planes[f1].Origin - planes[f0].Origin));
                        if (angleBetweenMidPoints > Math.PI * 0.5) {
                            num = Math.PI * 2 - num;
                        }
                        num = Math.Round(Rhino.RhinoMath.ToDegrees(num), 3);
                    }
                    //num= NGonCore.PlaneUtil.SmallerDihedralAngleDeg(planes[0], planes[1]);
                    dtAngle.Add(num, new GH_Path(allE.ElementAt(i)));
                    dtAngleLine.Add(l, new GH_Path(allE.ElementAt(i)));
                    dt2.AddRange(NGons[i], new GH_Path(allE.ElementAt(i)));
                }
            }
            //Rhino.RhinoApp.WriteLine(NGons.Length.ToString());
            DA.SetDataTree(0, dt);
            DA.SetDataTree(1, dt2);
            DA.SetDataTree(2, dtAngle);
            DA.SetDataTree(3, dtAngleLine);
            // DA.SetDataTree(0, GrasshopperUtil.JaggedArraysToTree(NGons,iteration));
            //Instead of mesh edge -> adjacent mesh face

        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.Icons_NGons_AdjEF;


        public override Guid ComponentGuid => new Guid("{33e21565-d38e-4571-85f0-0da265872d2b}");
    }
}