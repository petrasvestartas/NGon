using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
   public static class NGonGeometryUtil {


        public static List<Polyline> CutMesh(this Mesh mesh, List<Plane> P) {
            
            var plines = mesh.GetPolylines().ToList();

            for(int i = 0; i< P.Count; i++) {
                plines=CutMesh(plines, P[i]);
            }
            return plines;

        }

            public static List<Polyline> CutMesh(this List<Polyline> plines3D, Plane P) {

           // Rhino.RhinoApp.WriteLine("H");

            double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            Transform xform = Transform.PlaneToPlane(P, Plane.WorldXY);
            Transform xformI = Transform.PlaneToPlane(Plane.WorldXY, P);

            //Transform to XY Plane
            Polyline[] plines = new Polyline[plines3D.Count];
           

            for(int i = 0; i < plines.Length; i++) {
                Polyline pline = new Polyline(plines3D[i]);
                pline.Transform(xform);
                plines[i] = pline;
            }



           

            List<Polyline> plinesCulled = new List<Polyline>();
            for (int i = 0; i < plines.Length; i++) {

                bool flag = true;
                bool[] below = new bool[plines[i].Count];
                int counter = 0;
                for (int j = 0; j < plines[i].Count; j++) {
                    if (plines[i][j].Z < 0) {
                        below[j] = true;
                        flag = false;
                        counter++;
                    } else {
                        below[j] = false;
                    }
                }

                //For faces that coincide with plane
                if (counter != 0 && counter != plines[i].Count) {



                    Curve curve = plines[i].ToNurbsCurve();
                    Rhino.Geometry.Intersect.CurveIntersections ci = Rhino.Geometry.Intersect.Intersection.CurvePlane(curve, Plane.WorldXY, tolerance * 0.01);

                    List<double> t = new List<double>();

                    for (int j = 0; j < ci.Count; j++) {
                        t.Add(ci[j].ParameterA);
                    }

                    Curve[] curves = curve.Split(t);
                    foreach (Curve c in curves) {
                        if (c.PointAt(0.5 * (c.Domain.T0 + c.Domain.T1)).Z > tolerance) {
                            Polyline splitPline;
                            if (c.TryGetPolyline(out splitPline)) {
                                splitPline.Add(splitPline[0]);
                                plinesCulled.Add(splitPline);
                            }
                        }
                    }


                }



                if (flag) {

                    plinesCulled.Add(plines[i]);
                }


            }

            for (int i = 0; i < plinesCulled.Count; i++) {
                Polyline plineCopy = plinesCulled[i];
                plineCopy.Transform(xformI);
                plinesCulled[i] = plineCopy;
            }

            return plinesCulled;
        }
    }
}
