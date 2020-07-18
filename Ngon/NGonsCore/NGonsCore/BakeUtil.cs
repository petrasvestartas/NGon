using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore {
    public static class BakeUtil {

        public static void WriteLine(this string s) => Rhino.RhinoApp.WriteLine(s);

        public static void Write(this string s) => Rhino.RhinoApp.Write(s);

        public static void Bake(this Mesh l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(l);

        public static void Bake(this Plane l, double w = 0.1) => Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(l, new Interval(-w * 0.5, w * 0.5), new Interval(-w * 0.5, w * 0.5)));

        public static void Bake(this Point3d l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(l);

        public static void Bake(this IEnumerable<Point3d> l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(l);

        public static void Bake(this Line l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(l);

        public static void Bake(this Polyline l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(l);

        public static void Bake(this Curve l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddCurve(l);

        public static void Bake(this IEnumerable<Line> L) {
            foreach (var l in L)
                l.Bake();
        }

        public static void Bake(this IEnumerable<Polyline> L) {
            foreach (var l in L)
                l.Bake();
        }

        public static void Bake(this IEnumerable<Curve> L) {
            foreach (var l in L)
                l.Bake();
        }

        public static void Bake(this IEnumerable<Plane> L, double w = 0.1)
        {
            foreach (var l in L)
                l.Bake(w);
        }

        public static void Bake(this IEnumerable<Mesh> L)
        {
            foreach (var l in L)
                l.Bake();
        }






    }
}
