using Grasshopper;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore {

    public class Pipes : IEnumerable<Pipe> {

        private List<Pipe> _pipes = new List<Pipe>();

        public ClosestLinesT _adj = new ClosestLinesT();


        //consturctor
        public Pipes() {
        }

        public Pipes(List<Pipe> pipes) {
            for (int i = 0; i < pipes.Count; i++) {
                _pipes.Add(pipes[i].Duplicate());
            }
        }

        public Pipes(List<Line> L, double tol, List<double> R0, List<double> R1, int Sides, List<Plane> EndPlanes0, List<Plane> EndPlanes1) {
            //Create pipie class instead of outputting too much stuff
            this._adj = new ClosestLinesT();
            List<Line> cpLines = L.ClosestLines(tol, ref this._adj, false);
            this._adj.cp = cpLines;

            //CreatePipes
            for (int i = 0; i < L.Count; i++) {//* Math.Sqrt(2)
                _pipes.Add(L[i].CreatePipe(i, Plane.Unset, R0[Math.Min(i, R0.Count - 1)] , Plane.Unset, Plane.Unset, Sides, R1[Math.Min(i, R1.Count - 1)] ));
                _pipes[i].meshloft.FillHoles();
                _pipes[i].meshloft.WeldUsingRTree(0.01);
            }

        }

 

        public Pipes Duplicate() {
            Pipes pipes = new Pipes();
            for (int i = 0; i < _pipes.Count; i++) {
                pipes.Add(_pipes[i].Duplicate());
            }

            pipes._adj = this._adj;
            return pipes;
        }


        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator() {
            return this._pipes.GetEnumerator();
        }

        IEnumerator<Pipe> IEnumerable<Pipe>.GetEnumerator() {
            return this._pipes.GetEnumerator();
        }

        //Methods

        public Pipe this[int index] {
            get { return _pipes[index]; }
            set { this._pipes[index] = value.Duplicate(); }
        }

        public int Count() {
            return this._pipes.Count;
        }

        public void Add(Pipe pipe) {
            this._pipes.Add(pipe.Duplicate());
        }



        //Properties
        public DataTree<Line> GetLines() {
            DataTree<Line> dt = new DataTree<Line>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.Add(_pipes[i].line, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Mesh> GetMeshes() {
            DataTree<Mesh> dt = new DataTree<Mesh>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.Add(_pipes[i].meshloft, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Brep> GetBreps() {
            DataTree<Brep> dt = new DataTree<Brep>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.Add(_pipes[i].breploft, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Brep> GetBrepsCuts() {
            DataTree<Brep> dt = new DataTree<Brep>();
            for (int i = 0; i < this._pipes.Count; i++)
                dt.AddRange(this._pipes[i].brepCuts, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }



        public DataTree<Polyline> GetMeshProjections() {
            DataTree<Polyline> dt = new DataTree<Polyline>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.AddRange(_pipes[i].meshProjections, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Line> GetMeshProjectionsLines() {
            DataTree<Line> dt = new DataTree<Line>();
            for (int i = 0; i < _pipes.Count; i++)
                for (int j = 0; j < _pipes[i].meshProjectionsLines.Count; j++) {
                    dt.AddRange(_pipes[i].meshProjectionsLines[j], new Grasshopper.Kernel.Data.GH_Path(i));
                }
            return dt;
        }

        public DataTree<Polyline> GetBoundingBoxCuts() {
            DataTree<Polyline> dt = new DataTree<Polyline>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.AddRange(_pipes[i].boundingBoxCuts, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Line> GetDrillingLines() {
            DataTree<Line> dt = new DataTree<Line>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.AddRange(_pipes[i].drillingHoles, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<int> GetNeiboursID() {
            DataTree<int> dt = new DataTree<int>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.AddRange(_pipes[i].neiID, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<int> GetID() {
            DataTree<int> dt = new DataTree<int>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.Add(_pipes[i].ID, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Plane> GetBasePlane() {
            DataTree<Plane> dt = new DataTree<Plane>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.Add(_pipes[i].plane, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Circle> GetCirlces() {
            DataTree<Circle> dt = new DataTree<Circle>();
            for (int i = 0; i < _pipes.Count; i++) {
                dt.Add(_pipes[i].c0, new Grasshopper.Kernel.Data.GH_Path(i));
                dt.Add(_pipes[i].c1, new Grasshopper.Kernel.Data.GH_Path(i));

            }
            return dt;
        }

        public DataTree<Plane> GetJointPlanes() {
            DataTree<Plane> dt = new DataTree<Plane>();
            for (int i = 0; i < _pipes.Count; i++)
                dt.AddRange(_pipes[i].jointPlanes, new Grasshopper.Kernel.Data.GH_Path(i));
            return dt;
        }

        public DataTree<Line> GetNeiAxis() {

            DataTree<Line> dt = new DataTree<Line>();
            for (int i = 0; i < _pipes.Count; i++) {
                for (int j = 0; j < _pipes[i].neiID.Count; j++) {
                    int neighbour = _pipes[i].neiID[j];
                    dt.Add(_pipes[neighbour].line, new Grasshopper.Kernel.Data.GH_Path(i));
                }
            }

            return dt;
        }

        public DataTree<Mesh> GetNeiMeshes() {

            DataTree<Mesh> dt = new DataTree<Mesh>();
            for (int i = 0; i < _pipes.Count; i++) {
                for (int j = 0; j < _pipes[i].neiID.Count; j++) {
                    int neighbour = _pipes[i].neiID[j];
                    dt.Add(_pipes[neighbour].meshloft, new Grasshopper.Kernel.Data.GH_Path(i));
                }
            }

            return dt;
        }







        public void Orient(Plane plane) {
            if (plane != null) {
                if (plane != Plane.Unset && plane.XAxis != Vector3d.Zero && plane.YAxis != Vector3d.Zero && plane.ZAxis != Vector3d.Zero) {
                    for (int i = 0; i < _pipes.Count; i++)
                        _pipes[i].Transform(Transform.PlaneToPlane(_pipes[i].plane, plane));
                }
            }
        }
    }

    public class Pipe {

        public int ID = -1;
        public List<int> neiID = new List<int>();

        public Line line;
        public Polyline poly0;
        public Polyline poly1;

        public Polyline[] endJoint0 = new Polyline[0];
        public Polyline[] endJoint1 = new Polyline[0];

        public Plane plane;
        public Plane cutter0;
        public Plane cutter1;
        public double radius;
        public Mesh meshloft;
        public Brep breploft;
        public Circle c0;
        public Circle c1;

        public List<Polyline> meshProjections = new List<Polyline>();
        public List<Polyline> boundingBoxCuts = new List<Polyline>();
        public List<Line[]> meshProjectionsLines = new List<Line[]>();
        public List<Line> drillingHoles = new List<Line>();
        public List<Plane> jointPlanes = new List<Plane>();

        public PipeJoint PipeJoint0;
        public PipeJoint PipeJoint1;

        public List<Brep> brepCuts = new List<Brep>();

        public void AddCuts(Polyline p) {
                boundingBoxCuts.Add(p.Duplicate());
        }

        public void AddCuts(IEnumerable<Polyline> p) {
            foreach (Polyline poly in p)
                boundingBoxCuts.Add(poly.Duplicate());
        }

        public Pipe(int id, Line line, Plane plane, double radius, Plane cutter0, Plane cutter1, Polyline poly0, Polyline poly1, Circle c0, Circle c1) {
            this.ID = id;
            this.line = line;
            this.plane = plane;
            this.radius = radius;
            this.cutter0 = cutter0;
            this.cutter1 = cutter1;
            this.poly0 = poly0;
            this.poly1 = poly1;
            this.c0 = c0;
            this.c1 = c1;
        }

        public Pipe Duplicate() {




            Pipe pipe = new Pipe(this.ID, new Line(line.From, line.To), new Plane(plane), radius, new Plane(cutter0), new Plane(cutter1), new Polyline(poly0), new Polyline(poly1), c0, c1);

            pipe.meshloft = meshloft.DuplicateMesh();
            if (pipe.breploft != null)
                if (pipe.breploft.IsValid)
                    pipe.breploft = breploft.DuplicateBrep();

            foreach (Polyline p in boundingBoxCuts)
                pipe.boundingBoxCuts.Add(new Polyline(p));

            foreach (Polyline p in meshProjections)
                pipe.meshProjections.Add(new Polyline(p));

            foreach (Line[] p in meshProjectionsLines) {
                Line[] lines = new Line[p.Length];
                for (int i = 0; i < p.Length; i++) {
                    lines[i] = p[i];
                }
                pipe.meshProjectionsLines.Add(lines);
            }

            foreach (int nei in this.neiID)
                pipe.neiID.Add(nei);

            foreach (Line l in drillingHoles)
                pipe.drillingHoles.Add(l);

            foreach (Plane pln in jointPlanes)
                pipe.jointPlanes.Add(new Plane(pln));

            pipe.endJoint0 = new Polyline[this.endJoint0.Length];
            for (int i = 0; i < pipe.endJoint0.Length; i++) {
                pipe.endJoint0[i] = new Polyline(this.endJoint0[i]);
            }

            pipe.endJoint1 = new Polyline[this.endJoint1.Length];
            for (int i = 0; i < pipe.endJoint1.Length; i++) {
                pipe.endJoint1[i] = new Polyline(this.endJoint1[i]);
            }

            if (PipeJoint0 != null)
                pipe.PipeJoint0 = this.PipeJoint0.Duplicate();

            if (PipeJoint1 != null)
                pipe.PipeJoint1 = this.PipeJoint1.Duplicate();

            pipe.breploft =this.breploft.DuplicateBrep();

            pipe.brepCuts = new List<Brep>();
            for (int i = 0; i < this.brepCuts.Count; i++) {
                pipe.brepCuts.Add(this.brepCuts[i].DuplicateBrep());
            }

            return pipe;
        }

        public void Transform(Transform t) {



            this.line.Transform(t);
  
            this.cutter0.Transform(t);
            this.cutter1.Transform(t);
            this.poly0.Transform(t);
            this.poly1.Transform(t);
            this.c0.Transform(t);
            this.c1.Transform(t);

        /*
            for(int i = 0; i < this.boundingBoxCuts.Count;i++) {
                Polyline ppp = boundingBoxCuts[i].Duplicate();
                ppp.Transform(t);
                this.boundingBoxCuts[i] = ppp;
            }
 */
            this.boundingBoxCuts.Transform(t);
            this.meshProjections.Transform(t);
            this.meshProjectionsLines = this.meshProjectionsLines.Transform(t);

            this.drillingHoles = this.drillingHoles.Transform(t);
            this.jointPlanes = this.jointPlanes.Transform(t);

            this.meshloft.Transform(t);

            this.endJoint0.Transform(t);
            this.endJoint1.Transform(t);

            if (PipeJoint0 != null)
                this.PipeJoint0.Transform(t);

            if (PipeJoint1 != null)
               this.PipeJoint1.Transform(t);

            this.breploft.Transform(t);

            for (int i = 0; i < this.brepCuts.Count; i++) {
                this.brepCuts[i].Transform(t); 
            }
            this.plane.Transform(t);
        }



    }

    public class ClosestLinesT {
        public List<Line> cp = new List<Line>();
        public List<Line> L0 = new List<Line>();
        public List<Line> L1 = new List<Line>();
        public List<int> ID0 = new List<int>();
        public List<int> ID1 = new List<int>();
        public List<double> T0 = new List<double>();
        public List<double> T1 = new List<double>();

        public override string ToString()
        {
            return "ClosestLinesT"+ " " + L0.Count.ToString();
        }

    }

    public static class PipeUtil {
        public static List<Line> ClosestLines(this List<Line> L, double tolerance, ref ClosestLinesT t, bool Bbox = true) {
            
            List<Line> lines = new List<Line>();

            HashSet<long> pairs = new HashSet<long>();

            List<int> pairA = new List<int>();
            List<int> pairB = new List<int>();
            List<double> pairA_T = new List<double>();
            List<double> pairB_T = new List<double>();

            List<Line> pairA_L = new List<Line>();
            List<Line> pairB_L = new List<Line>();


            t = new ClosestLinesT();


            for (int i = 0; i < L.Count; i++) {
                for (int j = 0; j < L.Count; j++) {
                    if (i == j) continue;




                    long key0 = MathUtil.GetKey(i, j);
                    long key1 = MathUtil.GetKey(j, i);

                    if (pairs.Contains(key0)) continue;

                    pairs.Add(key0);
                    pairs.Add(key1);

                    if (Bbox) {
                        BoundingBox bbox0 = L[i].BoundingBox;
                        BoundingBox bbox1 = L[j].BoundingBox;
                        if (!BoundingBoxUtil.Intersects(bbox0, bbox1))
                            continue;
                    }

                    double t0, t1;
                    if (Rhino.Geometry.Intersect.Intersection.LineLine(L[i], L[j], out t0, out t1, tolerance, true)) {
                        Line line = new Line(L[i].PointAt(t0), L[j].PointAt(t1));
                        //if(line.Length < tolerance)
                        lines.Add(line);


                        t.T0.Add(t0);
                        t.T1.Add(t1);
                        t.ID0.Add(i);
                        t.ID1.Add(j);
                        t.L0.Add(L[i]);
                        t.L1.Add(L[j]);


                    }


                }
            }



            return lines;
        }


        /// <summary>
        /// if plane is Unset, the default plane is new Plane(origin, Line.Direction);
        /// </summary>
        /// <param name="line"></param>
        /// <param name="id"></param>
        /// <param name="plane_"></param>
        /// <param name="radius"></param>
        /// <param name="cutter0"></param>
        /// <param name="cutter1"></param>
        /// <param name="sides"></param>
        /// <param name="radius1"></param>
        /// <param name="createBrepLoft"></param>
        /// <returns></returns>
        public static Pipe CreatePipe(this Line line, int id, Plane plane_, double radius, Plane cutter0, Plane cutter1, int sides = 4, double radius1 = -1, bool createBrepLoft = false) {

            //Rhino.RhinoApp.WriteLine(radius.ToString());
            Plane plane = (plane_ != Plane.Unset) ? plane_ : new Plane(line.From, line.Direction);
            double r = Math.Max(0.01, radius);
            int s = Math.Max(3, sides);
            double r1 = (radius1 == -1) ? r : Math.Max(radius, radius1);

            

            Plane p0 = new Plane(line.From, plane.XAxis, plane.YAxis);
            Plane p1 = new Plane(line.To, plane.XAxis, plane.YAxis);

            Polyline poly0 = NGonsCore.PolylineUtil.Polygon(s, r, p0,Math.PI*0.25,false);
            Polyline poly1 = NGonsCore.PolylineUtil.Polygon(s, r1, p1, Math.PI * 0.25, false);
            Circle c0 = new Circle(p0, r);
            Circle c1 = new Circle(p1, r1);
     

            Pipe pipe = new Pipe(id, line, plane, radius, cutter0, cutter1, poly0, poly1, c0, c1);

            //Cut with planes if they are given
            if (cutter0 != Plane.Unset && cutter0 != Plane.Unset)
            {
                Polyline result0 = new Polyline();
                Polyline result1 = new Polyline();

                //Line[] lines = new Line[poly0.Count - 1];

                for (int i = 0; i < poly0.Count - 1; i++)
                {
                    Line l = new Line(poly0[i], poly1[i]);
                    Point3d cutP0 = NGonsCore.PlaneUtil.LinePlane(l, cutter0);
                    Point3d cutP1 = NGonsCore.PlaneUtil.LinePlane(l, cutter1);
                    result0.Add(cutP0);
                    result1.Add(cutP1);
                }

                result0.Close();
                result1.Close();


                pipe.meshloft = result0.LoftMeshFast(result1, false);
                pipe.breploft = Brep.CreatePipe(line.ToNurbsCurve(), new double[] { 0, 1 }, new double[] { r, r1 }, false, PipeCapMode.Flat, true, 0.01, 0.01)[0];
                //line.Bake();
                return pipe;
            }
            else
            {
                // Rhino.RhinoApp.WriteLine(r.ToString() + " " + r1.ToString());
             

                pipe.meshloft = poly0.LoftMeshFast(poly1, false);
                pipe.breploft = Brep.CreatePipe(line.ToNurbsCurve(), new double[] { 0, 1 }, new double[] { r, r1 }, false, PipeCapMode.Flat, true, 0.01, 0.01)[0];
            }
            return pipe;
            //if (createBrepLoft) pipe.breploft = Surface.CreateExtrusion(c0.ToNurbsCurve(), line.Direction).ToBrep();
  

        }
    }

    /// <summary>
    /// Pipe is used to create generic joints
    /// And then through transformation apply them
    /// </summary>
    public class PipeJoint {

        
        public Plane plane0;
        public Plane plane1;

        public List<Polyline> maleCrv0 = new List<Polyline>();
        public List<Polyline> maleCrv1 = new List<Polyline>();
        public List<Polyline> femaleCrv0 = new List<Polyline>();
        public List<Polyline> femaleCrv1 = new List<Polyline>();

        public Mesh maleMesh;
        public Mesh femaleMesh;
        public Brep maleBrep;
        public Brep femaleBrep;

        public PipeJoint(Plane Pln0, Plane Pln1, List<Polyline> MaleCrv0, List<Polyline> MaleCrv1, List<Polyline> FemaleCrv0, List<Polyline> FemaleCrv1) {
            this.plane0 = new Plane(Pln0);
            this.plane1 = new Plane(Pln1);
            this.maleCrv0 = MaleCrv0.Duplicate();
            this.maleCrv1 = MaleCrv1.Duplicate();
            this.femaleCrv0 = FemaleCrv0.Duplicate();
            this.femaleCrv1 = FemaleCrv1.Duplicate();

        }

        public PipeJoint Duplicate() {
            PipeJoint pipeJoints = new PipeJoint(this.plane0, this.plane1, this.maleCrv0, this.maleCrv1, this.femaleCrv0, this.femaleCrv1);
            if (maleMesh != null)
                pipeJoints.maleMesh = this.maleMesh.DuplicateMesh();
            if (femaleMesh != null)
                pipeJoints.femaleMesh = this.femaleMesh.DuplicateMesh();
            if (maleBrep != null)
                pipeJoints.maleBrep = this.maleBrep.DuplicateBrep();
            if (femaleBrep != null)
                pipeJoints.femaleBrep = this.femaleBrep.DuplicateBrep();
            return pipeJoints;
        }

        public void Transform(Transform xform) {


            this.plane0.Transform(xform);
            this.plane1.Transform(xform);
            this.maleCrv0.Transform(xform);
            this.maleCrv1.Transform(xform);
            this.femaleCrv0.Transform(xform);
            this.femaleCrv1.Transform(xform);
            if (this.maleMesh != null)
                this.maleMesh.Transform(xform);
            if (this.femaleMesh != null)
                this.femaleMesh.Transform(xform);
            if (this.maleBrep != null)
                this.maleBrep.Transform(xform);
            if (this.femaleBrep != null)
                this.femaleBrep.Transform(xform);
        }

        public void OrientScale(Plane target, double width, double height) {

            Transform scale = Rhino.Geometry.Transform.Scale(this.plane0, width, width, height);
            Transform orient = Rhino.Geometry.Transform.PlaneToPlane(this.plane1, target);
            Transform xform = Rhino.Geometry.Transform.Multiply(orient,scale );

            this.plane0 = new Plane(target);
            this.plane1 = new Plane(target);
            this.maleCrv0.Transform(xform);
            this.maleCrv1.Transform(xform);
            this.femaleCrv0.Transform(xform);
            this.femaleCrv1.Transform(xform);
            if (this.maleMesh != null)
                this.maleMesh.Transform(xform);
            if (this.femaleMesh != null)
                this.femaleMesh.Transform(xform);
            if (this.maleBrep != null)
                this.maleBrep.Transform(xform);
            if (this.femaleBrep != null)
                this.femaleBrep.Transform(xform);
        }

    }
}
