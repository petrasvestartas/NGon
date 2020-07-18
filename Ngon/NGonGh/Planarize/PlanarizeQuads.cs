using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace SubD.Planarize {

    public class PlanarizeQuads : GH_Component {
        public struct Prop {
            public Mesh mesh;
            public double tolerance;
            public bool[] byrefplan;
            public double[] byrefdev;
            public Point3d[] cen;
            public Plane[] pl;
           // public Dictionary<int,int> faces;
            public int[] faces;
        }


        public PlanarizeQuads() : base("PlanarizeQuads", "PlanarizeQuads", "PlanarizeQuads ", "SubD", "Planarize") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            //pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("Fix Naked", "N", "0:None 1:Fixed 2:Follow normal 3:Positive normal 4:Negative normal 5:Negotiate", 0, 0);
            pManager.AddIntegerParameter("Iterations", "I", "Iterations", 0, 1);
            pManager.AddNumberParameter("Tolerance", "T", "Planarity tolerance", 0, 0.0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            //pManager.AddCurveParameter("Polylines", "P", "Polylines for mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Planar", "P", "Planar", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {

           
            Mesh mesh = new Mesh();
            bool flag1 = DA.GetData(0, ref mesh);

            int iterations = 0;
            bool flag2 = DA.GetData(1, ref iterations);

            double tol = 0.0;
            bool flag3 = DA.GetData(2, ref tol);


            if (flag1 && flag2) {

                //Input 
                iterations = (iterations < 1) ? 1 : iterations;

                Dictionary<int, int> faces = mesh.QuadFacesDict();//Faces(mesh);
                int[] facesArray = mesh.QuadFaces();//FacesArray(mesh);

                bool[] planarity = new bool[faces.Count];
                Plane[] planes = GetPlanes(mesh, tol, ref planarity, facesArray);
                int[][] faceMap = mesh.QuadFaceMap(faces);//FaceMap(mesh, faces);
                bool[] nakedStatus = mesh.GetNakedEdgePointStatus();

                //Solution
                for (int i = 0; i < iterations; i++)
                    Planarization(ref mesh, faces, facesArray, planarity, ref planes, faceMap, nakedStatus, tol);

                //Ouput
                DA.SetData(0, mesh);
                if(tol > 0)
                    DA.SetDataList(1, planarity);
            }
        }

        private void Planarization(ref Mesh mesh, Dictionary<int, int> faces, int[] facesArray, bool[] planarity, ref Plane[] planes, int[][] faceMap, bool[] nakedStatus, double tol)
        {
            for (int j = 0; j < mesh.Vertices.Count; j++) {

                if (!nakedStatus[j]) {

                        //Take all neighbour planes to vertex and project it n times
                        //then get average point

                        Point3d averagePoint = new Point3d();

                        for (int l = 0; l < faceMap[j].Length; l++) {
                            int y = faces[faceMap[j][l]];
                            averagePoint += planes[y].ClosestPoint(mesh.Vertices[j]);
                        }

                        mesh.Vertices[j] = (Point3f)(averagePoint / faceMap[j].Length);

                }//if
            }//for


            planes = GetPlanes(mesh, tol, ref planarity, facesArray);

           
        }


        private Plane[] GetPlanes(Mesh m, double tolerance, ref bool[] planar, int[] faces) {

            Prop closure = new Prop();
            closure.mesh = m;
            closure.tolerance = tolerance;
            closure.faces = faces;
            closure.pl = new Plane[faces.Length];
            closure.byrefplan = new bool[planar.Length];
            closure.byrefdev = new double[planar.Length];

            //Get mesh face vertices and fit them to plane
            Parallel.For(0, closure.faces.Length, delegate (int i) {

                Point3d[] array = new Point3d[4];
                int t = closure.faces[i];

                for (int k = 0; k < 4; k++)
                    array[k] = closure.mesh.Vertices[closure.mesh.Faces[t][k]];
    

                Plane.FitPlaneToPoints(array, out closure.pl[i], out double num3);
                closure.byrefplan[i] = num3 <= closure.tolerance;
                closure.byrefdev[i] = num3;
            });

            planar = closure.byrefplan;

            return closure.pl;
        }


        public Dictionary<int, int> Faces(Mesh mesh) {
            Dictionary<int, int> id = new Dictionary<int, int>();
            for (int i = 0; i < mesh.Faces.Count; i++)
                if (mesh.Faces[i].IsQuad)
                    id.Add(i, id.Count);
            return id;
        }

        public int[] FacesArray(Mesh mesh) {
            List<int> id = new List<int>();
            for (int i = 0; i < mesh.Faces.Count; i++)
                if (mesh.Faces[i].IsQuad)
                    id.Add(i);
            return id.ToArray();
        }

        public int[][] FaceMap(Mesh mesh, Dictionary<int,int> qfaces) {

            int[][] id = new int[mesh.Vertices.Count][];

            for (int i = 0; i < mesh.Vertices.Count; i++) {
                List<int> quads = new List<int>();
                int[] f = mesh.Vertices.GetVertexFaces(i);
                for (int j = 0; j < f.Length; j++)
                    if (qfaces.ContainsKey(f[j]))
                       quads.Add(f[j]);
                id[i] = quads.ToArray();

            }


            return id;


        }

        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.planarizequads;

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf4095");
    }
}