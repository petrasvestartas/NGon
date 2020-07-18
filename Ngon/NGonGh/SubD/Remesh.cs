using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.mesh;
using NGonsCore.geometry3Sharp.mesh_generators;
using NGonsCore.geometry3Sharp.mesh_rhino;
using NGonsCore.geometry3Sharp.spatial;
using Rhino.Geometry;
using NGonsCore;
using NGonsCore.geometry3Sharp.core;
using SubD.Properties;
using MeshUtil = NGonsCore.geometry3Sharp.mesh.MeshUtil;
using NGonsCore.geometry3Sharp.shapes3;

namespace SubD.SubD {
    public class Remesh : GH_Component {

        public Remesh()
          : base("Remesh", "Remesh",
              "Remesh",
              "SubD", "Sub") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Min", "Min", "Min edge length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max", "Max", "Max edge length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Angle for snapping", GH_ParamAccess.item,30);
            pManager.AddIntegerParameter("T", "T", "1 - region, 2 - basic closed, 3 - smoothinh, 4 - constraints vertex curves 0/n - constraints fixed vertices", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("I", "I", "I", GH_ParamAccess.item, 10);

            for (int i = 1; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

    
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {


            double min = 1;
            double max = 10;
            bool flag1 =DA.GetData(1, ref min);
            bool flag2 = DA.GetData(2, ref max);
            double angle = 30;
            DA.GetData(3, ref angle);
            int iterations = 1;
            int type = 0;
            DA.GetData(4, ref type);
            DA.GetData(5, ref iterations);
            iterations = (iterations < 0) ? 0 : iterations;



            Mesh m = new Mesh();
            
            if (DA.GetData(0, ref m))
            {
                if (!flag1 && !flag2) {
                    for (int i = 0; i < m.TopologyEdges.Count; i++)
                        min += m.TopologyEdges.EdgeLine(i).Length;
                    min/= m.TopologyEdges.Count;
                    min = Math.Round(min,3);
                    max = min;
                    min *= 0.5;

                }
                

                //Input
                m.Faces.ConvertQuadsToTriangles();
                m.Clean();
                DMesh3 gMesh3 = NGonsCore.geometry3Sharp.mesh_rhino.mesh_rhino.ToDMesh3(m); ;


                switch (type)
                {
                    case (1):
                        gMesh3 = remesh_region(iterations, gMesh3, min, max, angle);
                        base.Message = "Min: " + min.ToString() + "\n" + "Max: " + max.ToString() + "\n" + "region";
                    break;
                    case (2):
                        gMesh3 = remesh_basic_closed(iterations, gMesh3, min, max, angle);
                        base.Message = "Min: " + min.ToString() + "\n" + "Max: " + max.ToString() + "\n" + "basic closed";
                    break;
                    case (3):
                        gMesh3 = remesh_smoothing(iterations, gMesh3);
                        base.Message = "Min: " + min.ToString() + "\n" + "Max: " + max.ToString() + "\n" + "smoothing";
                    break;
                    case (4):
                        gMesh3 = remesh_constraints_vertcurves(iterations, gMesh3, min, max, angle);
                        base.Message = "Min: " + min.ToString() + "\n" + "Max: " + max.ToString() + "\n" + "constraints vertex curves";
                    break;

                    default:
                        gMesh3 = remesh_constraints_fixedverts(iterations, gMesh3, min, max, angle);
                        base.Message = "Min: " + min.ToString() + "\n" + "Max: " + max.ToString() + "\n" + "contraints fixed vertex";
                    break;
                        
                }



                //Compact and output
                gMesh3 = new DMesh3(gMesh3, true);
                DA.SetData(0, gMesh3.ToRhinoMesh());
            }
        }



        public DMesh3 remesh_constraints_fixedverts(int iterations, DMesh3 mesh, double min, double max,double angle)
        {

            AxisAlignedBox3d bounds = mesh.CachedBounds;

            // construct mesh projection target
            DMesh3 meshCopy = new DMesh3(mesh);
            meshCopy.CheckValidity();
            DMeshAABBTree3 tree = new DMeshAABBTree3(meshCopy);
            tree.Build();
            MeshProjectionTarget target = new MeshProjectionTarget() { Mesh = meshCopy, Spatial = tree };

            // construct constraint set
            MeshConstraints cons = new MeshConstraints();

            //EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip | EdgeRefineFlags.NoCollapse;
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;

            foreach (int eid in mesh.EdgeIndices()) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > angle) {
                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags));
                    Index2i ev = mesh.GetEdgeV(eid);
                    int nSetID0 = (mesh.GetVertex(ev[0]).y > bounds.Center.y) ? 1 : 2;
                    int nSetID1 = (mesh.GetVertex(ev[1]).y > bounds.Center.y) ? 1 : 2;
                    cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(true, nSetID0));
                    cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(true, nSetID1));
                }
            }

            Remesher r = new Remesher(mesh);
            r.Precompute();
            r.SetExternalConstraints(cons);
            r.SetProjectionTarget(target);
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = min;
            r.MaxEdgeLength = max;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 1;


              for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }


            return mesh;

        }

        public DMesh3 remesh_basic_closed(int iterations, DMesh3 mesh,double min, double max, double angle) {

            Remesher r = new Remesher(mesh);
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = min;
            r.MaxEdgeLength = max;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.1f;

            r.EnableFlips = r.EnableSmoothing = false;
            r.MinEdgeLength = min;
            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            r.MinEdgeLength = min;
            r.MaxEdgeLength = max;
            r.EnableFlips = r.EnableCollapses = r.EnableSmoothing = true;

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            r.EnableSplits = r.EnableCollapses = false;

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            return mesh;

        }

        public DMesh3 remesh_smoothing(int iterations, DMesh3 mesh) {

            Remesher r = new Remesher(mesh);
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = false;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 0.5f;
            r.SmoothType = Remesher.SmoothTypes.MeanValue;

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            return mesh;
        }
        
        public DMesh3 remesh_constraints_vertcurves(int iterations, DMesh3 mesh, double min, double max, double angle) {


            mesh.CheckValidity();
            AxisAlignedBox3d bounds = mesh.CachedBounds;

            // construct mesh projection target
            DMesh3 meshCopy = new DMesh3(mesh);
            meshCopy.CheckValidity();
            DMeshAABBTree3 tree = new DMeshAABBTree3(meshCopy);
            tree.Build();
            MeshProjectionTarget mesh_target = new MeshProjectionTarget() {
                Mesh = meshCopy,
                Spatial = tree
            };

            // cylinder projection target
            CylinderProjectionTarget cyl_target = new CylinderProjectionTarget() {
                Cylinder = new Cylinder3d(new Vector3D(0, 1, 0), Vector3D.AxisY, 1, 2)
            };

            //IProjectionTarget target = mesh_target;
            IProjectionTarget target = cyl_target;

            // construct projection target circles
            CircleProjectionTarget bottomCons = new CircleProjectionTarget() {
                Circle = new Circle3d(bounds.Center, 1.0)
            };
            bottomCons.Circle.Center.y = bounds.Min.y;
            CircleProjectionTarget topCons = new CircleProjectionTarget() {
                Circle = new Circle3d(bounds.Center, 1.0)
            };
            topCons.Circle.Center.y = bounds.Max.y;


            // construct constraint set
            MeshConstraints cons = new MeshConstraints();

            //EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip | EdgeRefineFlags.NoCollapse;
            EdgeRefineFlags useFlags = EdgeRefineFlags.NoFlip;

            bool bConstrainVertices = true;
            foreach (int eid in mesh.EdgeIndices()) {
                double fAngle = MeshUtil.OpeningAngleD(mesh, eid);
                if (fAngle > 30.0f) {
                    Index2i ev = mesh.GetEdgeV(eid);
                    Vector3D ev0 = mesh.GetVertex(ev[0]);
                    Vector3D ev1 = mesh.GetVertex(ev[1]);
                    CircleProjectionTarget loopTarget = null;
                    if (ev0.y > bounds.Center.y && ev1.y > bounds.Center.y)
                        loopTarget = topCons;
                    else if (ev0.y < bounds.Center.y && ev1.y < bounds.Center.y)
                        loopTarget = bottomCons;

                    cons.SetOrUpdateEdgeConstraint(eid, new EdgeConstraint(useFlags, loopTarget));
                    if (bConstrainVertices && loopTarget != null) {
                        cons.SetOrUpdateVertexConstraint(ev[0], new VertexConstraint(loopTarget));
                        cons.SetOrUpdateVertexConstraint(ev[1], new VertexConstraint(loopTarget));
                    }
                }
            }


            Remesher r = new Remesher(mesh);
            //r.SetExternalConstraints(cons);
            r.SetProjectionTarget(target);
            r.Precompute();
            r.ENABLE_PROFILING = true;
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = min;
            r.MaxEdgeLength = max;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 1.0f;

                for (int k = 0; k < iterations; ++k) {
                    r.BasicRemeshPass();
                    mesh.CheckValidity();
                }

            return mesh;

        }
        
        public DMesh3 remesh_region(int iterations, DMesh3 mesh, double min, double max, double angle) {


            int[] tris = GetTrisOnPositiveSide(mesh, new Frame3f(Vector3F.Zero, Vector3F.AxisY));

            RegionRemesher r = new RegionRemesher(mesh, tris);
            r.Region.SubMesh.CheckValidity(true);

            r.Precompute();
            r.EnableFlips = r.EnableSplits = r.EnableCollapses = true;
            r.MinEdgeLength = min;
            r.MaxEdgeLength = max;
            r.EnableSmoothing = true;
            r.SmoothSpeedT = 1.0f;

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }


            r.BackPropropagate();

            for (int k = 0; k < iterations; ++k) {
                r.BasicRemeshPass();
                mesh.CheckValidity();
            }

            r.BackPropropagate();

            return mesh;
        }

        public int[] GetTrisOnPositiveSide(DMesh3 mesh, Frame3f plane) {
            DVector<int> keep_tris = new DVector<int>();

            Vector3D[] tri = new Vector3D[3];
            foreach (int tid in mesh.TriangleIndices()) {
                mesh.GetTriVertices(tid, ref tri[0], ref tri[1], ref tri[2]);
                bool ok = true;
                for (int j = 0; j < 3; ++j) {
                    double d = (tri[j] - plane.Origin).Dot(plane.Z);
                    if (d < 0)
                        ok = false;
                }
                if (ok)
                    keep_tris.Add(tid);
            }

            return keep_tris.GetBuffer();
        }


        protected override System.Drawing.Bitmap Icon => Resources.Remesh_35;
        
        public override Guid ComponentGuid => new Guid("9e4d0bf8-1f58-4c07-b5e6-a4d5706a205c");
    }
}