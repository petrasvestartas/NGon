using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore {
    public static class MeshDual {

        public static Mesh Dual(this Mesh mesh, int type = 0, Surface surface = null, List<Point3d> pts = null) {

            Mesh x = mesh.DuplicateMesh();

            bool hasOnlyTriangleQuadNgons = true;
            for (int i = 0; i < x.Ngons.Count; i++)
                if (x.Ngons[i].BoundaryVertexCount > 4) {
                    hasOnlyTriangleQuadNgons = false;
                    break;
                }
            if (hasOnlyTriangleQuadNgons)
                x.Ngons.Clear();


            int Type = Math.Abs(type);


            Mesh dual = new Mesh();

            if (surface != null) {
                Surface s = surface;
                s.SetDomain(0, new Interval(0, 1));
                s.SetDomain(1, new Interval(0, 1));
            }

            if (x.Ngons.Count == 0) {




                //Get face center
                int count = 0;
                foreach (MeshFace mf in x.Faces) {

                    Point3d barycenter = Point3d.Origin;

                    if (pts != null)
                        if (pts.Count == x.Faces.Count)
                            Type = 2;

                    switch (Type) {
                      
                        case (2):
                        barycenter = pts[count];
                        break;

                        case (1):
                        if (mf.IsQuad) {
                            barycenter = new Polyline(new Point3d[] { x.Vertices[mf.A], x.Vertices[mf.B], x.Vertices[mf.C], x.Vertices[mf.D], x.Vertices[mf.A] }).CenterPoint();
                        } else {
                            barycenter = new Polyline(new Point3d[] { x.Vertices[mf.A], x.Vertices[mf.B], x.Vertices[mf.C], x.Vertices[mf.A] }).CenterPoint();
                        }
                        break;

                        default:

                        barycenter += (x.Vertices[mf.A]);
                        barycenter += (x.Vertices[mf.B]);
                        barycenter += (x.Vertices[mf.C]);

                        if (mf.IsQuad) {
                            barycenter += (x.Vertices[mf.D]);
                            barycenter /= 4;
                        } else {
                            barycenter /= 3;
                        }
                        break;
                    }
                    


                    dual.Vertices.Add(barycenter);
                    count++;
                }


                //Apply dual
                List<Polyline> polylines = new List<Polyline>();
                bool[] nakedv = x.GetNakedEdgePointStatus();
                Point3d[] midPoints = MeshUtilSimple.EdgeMidPoints(x);
                dual.Vertices.AddVertices(midPoints);

                //int[] edges = x.TopologyVertices.ConnectedEdges(vID);
                //foreach (int edgeID in edges) {
                //    int[] edgeFaces = x.TopologyEdges.GetConnectedFaces(edgeID);
                //    if (edgeFaces.Length == 1) {
                //        nakedFaces.Add(edgeFaces[0]);
                //        nakedEdges.Add(edgeID);
                //        nakedEdgesP.Add(midPoints[edgeID]);
                //        // nakedEdgesPDistToLastP.Add(lastPoint.DistanceToSquared(midPoints[edgeID]));
                //    }
                //}

                    for (int i = 0; i < x.Vertices.Count; i++) {

                    if (nakedv[i] && type < 0)
                        continue;

                        int vID = x.TopologyVertices.TopologyVertexIndex(i);
                    int[] vf = x.TopologyVertices.ConnectedFaces(vID);
                  

                    int[] sortedFaces = SortFacesConnectedToTopologyVertex(x, vf);

                    //You can select vertices from dual mesh
                    //Or if you calculate barycenter here, then you will have duplicate points

                    if (vf.Length > 0) {//2

                        Polyline polyline = new Polyline();
                        List<int> id = new List<int>();

                        for (int j = 0; j < sortedFaces.Length; j++) {
                            polyline.Add(dual.Vertices[sortedFaces[j]]);
                            id.Add(sortedFaces[j]);
                        }

              


                        if (nakedv[i] ) {

                            Point3d lastPoint = polyline.Last();
                            List<int> nakedEdges = new List<int>();
                            List<Point3d> nakedEdgesP = new List<Point3d>();
                            List<double> nakedEdgesPDistToLastP = new List<double>();
                            List<int> nakedFaces = new List<int>();

                            int[] edges = x.TopologyVertices.ConnectedEdges(vID);
                            int numberOfNakedV = 0;
                            foreach (int edgeID in edges) {
                                int[] edgeFaces = x.TopologyEdges.GetConnectedFaces(edgeID);
                                if (edgeFaces.Length == 1) {
                                    numberOfNakedV++;
                                    nakedFaces.Add(edgeFaces[0]);
                                    nakedEdges.Add(edgeID);
                                    nakedEdgesP.Add(midPoints[edgeID]);
                                   // nakedEdgesPDistToLastP.Add(lastPoint.DistanceToSquared(midPoints[edgeID]));
                                }
                            }

                            if (nakedFaces.Count == 2) {
                                if (id.Last() == nakedFaces[0]) {
                                    polyline.Add(nakedEdgesP[0]);

                                    if (vf.Length <= 10)
                                        polyline.Add(x.Vertices[vID]);

                                    polyline.Add(nakedEdgesP[1]);

                                    //id.Add(nakedFaces[0]);
                                    //id.Add(nakedFaces[1]);
                                } else {
                                    polyline.Add(nakedEdgesP[1]);
                                    if (vf.Length <= 10)
                                        polyline.Add(x.Vertices[vID]);

                                    polyline.Add(nakedEdgesP[0]);
                                    //id.Add(nakedFaces[1]);
                                    //id.Add(nakedFaces[0]);
                                }
                            }



                        }
          

                        polyline.Add(polyline[0]);
                        polylines.Add(polyline);


                        //MeshFace[] faces = polyline.TriangulateClosedPolyline();
                        //if (faces != null) {

                        //    int f = dual.Faces.Count;//current dual faces
                        //    int[] faceId = new int[faces.Length];//triangulate polyline faces


                        //    for (int j = 0; j < faces.Length; j++) {
                        //        //id - face id
                        //        dual.Faces.AddFace(id[faces[j].A], id[faces[j].B], id[faces[j].C]);
                        //        faceId[j] = f++;
                        //    }

                        //    dual.Ngons.AddNgon(MeshNgon.Create(id, faceId));

                        //}
                    }
                }
                dual = MeshCreate.MeshFromPolylines(polylines,0);
            } else {

                if (pts != null)
                    if (pts.Count == x.Ngons.Count)
                        Type = 2;


                int[][] tv = x.GetNGonsTopoBoundaries();
                HashSet<int> allV = x.GetAllNGonsVertices();
                List<Polyline> polylines = new List<Polyline>();
                int[][] vf = x.GetNGonsConnectedToNGonVertices(allV);
                int[][] vf_ = new int[vf.Length][];
                List<int>[] ff = x.GetNgonFaceAdjacencyOrdered();
                bool[] naked = x.GetNakedNGonPointStatus(allV);

                for (int i = 0; i < vf.Length; i++) {

                    if (naked[i])
                        continue;

                    vf_[i] = new int[vf[i].Length];
                    HashSet<int> tempHash = new HashSet<int>();
                    Polyline outline = new Polyline();


                    for (int j = 0; j < vf[i].Length; j++) {

                        if (j == 0) {
                            vf_[i][j] = vf[i][0];
                            tempHash.Add(vf[i][0]);

        
                            switch (Type) {



                                case (2):
                                outline.Add(pts[vf[i][0]]);
                                break;

                                case (1):
                                outline.Add(x.GetNgonCenter(vf[i][0]));
                                break;

                                default:
                                outline.Add(x.Ngons.GetNgonCenter(vf[i][0]));
                                break;
                            }



                        } else {

                            List<int> ffLocal = ff[vf_[i][j - 1]];
                            List<int> neiF = ffLocal.Intersect(vf[i]).ToList();

                            foreach (int nf in neiF)
                                if (tempHash.Add(nf)) {
                                    vf_[i][j] = nf;


                                    switch (Type) {

                                        case (2):
                                        outline.Add(pts[nf]);
                                        break;

                                        case (1):
                                        outline.Add(x.GetNgonCenter(nf));
                                        break;

                                        default:
                                        outline.Add(x.Ngons.GetNgonCenter(nf));
                                        break;
                                    }


                                    break;
                                }//if
                        }//if
                    }//for j

                    outline.Close();
                    polylines.Add(outline);
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(outline);

                }//for i

                dual = MeshCreate.MeshFromPolylines(polylines, 0.01);

            }//if ngon



            dual.Clean();

            //var bfs = NGonCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(dual, "0");
            //dual = NGonCore.MeshCreate.MeshFromPolylines(dual.UnifyWinding(bfs.Item2[0].ToArray()), 0.01);

            return dual;
        }

        private static int[] SortFacesConnectedToTopologyVertex(this Mesh mesh, int[] vf) {


            int n = vf.Length;
            //Print(n.ToString());

            //Can be only one face connected to naked edge or two
            //These cases does not form  mesh face
            if (n <= 2)
                return vf;

            var vfList = vf.ToList();

            //Start with one of naked faces
            for (int i = 0; i < n; i++) {

                int[] e = mesh.TopologyEdges.GetEdgesForFace(vf[i]);
                for (int j = 0; j < e.Length; j++) {

                    int[] ef = mesh.TopologyEdges.GetConnectedFaces(e[j]);

                    if (ef.Length == 1) {
                        int naked = vf[i];
                        vfList.RemoveAt(i);
                        vfList.Insert(0, naked);
                        break;
                    }

                }

            }

            vf = vfList.ToArray();


            //Output
            int[] sortedFaceID = new int[n];
            sortedFaceID[0] = vf[0];

            //Face vertices
            //flattened array 0   1  2  3  4
            //                19 25 26 29 34
            int[][] faceE = new int[n][];
            for (int i = 0; i < n; i++)
                faceE[i] = mesh.TopologyEdges.GetEdgesForFace(vf[i]);


            //Visited list
            List<int> notvisited = Enumerable.Range(1, n - 1).ToList();






            int counter = 1;
            int lastId = 0;

            //n-1 because last step is done below
            for (int i = 1; i < n - 1; i++) {

                //Print("next loop");
                int k = 0;
                foreach (int j in notvisited) {

                    var intersection = Enumerable.Intersect(faceE[lastId], faceE[j]);

                    if (intersection.Count() != 0) {
                        sortedFaceID[counter++] = vf[j];
                        lastId = j;
                        notvisited.RemoveAt(k);
                        break;
                    }
                    k++;

                }//foreach

                //foreach(int j in notvisited)
                // Print(j.ToString());

            }//for i

            //the last item, may not have shared edges for cases of naked edges
            sortedFaceID[n - 1] = vf[notvisited[0]];


            return sortedFaceID;

        }



    }
}
