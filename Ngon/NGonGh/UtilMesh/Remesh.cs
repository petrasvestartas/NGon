using System;
using System.Collections.Generic;
using g3;
using Grasshopper.Kernel;
using NGonsCore.Clipper;
using Rhino.Geometry;
using Rhino;
using NGonsCore;

namespace NGonGh.Utils {
    public class Remesh : GH_Component {
        /// <summary>
        /// Initializes a new instance of the MeshPipe class.
        /// </summary>
        public Remesh()
          : base("Remesh", "Remesh",
              "Remesh",
            "NGon", "Utilities Mesh") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("FixEdges", "FixEdges", "FixEdges", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("EdgeLength", "EdgeLength", "EdgeLength", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations",GH_ParamAccess.item,10);
            pManager.AddPointParameter("FixPt", "FixPt", "FixPt",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Project", "Project", "Project", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Loops", "Loops", "Preserve Boundary Loops", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = DA.Fetch<Mesh>("Mesh");
            bool fixEdges = DA.Fetch<bool>("FixEdges");
            double l = DA.Fetch<double>("EdgeLength");
            int iterations = DA.Fetch<int>("Iterations");
            List < Point3d > fixPt= DA.FetchList<Point3d>("FixPt");
            bool project = DA.Fetch<bool>("Project");
            bool loops = DA.Fetch<bool>("Loops");

            Mesh mesh = m.DuplicateMesh();
            mesh.Faces.ConvertQuadsToTriangles();

            double len = (l ==0) ? mesh.GetBoundingBox(false).Diagonal.Length * 0.1 : l;

         




       
            //r.PreventNormalFlips = true;

            List<int> ids = new List<int>();
            Point3d[] pts = mesh.Vertices.ToPoint3dArray();
            foreach (Point3d p in fixPt)
                ids.Add(NGonsCore.PointUtil.ClosestPoint(p,pts));





            DMesh3 dmesh = mesh.ToDMesh3();
            Remesher r = new Remesher(dmesh);
            r.Precompute();
            r.SetTargetEdgeLength(len);
            r.SmoothSpeedT = 0.5;
            if(project)
             r.SetProjectionTarget(MeshProjectionTarget.Auto(dmesh));
            
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.EnableSmoothing = true;


            MeshConstraints cons = new MeshConstraints();


            if (ids.Count > 0) {
                foreach (int id in ids) {
                    //cons.SetOrUpdateVertexConstraint(id, new VertexConstraint(true, 1));
                    cons.SetOrUpdateVertexConstraint(id, VertexConstraint.Pinned);
                }
            }
            r.SetExternalConstraints(cons);

            r.Precompute();

            if (fixEdges) {

                //r.SetExternalConstraints(new MeshConstraints());
                MeshConstraintUtil.FixAllBoundaryEdges(r);
               MeshConstraintUtil.FixAllBoundaryEdges_AllowSplit(cons, dmesh, 0);
               //MeshConstraintUtil.FixAllBoundaryEdges_AllowCollapse(cons, dmesh, 0);
                
            }

            if (loops) {
                MeshConstraintUtil.PreserveBoundaryLoops(r);//project to edge
                                                            //MeshConstraintUtil.PreserveBoundaryLoops(cons,dmesh);//project to edge


            }
            r.SetExternalConstraints(cons);


            for (int k = 0; k < iterations; ++k) 
                r.BasicRemeshPass();








            //output
            if (ids.Count > 0 && !fixEdges) {
                this.Message = "Vertices";
            } else if (ids.Count == 0 && fixEdges) {
                this.Message = "Edges";
            } else if (ids.Count > 0 && fixEdges) {
                this.Message = "Vertices + Edges";
            } else {
                this.Message = "";
            }



            dmesh = new DMesh3(dmesh, true);
            Mesh rmesh = dmesh.ToRhinoMesh();
         
            if (loops) {
     

                Mesh mesh_ = rmesh.DuplicateMesh();
                Rhino.IndexPair[] closestEdges = new Rhino.IndexPair[fixPt.Count];

                int counter = 0;
                foreach (Point3d p in fixPt) {
                    double[] d = new double[rmesh.TopologyEdges.Count];
                    int[] eid = new int[rmesh.TopologyEdges.Count];

                    for (int i = 0; i < rmesh.TopologyEdges.Count; i++) {
                        if (rmesh.TopologyEdges.GetConnectedFaces(i).Length == 1) {
                            Line line = rmesh.TopologyEdges.EdgeLine(i);
                            line.ClosestPoint(p,true);
                            d[i] = line.ClosestPoint(p, true).DistanceToSquared(p); //line.From.DistanceToSquared(p) + line.To.DistanceToSquared(p);
                        } else {
                            d[i] = 99999;
                        }
                        eid[i] = i;
                    }



                    Array.Sort(d, eid);

                    closestEdges[counter++] = rmesh.TopologyEdges.GetTopologyVertices(eid[0]);
                }

                for (int i = 0; i < fixPt.Count; i++) {
                    mesh_.Vertices.Add(fixPt[i]);
                    mesh_.Faces.AddFace(rmesh.Vertices.Count + i, closestEdges[i].I, closestEdges[i].J);
                }
                rmesh = mesh_;


            }
            rmesh.UnifyNormals();
            rmesh.RebuildNormals();
           // rmesh.UnifyNormals();







            DA.SetData(0,  rmesh);




            }

        protected override System.Drawing.Bitmap Icon {
            get {

               return Properties.Resources.remesh;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("3718d679-f4f2-40ee-9c0f-14ad2b9e9288"); }
        }
    }
}