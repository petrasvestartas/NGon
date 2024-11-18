using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
//Eplanes edge length
namespace NGonCore {
    public static class PropertiesMesh {

        public struct MeshProperties {

            public Mesh mesh;

            //Vertices
            public int[][] nGonTV;

            public Point3d[][] faceVertices;
            public HashSet<int> allV;
            public Point3d[] allVPoint3d;


            //Edges
            public HashSet<int> allE;

            public Dictionary<int, int> allEDict; //mapping edges id 2 6 8 to series 0 1 2
            public int[][] ngonEdges;
            public int[][] edgeV;


            //Faces
            public int[][] vertexNGons;

            public int[][] eNgons;

            //Planes
            public Plane[] planesO;

            public Plane[] planesOffset;
            public Plane[] planesOffset2;

            //Lines
            public Line[] innerLines;

            //JointTypes
            public int[][] jointTypes;

            //Values
            public double offsetDist;

            public Plane[][] edgePlanes;

            public double maxLength;

        }

        public static MeshProperties GetMeshProperties(this Mesh mesh, double offsetDist, double jointWidth, double rotation, int[] maleFemale, double maxLength) {
            //Base structure to hold mesh and its properties
            MeshProperties mp = new MeshProperties();


            //Mesh
            mp.mesh = mesh; //NGonCore.PolylineUtil.Loft(polylines.ToArray());

            //Vertices
            mp.nGonTV = mp.mesh.GetNGonsTopoBoundaries(); //1.Get all edges
            mp.faceVertices = mp.mesh.GetNGonsBoundariesPoint3d(mp.mesh.GetNGonsBoundaries());
            mp.allV = mp.mesh.GetAllNGonsTopoVertices();
            mp.vertexNGons = mp.mesh.GetNGonsConnectedToNGonTopologyVertices(mp.allV, false);


            //Edges
            mp.allE = mp.mesh.GetAllNGonEdges(mp.nGonTV);
            mp.allEDict = GrasshopperUtil.DictFromHash(mp.allE);
            mp.edgeV = mp.mesh.GetAllNGonEdges_TopoVertices(mp.nGonTV, mp.allE);

            //Faces
            mp.ngonEdges = mp.mesh.GetNGonFacesEdges(mp.nGonTV); //2.Get all edges in ngon faces
            mp.eNgons = mp.mesh.GetNgonsConnectedToNGonsEdges(mp.allE); //3.Get ngons connected to edges

            //Planes
            Plane[] planes = mp.mesh.GetNgonPlanes();
            mp.planesO = planes;


            mp.planesOffset = mp.planesO.MovePlaneArrayByAxis(offsetDist, -1, 2, false);
            mp.planesOffset2 = mp.planesO.MovePlaneArrayByAxis(offsetDist * 2, -1, 2, false);


            //Lines
            mp.innerLines = mp.mesh.GetAllNGonEdgesLines(mp.allE); //Plane origin points will be on those lines
            mp.allVPoint3d = new Point3d[mp.allV.Count]; //id = allV
            Dictionary<int, Point3d> allVMoved = new Dictionary<int, Point3d>(mp.allV.Count);

            int n = 0;
            foreach (int id in mp.allV) {
                int[] nn = mp.vertexNGons[n];
                Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(mp.planesOffset[nn[0]], mp.planesOffset[nn[1]],
                    mp.planesOffset[nn[2]], out mp.allVPoint3d[n]);
                allVMoved.Add(id, mp.allVPoint3d[n]);
                n++;
            }

            for (int i = 0; i < mp.edgeV.Length; i++)
                mp.innerLines[i] = new Line(allVMoved[mp.edgeV[i][0]], allVMoved[mp.edgeV[i][1]]);

            //JointTypes
            //ToDo: Automate faces: start from bisectors that are 1-1, then full faces

            //Joints for 2 valence edges
            mp.jointTypes = new int[mp.allE.Count][];


            //Joint types automation which one is male, female or incase of bisecto none
            //1 - None 2 - Female, 3 - Male
            Dictionary<int, Plane> bisectors = new Dictionary<int, Plane>();

            double minAngle = 3.14159 / 18 * 3;
            double maxAngle = 3.14159 / 18 * 15;
            Dictionary<int, int> swap = new Dictionary<int, int>() { { 3, 2 }, { 2, 3 } };

            for (int i = 0; i < mp.allE.Count; i++) {

                int[] ngons = mp.eNgons[i];

                switch (ngons.Length) {

                    //For naked Edges
                    case (1):
                    mp.jointTypes[i] = new int[1] { 0 };
                    break;

                    //For 2 valence edge
                    case (2):
                    double angle = Vector3d.VectorAngle(planes[ngons[0]].ZAxis, planes[ngons[1]].ZAxis);
                    if (angle < minAngle || angle > maxAngle) {
                        Plane bisector = NGonCore.PlaneUtil.BisectorPlane(planes[ngons[0]], planes[ngons[1]]);
                        bisectors.Add(i, bisector);
                        mp.jointTypes[i] = new int[2] { 1, 1 };
                    } else {

                        int a = maleFemale[ngons[0]];
                        int b = maleFemale[ngons[1]];

                        if(a == 1 || b == 1)
                            mp.jointTypes[i] = new int[2] { 1, 1 };
                        //else if (a == -1 || b == -1)
                            //mp.jointTypes[i] = new int[2] { -1, -1 };
                        else if (Math.Abs(a) != Math.Abs(b))
                            mp.jointTypes[i] = new int[2] { Math.Abs(a), Math.Abs(b) };
                        else if (a == b)
                            mp.jointTypes[i] = new[] { 3, 2 };
                        else if (a > b)
                            mp.jointTypes[i] = new[] { a, swap[Math.Abs(b)] };
                        else
                            mp.jointTypes[i] = new[] { swap[Math.Abs(a)], b };
                    }
                    break;

                    //ToDo: For n valence edges not implemented
                    default:
                    mp.jointTypes[i] = Enumerable.Range(9, ngons.Length).ToArray();
                    break;

                }//Switch
            }//For

            //Values
            mp.offsetDist = offsetDist;

            mp.maxLength = maxLength;

            mp.edgePlanes = EPlanes(mp.mesh, mp.allE, mp.allEDict, mp.planesO, mp.planesOffset, mp.eNgons, jointWidth, rotation, mp.innerLines, mp.ngonEdges, mp.jointTypes, mp.vertexNGons, mp.allV.ToArray(),mp.maxLength);


            //Output
            return mp;
        }


