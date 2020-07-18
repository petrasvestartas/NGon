using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using NGonsCore;
using NGonsCore.Clipper;
using Rhino.Geometry;

namespace NGonGh.Utils {
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


            Dictionary<int, int> eDict = new Dictionary<int, int>();
            int[][] fe = M.GetNGonFacesEdges(tv);

            int i = 0;
            foreach (int meshedge in ehash) {
                eDict.Add(meshedge, i++);
            }

            int[][] fe_ = new int[fe.Length][];
            int[][] fe_0 = new int[fe.Length][];

            for (i = 0; i < fe.Length; i++) {
                fe_[i] = new int[fe[i].Length];
                fe_0[i] = new int[fe[i].Length];

                for (int j = 0; j < fe[i].Length; j++) {
                    fe_[i][j] = eDict[fe[i][j]];
                }

                for (int j = 0; j < fe[i].Length; j++) {
                    fe_0[i][j] = eDict[fe[i][(j+1) % fe[i].Length]];
                }



            }



            List<Vector3d> vecs = new List<Vector3d>();

            i = 0;
            foreach (int n in e) {

                int[] edgeFaces = ef[i];

                Vector3d vec = Vector3d.Zero;

                if (N) {
                    for (int j = 0; j < edgeFaces.Length; j++) {
                        vec += planes[edgeFaces[j]].ZAxis;
                    }
                    vec /= edgeFaces.Length;
                    base.Message = "Average";
                } else {

                    int[] triangleFaces = M.TopologyEdges.GetConnectedFaces(n);

                    for (int j = 0; j < triangleFaces.Length; j++) {
                        vec += M.FaceNormals[triangleFaces[j]]; 
                    }
                    vec /= triangleFaces.Length;
                    base.Message = "Face";

                }

                /*
                //l.Transform(Transform.Rotation())
                Rhino.IndexPair ip = M.TopologyEdges.GetTopologyVertices(n);
                int v0 = M.TopologyVertices.MeshVertexIndices(ip.I)[0];
                int v1 = M.TopologyVertices.MeshVertexIndices(ip.J)[0];
                Vector3d vec = new Vector3d(
                  (M.Normals[v0].X + M.Normals[v1].X) * 0.5,
                  (M.Normals[v0].Y + M.Normals[v1].Y) * 0.5,
                  (M.Normals[v0].Z + M.Normals[v1].Z) * 0.5
                  );
                  */

                vecs.Add(vec);
                i++;
            }

            Line[] lines = M.GetAllNGonEdgesLines(ehash);
            //List<Line> ln = new List<Line>();

            for(int j = 0; j < lines.Length; j++) {
                //Line l = lines[j];
                lines[j].Transform(Rhino.Geometry.Transform.Scale(lines[j].PointAt(0.5), S));
                lines[j].Transform(Rhino.Geometry.Transform.Rotation(A,vecs[j], lines[j].PointAt(0.5)));
            }


            DA.SetDataList(0, lines);
            DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IE2(fe_));
            DA.SetDataTree(2, NGonsCore.GrasshopperUtil.IE2(fe_0));
            DA.SetDataList(3, vecs);


        }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.Reciprocal1;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b148c9-c00b-4bf1-b2b4-1423a5ee1f08"); }
        }
    }
}