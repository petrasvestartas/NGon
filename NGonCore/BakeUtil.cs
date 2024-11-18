using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public static class BakeUtil {

        public static void WriteLine(this string s) => Rhino.RhinoApp.WriteLine(s);

        public static void Write(this string s) => Rhino.RhinoApp.Write(s);

        public static void Bake(this Brep l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddBrep(l);
        public static void Bake(this Box l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddBox(l);

        public static void Bake(this Mesh l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(l);

        public static void Bake(this Plane l, double w = 0.1) => Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(l, new Interval(-w * 0.5, w * 0.5), new Interval(-w * 0.5, w * 0.5)));

        public static void BakeAxes(this Plane l, double w = 0.1) {
            //Check if there is such group
            string[] names = Rhino.RhinoDoc.ActiveDoc.Groups.GroupNames(true);
            // if ( names != null)
            //if (!names.Contains("Plane"))
            //Rhino.RhinoDoc.ActiveDoc.Groups.Add("Plane");
            //else
           int gIndex = Rhino.RhinoDoc.ActiveDoc.Groups.Add("Plane");

            //Rhino.DocObjects.Group g = Rhino.RhinoDoc.ActiveDoc.Groups.FindName("Plane");

            //int layerindex = Rhino.RhinoDoc.ActiveDoc.Layers.Add("Plane", r);
            Rectangle3d rect = new Rectangle3d(l, new Interval(-w * 0.5, w * 0.5), new Interval(-w * 0.5, w * 0.5));
            Brep brep = Brep.CreateFromCornerPoints(rect.Corner(0), rect.Corner(1), rect.Corner(2), rect.Corner(3), Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);


            Guid g0 = Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(rect, new Rhino.DocObjects.ObjectAttributes { PlotColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeight = 1.5, DisplayOrder = 1 });

            Guid g4 = Rhino.RhinoDoc.ActiveDoc.Objects.AddBrep(brep, new Rhino.DocObjects.ObjectAttributes { PlotColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeight = 1.5, DisplayOrder = 0 });
            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g4);

            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g0);
            Guid g1 = Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(l.Origin, l.Origin + l.XAxis * w * 0.5), new Rhino.DocObjects.ObjectAttributes { ObjectDecoration = Rhino.DocObjects.ObjectDecoration.EndArrowhead, PlotColor = System.Drawing.Color.FromArgb(255, 0, 0), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(255, 0, 0), PlotWeight = 0.5, DisplayOrder = 1 });
            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g1);
            Guid g2 = Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(l.Origin, l.Origin + l.YAxis * w * 0.5), new Rhino.DocObjects.ObjectAttributes { ObjectDecoration = Rhino.DocObjects.ObjectDecoration.EndArrowhead, PlotColor = System.Drawing.Color.FromArgb(0, 200, 200), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 200, 200), PlotWeight = 0.5, DisplayOrder = 1 });
            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g2);
            Guid g3 = Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(new Line(l.Origin, l.Origin + l.ZAxis * w * 0.5), new Rhino.DocObjects.ObjectAttributes { ObjectDecoration = Rhino.DocObjects.ObjectDecoration.EndArrowhead, PlotColor = System.Drawing.Color.FromArgb(0, 0, 255), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 0, 255), PlotWeight = 0.5, DisplayOrder = 1 });
            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g3);

            Guid g5 = Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(l.Origin, new Rhino.DocObjects.ObjectAttributes { ObjectDecoration = Rhino.DocObjects.ObjectDecoration.EndArrowhead, PlotColor = System.Drawing.Color.FromArgb(0, 0, 255), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 0, 255), PlotWeight = 0.5, DisplayOrder = 1 });
            Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g5);
        }


        public static void Bake(this Point3d l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(l);

        public static void Bake(this IEnumerable<Point3d> l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPoints(l);

        public static void Bake(this Line l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(l);

        public static void Bake(this Polyline l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(l);
        public static void BakeGroup(this IEnumerable<Polyline> plines) {

            int gIndex = Rhino.RhinoDoc.ActiveDoc.Groups.Add("Group");
            foreach (Polyline p in plines) {
                Guid g0 = Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(p, new Rhino.DocObjects.ObjectAttributes { PlotColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject, PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject, ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = System.Drawing.Color.FromArgb(0, 0, 0), PlotWeight = 1.5, DisplayOrder = 1 });
                Rhino.RhinoDoc.ActiveDoc.Groups.AddToGroup(gIndex, g0);
            }


        }
    public static void Bake(this Curve l) => Rhino.RhinoDoc.ActiveDoc.Objects.AddCurve(l);

        public static void Bake(this IEnumerable<Line> L) {
            foreach (var l in L) {
                if (l == Line.Unset) continue;
                l.Bake();

            }
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