        public static Dictionary<int, Plane> GetBisectors(HashSet<int> allE, Plane[] planesO, int[][] eNgons) {

            Dictionary<int, Plane> bisectors = new Dictionary<int, Plane>();

            double minAngle = Math.PI / 18 * 3;
            double maxAngle = Math.PI / 18 * 15;

            for (int i = 0; i < allE.Count; i++) {
                int[] ngons = eNgons[i];
                double angle = Vector3d.VectorAngle(planesO[ngons[0]].ZAxis, planesO[ngons[1]].ZAxis);
                if (angle < minAngle || angle > maxAngle) {
                    Plane bisector = NGonCore.PlaneUtil.BisectorPlane(planesO[ngons[0]], planesO[ngons[1]]);
                    bisectors.Add(i, bisector);
                }
            }

            return bisectors;
        }


        public static Dictionary<int, Plane> GetCustomAveragePlanes(HashSet<int> allE, Plane[] planesO, int[][] eNgons,  int[] allV, Mesh mesh, int[][] ngonEdges, Dictionary<int, int> allEDict) {

            Dictionary<int, Plane> bisectors = new Dictionary<int, Plane>();


            double minAngle = Math.PI / 18 * 3;
            double maxAngle = Math.PI / 18 * 15;

            for (int i = 0; i < allE.Count; i++) {
                int[] ngons = eNgons[i];
                double angle = Vector3d.VectorAngle(planesO[ngons[0]].ZAxis, planesO[ngons[1]].ZAxis);
                if (angle < minAngle || angle > maxAngle) {


                    //Get connected edges to current edges
                    int[] edgesA = ngonEdges[ngons[0]];
                    int[] ngonPairA = new int[0];
                    int[] edgesB = ngonEdges[ngons[1]];
                    int[] ngonPairB = new int[0]; 


                    Rhino.IndexPair ip = mesh.TopologyEdges.GetTopologyVertices(allE.ElementAt(i));

                    for (int j = 0; j < edgesA.Length; j++) {
                        Rhino.IndexPair tempIp = mesh.TopologyEdges.GetTopologyVertices(edgesA[j]);
                        if (ip.I != tempIp.I && ip.J != tempIp.J && ip.J != tempIp.I && ip.I != tempIp.J) {
                            ngonPairA = eNgons[allEDict[edgesA[j]]];
                            //GrasshopperUtil.Debug(ngonPairA[0]);
                            //GrasshopperUtil.Debug(ngonPairA[1]);
                            break;
                        }
                    }

                    for (int j = 0; j < edgesB.Length; j++) {
                        Rhino.IndexPair tempIp = mesh.TopologyEdges.GetTopologyVertices(edgesB[j]);
                        if (ip.I != tempIp.I && ip.J != tempIp.J && ip.J != tempIp.I && ip.I != tempIp.J) {
                            ngonPairB= eNgons[allEDict[edgesB[j]]];
                            //GrasshopperUtil.Debug(ngonPairB[0]);
                            //GrasshopperUtil.Debug(ngonPairB[1]);
                            break;
                        }
                    }


                    int faceA = ngonPairB.Except(ngons).First();
                    int faceB = ngonPairA.Except(ngons).First();

                    Plane ptemp = planesO[faceA];
                    ptemp.Flip();

                     Plane bisector = PlaneUtil.AveragePlane(new Plane[] { ptemp, planesO[faceB] }); ;
                     bisectors.Add(i, bisector);
                }
            }

            return bisectors;
        }


