using NGonsCore.geometry3Sharp.distance;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.queries;
using NGonsCore.geometry3Sharp.shapes3;

namespace NGonsCore.geometry3Sharp.spatial
{
    public class MeshProjectionTarget : IProjectionTarget
    {
        public NGonsCore.geometry3Sharp.mesh.DMesh3 Mesh { get; set; }
        public ISpatial Spatial { get; set; }

        public MeshProjectionTarget() { }
        public MeshProjectionTarget(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, ISpatial spatial)
        {
            Mesh = mesh;
            Spatial = spatial;
        }

        public Vector3D Project(Vector3D vPoint, int identifier = -1)
        {
            int tNearestID = Spatial.FindNearestTriangle(vPoint);
            DistPoint3Triangle3 q = MeshQueries.TriangleDistance(Mesh, tNearestID, vPoint);
            return q.TriangleClosest;
        }
    }



    public class PlaneProjectionTarget : IProjectionTarget
    {
        public Vector3D Origin;
        public Vector3D Normal;

        public Vector3D Project(Vector3D vPoint, int identifier = -1)
        {
            Vector3D d = vPoint - Origin;
            return Origin + (d - d.Dot(Normal) * Normal);
        }
    }




    public class CircleProjectionTarget : IProjectionTarget
    {
        public Circle3d Circle;

        public Vector3D Project(Vector3D vPoint, int identifier = -1)
        {
            DistPoint3Circle3 d = new DistPoint3Circle3(vPoint, Circle);
            d.GetSquared();
            return d.CircleClosest;
        }
    }



    public class CylinderProjectionTarget : IProjectionTarget
    {
        public Cylinder3d Cylinder;

        public Vector3D Project(Vector3D vPoint, int identifer = -1)
        {
            DistPoint3Cylinder3 d = new DistPoint3Cylinder3(vPoint, Cylinder);
            d.GetSquared();
            return d.CylinderClosest;
        }
    }




    public class SequentialProjectionTarget : IProjectionTarget
    {
        public IProjectionTarget[] Targets { get; set; }

        public SequentialProjectionTarget() { }
        public SequentialProjectionTarget(params IProjectionTarget[] targets)
        {
            Targets = targets;
        }

        public Vector3D Project(Vector3D vPoint, int identifier = -1)
        {
            Vector3D vCur = vPoint;
            for ( int i = 0; i < Targets.Length; ++i ) {
                vCur = Targets[i].Project(vCur, identifier);
            }
            return vCur;
        }
    }

}
