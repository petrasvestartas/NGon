using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoGeometry {
    public static class MakaDimensions {


        public static Polyline MakaOutline(double Z=0) {
            return new Polyline(){
        new Point3d(0, 0, Z),
        new Point3d(1500, 0, Z),
        new Point3d(1500, 2500,Z),
        new Point3d(0, 2500, Z),
        new Point3d(0, 0, Z)
        };
        }

        public static Tuple<BoundingBox, List<Line>> MakaOutlineSafety(double Z0 = 420, double Z1 = 720, double o1 = 200, double o2 = 300, double o3 = 200, double o4 = 200) {



            //Bottom Outline
            Polyline m0 = MakaOutline();

            m0[0] = new Point3d(m0[0].X + o4, m0[0].Y + o1, Z0);
            m0[1] = new Point3d(m0[1].X - o2, m0[1].Y + o1, Z0);
            m0[2] = new Point3d(m0[2].X - o2, m0[2].Y - o3, Z0);
            m0[3] = new Point3d(m0[3].X + o4, m0[3].Y - o3, Z0);

            m0[4] = m0[0];

            Polyline m1 = MakaOutline();

            //Top Outline
            m1[0] = new Point3d(m1[0].X + o4, m1[0].Y + o1, Z1);
            m1[1] = new Point3d(m1[1].X - o2, m1[1].Y + o1, Z1);
            m1[2] = new Point3d(m1[2].X - o2, m1[2].Y - o3, Z1);
            m1[3] = new Point3d(m1[3].X + o4, m1[3].Y - o3, Z1);

            m1[4] = m1[0];

            BoundingBox bbox = new BoundingBox(m0[0], m1[2]);

            //For Measurement

            Polyline m = MakaOutline(420);
            Polyline m_ = MakaOutline(0);

            List<Line> lines = new List<Line>{
        new Line(m0[0], m0.SegmentAt(1).Direction / m0.SegmentAt(1).Length * -o1),
        new Line(m0[1], m0.SegmentAt(2).Direction / m0.SegmentAt(2).Length * -o2),
        new Line(m0[3], m0.SegmentAt(3).Direction / m0.SegmentAt(3).Length * -o3),
        new Line(m0[0], m0.SegmentAt(0).Direction / m0.SegmentAt(0).Length * -o4),


        m0.SegmentAt(0),
        m0.SegmentAt(3),
        m_.SegmentAt(0),
        m_.SegmentAt(3)

        };

            for (int i = 0; i < lines.Count; i++) {
                lines[i] = new Line(new Point3d(lines[i].FromX, lines[i].FromY, 0), new Point3d(lines[i].ToX, lines[i].ToY, 0));
            }

            lines.Add(new Line(m0[1], m1[1]));
            lines.Add(new Line(new Point3d(m0[1].X, m0[1].Y, 0), m0[1]));



            //return new Polyline[]{m0,m1};
            return new Tuple<BoundingBox, List<Line>>(bbox, lines);
        }


        public static BoundingBox MakaBBox() {
            return new BoundingBox(new Point3d(0, 0, 0), new Point3d(1500, 2500, 800));
        }

        public static Polyline[] MakaDivisions() {
            Polyline TableDivision0 = new Polyline() {
        new Point3d(373, 2500, 0),
        new Point3d(373, 0, 0)
        };

            Polyline TableDivision1 = new Polyline() {
        new Point3d(760, 2500, 0),
        new Point3d(760, 0, 0)
        };


            Polyline TableDivision2 = new Polyline() {
        new Point3d(1145, 2500, 0),
        new Point3d(1145, 0, 0)
        };

            return new Polyline[]{
      TableDivision0,
      TableDivision1,
      TableDivision2
      };
        }

        public static Circle[] ScrewZones() {
            List<Point3d> pts = new List<Point3d>  {
      new Point3d(1297.0, 1300, 0),
      new Point3d(1297, 700, 0),
      new Point3d(698, 201, 0),
      new Point3d(796.0, 1300, 0),
      new Point3d(192, 700, 0),
      new Point3d(796.0, 801, 0),
      new Point3d(698, 700, 0),
      new Point3d(698.0, 801, 0),
      new Point3d(192.0, 801, 0),
      new Point3d(192.0, 1300, 0),
      new Point3d(1297.0, 801, 0),
      new Point3d(192, 201, 0),
      new Point3d(1297.0, 201, 0),
      new Point3d(796, 201, 0),
      new Point3d(698.0, 1300, 0),
      new Point3d(796, 2301, 0),
      new Point3d(698, 2301, 0),
      new Point3d(192, 2301, 0),
      new Point3d(796, 700, 0),
      new Point3d(447, 451, 0),
      new Point3d(1045, 451, 0),
      new Point3d(447, 1052, 0),
      new Point3d(1045, 1052, 0),
      new Point3d(1297, 2301, 0)
      };

            Circle[] circles = new Circle[pts.Count];
            for (int i = 0; i < pts.Count; i++) {
                circles[i] = new Circle(pts[i], 30);
            }

            return circles;
        }


    }
}
