using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Utils
{
    public class RecipricalExtend : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalExtend()
          : base("RecipricalExtend", "RecipricalExtend",
              "Rotate mesh edge and extend to the next line",
              "NGon", "Reciprocal")
        {

        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Angle", GH_ParamAccess.item, 0.7);
            pManager.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item, 1.4);
            pManager.AddBooleanParameter("NormalType", "T", "0 - normal is used as an average ngon plane, 1 - normal of adjacent triangle face", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Height", "H", "Height", GH_ParamAccess.item, 1);

            // pManager.AddBooleanParameter("Ending", "F", "", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "CM", "Curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C0", "Curve", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C1", "Curve", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "Plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("EndPlanes", "EP", "EndPlanes", GH_ParamAccess.list);

            //pManager.AddIntegerParameter("ID0", "I0", "First Line for collision", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("ID1", "I1", "Second Line for collision", GH_ParamAccess.list);
            //pManager.AddVectorParameter("Vectors", "V", "Rotation Vectors", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh M = DA.Fetch<Mesh>("Mesh");
            double A = DA.Fetch<double>("Angle");
            double S = DA.Fetch<double>("Scale");
            bool N = DA.Fetch<bool>("NormalType");
            double D = DA.Fetch<double>("Height");

            int[][] tv = M.GetNGonsTopoBoundaries();
            HashSet<int> ehash = M.GetAllNGonEdges(tv);
            int[] e = ehash.ToArray();
            int[][] ef = M.GetNgonsConnectedToNGonsEdges(ehash, false);
            Plane[] planes = M.GetNgonPlanes();


            Dictionary<int, int> eDict = new Dictionary<int, int>();
            int[][] fe = M.GetNGonFacesEdges(tv);

            int i = 0;
            foreach (int meshedge in ehash)
            {
                eDict.Add(meshedge, i++);
            }

            int[][] fe_ = new int[fe.Length][];
            int[][] fe_0 = new int[fe.Length][];


            for (i = 0; i < fe.Length; i++)
            {
                fe_[i] = new int[fe[i].Length];
                fe_0[i] = new int[fe[i].Length];

                for (int j = 0; j < fe[i].Length; j++)
                {
                    fe_[i][j] = eDict[fe[i][j]];
                }

                for (int j = 0; j < fe[i].Length; j++)
                {
                    fe_0[i][j] = eDict[fe[i][(j + 1) % fe[i].Length]];
                }



            }



            List<Vector3d> vecs = new List<Vector3d>();

            i = 0;
            foreach (int n in e)
            {

                int[] edgeFaces = ef[i];

                Vector3d vec = Vector3d.Zero;

                if (N)
                {
                    for (int j = 0; j < edgeFaces.Length; j++)
                    {
                        vec += planes[edgeFaces[j]].ZAxis;
                    }
                    vec /= edgeFaces.Length;
                    base.Message = "Average";
                }
                else
                {

                    int[] triangleFaces = M.TopologyEdges.GetConnectedFaces(n);

                    for (int j = 0; j < triangleFaces.Length; j++)
                    {
                        vec += M.FaceNormals[triangleFaces[j]];
                    }
                    vec /= triangleFaces.Length;
                    base.Message = "Face";

                }


                vecs.Add(vec);
                i++;
            }

            Line[] lines = M.GetAllNGonEdgesLines(ehash);
            //List<Line> ln = new List<Line>();

            for (int j = 0; j < lines.Length; j++)
            {
                //Line l = lines[j];
                lines[j].Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S));
                lines[j].Transform(Rhino.Geometry.Transform.Rotation(A, vecs[j], lines[j].PointAt(0.5)));
            }


            //2nd part extend

            Plane[] linePlanes = new Plane[lines.Length];
            for (i = 0; i < lines.Length; i++)
            {
                linePlanes[i] = new Plane(lines[i].PointAt(0.5), lines[i].Direction, vecs[i]);
            }




            Plane[][] endPlanes = new Plane[0][];

            DA.SetDataList(1, GetLines(lines, linePlanes, fe_, out endPlanes, D));
            DA.SetDataList(2, GetLines(lines, linePlanes, fe_, out endPlanes, -D));
            DA.SetDataList(0, GetLines(lines, linePlanes, fe_, out endPlanes));
            DA.SetDataList(3, linePlanes);
            DA.SetDataTree(4, NGonCore.GrasshopperUtil.IE2(endPlanes));
            //DA.SetDataTree(1, NGonCore.GrasshopperUtil.IE2(fe_));
            //DA.SetDataTree(2, NGonCore.GrasshopperUtil.IE2(fe_0));
            //DA.SetDataList(3, vecs);


        }

        public Line[] GetLines(Line[] lines, Plane[] linePlanes, int[][] fe_, out Plane[][] endPlanes, double move = 0)
        {
            int i = 0;


            Line[] linesMoved = new Line[lines.Length];
            for (i = 0; i < lines.Length; i++)
            {
                linesMoved[i] = lines[i];
                linesMoved[i].Transform(Rhino.Geometry.Transform.Translation(linePlanes[i].YAxis * move));

            }

            List<Point3d>[] points = new List<Point3d>[lines.Length];
            List<int>[] planeIDS = new List<int>[lines.Length];

            for (i = 0; i < linesMoved.Length; i++)
            {
                points[i] = new List<Point3d>();
                planeIDS[i] = new List<int>();
            }

            for (i = 0; i < fe_.Length; i++)
            {

                for (int j = 0; j < fe_[i].Length; j++)
                {

                    int current = fe_[i][j];
                    int prev = fe_[i][NGonCore.MathUtil.Wrap(j - 1, fe_[i].Length)];
                    int next = fe_[i][NGonCore.MathUtil.Wrap(j + 1, fe_[i].Length)];

                    //Rhino.RhinoApp.WriteLine(prev.ToString() + " " + next.ToString() + " " + current.ToString());

                    Point3d p0 = NGonCore.PlaneUtil.LinePlane(linesMoved[current], linePlanes[prev]);
                    Point3d p1 = NGonCore.PlaneUtil.LinePlane(linesMoved[current], linePlanes[next]);
                    points[current].Add(p0);
                    points[current].Add(p1);
                    planeIDS[current].Add(prev);
                    planeIDS[current].Add(next);
                    //Line extendedLine = (linesMoved[current].From.DistanceToSquared(p0) < linesMoved[current].From.DistanceToSquared(p1)) ? new Line(p0,p1) : new Line(p1, p0);



                }

            }

            Line[] linesMid = new Line[linesMoved.Length];
            endPlanes = new Plane[linesMoved.Length][];

            for (i = 0; i < linesMoved.Length; i++)
            {
                Line line = linesMoved[i];

                double[] t = new double[points[i].Count];
                int[] id = new int[points[i].Count];

                for (int j = 0; j < points[i].Count; j++)
                {
                    t[j] = line.ClosestParameter(points[i][j]);
                    //Rhino.RhinoApp.WriteLine(t[j].ToString());
                    id[j] = j;
                }

                Array.Sort(t, id);
                int s = id[0];
                int e = id[id.Length - 1];
                linesMid[i] = new Line(points[i][s], points[i][e]);
                //Rhino.RhinoApp.WriteLine(s.ToString() + " " + e.ToString() + " " + planeIDS[i][s].ToString() + " " + e.ToString());
                //Rhino.RhinoApp.WriteLine(points[i].Count.ToString() + " " + planeIDS[i].Count.ToString() );
                Plane ps = new Plane(linePlanes[planeIDS[i][s]]);
                ps.Origin = points[i][s];
                Plane pe = new Plane(linePlanes[planeIDS[i][e]]);
                pe.Origin = points[i][e];
                endPlanes[i] = new Plane[] { ps, pe };

            }

            return linesMid;
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.Reciprocal2;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-1421a5ee1f14"); }
        }
    }
}