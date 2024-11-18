using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore {
    public static class MeshOffset {


        /// <summary>
        /// Only works if volume is closed
        /// And has edges that share 2 faces
        /// And produce correct result with 3 valence edges
        /// Same result PlaneBisectorOffset
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Polyline[] PlanarOffset(this Mesh mesh,double dist) {

            int[][] boundaries = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(boundaries);
            int[] allEArray = allE.ToArray();


            //Face adjacency is not ordered
            //I pressume to get ordered adjacency you need to loop through all edges sharing faces

            //Flavour 1 - edge ngons
            int[][] edgeAdj = mesh.GetNgonsConnectedToNGonsEdges(allE);
            //Flovour 2 - ngon edges
            int[][] nGonTV = mesh.GetNGonsTopoBoundaries();
            int[][] ngonE = mesh.GetNGonFacesEdges(nGonTV);
            //Flavour 3 - plane array
            Plane[][] neighborPlanes = new Plane[mesh.Ngons.Count][];
            Plane[] planesO = mesh.GetNgonPlanes();
            Plane[] planes = mesh.GetNgonPlanes();

            Polyline[] plines = mesh.GetPolylines();

            //Offset planes
            for (int j = 0; j < planesO.Length; j++)
                planesO[j].Translate(planesO[j].Normal * -dist);

            //Get Neighbour planes
            for (int i = 0; i < mesh.Ngons.Count; i++) {

                neighborPlanes[i] = new Plane[ngonE[i].Length];

                for (int j = 0; j < ngonE[i].Length; j++) {
                    int mappedEdgeID = Array.IndexOf(allEArray, ngonE[i][j]);
                    int neighborFaceID = (edgeAdj[mappedEdgeID][0] == i) ? edgeAdj[mappedEdgeID][1] : edgeAdj[mappedEdgeID][0];

                    if (neighborFaceID > -1) {
                        neighborPlanes[i][j] = planesO[neighborFaceID];
                        
                    } else {
                       Line line = plines[i].SegmentAt(j);
                        neighborPlanes[i][j] = new Plane(line.PointAt(0.5), line.Direction, planesO[i].ZAxis);
                        //neighborPlanes[i][j].Bake(1);
                    }


                }
            }

            //Intersect offset planes
            Polyline[] polylines = new Polyline[mesh.Ngons.Count];

            for (int i = 0; i < mesh.Ngons.Count; i++) {
                polylines[i] = new Polyline();
                for (int j = 0; j < neighborPlanes[i].Length; j++) {
                    Point3d intersectionPt;
                    Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(planesO[i], neighborPlanes[i][j], neighborPlanes[i][NGonCore.MathUtil.Wrap(j - 1, neighborPlanes[i].Length)], out intersectionPt  );

                    polylines[i].Add(intersectionPt);
                }
                polylines[i].Close();
            }

            return polylines;
        }


        /// <summary>
        /// Only works if volume is closed
        /// And has edges that share 2 faces
        /// And produce correct result with 3 valence edges
        /// Same Result as PlaneOffset
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static Polyline[] PlanarBisectorOffset(this Mesh mesh, double dist) {
            int[][] boundaries = mesh.GetNGonsTopoBoundaries();
            HashSet<int> allE = mesh.GetAllNGonEdges(boundaries);
            int[] allEArray = allE.ToArray();
            HashSet<int> allTV = mesh.GetAllNGonsTopoVertices();
            int[][] edgeAdj = mesh.GetNgonsConnectedToNGonsEdges(allE);
            Plane[] bisectors = new Plane[allE.Count];

            int[][] nGonTV = mesh.GetNGonsTopoBoundaries();
            int[][] ngonE = mesh.GetNGonFacesEdges(nGonTV);


            //Get ngons planes and copy it to have offset and original set
            Plane[] planesO = mesh.GetNgonPlanes();
            Plane[] planes = mesh.GetNgonPlanes();

            for (int j = 0; j < planesO.Length; j++)
                planesO[j].Translate(planesO[j].Normal * -dist);



            //Get Edge Bisectors
            for (int i = 0; i < edgeAdj.Length; i++)
                bisectors[i] = NGonCore.PlaneUtil.BisectorPlane(planes[edgeAdj[i][0]], planes[edgeAdj[i][1]]);

            //Intersect offset edges
            Polyline[] polylines = new Polyline[mesh.Ngons.Count];

            for (int i = 0; i < ngonE.Length; i++) {
                polylines[i] = new Polyline();
                for (int j = 0; j < ngonE[i].Length; j++) {
                    int a = Array.IndexOf(allEArray, ngonE[i][j]);
                    int b = Array.IndexOf(allEArray, ngonE[i][NGonCore.MathUtil.Wrap(j - 1, ngonE[i].Length)]);
                    Point3d intersectionPt;
                    Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(planesO[i],   bisectors[a],   bisectors[b], out intersectionPt  );
                    polylines[i].Add(intersectionPt);
                }
                polylines[i].Close();
            }
            return polylines;
        }


    }
}
