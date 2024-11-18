using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public class Tile2D {

        public int id = -1;


        public int[] e;// = new int[] { 0, 1, 2, 3 };
        public int[] eID;
        public bool[] eFlag;
        public Polyline basePolyline;
        

        public List<Polyline>[] geo ;
        public int[] geoID;// = new int[] { 0, 1, 2, 3 };

        public int[] adj0;
        public int[] adj1;

        public int shiftTimes = 0;

        public int[] nodes;
        public int[][] eToSwap;

        public Tile2D(int id, Polyline basePolyline, int[] e, int[] nodes, int[][] eToSwap) {

            this.id = id;
            this.basePolyline = basePolyline;
            this.e = e;
            this.eID = Enumerable.Range(0, e.Length).ToArray();

            this.eToSwap = eToSwap;
            this.nodes = nodes;
        }


        public Tile2D(int id, Polyline basePolyline, List<Polyline>[] geo, int[] e, int[] geoID, int[] adj0, int[] adj1) {
            this.id = id;
            this.basePolyline = basePolyline;
            //this.targetPolyline = targetPolyline;
            this.geo = geo;
            this.e = e;
            this.geoID = geoID;
            this.eID = Enumerable.Range(0, e.Length).ToArray();

            //adj
            this.adj0 = adj0;
            this.adj1 = adj1;
        }


        public void ShiftPolylines() {


            geo = geo.shiftRight();
            geoID = geoID.shiftRight();

        }

        public void Shift() {
            e = e.shiftRight();
            eID.Duplicate();


            Point3d[] pts = new Point3d[e.Length];
            for (int j = 0; j < pts.Length; j++)
                pts[j] = basePolyline[j];
            pts = pts.shiftRight();
            basePolyline = new Polyline(pts);
            basePolyline.Close();


        }

        public Tile2D Duplicate() {

            List<Polyline>[] geoDup = geo.Duplicate();

            int[] eCopy = e.Duplicate();
            int[] geoIDCopy = geoID.Duplicate();
            int[] adj0Copy =adj0.Duplicate();
            int[] adj1Copy = adj1.Duplicate();
            

            Tile2D tile2d = new Tile2D(this.id, this.basePolyline.Duplicate(), geoDup, eCopy, geoIDCopy, adj0Copy, adj1Copy);
            tile2d.shiftTimes = this.shiftTimes;

            bool[] eFlagCopy = eFlag.Duplicate();
            tile2d.eToSwap = this.eToSwap.Duplicate();
            tile2d.nodes = this.nodes.Duplicate();
            tile2d.eID = this.eID.Duplicate();



            return tile2d;
        }

        public void Map(Polyline targetPolyline) {


            Mesh m0 = new Mesh();
            m0.Vertices.Add(this.basePolyline[0]);
            m0.Vertices.Add(this.basePolyline[1]);
            m0.Vertices.Add(this.basePolyline[2]);
            if (this.basePolyline.Count == 5) {
               
                m0.Vertices.Add(this.basePolyline[3]);
                Point3d c = (this.basePolyline[0] + this.basePolyline[1] + this.basePolyline[2] + this.basePolyline[3]) * 0.25;
                m0.Vertices.Add(c);
                //m0.Faces.AddFace(2, 1, 0);
                //m0.Faces.AddFace(0, 3, 2);
                m0.Faces.AddFace(0, 1, 4);
                m0.Faces.AddFace(1, 2, 4);
                m0.Faces.AddFace(2, 3, 4);
                m0.Faces.AddFace(3, 0, 4);
            } else {
                Point3d c = (this.basePolyline[0] + this.basePolyline[1] + this.basePolyline[2] ) * 1/3;
                m0.Vertices.Add(c);
                //m0.Faces.AddFace(2, 1, 0);
                m0.Faces.AddFace(0, 1, 3);
                m0.Faces.AddFace(1, 2, 3);
                m0.Faces.AddFace(2, 0, 3);
            }

          
                

            Mesh m1 = new Mesh();
            m1.Vertices.Add(targetPolyline[0]);
            m1.Vertices.Add(targetPolyline[1]);
            m1.Vertices.Add(targetPolyline[2]);


            if (this.basePolyline.Count == 5) {

                m1.Vertices.Add(targetPolyline[3]);
                Point3d c = (targetPolyline[0] + targetPolyline[1] + targetPolyline[2] + targetPolyline[3]) * 0.25;
                m1.Vertices.Add(c);
                //m0.Faces.AddFace(2, 1, 0);
                //m0.Faces.AddFace(0, 3, 2);
                m1.Faces.AddFace(0, 1, 4);
                m1.Faces.AddFace(1, 2, 4);
                m1.Faces.AddFace(2, 3, 4);
                m1.Faces.AddFace(3, 0, 4);
            } else {
                Point3d c = (targetPolyline[0] + targetPolyline[1] + targetPolyline[2]) * 1 / 3;
                m1.Vertices.Add(c);
                //m0.Faces.AddFace(2, 1, 0);
                m1.Faces.AddFace(0, 1, 3);
                m1.Faces.AddFace(1, 2, 3);
                m1.Faces.AddFace(2, 0, 3);
            }


            //if (geoID.Length == 4) {
            //    m1.Vertices.Add(targetPolyline[3]);

            //    //m1.Faces.AddFace(2, 1, 0);
            //    //m1.Faces.AddFace(0, 3, 2);
            //} else {
            //    //m1.Faces.AddFace(2, 1, 0);

            //}

            //if (geoID.Length == 4)
           


            //Brep b0 = Brep.CreateFromCornerPoints(s[0], s[1], s[2], s[3], 0.01);
            //Brep b1 = Brep.CreateFromCornerPoints(t[0], t[1], t[2], t[3], 0.01);
            //return NGonCore.PolylineUtil.MappedFromSurfaceToSurface(TC, b0.Surfaces[0], b1.Surfaces[0]);
            for(int i = 0; i < this.geo.Length; i++) {
                this.geo[i] = NGonCore.PolylineUtil.MappedFromMeshToMesh(this.geo[i], m0, m1);
            }
        
            this.basePolyline = targetPolyline;
        }

        public override string ToString() {

            string log = "Tile2D ID" + this.id.ToString() + " ";

            log += "  | ";

            foreach (int i in this.e)
                log += (" e" + i.ToString());

            log += "  | ";

            if (this.adj0 != null) {
                foreach (int i in this.adj0)
                    log += (" adjA" + i.ToString());

                log += "  | ";
            }

            if (this.adj1 != null) {
                foreach (int i in this.adj1)
                    log += (" adjB" + i.ToString());

                log += "  | ";
            }


            if (this.geo != null) {
                log += (" NGeo " + geo.Length.ToString());
            }
                return log;
        }


    }
}
