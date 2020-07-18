using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGonsCore;

namespace NGonsCore.SpecificGeometry {
    public static class Reciprocal {

        public static DataTree<Polyline> ReciprocalFrame(this Mesh mesh, double angle = 10, double dist = 0.25, double width = 0.1) {


            DataTree<Polyline> polylines = new DataTree<Polyline>();


            try {

                var display = new List<Line>();
                double thickness = width;

                int[][] tv = mesh.GetNGonsTopoBoundaries();
                HashSet<int> tvAll = mesh.GetAllNGonsTopoVertices();
                HashSet<int> e = mesh.GetAllNGonEdges(tv);

                Line[] lines = mesh.GetAllNGonEdgesLines(e);
                Line[] lines1 = mesh.GetAllNGonEdgesLines(e);

                bool[] nakedV = mesh.GetNakedEdgePointStatus();


                int[][] fe = mesh.GetNGonFacesEdges(tv);

                Plane[] planes = new Plane[lines.Length];

                Vector3d[] vecs = new Vector3d[mesh.TopologyEdges.Count];

                int j = 0;
                foreach (int i in e) {
                    Line l = mesh.TopologyEdges.EdgeLine(i);
                    Rhino.IndexPair ip = mesh.TopologyEdges.GetTopologyVertices(i);
                    int v0 = mesh.TopologyVertices.MeshVertexIndices(ip.I)[0];
                    int v1 = mesh.TopologyVertices.MeshVertexIndices(ip.J)[0];
                    Vector3d vec = new Vector3d(
                      (mesh.Normals[v0].X + mesh.Normals[v1].X) * 0.5,
                      (mesh.Normals[v0].Y + mesh.Normals[v1].Y) * 0.5,
                      (mesh.Normals[v0].Z + mesh.Normals[v1].Z) * 0.5
                      );
                    vec.Unitize();
                    vecs[j] = vec;


                    if (mesh.TopologyEdges.GetConnectedFaces(i).Length == 2) {

                        l.Transform(Transform.Rotation(Rhino.RhinoMath.ToRadians(angle), vec, l.PointAt(0.5)));
                    }



                    Vector3d cross = Vector3d.CrossProduct(l.Direction, vec);
                    planes[j] = new Plane(l.PointAt(0.5), cross);
                    cross.Unitize();
                    l.Transform(Transform.Translation(cross * thickness));

                    lines[j] = l;
                    l.Transform(Transform.Translation(-cross * 2 * thickness));
                    lines1[j++] = l;

                }

                //ngon vertex edges
                int[][] connectedE = mesh.GetConnectedNGonEdgesToNGonTopologyVertices(tvAll, e);
                int[] allEArray = e.ToArray();
                int[] allvArray = tvAll.ToArray();

                Line[] linesCopy = new Line[lines.Length];
                Line[] linesCopy1 = new Line[lines.Length];
                Line[] linesCopyM = new Line[lines.Length];
                Line[] linesCopyM1 = new Line[lines.Length];
                // Line[] linesCopyMoved = new Line[lines.Length];

                for (int i = 0; i < lines.Length; i++) {
                    linesCopy[i] = new Line(lines[i].From, lines[i].To);
                    linesCopy1[i] = new Line(lines1[i].From, lines1[i].To);
                    linesCopyM[i] = new Line(lines[i].From + vecs[i] * dist, lines[i].To + vecs[i] * dist);
                    linesCopyM1[i] = new Line(lines1[i].From + vecs[i] * dist, lines1[i].To + vecs[i] * dist);
                }

                for (int i = 0; i < connectedE.Length; i++) {


                    //Defines planes
                    int total = connectedE[i].Length;
                    Plane[] projectionPlanes = new Plane[total];

                    int start = total - 1;
                    for (j = start; j < start + total; j++) {
                        int currentEdge = connectedE[i][j % total];
                        int localID = Array.IndexOf(allEArray, currentEdge);
                        projectionPlanes[j - start] = planes[localID];
                    }


                    //Intersect lines

                    for (j = 0; j < connectedE[i].Length; j++) {


                        int currentEdge = connectedE[i][j];
                        if (mesh.TopologyEdges.GetConnectedFaces(currentEdge).Length == 1)
                            continue;

                        int localID = Array.IndexOf(allEArray, currentEdge);
                        Line currentLine = linesCopy[localID];
                        Line currentLine1 = linesCopy1[localID];
                        Line currentLineM = linesCopyM[localID];
                        Line currentLineM1 = linesCopyM1[localID];
                        IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(currentEdge);

                        double lineT, lineT1;
                        Plane currentPlane = new Plane(projectionPlanes[j]);




                        double flag = 1;

                        //Check length
                        Plane tempPlane = new Plane(currentPlane);
                        Line temp2 = new Line(currentLine.From, currentLine.To);
                        tempPlane.Origin += tempPlane.ZAxis * (thickness);
                        Line temp = new Line(currentLine.From, currentLine.To);
                        double tt;
                        Rhino.Geometry.Intersect.Intersection.LinePlane(temp2, tempPlane, out tt);
                        if (allvArray[i] == pair.I) temp.From = temp2.PointAt(tt); else temp.To = temp2.PointAt(tt);

                        double lineLen0 = temp.Length;

                        tempPlane = new Plane(currentPlane);
                        temp2 = new Line(currentLine.From, currentLine.To);
                        tempPlane.Origin += tempPlane.ZAxis * (-thickness);
                        temp = new Line(currentLine.From, currentLine.To);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(temp2, tempPlane, out tt);
                        if (allvArray[i] == pair.I) temp.From = temp2.PointAt(tt); else temp.To = temp2.PointAt(tt);

                        double lineLen1 = temp.Length;
                        //End Check Length

                        if (lineLen1 < lineLen0)
                            flag = -1;




                        currentPlane.Origin += currentPlane.ZAxis * (flag * thickness);

                        Rhino.Geometry.Intersect.Intersection.LinePlane(currentLine, currentPlane, out lineT);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(currentLine1, currentPlane, out lineT1);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(currentLineM, currentPlane, out lineT);
                        Rhino.Geometry.Intersect.Intersection.LinePlane(currentLineM1, currentPlane, out lineT1);


                        if (allvArray[i] == pair.I) {
                            currentLine.From = currentLine.PointAt(lineT);
                            currentLine1.From = currentLine1.PointAt(lineT1);
                            currentLineM.From = currentLineM.PointAt(lineT);
                            currentLineM1.From = currentLineM1.PointAt(lineT1);
                        } else {
                            currentLine.To = currentLine.PointAt(lineT);
                            currentLine1.To = currentLine1.PointAt(lineT1);
                            currentLineM.To = currentLineM.PointAt(lineT);
                            currentLineM1.To = currentLineM1.PointAt(lineT1);
                        }

                        linesCopy[localID] = currentLine;
                        linesCopy1[localID] = currentLine1;
                        linesCopyM[localID] = currentLineM;
                        linesCopyM1[localID] = currentLineM1;




                    }

                }



                for (int i = 0; i < linesCopy.Length; i++) {

                    if (mesh.TopologyEdges.GetConnectedFaces(allEArray[i]).Length == 1)
                        continue;

                    polylines.AddRange(new Polyline[]{
          new Polyline(new Point3d[]{linesCopy[i].From,linesCopy[i].To,linesCopy1[i].To,linesCopy1[i].From,linesCopy[i].From}),
          new Polyline(new Point3d[]{linesCopyM[i].From,linesCopyM[i].To,linesCopyM1[i].To,linesCopyM1[i].From,linesCopyM[i].From})
          }, new GH_Path(i));

                }



                //A = linesCopy;
                //B = linesCopy1;
                //C = linesCopyM;
                //D = linesCopyM1;
                //E = polylines;



            } catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

            return polylines;




        }

    }
}
        
