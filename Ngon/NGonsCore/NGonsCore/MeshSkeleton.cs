using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore {
    public static class MeshSkeleton {


        public static List<Polyline> GetCurves(Mesh M, List<Point3d> P, out Grasshopper.DataTree<Line> threeValence, out Grasshopper.DataTree<Point3d> naked) {


            threeValence = new Grasshopper.DataTree<Line>();
            naked = new Grasshopper.DataTree<Point3d>();
            HashSet<long> hash = new HashSet<long>();
            List<Line> lines = new List<Line>();
            //List<Tuple<int,int>> edges = new List<Tuple<int,int>>();
            Dictionary<int, Point3d> corners = new Dictionary<int,Point3d>();
            HashSet<int> visited3valence = new HashSet<int>();

            List<Point3d> innerValence = new List<Point3d>();


            List<Point3d> endValence = new List<Point3d>();

            HashSet<int> nv = new HashSet<int>();
         

            List<Point3d> P_ = P;
            if (P.Count != M.Faces.Count) {
                P_ = new List<Point3d>(M.MeshFaceCenters());
            }




            for (int i = 0; i < M.Faces.Count; i++) {
                int[] ff = M.Faces.AdjacentFaces(i);




                //1. for three valence output
                if (ff.Length > 2) {

                    if (visited3valence.Add(M.Faces[i].A)) {
                        threeValence.Add(new Line(P_[i], M.Vertices[M.Faces[i].A]), new Grasshopper.Kernel.Data.GH_Path(i));
                    }

                   if (visited3valence.Add(M.Faces[i].B)) {
                        threeValence.Add(new Line(P_[i], M.Vertices[M.Faces[i].B]), new Grasshopper.Kernel.Data.GH_Path(i));
                   }

                   if (visited3valence.Add(M.Faces[i].C)) {
                        threeValence.Add(new Line(P_[i], M.Vertices[M.Faces[i].C]), new Grasshopper.Kernel.Data.GH_Path(i));
                    }
                    nv.Add(i);
                }//if





                //2. for naked
                if (ff.Length == 1) {

                    Point3d corner = M.Vertices[M.Faces[i].A];
                    int id = M.Faces[i].A;

                    if (M.Vertices.GetConnectedVertices(M.Faces[i].B).Length == 2) {
                        corner = M.Vertices[M.Faces[i].B];
                        id = M.Faces[i].B;
                    } else if (M.Vertices.GetConnectedVertices(M.Faces[i].C).Length == 2) {
                        corner = M.Vertices[M.Faces[i].C];
                        id = M.Faces[i].C;
                    }
                    naked.Add(corner, new Grasshopper.Kernel.Data.GH_Path(i));

                    corners.Add(i,corner);

                   

             
                        lines.Add(new Line(P_[i], corner));
                    

                    nv.Add(i);
                }//if







                //3. The skeleton
                for (int j = 0; j < ff.Length; j++) {

                    //Create a pair key
                    long key0 = GetKey(i, ff[j]);
                    long key1 = GetKey(ff[j], i);

                    //if one of them does not exist add line segment both keys
                    if (!hash.Contains(key0)) {
                        lines.Add(new Line(P_[i], P_[ff[j]]));
                        hash.Add(key0);
                        hash.Add(key1);
                    }//if


                }//for j connected faces


            }//for i faces





            //get connected paths

            int[] nvArray = nv.ToArray();
            HashSet<int> visited = new HashSet<int>();
            HashSet<int> leftovers = new HashSet<int>();

            bool[] flag = new bool[M.Faces.Count];

            List<Polyline> polylines = new List<Polyline>();

            //loop through starting faces and search the end
            List<int> nv_ = new List<int>();
            foreach (int id in nv) {
                int[] ff = M.Faces.AdjacentFaces(id);
                //Rhino.RhinoApp.WriteLine(ff.Length.ToString());
                for(int i = 0; i < ff.Length; i++) {
                    nv_.Add(id);
                }
            }


            // Rhino.RhinoApp.WriteLine("LoopStarts");
            HashSet<long> hashPairs = new HashSet<long>();


            foreach (int id in nv_) {
                //Rhino.RhinoApp.WriteLine("");
               // Rhino.RhinoApp.WriteLine("start " + id.ToString());
           



                //List<int> path = new List<int>() { id };
                Polyline polyline = new Polyline();
                HashSet<int> localVisit = new HashSet<int>();
                polyline.Add(P_[id]);
                localVisit.Add(id);

                int start = id;
                int current = id;
                bool run = true;
                int counter = 0;
                bool multivalence = false;
         

                while (run && counter < 100) {

                    //first get neighbour faces
                    int[] ff = M.Faces.AdjacentFaces(current);
                    int nei = -1;


                    for (int i = 0; i < ff.Length; i++) {

 
                        if (!visited.Contains(ff[i]) && 
                            ff[i]!=start 
                            && !nv.Contains(ff[i])) {
                            nei = ff[i];
                            break;
                        }
                    }

                    //Rhino.RhinoApp.WriteLine(nei.ToString());

                    if (nei != -1) {

                        //if one of end faces top
                        if (nv.Contains(nei)) {
                            //polyline.Add(P_[nei]);
                            run = false;
                        }
                        //visited.Add(nei);
                        polyline.Add(P_[nei]);
                        if (!nv.Contains(nei)) {
                            visited.Add(nei);
                            localVisit.Add(nei);
                            flag[nei] = true;
                        }
                        current = nei;

                    }

        

                    //if only one line segment between two starting points
                    //either multi valece or naked
                    else  if(current == id && nei == -1 && counter == 0 ) {

                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(P_[id]);

                        //if (M.Faces.AdjacentFaces(id).Length == 1) {
                            //polyline.Insert(0,corners[id]);
                        //}

                        for (int i = 0; i < ff.Length; i++) {

                            var key0 = GetKey(id, ff[i]);
                            var key1 = GetKey(ff[i], id);





                            if (ff[i] != start && !visited.Contains(ff[i])) {



                                if(hashPairs.Add(key0) && hashPairs.Add(key1)) {


                                  polyline.Add(P_[ff[i]]);
                                    Polyline temp = new Polyline(polyline);

                                    //if corner
                                    if (M.Faces.AdjacentFaces(ff[i]).Length == 1) {
                                        temp.Add(corners[ff[i]]);
                                    }

                                    if (M.Faces.AdjacentFaces(id).Length == 1) {
                                        temp.Insert(0,corners[id]);
                                    }

                                    polylines.Add(temp);
                                    break;
                                }


                                //nei = ff[i];
                               
                            }


                        }


                            break;
                    } else {//if it has no neighbours check again



                         ff = M.Faces.AdjacentFaces(current);
                        for (int i = 0; i < ff.Length; i++) {
                            if (nv.Contains(ff[i]) && !localVisit.Contains(ff[i])) {

                                //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(M.Faces.GetFaceCenter(ff[i]));
                                //nei = ff[i];
                                polyline.Add(P_[ff[i]]);
                                localVisit.Add(ff[i]);
                                break;
                            }
                        }
                        break;
                    }

                    counter++;
                }

                if (polyline.Count > 2) {



                    int last = localVisit.Last();
                    int first = localVisit.First();

                    int[] f0 = M.Faces.AdjacentFaces(last);
                    int[] f1 = M.Faces.AdjacentFaces(first);

          

                    if (f0.Contains(first) || f1.Contains(last)) {
                        polyline.Close();

               
                    }





                    if (M.Faces.AdjacentFaces(last).Length == 1) {
                        polyline.Add(corners[last]);
                    }

                    if (M.Faces.AdjacentFaces(first).Length == 1) {
                        polyline.Insert(0,corners[first]);
                    }

                    polylines.Add(polyline);



                //} else if (polyline.Count== 1) {
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(polyline[0]);
                }

            }










            //Output
            List<Polyline> outPolylines = new List<Polyline>();

            foreach (var ln in lines) {
                outPolylines.Add(new Polyline(new Point3d[] { ln.From, ln.To }));
            }//foreach







            //return outPolylines;
            return polylines;
        }

        public static long GetKey(int i, int j) {
            return (UInt32)i << 16 | (UInt32)j;
        }
    }
}
