using System.Collections.Generic;
using NGonsCore.geometry3Sharp.curve;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.mesh {
	
	public static class MeshUtil {





		// t in range [0,1]
		public static Vector3D UniformSmooth(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int vID, double t) 
		{
			Vector3D v = mesh.GetVertex(vID);
			Vector3D c = MeshWeights.OneRingCentroid(mesh, vID);
			return (1-t)*v + (t)*c;
		}

		// t in range [0,1]
		public static Vector3D MeanValueSmooth(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int vID, double t) 
		{
			Vector3D v = mesh.GetVertex(vID);
			Vector3D c = MeshWeights.MeanValueCentroid(mesh, vID);
			return (1-t)*v + (t)*c;
		}

		// t in range [0,1]
		public static Vector3D CotanSmooth(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int vID, double t) 
		{
			Vector3D v = mesh.GetVertex(vID);
			Vector3D c = MeshWeights.CotanCentroid(mesh, vID);
			return (1-t)*v + (t)*c;
		}


		public static void ScaleMesh(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, Frame3f f, Vector3F vScale) {
			foreach ( int vid in mesh.VertexIndices() ) {
				Vector3D v = mesh.GetVertex(vid);
				Vector3F vScaledInF = f.ToFrameP((Vector3F)v) * vScale;
				Vector3D vNew = f.FromFrameP(vScaledInF);
				mesh.SetVertex(vid, vNew);

				// TODO: normals
			}
		}




        public static double OpeningAngleD(NGonsCore.geometry3Sharp.mesh.DMesh3 mesh, int eid)
        {
            Index2i et = mesh.GetEdgeT(eid);
            if (et[1] == NGonsCore.geometry3Sharp.mesh.DMesh3.InvalidID)
                return double.MaxValue;     // boundary edge!!

            Vector3D n0 = mesh.GetTriNormal(et[0]);
            Vector3D n1 = mesh.GetTriNormal(et[1]);
            return Vector3D.AngleD(n0, n1);
        }





        public static DCurve3 ExtractLoopV(IMesh mesh, IEnumerable<int> vertices) {
            DCurve3 curve = new DCurve3();
            foreach (int vid in vertices)
                curve.AppendVertex(mesh.GetVertex(vid));
            curve.Closed = true;
            return curve;
        }
        public static DCurve3 ExtractLoopV(IMesh mesh, int[] vertices) {
            DCurve3 curve = new DCurve3();
            for (int i = 0; i < vertices.Length; ++i)
                curve.AppendVertex(mesh.GetVertex(vertices[i]));
            curve.Closed = true;
            return curve;
        }

	}
}
