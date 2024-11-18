using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.Planarize {

    public class PlanarizeNGonsCoPlanarPoints : GH_Component {

        bool run = false;
        bool reset = false;
        bool staticSolver = false;


        protected override void AfterSolveInstance()
        {
            if (!this.run || reset || staticSolver)
            {
                return;
            }
            GH_Document gH_Document = base.OnPingDocument();
            if (gH_Document == null) { 
                return;
            }
            GH_Document.GH_ScheduleDelegate gH_ScheduleDelegate = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
            gH_Document.ScheduleSolution(1, gH_ScheduleDelegate);
        }

        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }


        public PlanarizeNGonsCoPlanarPoints() : base("Planarize2", "Planarize2", "Planarize2 ", "NGon","Planarize") { }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {

            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tangential", "Tangential", "Tangential", 0, 0.0001);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations", 0, 1);
            pManager.AddBooleanParameter("StaticSolver", "StaticSolver", "StaticSolver", 0, false);
            pManager.AddBooleanParameter("Run", "Run", "Run", 0, false);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", 0, false);

            for(int i=1; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {

            //pManager.AddCurveParameter("Curve", "C", "Curve", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);


        }

        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh mesh_ = new Mesh();

          
            int iterations = 10;
            
            double tangentialT = 0.0001;
            // bool run = false;
            //bool reset = false;
            DA.GetData(1,ref tangentialT);
            DA.GetData(2, ref iterations);
            DA.GetData(3,ref this.staticSolver);
            DA.GetData(4,ref this.run);
            DA.GetData(5,ref this.reset);

            if (DA.GetData(0, ref mesh_))
            {
                //if (mesh.IsValid)
                //{



                    // this.tolerance = tolerance;
                    //  W = weight;
                    //  this.Naked = naked;

                    if (iterations < 0)
                        iterations = 0;


                    //Solution
                    if (reset || planar == null)
                    {
                        Setup(mesh_);
                        counter = 0;
                        base.Message = counter.ToString();
                    }

                    if (deviation >= tolerance)
                    {
                        if (!reset && run && staticSolver == false)
                        {
                       // Rhino.RhinoApp.WriteLine("Hi");
                            //Interactive

                            for (int i = 0; i < iterations; i++)
                                Solve2(ref this.mesh, allNGonVArray, ref deviation, faceMap, nakedStatus,  ref planar,  tangentialT);//ref NGonPlanes

                            counter += iterations;
                            base.Message = counter.ToString();
                           //Component.ExpireSolution(true);


                        }
                        else if (!reset && run && staticSolver ==true)
                        {
                        Setup(mesh_);
                        counter = 0;
                        //ase.Message = counter.ToString();

                        //Zombie
                        for (int i = 0; i < iterations; i++)
                            Solve2(ref this.mesh, allNGonVArray, ref deviation, faceMap, nakedStatus,  ref planar,  tangentialT);//ref NGonPlanes,
                            counter += iterations;
                            base.Message = counter.ToString() + " " + deviation.ToString();
                        }
                    }
                //DA.SetDataList(0,mesh.GetPolylines());
                DA.SetData(0,this.mesh);

               // }
            }
        }

        double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 0.02;
        double deviation = 1.7976931348623157E+308;
        int counter = 0;
        double W = 0.99999;
        bool Naked = true;

        Mesh mesh;
        Mesh meshOriginal;

        bool[] planar;//mesh faces
        //Plane[] NGonPlanes;
        int[] allNGonVArray;
        int[] allNGonEArray;
        int[][] faceMap;
        bool[] nakedStatus;
        bool[] nakedStatusSimple;
        Vector3f[] vertexNormals;
        List<Point3d> pts;
        List<int>[] conV;
        int[][] faceV;
        Dictionary<int, int> allNGonVArrayFlip;

        public void Setup(Mesh m)
        {

            this.mesh = m.DuplicateMesh();
            this.meshOriginal = m.DuplicateMesh();
            this.mesh.RebuildNormals();
            planar = new bool[this.mesh.Ngons.Count];//mesh faces
            //NGonPlanes = this.GetPlanes(this.mesh, tolerance, ref deviation, ref planar);

            faceV = this.mesh.GetNGonsTopoBoundaries();

            HashSet<int> allNGonV = this.mesh.GetAllNGonsVertices();
            allNGonVArray = allNGonV.ToArray();

           allNGonVArrayFlip= new Dictionary<int, int>();
            for (int i = 0; i < allNGonVArray.Length; i++)
                allNGonVArrayFlip.Add(allNGonVArray[i],i);


            HashSet<int> allNGonE = NGonCore.MeshUtil.GetAllNGonEdges(mesh, mesh.GetNGonsTopoBoundaries());
            allNGonEArray = allNGonE.ToArray();

            faceMap = this.mesh.GetNGonsConnectedToNGonVertices(allNGonV);
            nakedStatus = this.mesh.GetNakedNGonPointStatus(allNGonV);
            nakedStatusSimple = this.mesh.GetNakedEdgePointStatus();
            vertexNormals = this.mesh.GetNgonNormals3f();
            pts = new List<Point3d>();
            conV = this.mesh.GetConnectedNgonVertices(allNGonEArray, allNGonVArray);


        }

        public void Solve2(ref Mesh mesh, int[] allNGonVArray, ref double deviation, int[][] faceMap, bool[] nakedStatus, ref bool[] planar, double tangentialT)//ref Plane[] NGonPlanes, 
        {

            if (tangentialT > 0)
            {

                //https://github.com/Dan-Piker/K2Goals/blob/master/TangentialSmooth.cs

                Point3d[] averageP = new Point3d[allNGonVArray.Length];
                Vector3d[] averagePVecs = new Vector3d[allNGonVArray.Length];
                Vector3d[][] averagePVecs_ = new Vector3d[allNGonVArray.Length][];
                int[] averagePCount = new int[allNGonVArray.Length];
                for (int i = 0; i < allNGonVArray.Length; i++)
                {
                    //if (this.nakedStatusSimple[allNGonVArray[i]])
                    //  continue;
                    //if (conV[i].Count != 3)
                    //  continue;

                    //Position center point
                    //Vec array for surrounding vertices
                    int n = conV[i].Count + 1;
                    averagePVecs_[i] = new Vector3d[n];

                    Point3d Avg = new Point3d();
                    var Vecs = new Vector3d[n - 1];

                    for (int j = 1; j < n; j++)
                    {
                        //Avg = Avg + p[PIndex[i]].Position;
                        //Vecs[i - 1] = p[PIndex[i]].Position - p[PIndex[0]].Position;
                        //int localID = Array.IndexOf(allNGonVArray, conV[i][j]);
                        //check why n-1
                        Avg += mesh.TopologyVertices[conV[i][j - 1]]; //sum up all vertices
                        Vecs[j - 1] = mesh.TopologyVertices[conV[i][j - 1]] - mesh.TopologyVertices[allNGonVArray[i]]; //get vector to center point
                    }
                    double Inv = 1.0 / (n - 1);
                    Avg *= Inv;

                    //Vector3d Smooth = 0.5 * (Avg - p[PIndex[0]].Position);
                    Vector3d Smooth = 0.5 * (Avg - mesh.TopologyVertices[allNGonVArray[i]]);


                    Vector3d Normal = new Vector3d();
                    for (int j = 0; j < Vecs.Length; j++)
                    {
                        Normal += Vector3d.CrossProduct(Vecs[j], Vecs[(j + 1) % Vecs.Length]);
                    }
                    Normal.Unitize();
                    Smooth -= Normal * (Normal * Smooth);


                    //Assign values
                    averagePVecs_[i][0] = Smooth;
                    averagePVecs[i] += Smooth;
                    averagePCount[i] += 1;
                    //Rhino.RhinoApp.WriteLine(Smooth.ToString());
                    Smooth *= -Inv;


                    for (int j = 1; j < n; j++)
                    {
                        int localID = Array.IndexOf(allNGonVArray, conV[i][j - 1]);
                        averagePVecs_[i][j] = Smooth;
                        averagePVecs[localID] = Smooth;
                        averagePCount[localID] += 1;
                        //Rhino.RhinoApp.WriteLine(Smooth.ToString());
                    }
                }

                /*
                //Assgn vectors for 1 vertex
                for (int i = 0; i < allNGonVArray.Length; i++)
                {
                    if (this.nakedStatusSimple[allNGonVArray[i]])
                      continue;
                   mesh.TopologyVertices[allNGonVArray[i]] += (Vector3f)(averagePVecs[i] / averagePCount[i]);
                }
                */

                //Assign vector one by one for currect point and neighbours
                for (int i = 0; i < allNGonVArray.Length; i++)
                {
                    if (this.nakedStatusSimple[allNGonVArray[i]])
                        continue;

                    mesh.TopologyVertices[allNGonVArray[i]] += (Vector3f)(averagePVecs_[i][0] / averagePCount[i] * tangentialT);

                    for (int j = 1; j < averagePVecs_[i].Length; j++)
                    {
                        int localID = Array.IndexOf(allNGonVArray, conV[i][j - 1]);
                        //Rhino.RhinoApp.WriteLine((averagePVecs_[i][j] / averagePCount[i]).ToString());
                        mesh.TopologyVertices[allNGonVArray[localID]] += (Vector3f)(averagePVecs_[i][j] / averagePCount[i] * tangentialT);
                    }
                }



            }
            










            double currentDeviation = 0;

            Point3d[] allPts = new Point3d[allNGonVArray.Length];
            double[] allW = new double[allNGonVArray.Length];

            //Get vector per face
            for (int i = 0; i < faceV.Length; i++) {

                Point3d[] facePts = new Point3d[faceV[i].Length];
                for (int j = 0; j < faceV[i].Length; j++)
                    facePts[j] = mesh.TopologyVertices[faceV[i][j]];

                Plane.FitPlaneToPoints(facePts, out Plane plane, out double dev);
                currentDeviation += dev;

                for (int j = 0; j < faceV[i].Length; j++) {
                    int localID = allNGonVArrayFlip[faceV[i][j]];
                    Point3d cp = plane.ClosestPoint(mesh.TopologyVertices[faceV[i][j]]);
                    allW[localID] += 1;
                    allPts[localID] += cp;
                }

            }


            for (int i = 0; i < allNGonVArray.Length; i++)
            {
                mesh.TopologyVertices[allNGonVArray[i]] = (Point3f)(allPts[i] / allW[i]);
                deviation = currentDeviation;/// faceV.Length;
            }


        }

        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.Planarize2;

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf6912");
    }
}

