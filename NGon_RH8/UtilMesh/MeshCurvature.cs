using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using System.Drawing;

namespace NGon_RH8.Utils {
    public class Mesh_Cuvarture_Component : GH_Component {
        List<double> Kmed = new List<double>();
        List<List<double>> KT = new List<List<double>>();
        List<Color> vertexCol = new List<Color>();
        double up;
        double low;
        Color A = Color.FromArgb(255, 0, 0);
        Color B = Color.FromArgb(255, 255, 0);
        Color C = Color.FromArgb(255, 0, 255);
        int[] ind;
        Rhino.Geometry.Collections.MeshTopologyVertexList topvertex;
        Mesh mesh = new Mesh();
        NGonCore.MeshUtilSimple.Mesh_Cuvature_Utilities_Debugging mainK;
        double v1;
        double v2;
        List<Line> ln = new List<Line>();
        int method = 2;
        double kmmin;
        double kmmax;
        double aMeshEdgeLength=0;
      

        //FINISH CONTROLLERS AND ADD MESH OUTPUT PREVIEW AND MESH FACES AND TEST WITH BUNNY TO normalize, maximize biggest vectors

        #region constructor
        public Mesh_Cuvarture_Component() : base("Mesh Curvature", "Mesh Curvature", "Mesh Curvature", "NGon", "Utilities Mesh") {
            min = 0;
            max = 0;
            Flag = false;
            display = false;
            vectorScale = 10;
            exp1 = 0;
            exp2 = 0;
            curvatureType = 3;

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("CurvatureType", "CurvatureType", "0 = Min, 1 = Max, 2 = Gaussian, 3 = Mean, 4 = Absolute, 5 = Difference, 6 = Rms  ", GH_ParamAccess.item, 3);
            //pManager.AddBooleanParameter("Ring / Sphere", "Ring / Sphere", "Ring = true, Sphere = false", GH_ParamAccess.item, true);
            pManager.AddNumberParameter(" Radius", " Radius", " Radius", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("Method", "Method", "0 = Quadratic fitting 3 coefficients, 1 = Quadratic fitting 4 coefficients, 2 = Quadratic fitting 6 coefficients, 3 = Cubic fitting 7 coefficients ", GH_ParamAccess.item, 1);
            //pManager.AddBooleanParameter("Static", "Static", "Static", GH_ParamAccess.item, false);
            pManager.AddIntervalParameter("Interval", "Interval", "Interval",GH_ParamAccess.item,new Interval(0,0));
            pManager.AddColourParameter("3Colors", "3Colors", "3 colors",GH_ParamAccess.list);
            pManager.AddBooleanParameter("On", "On", "On", GH_ParamAccess.item, true);
            pManager[0].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Curvature Values", "V", "Curvature Values", GH_ParamAccess.list);
        }
        #endregion
        



        #region properties - values - guid - bitmap
        public int min { get; set; }
        public int max { get; set; }
        public bool Flag { get; set; } //Check if form is opened or not
        public bool display { get; set; } //Gradient or 3 colors
        public int vectorScale { get; set; } //Line length
        public int exp1 { get; set; }
        public int exp2 { get; set; }
        public int curvatureType { get; set; } //curvature Type

        public override Guid ComponentGuid {
            get { return new Guid("{dc4a319c-e358-4e45-8825-7df193eddb1f}"); }
        }

        protected override Bitmap Icon { get { return Properties.Resources.Curvature; } }
        #endregion

        #region solution
        protected override void SolveInstance(IGH_DataAccess DA) {


            low = -(double)(min) * Math.Pow(10, exp1);
            up = (double)(max) * Math.Pow(10, exp1);

            //Input
            bool d1 = true;
            double distance = 2;
            method = 2;
            //curvatureType = 3;
            Interval interval = new Interval(0, 0);

            if (!DA.GetData(0, ref this.mesh)) return;

            ///List<Color> colors = new List<Color>() { Color.FromArgb(255, 0, 0), Color.FromArgb(255, 255, 0), Color.FromArgb(255, 255, 255) };
            List<Color> colors = new List<Color>();
            int ct=0;
            if (DA.GetData(1, ref ct))
                curvatureType = ct;
            
           // DA.GetData(2, ref d1);
            DA.GetData(2, ref distance);
            DA.GetData(3, ref method);
           // DA.GetData(5, ref animate);
            DA.GetData(4, ref interval);
            if (DA.GetDataList(5, colors) && colors.Count == 3) {
                A = colors[0];
                B = colors[1];
                C = colors[2];
            }

            bool run = true;
            DA.GetData(6, ref run);
            //NGonCore.MeshUtil.Clean(mesh);

            //Calculation
            #region
          

                

                topvertex = mesh.TopologyVertices;

                //Values from Mesh_Curvature_Utilities struct
                if (run) {
                    mainK = NGonCore.MeshUtilSimple.Mesh_Cuvature_Utilities_Debugging.MainK(d1, !d1, (int)distance, aMeshEdgeLength * distance, method, curvatureType, mesh, topvertex);

                    Kmed = mainK.y;
                    KT = mainK.x;

                    kmmin = Enumerable.Min(Kmed);
                    kmmax = Enumerable.Max(Kmed);

                    v1 = kmmin + 0.1 * (kmmax - kmmin);
                    v2 = kmmin + 0.9 * (kmmax - kmmin);
                    vertexCol = Enumerable.Repeat(Color.Black, mesh.Vertices.Count).ToList();


                    double[] KmedSort = new double[Kmed.Count];
                    Kmed.CopyTo(KmedSort);
                    KmedSort.ToList().Sort();

                    low = KmedSort[(int)(KmedSort.Length / 3)];
                    up = KmedSort[(int)(2 * (KmedSort.Length / 3))];
                    v1 = low;
                    v2 = up;


                        v1 = kmmin;
                        v2 = kmmax;
              
                } else if(interval.Min!= interval.Max) {
                    v1 = interval.Min;
                    v2 = interval.Max;
                }
                //if (display) colorMesh3Colors();
                //else colorMeshGradient();
                colorMeshGradient();


            #endregion


            //Output
            baseMessage();
            DA.SetData(0, mesh);
            DA.SetDataList(1, Kmed);
        }

        public void colorMesh3Colors() {
            for (int i = 0; i < topvertex.Count; i++) {
                if (Kmed[i] < v1) {
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = A;
                } else if (Kmed[i] < v2) {
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = B;
                } else {
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = C;
                }
            }

            mesh.VertexColors.SetColors(vertexCol.ToArray());
        }

        public void colorMeshGradient() {

            for (int i = 0; i < topvertex.Count; i++) {
                double val = this.KT[i][curvatureType];
                double vm = (v1 + v2) / 2;
                if (val <= vm) {
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = A;
                }

                if (v1 < val && val <= vm) {
                    int val1 = (int)(A.R + (B.R - A.R) * (val - v1) / (vm - v1));
                    int val2 = (int)(A.G + (B.G - A.G) * (val - v1) / (vm - v1));
                    int val3 = (int)(A.B + (B.B - A.B) * (val - v1) / (vm - v1));
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = Color.FromArgb(val1, val2, val3);
                }
                if (vm < val && val <= v2) {
                    int val1 = (int)(B.R + (C.R - B.R) * (val - vm) / (v2 - vm));
                    int val2 = (int)(B.G + (C.G - B.G) * (val - vm) / (v2 - vm));
                    int val3 = (int)(B.B + (C.B - B.B) * (val - vm) / (v2 - vm));
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = Color.FromArgb(val1, val2, val3);
                }
                if (val > v2) {
                    ind = topvertex.MeshVertexIndices(i);
                    vertexCol[ind[0]] = C;
                }

            }
            mesh.VertexColors.SetColors(vertexCol.ToArray());
        }




        public void baseMessage() {

            #region base message


            string curvatureName = "Mean";

            switch (curvatureType) {
                case 0:
                curvatureName = "Max";
                break;
                case 1:
                curvatureName = "Min";
                break;
                case 2:
                curvatureName = "Gaussian";
                break;
                case 3:
                curvatureName = "Mean";
                break;
                case 4:
                curvatureName = "Absolute";
                break;
                case 5:
                curvatureName = "Difference";
                break;
                case 6:
                curvatureName = "Rms";
                break;
            }

            string methodName = "Quadratic fitting 3 coefficients";

            switch (method) {
                case 0:
                methodName = "Quadratic fitting 3 coefficients";
                break;
                case 1:
                methodName = "Quadratic fitting 4 coefficients";
                break;
                case 2:
                methodName = "Quadratic fitting 6 coefficients";
                break;
                case 3:
                methodName = "Cubic fitting 7 coefficients";
                break;
            }

            base.Message = "Min " + Math.Round(kmmin, 5).ToString() + " Max " + Math.Round(kmmax, 5).ToString() + /*System.Environment.NewLine + " 1/3 value " + Math.Round(low, 5).ToString() + " 2/3 value " + Math.Round(up, 5).ToString() +*/ System.Environment.NewLine + methodName.ToString() + System.Environment.NewLine + " Curvature Type: " + curvatureName.ToString();
            #endregion


        }

        #endregion


    }
}
