using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonCore;
using NGonCore.Clipper;
using Rhino.Geometry;

namespace NGon_RH8.Utils {
    public class RecipricalEdges : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public RecipricalEdges()
          : base("RecipricalEdges", "RecipricalEdges",
              "Rotate mesh edge by average normal",
              "NGon", "Reciprocal") {
       
            }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Angle", GH_ParamAccess.item,0.7);
            pManager.AddNumberParameter("Scale", "S", "Scale", GH_ParamAccess.item,1.4);
            pManager.AddBooleanParameter("NormalType","T","0 - normal is used as an average ngon plane, 1 - normal of adjacent triangle face",GH_ParamAccess.item,true);

            // pManager.AddBooleanParameter("Ending", "F", "", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddCurveParameter("Curve","C","Curve",GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID0", "I0", "First Line for collision", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ID1", "I1", "Second Line for collision", GH_ParamAccess.list);
            pManager.AddVectorParameter("Vectors", "V", "Rotation Vectors", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh M = DA.Fetch<Mesh>("Mesh");
            double A = DA.Fetch<double>("Angle");
            double S = DA.Fetch<double>("Scale");
            bool N = DA.Fetch<bool>("NormalType");

            int[][] tv = M.GetNGonsTopoBoundaries();
            HashSet<int> ehash = M.GetAllNGonEdges(tv);
            int[] e = ehash.ToArray();
            int[][] ef = M.GetNgonsConnectedToNGonsEdges(ehash, false);
            Plane[] planes = M.GetNgonPlanes();
   


            Dictionary<int, int> eDict = new Dictionary<int, int>();//me ne
            int[][] fe = M.GetNGonFacesEdges(tv);

            int i = 0;
            foreach (int meshedge in ehash) {
                eDict.Add(meshedge, i++);
            }

            /////////////////////////////////////////////////////////////////////////////////////////
            ///Adjacency
            /////////////////////////////////////////////////////////////////////////////////////////
            
            //int[][] fe_ = new int[fe.Length][];
            //int[][] fe_0 = new int[fe.Length][];

            var fe_ = new List<List<int>>(fe.Length);
            var fe_0 = new List<List<int>>(fe.Length);

            for (i = 0; i < fe.Length; i++) {

                var list_fe_ = new List<int>(fe[i].Length);
                var list_fe_0 = new List<int>(fe[i].Length);

                //fe_[i] = new int[fe[i].Length];
                //fe_0[i] = new int[fe[i].Length];

                for (int j = 0; j < fe[i].Length; j++) {
                    if (M.TopologyEdges.GetConnectedFaces(fe[i][j]).Length == 1) continue;
                    list_fe_.Add(eDict[fe[i][j]]);
                    //fe_[i][j] = eDict[fe[i][j]];
                }

                for (int j = 0; j < fe[i].Length; j++) {
                    if (M.TopologyEdges.GetConnectedFaces(fe[i][(j + 1) % fe[i].Length]).Length == 1) continue;
                    list_fe_0.Add(eDict[fe[i][(j + 1) % fe[i].Length]]);
                    //fe_0[i][j] = eDict[fe[i][(j+1) % fe[i].Length]];
                }
                if (list_fe_.Count > 0) {
                    fe_.Add(list_fe_);
                    fe_0.Add(list_fe_0);
                }



            }

            /////////////////////////////////////////////////////////////////////////////////////////
            ///Rotation Vectors
            /////////////////////////////////////////////////////////////////////////////////////////


            List<Vector3d> vecs = new List<Vector3d>();
            var lines = new List<Line>();

            if (N) {
                base.Message = "Average";
            } else {
                base.Message = "Face";
            }

            i = 0;
            foreach (int n in e) {

                
                int[] edgeFaces = ef[i];
                i++;
               if (edgeFaces.Length == 1) continue;

                Vector3d vec = Vector3d.Zero;

                if (N) {

                    //Rhino.RhinoApp.WriteLine(edgeFaces.Length.ToString());
                    for (int j = 0; j < edgeFaces.Length; j++) {
                        vec += planes[edgeFaces[j]].ZAxis;
                    }
                    vec /= edgeFaces.Length;
                   

                } else {

                    int[] triangleFaces = M.TopologyEdges.GetConnectedFaces(n);

                    for (int j = 0; j < triangleFaces.Length; j++) {
                        vec += M.FaceNormals[triangleFaces[j]]; 
                    }
                    vec /= triangleFaces.Length;
                   

                }


                vecs.Add(vec);
                lines.Add(M.TopologyEdges.EdgeLine(n));
              
            }

            //Line[] lines = M.GetAllNGonEdgesLines(ehash);
            //List<Line> ln = new List<Line>();






            /////////////////////////////////////////////////////////////////////////////////////////
            ///Line Scale
            /////////////////////////////////////////////////////////////////////////////////////////


            for (int j = 0; j < lines.Count; j++) {

                Line l = new Line(lines[j].From, lines[j].To);
            
                //Scale to fixed length or not
                if (S < 0) {
                    l.Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S/lines[j].Length));
                } else {
                    l.Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S));
                }

                l.Transform(Rhino.Geometry.Transform.Rotation(A,vecs[j], lines[j].PointAt(0.5)));
                lines[j] = l;
            }

            /////////////////////////////////////////////////////////////////////////////////////////
            ///Output
            /////////////////////////////////////////////////////////////////////////////////////////

            DA.SetDataList(0, lines);
            DA.SetDataTree(1, NGonCore.GrasshopperUtil.IE2(fe_));
            DA.SetDataTree(2, NGonCore.GrasshopperUtil.IE2(fe_0));
            DA.SetDataList(3, vecs);



            }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.ReciprocalSimple;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-1423a5ee1f08"); }
        }
    }
}