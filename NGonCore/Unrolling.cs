using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore {




    public static class Unrolling {



        public static Curve[][] Packing(Curve[][] g, Plane[][] p, double x = 1, double y = 1) {

            Curve[][] gT = new Curve[g.Length][];


            double tempDist2 = 0;
            int len0 = g.Length;


            for (int i = 0; i < len0; i++) {

                int len1 = g[i].Length;
                if (len1 == p[i].Length) {

                    gT[i] = new Curve[len1];

                    double massAddition = 0;
                    double tempDist = 0;

                    for (int j = 0; j < len1; j++) {

                        //Orient to xy plane
                        Transform orientation = Transform.PlaneToPlane(p[i][j], Plane.WorldXY);
                        Curve g1 = g[i][j].DuplicateCurve();
                        g1.Transform(orientation);


                        //Move to origin
                        BoundingBox bbox = g1.GetBoundingBox(false);
                        Vector3d vec = Vector3d.Subtract((Vector3d)Point3d.Origin, (Vector3d)bbox.GetCorners()[3]);
                        g1.Transform(Transform.Translation(vec));

                        //Move by bbox edge length using mass addition below
                        g1.Transform(Transform.Translation(new Vector3d(massAddition, 0, 0)));

                        double dist = bbox.GetCorners()[0].DistanceTo(bbox.GetCorners()[1]) + x;
                        massAddition += dist;

                        //Move for dataTreeDisplay
                        double dist2 = bbox.GetCorners()[1].DistanceTo(bbox.GetCorners()[2]);
                        tempDist = Math.Max(tempDist, dist2);
                        g1.Transform(Transform.Translation(new Vector3d(0, -tempDist2, 0)));


                        gT[i][j] = g1;

                    }


                    tempDist2 += tempDist + y;

                }//if
            }//for
            return gT;
        }


    }
}