        //Get ngon planes
        public static Plane[][] EPlanes(Mesh mesh, HashSet<int> allE, Dictionary<int, int> allEDict, Plane[] planesO, Plane[] planesOffset, int[][] eNgons, double jointWidth, double rotation, Line[] innerLines, int[][] ngonEdges, int[][] jointTypes, int[][] vertexNGons, int[] allV, double maxLength) {


            Line[] eLine = mesh.GetAllNGonEdgesLines(allE);//Plane origin points will be on those lines
            Plane[][] mp;

            

            //EDGE PLANES
            Plane[] edgePlanes = new Plane[allEDict.Count];

            //Loop through all bisector edge faces and check if that edge belongs to one of edge faces
            //Dictionary<int, Plane> bisectors = GetBisectors(allE, planesO, eNgons);
            Dictionary<int, Plane> bisectors = GetCustomAveragePlanes(allE, planesO, eNgons,allV,mesh,ngonEdges,allEDict);



            for (int i = 0; i < allEDict.Count; i++) {

                Plane planeForXAxisRotation = (jointTypes[i][0] == 3) ? planesO[eNgons[i][0]] : planesO[eNgons[i][1]]; // Check which edges are connected with bisectors
                edgePlanes[i] = PlaneUtil.PlaneFromLinePlane(innerLines[i], planeForXAxisRotation); //Take edge vector and rotate it by male plane and in this way get edgePlane

                
                foreach (KeyValuePair<int, Plane> pair in bisectors)
                {

                    //Test if edge is connected to bisector edge
                    //Get connected ngons of bisector edge

                    //Insertion direction:
                    //1. Bisector plane 2Dof insertion freedom
                    //To insert all edges correctly - insertion direction is limited to 1 Dof
                    //2. It is a bisector of mesh faces connected to edge vertices, excluding faces connected to edges
                    int[] bisectorNgons = eNgons[pair.Key];

                    bool flag1 = eNgons[i].Contains(bisectorNgons[0]);
                    bool flag2 = eNgons[i].Contains(bisectorNgons[1]);
                
                    if (flag1 || flag2)
                    {

                        Plane modifiedBisectorPlane = new Plane(edgePlanes[i].Origin, pair.Value.XAxis, pair.Value.YAxis); //Move bisector plane to the middle of edge

                        Rhino.IndexPair ip = mesh.TopologyEdges.GetTopologyVertices(allE.ElementAt(i));
                        Rhino.IndexPair bip = mesh.TopologyEdges.GetTopologyVertices(allE.ElementAt(pair.Key));

                        //If edge is connected to bisector edge
                        if ((ip.I == bip.I || ip.I == bip.J || ip.J == bip.I || ip.J == bip.J) && (i != pair.Key))
                        {
                            edgePlanes[i] = PlaneUtil.AlignPlane(edgePlanes[i], modifiedBisectorPlane);
                            //edgePlanes[i] = modifiedBisectorPlane;
                        }
 
                        else
                        {

                             planeForXAxisRotation = (jointTypes[i][0] == 3) ? planesO[eNgons[i][1]] : planesO[eNgons[i][0]]; // Check which edges are connected with bisectors
                            edgePlanes[i] = PlaneUtil.PlaneFromLinePlane(innerLines[i], planeForXAxisRotation); //Take edge vector and rotate it by male plane and in this way get edgePlane
                            //Get vertex ngons
                            int[] CurrentEdgeVertexNgons_I = vertexNGons[Array.IndexOf(allV, ip.I)];
                            int[] CurrentEdgeVertexNgons_J = vertexNGons[Array.IndexOf(allV, ip.J)];
                            //Boolean difference between two

                            int faceI = CurrentEdgeVertexNgons_I.Except(CurrentEdgeVertexNgons_J).First();
                            // int faceJ = CurrentEdgeVertexNgons_J.Except(CurrentEdgeVertexNgons_I).First();
                            Plane bi = planesO[faceI];
                            Plane mPlane = new Plane(edgePlanes[i].Origin, bi.XAxis, bi.YAxis);

                            //edgePlanes[i].Rotate(Math.PI*0.5, edgePlanes[i].ZAxis);
                            //edgePlanes[i] = new Plane(edgePlanes[i].Origin, edgePlanes[i].YAxis, edgePlanes[i].XAxis);
                                      edgePlanes[i] = PlaneUtil.AlignPlane(edgePlanes[i], mPlane);
                            //edgePlanes[i].Rotate(Math.PI * 0.5, edgePlanes[i].ZAxis);
                            //edgePlanes[i].Rotate(Math.PI * 0.5, edgePlanes[i].ZAxis);
                        }
                        break;
                    }
    
                }

            }




                //OFFSET PLANES
                mp = new Plane[allEDict.Count][];
            

            HashSet<int> edges = new HashSet<int>();
            int[] e1 = ngonEdges[6];
            int[] e2 = ngonEdges[7];
            for (int i = 0; i < e1.Length; i++)
                edges.Add(allEDict[e1[i]]);
            for (int i = 0; i < e2.Length; i++)
                edges.Add(allEDict[e2[i]]);
            Rhino.RhinoApp.ClearCommandHistoryWindow();
            foreach (var edge in edges)
            {
                GrasshopperUtil.Debug(edge);
            }




            for (int i = 0; i < allEDict.Count; i++) {

                

                mp[i] = new Plane[0];
                if (jointTypes[i][0] != -1 && jointTypes[i][1] != -1) {

                    //Direction
                    Vector3d vec = Vector3d.Subtract((Vector3d)innerLines[i].From, (Vector3d)innerLines[i].To);
                    vec.Unitize();
                    vec *= jointWidth;
                    //Distance
                    double dist = innerLines[i].Length - jointWidth; //Draw at least one joint
                    int n = (int)(innerLines[i].Length * 0.5 / jointWidth) - 1;



                    if (innerLines[i].Length > maxLength) {

                        vec.Unitize();
                        double jointWidth2 = innerLines[i].Length / 10;
                        vec *= jointWidth2;

                        dist = innerLines[i].Length - jointWidth2; //Draw at least one joint
                        n = (int)((innerLines[i].Length * 0.5 / jointWidth2) - 1);

                    }


                    n = (n <= 0) ? 0 : n;



                    //Planes
                    //Define center planes
                    mp[i] = new Plane[n * 2 + 2];
                    if (dist > 0) {
                        mp[i][n] = new Plane(edgePlanes[i]);
                        mp[i][n].Translate(vec * 0.5);
                        mp[i][n + 1] = new Plane(edgePlanes[i]);
                        mp[i][n + 1].Translate(-vec * 0.5);
                    }
                    //Define rest of the planes
                    for (int j = 1; j < n + 1; j++) {
                        int t = n - j;
                        mp[i][t] = new Plane(edgePlanes[i]);
                        mp[i][t].Translate((j * vec) + (vec * 0.5));
                        mp[i][j + n + 1] = new Plane(edgePlanes[i]);
                        mp[i][j + n + 1].Translate((j * -vec) - (vec * 0.5));
                    }

                    if (dist < 0)
                        mp[i] = new Plane[0];


                    //if (edges.Contains(i))
                       // mp[i] = new Plane[0];


                }
            }

            //ROTATE PLANES
            RotatePlaneArray(mp, rotation);

            //OUTPUT
            return mp;
        }

        public static void RotatePlaneArray(Plane[][] planeSeq, double angle) {

            for (int i = 0; i < planeSeq.Length; i++) {
                if (i == 1 || i == 3 || i == 8 || i == 11)//Manual
                    continue;
                for (int j = 0; j < planeSeq[i].Length; j++) {

                    angle *= -1;
                    planeSeq[i][j].Transform(Transform.Rotation(-angle, planeSeq[i][j].YAxis, planeSeq[i][j].Origin));

                }
            }


        }


    }
}
