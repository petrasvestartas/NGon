using System;
using System.Collections.Generic;
using g3;
using Grasshopper.Kernel;
using NGonCore.Clipper;
using Rhino.Geometry;
using Rhino;
using NGonCore;

namespace NGon_RH8.Utils {
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
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.list);
            pManager.AddBooleanParameter("FixEdges", "FixEdges", "FixEdges", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("EdgeLength", "EdgeLength", "EdgeLength", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Iterations", "Iterations", "Iterations",GH_ParamAccess.item,10);
            pManager.AddPointParameter("FixPt", "FixPt", "FixPt",GH_ParamAccess.list);
            pManager.AddBooleanParameter("Project", "Project", "Project", GH_ParamAccess.item, true);
            //pManager.AddBooleanParameter("Loops", "Loops", "Preserve Boundary Loops", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
           // pManager[6].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh","M","Mesh",GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            //https://github.com/gradientspace/geometry3SharpDemos/blob/master/geometry3Test/test_Remesher.cs
            var m = DA.FetchList<Mesh>("Mesh");
            bool fixEdges = DA.Fetch<bool>("FixEdges");
            double l = DA.Fetch<double>("EdgeLength");
            int iterations = DA.Fetch<int>("Iterations");
            List < Point3d > fixPt= DA.FetchList<Point3d>("FixPt");
            bool project = DA.Fetch<bool>("Project");
            //bool loops = DA.Fetch<bool>("Loops");
            //loops = !loops;

            DMesh3 dmesh = new DMesh3();
            dmesh.EnableTriangleGroups();
            double len = 0;
            List<int> ids = new List<int>();

            int vCount = 0;
            int fCount = 0;
            for (int i = 0; i < m.Count; i++) {

                Mesh mesh = m[i].DuplicateMesh();
                mesh.Faces.ConvertQuadsToTriangles();


                mesh.Compact();
                mesh.Vertices.CombineIdentical(true, true);
                mesh.Vertices.CullUnused();
                mesh.Weld(3.14159265358979);
                mesh.FaceNormals.ComputeFaceNormals();
                mesh.RebuildNormals();



                Point3d[] pts = mesh.Vertices.ToPoint3dArray();
                foreach (Point3d p in fixPt)
                    ids.Add(NGonCore.PointUtil.ClosestPoint(p, pts));

                DMesh3 dmeshTemp = mesh.ToDMesh3(i+1);
                //Rhino.RhinoApp.WriteLine(dmeshTemp.HasTriangleGroups.ToString());
                MeshEditor.Append(dmesh, dmeshTemp);
                //dmesh=MeshEditor.Combine(dmesh, dmeshTemp);
                //Rhino.RhinoApp.WriteLine(dmesh.HasTriangleGroups.ToString());
                vCount += mesh.Vertices.Count;
                fCount += mesh.Faces.Count;
            }
            //Rhino.RhinoApp.WriteLine(dmesh.HasTriangleGroups.ToString());

            len = (l == 0) ? m[0].GetBoundingBox(false).Diagonal.Length * 0.1 : l;

           




           
             Remesher r = new Remesher(dmesh);
            r.Precompute();

            MeshConstraints cons = new MeshConstraints();


            if (ids.Count > 0)
                foreach (int id in ids)
                    cons.SetOrUpdateVertexConstraint(id, VertexConstraint.Pinned);


            if (fixEdges) {
                MeshConstraintUtil.FixAllBoundaryEdges_AllowSplit(cons, dmesh, 1);
                MeshConstraintUtil.FixAllBoundaryEdges_AllowCollapse(cons, dmesh, 1);
                MeshConstraintUtil.PreserveBoundaryLoops(r);
            }


            for (int i = 0; i < ids.Count; i++) {
                //cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags));
               cons.SetOrUpdateVertexConstraint(ids[i], new VertexConstraint(true, 0));
            }



            if (m.Count > 1) {
                int set_id = 1;
                int[][] group_tri_sets = FaceGroupUtil.FindTriangleSetsByGroup(dmesh);
                foreach (int[] tri_list in group_tri_sets) {
                    MeshRegionBoundaryLoops loops = new MeshRegionBoundaryLoops(dmesh, tri_list);
                    foreach (EdgeLoop loop in loops) {
                        MeshConstraintUtil.ConstrainVtxLoopTo(r, loop.Vertices,
                            new DCurveProjectionTarget(loop.ToCurve()), set_id++);
                    }
                }
            }





            //Set Parameters and remesh
            r.SetTargetEdgeLength(len);
       
            if (project) {
                r.SmoothSpeedT = 0.5;
                r.SetProjectionTarget(MeshProjectionTarget.Auto(dmesh));
            } else {
               r.SmoothSpeedT =0.1;
                r.SetProjectionTarget(MeshProjectionTarget.Auto(dmesh));
            }

           // r.SetExternalConstraints(cons);
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.EnableSplits = true;
            r.EnableSmoothing = true;
            

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
            }

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
            //MeshEditor.RemoveFinTriangles(dmesh);
            //MeshEditor.RemoveFinTriangles(dmesh);



            Mesh rmesh = dmesh.ToRhinoMesh();


            List<int> cpFaces = new List<int>(fixPt.Count);
            var hash = new HashSet<int>();
            foreach (Point3d p in fixPt) {
                MeshPoint mp = rmesh.ClosestMeshPoint(p, 0.01);
                cpFaces.Add(mp.FaceIndex);
                hash.Add(mp.FaceIndex);
            }
            var array = cpFaces.ToArray();
            Array.Sort(array);

            bool[] f = rmesh.GetNakedEdgePointStatus();

            List<int> facesToRemove = new List<int>();
            for (int i = 0; i < rmesh.Vertices.Count; i++) {
                if (!f[i]) continue;

                int[] faces = rmesh.TopologyVertices.ConnectedFaces(i);
                if (faces.Length == 1)
                    if (!hash.Contains(faces[0]))
                        facesToRemove.Add(faces[0]);
            }

            rmesh.Faces.DeleteFaces(facesToRemove);
           rmesh.RebuildNormals();
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