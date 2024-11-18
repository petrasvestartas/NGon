//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TriangleNet.Geometry;
//using TriangleNet.Topology;

//namespace RhinoGeometry {
//    public static class Triangulate {
//        public static Rhino.Geometry.Mesh MeshFromClosedPolylineWithHoles(Polyline[] _polylines) {


//            Rhino.Geometry.Mesh m = new Rhino.Geometry.Mesh();
//            TriangleNet.Mesh tMesh;
//            List<int> elements = new List<int>();
//            List<Point3d> allPt = new List<Point3d>();
//            Plane plane;
//            List<int> holes = new List<int>();
//            Polyline[] polylines = new Polyline[_polylines.Length];
//            Polygon polygon = new Polygon();


//            for (int i = 0; i < _polylines.Length; i++)
//                polylines[i] = new Polyline(_polylines[i]);

//            if (polylines.Length == 0) return new Rhino.Geometry.Mesh();


//            int counter = 0;
//            foreach (Polyline poly in polylines) {
//                if (!poly.IsValid)
//                    return new Rhino.Geometry.Mesh();
//                else if (counter > 0)
//                    holes.Add(counter);
//                counter++;
//            }


//            Plane.FitPlaneToPoints(polylines[0], out plane);
//            for (int i = 0; i < polylines.Length; i++) {
//                allPt.AddRange(polylines[i].GetRange(0, polylines[i].Count - 1));
//                if (i == 0) {
//                    Point3d center = polylines[i].CenterPoint();
//                    plane.Origin = center;
//                }
//                polylines[i].Transform(Transform.PlaneToPlane(plane, Plane.WorldXY));
//            }



//            for (int i = 0; i < polylines.Length; i++) {

//                TriangleNet.Geometry.Vertex[] points = new TriangleNet.Geometry.Vertex[polylines[i].Count - 1];

//                for (int j = 0; j < polylines[i].Count - 1; j++) {
//                    Point3d item = polylines[i][j];
//                    points[j] = new TriangleNet.Geometry.Vertex(item.X, item.Y);
//                }


//                if (!holes.Contains(i)) {

//                    polygon.Add(new Contour(points, i, true), false);
//                    polygon.Regions.Add(new RegionPointer(2.5, 0, i));
//                } else {
//                    polygon.Add(new Contour(points, i, false), true);
//                    polygon.Regions.Add(new RegionPointer(1.5, 0, i));
//                }

//            }


//            tMesh = (TriangleNet.Mesh)polygon.Triangulate(); //bug


//            m.Vertices.AddVertices(allPt);



//            // foreach (TriangleNet.Geometry.Vertex v in tMesh.Vertices) {
//            // m.Vertices.Add(v.X, v.Y, 0);
//            //}

//            foreach (Triangle t in tMesh.Triangles) {
//                m.Faces.AddFace(t.GetVertexID(0), t.GetVertexID(1), t.GetVertexID(2));
//            }



//            //m.Clean();

//            return m;




//        }
//    }
//}
