using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NGonsCore.Nexorades
{
    public class Nexors : IEnumerable<Nexor>
    {

        private List<Nexor> _nexors = new List<Nexor>();
        Dictionary<Tuple<int, int>, int> _id = new Dictionary<Tuple<int, int>, int>();
        List<Tuple<int, int>> id_ = new List<Tuple<int, int>>();

        int counter = 0;

        //Volumetric properties
        double depth = -0.1;
        double offset0 = 0.02;
        double plateThickness = 0.012;
        double scale = 1;
        double movePipeAxis = 0.04;
        double extend = 1;//center beam thickness

        //Projection to the ground
        double z = -0.2;
        Plane ground = new Plane(new Point3d(0, 0, -0.12), Vector3d.ZAxis);


        //Constructor
        public Nexors()
        {

        }

        public Nexors Duplicate()
        {

            Nexors nexors = new Nexors();
            try
            {
                for (int i = 0; i < _nexors.Count; i++)
                    nexors.Add(this._nexors[i].Duplicate(), this.id_[i].Item1, this.id_[i].Item2);

                nexors._id = this._id;
                nexors.id_ = this.id_;
            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

            return nexors;
        }

        //Base methods
        IEnumerator IEnumerable.GetEnumerator() => this._nexors.GetEnumerator();
        IEnumerator<Nexor> IEnumerable<Nexor>.GetEnumerator() => this._nexors.GetEnumerator();
        public int Count() => this._nexors.Count;

        //Methods

        public Nexor this[int i, int j] {
            get { return _nexors[_id[new Tuple<int, int>(i, j)]]; }
            set { this._nexors[_id[new Tuple<int, int>(i, j)]] = value.Duplicate(); }
        }



        public void Add(Line line, int i, int j)
        {
            this._nexors.Add(new Nexor(counter, i, j, line));
            this._id.Add(new Tuple<int, int>(i, j), counter);
            this.id_.Add(new Tuple<int, int>(i, j));

            counter++;
        }

        public void Add(Nexor nexor, int i, int j)
        {
            this._nexors.Add(nexor.Duplicate());
            this._id.Add(new Tuple<int, int>(i, j), counter);
            this.id_.Add(new Tuple<int, int>(i, j));
            counter++;
        }


        //Main Methods

        public DataTree<Polyline> Beams2(NGonsCore.MeshProps p)
        {


            DataTree<Polyline> dt = new DataTree<Polyline>();

            try
            {

                //Edge and End Planes
                Dictionary<int, int> E = p.M._EAll();//mesh edge - ngon edge
                for (int i = 0; i < p.M._countF(); i++)
                {

                    int[] fe = p.M._fe(i);


                    for (int j = 0; j < p.M._countE(i); j++)
                    {

                        if (this[i, j].isNexor != 0)
                        {

                            int e = E[fe[j]];//get ngon Edge by local meshedge
                            this[i, j].ePl90 = p.ePl90[e].ChangeOrigin(this[i, j].line.Center());
                            this[i, j].ePl = p.ePl[e].ChangeOrigin(this[i, j].line.Center());
                            this[i, j].ePl90Offset = this[i, j].ePl90.MovePlanebyAxis(depth);

                            int[] o = p.M._OppositeFE(i, j);
                            int i_ = (o[0] != -1) ? o[0] : i;
                            int j_ = (o[1] != -1) ? o[1] : j;
                            int naked = (o[0] != -1) ? 1 : -1;


                            Plane ePl0 = this[i, j].ePl;
                            Plane ePl1 = new Plane(this[i, j].ePl.Origin, this[i, j].ePl.YAxis, this[i, j].ePl.ZAxis);
                            Plane cPl = p.fPl[i];
                            Plane oPl = p.fPl[i_];

                            Point3d originC = PlaneUtil.PlanePlanePlane(ePl0, ePl1, cPl);
                            Point3d originO = PlaneUtil.PlanePlanePlane(ePl0, ePl1, oPl);

                            //Plane correspoding to current face plane
                            Plane fcPl = p.fePl[i][j].ChangeOrigin(originC);
                            Plane fcPl90 = p.fePl90[i][j].ChangeOrigin(originC);
                            Vector3d v0 = -fcPl.ZAxis * offset0 * scale;
                            Vector3d v1 = -fcPl.YAxis * plateThickness;

                            Plane fcPl0 = fcPl90;
                            //this[i, j].fcPl.Add(fcPl90);
                            this[i, j].fcPl.Add(fcPl.Translation(v0 * extend));
                            this[i, j].fcPl.Add(fcPl90.Translation(v0 + v1));
                            this[i, j].fcPl.Add(this[i, j].ePl.Translation(v0 * 2));

                            if (i == 10)
                            {
                                //Line line = new Line(originO.zy);
                                //this[i, j].fcPl.Bake();
                            }

                            //Plane corresponding to opposite plane
                            Plane foPl = p.fePl[i_][j_].ChangeOrigin(originO);
                            Plane foPl90 = p.fePl90[i_][j_].ChangeOrigin(originO);
                            v0 = -foPl.ZAxis * offset0 * scale * naked;//for naked opposite - do i need to flip?
                            v1 = -foPl.YAxis * plateThickness;



                            this[i, j].foPl.Add(this[i, j].ePl.Translation(v0 * 2));
                            this[i, j].foPl.Add(foPl90.Translation(v0 + v1));
                            this[i, j].foPl.Add(foPl.Translation(v0 * extend));
                            //this[i, j].foPl.Add(foPl90);
                            Plane foPl3 = foPl90;


                            //Average plane for chamfer middle part of the beam

                            Line l1 = PlaneUtil.PlanePlane(fcPl90, this[i, j].fcPl[0]);
                            Line l0 = PlaneUtil.PlanePlane(this[i, j].foPl[2], foPl90);

                            this[i, j].fcPl_foPl = new Plane((l0.Center() + l1.Center()) * 0.5, l0.Direction, l0.Center() - l1.Center());




                            //End Planes - next edge opposite
                            int ne = E[fe.Next(j)];//get next ngon edge
                            int[] no = p.M._OppositeFE(i, (j + 1).Wrap(p.M._countE(i)));//get next opposite edge
                            this[i, j].endPl0 = (no[0] != -1) ? this[i, j].endPl0 = p.ePl[ne].ChangeOrigin(this[no[0], no[1]].line.Center()) : this[i, j].endPl0 = p.ePl[ne].ChangeOrigin(this[i, j].line.To);//change origin because planes are located at mesh edge
                            this[i, j].endPl0 = this[i, j].endPl0.MovePlanebyAxis(offset0 * 2, this[i, j].line.Center());//move towards the center

                            //End Planes - prev edge opposite
                            int pe = E[fe.Prev(j)];//get previous ngon edge
                            int[] po = p.M._OppositeFE(i, (j - 1).Wrap(p.M._countE(i)));////get prev opposite edge
                            this[i, j].endPl1 = (po[0] != -1) ? p.ePl[pe].ChangeOrigin(this[po[0], po[1]].line.Center()) : p.ePl[pe].ChangeOrigin(this[i, j].line.From);//change origin because planes are located at mesh edge
                            this[i, j].endPl1 = this[i, j].endPl1.MovePlanebyAxis(offset0 * 2, this[i, j].line.Center());//move towards the center


                        }//is nexor
                    }//for j
                }//for i




                //Nodes profiles
                for (int i = 0; i < p.M._countF(); i++)
                {
                    int[] fe = p.M._fe(i);
                    for (int j = 0; j < p.M._countE(i); j++)
                        if (this[i, j].isNexor != 0)
                        {

                            List<Polyline> cutters0 = new List<Polyline>();
                    


                            List<Plane> sidePlanes = new List<Plane>();
                            sidePlanes.AddRange(this[i, j].fcPl);
                            sidePlanes.Add(this[i, j].ePl90Offset);
                            sidePlanes.AddRange(this[i, j].foPl);
                            sidePlanes.Add(this[i, j].fcPl_foPl);

                            if ((!p.M.IsNaked(fe[j])))//
                            {



                                //End Planes - next edge opposite
                                int[] op = p.M._OppositeFE(i, j, -1);
                                int[] on = p.M._OppositeFE(i, j, 1);



                                int[] no = p.M._OppositeFE(i, (j + 1).Wrap(fe.Length));//for end parts
                                int[] po = p.M._OppositeFE(i, (j - 1).Wrap(fe.Length));//for end parts


                                //First profile
                                Polyline p0 = PolylineUtil.PolylineFromPlanes(this[i, j].endPl0, sidePlanes);

                                //Plane sectionPlaneP0 = p.M.IsNaked(fe.Next(j)) ? this[i, (j + 1).Wrap(fe.Length)].ePl : this[op[0], op[1]].fcPl[0];
                                if (op[0] != -1) p0 = PolylineUtil.PolylineFromPlanes(this[op[0], op[1]].fcPl[0], sidePlanes);






                                List<Plane> sidePlanes1A = new List<Plane>();
                                sidePlanes1A.AddRange(this[i, j].fcPl);
                                sidePlanes1A.Add(this[i, j].ePl90Offset);
                                sidePlanes1A.Add(this[i, j].foPl[0]);
                                sidePlanes1A.Add(this[i, j].foPl[1].MovePlanebyAxis(plateThickness));
                                sidePlanes1A.Add(this[i, j].fcPl_foPl);

                                Plane sectionPlaneP3A_ = p.M.IsNaked(fe.Next(j)) ? this[i, (j + 1).Wrap(fe.Length)].foPl[0] : this[op[0], op[1]].foPl[0];
                                Plane sectionPlaneP3A = p.M.IsNaked(fe.Next(j)) ? this[i, (j + 1).Wrap(fe.Length)].fcPl[2] : this[op[0], op[1]].foPl[0];

                                if (p.M.IsNaked(fe.Next(j)) && (p.M.TopologyEdges.EdgeLine(fe.Next(j)).To.Z < z || p.M.TopologyEdges.EdgeLine(fe.Next(j)).From.Z < z))
                                {
                                    sectionPlaneP3A_ = ground;
                                    this[i, j].endPl0 = ground;
                       
                                }


                                List<Plane> sidePlanes1BExtended = new List<Plane>();
                                sidePlanes1BExtended.Add(this[i, j].fcPl[0]);
                                sidePlanes1BExtended.Add(this[i, j].ePl90Offset.MovePlanebyAxis(this.depth * 2, this[i, j].line.Center(), 2, true));
                                sidePlanes1BExtended.Add(this[i, j].foPl[0]);

                                sidePlanes1BExtended.Add(this[i, j].foPl[1].MovePlanebyAxis(plateThickness));
                                sidePlanes1BExtended.Add(this[i, j].fcPl_foPl);

                                Polyline p2A = PolylineUtil.PolylineFromPlanes(this[op[0], op[1]].fcPl[0], sidePlanes1A);
                                Polyline p3A = PolylineUtil.PolylineFromPlanes(sectionPlaneP3A, sidePlanes1A);//Top Removal - Inner
                                Polyline p3A_ = PolylineUtil.PolylineFromPlanes(sectionPlaneP3A_, sidePlanes1A);

                                Polyline p3A_Extended = PolylineUtil.PolylineFromPlanes(sectionPlaneP3A, sidePlanes1BExtended); //1
                                cutters0.Add(p3A_Extended);
                           

                                this[i, j].profiles.Add(p2A);
                                this[i, j].profiles.Add(p3A_);
                          

                                if (p.M.IsNaked(fe.Next(j))) this[i, j].endPl0 = sectionPlaneP3A_;


                                if (no[0] != -1)//i == 20 && j == 1 &&
                                {

                                    List<Plane> sidePlanes2 = new List<Plane>();
                                    sidePlanes2.AddRange(this[i, j].fcPl);
                                    sidePlanes2.Add(this[i, j].ePl90Offset);
                                    sidePlanes2.Add(this[i, j].foPl[0]);
                                    sidePlanes2.Add(this[no[0], no[1]].foPl[1].MovePlanebyAxis(plateThickness));
                                    sidePlanes2.Add(this[i, j].fcPl_foPl);

                                    List<Plane> sidePlanes2Extended = new List<Plane>();
                                    sidePlanes2Extended.Add(this[i, j].fcPl[0].MovePlanebyAxis(this.plateThickness, this[i, j].line.Center(), 2, false));
                                    sidePlanes2Extended.Add(this[i, j].ePl90Offset.MovePlanebyAxis(this.depth * 2, this[i, j].line.Center(), 2, true));
                                    sidePlanes2Extended.Add(this[i, j].foPl[0].MovePlanebyAxis(this.plateThickness, this[i, j].line.Center(), 2, false));
                                    sidePlanes2Extended.Add(this[no[0], no[1]].foPl[1].MovePlanebyAxis(plateThickness));
                                    sidePlanes2Extended.Add(this[i, j].fcPl_foPl);




                                    Polyline p4A = PolylineUtil.PolylineFromPlanes(this[i, j].endPl0, sidePlanes2);//Top Removal - End
                                    Polyline p4A_Extended = PolylineUtil.PolylineFromPlanes(this[i, j].endPl0.MovePlanebyAxis(0.00, this[i, j].line.Center(), 2, false), sidePlanes2Extended);//Top Removal - End - 0
                                    cutters0.Insert(0, p4A_Extended);
              
                                  


                                    this[i, j].profiles.Add(p3A);
                                    this[i, j].profiles.Add(p4A);


                                }



                                //Second profile
                                Polyline p1 = PolylineUtil.PolylineFromPlanes(this[i, j].endPl1, sidePlanes);

                                //Plane sectionPlaneP1 = p.M.IsNaked(fe.Prev(j)) ? this[i, (j - 1).Wrap(fe.Length)].ePl : this[on[0], on[1]].fcPl[0];
                                if (on[0] != -1) p1 = PolylineUtil.PolylineFromPlanes(this[on[0], on[1]].fcPl[0], sidePlanes);


                                List<Plane> sidePlanes1B = new List<Plane>();
                                sidePlanes1B.AddRange(this[i, j].fcPl);
                                sidePlanes1B.Add(this[i, j].ePl90Offset);
                                sidePlanes1B.Add(this[i, j].foPl[0]);
                                sidePlanes1B.Add(this[i, j].foPl[1].MovePlanebyAxis(plateThickness));
                                sidePlanes1B.Add(this[i, j].fcPl_foPl);




                                Plane sectionPlaneP3B_ = p.M.IsNaked(fe.Prev(j)) ? this[i, (j - 1).Wrap(fe.Length)].foPl[0] : this[on[0], on[1]].foPl[0];
                                Plane sectionPlaneP3B = p.M.IsNaked(fe.Prev(j)) ? this[i, (j - 1).Wrap(fe.Length)].fcPl[2] : this[on[0], on[1]].foPl[0];
                                if (p.M.IsNaked(fe.Prev(j)) && (p.M.TopologyEdges.EdgeLine(fe.Prev(j)).To.Z < z || p.M.TopologyEdges.EdgeLine(fe.Prev(j)).From.Z < z))
                                {
                                    sectionPlaneP3B_ = ground;
                                    this[i, j].endPl1 = ground;
                                    //p.M.TopologyEdges.EdgeLine(fe[j]).Center().Bake();
                                }

                                Polyline p2B = PolylineUtil.PolylineFromPlanes(this[on[0], on[1]].fcPl[0], sidePlanes1B);
                                Polyline p3B = PolylineUtil.PolylineFromPlanes(sectionPlaneP3B, sidePlanes1B);//Top Removal - Inner
                                Polyline p3B_ = PolylineUtil.PolylineFromPlanes(sectionPlaneP3B_, sidePlanes1B);

                                Polyline p3B_Extended = PolylineUtil.PolylineFromPlanes(sectionPlaneP3B, sidePlanes1BExtended);//Top Removal - Inner2
                                cutters0.Add(p3B_Extended);


                                this[i, j].profiles.Add(p2B);
                                this[i, j].profiles.Add(p3B_);

          

                                if (p.M.IsNaked(fe.Prev(j))) this[i, j].endPl1 = sectionPlaneP3B_;
             



                                if (po[0] != -1)//i == 20 && j == 1 && 
                                {



                                    List<Plane> sidePlanes2 = new List<Plane>();
                                    sidePlanes2.AddRange(this[i, j].fcPl);
                                    sidePlanes2.Add(this[i, j].ePl90Offset);
                                    sidePlanes2.Add(this[i, j].foPl[0]);
                                    sidePlanes2.Add(this[po[0], po[1]].foPl[1].MovePlanebyAxis(plateThickness));
                                    sidePlanes2.Add(this[i, j].fcPl_foPl);

                                    List<Plane> sidePlanes2Extended = new List<Plane>();
                                    sidePlanes2Extended.Add(this[i, j].fcPl[0].MovePlanebyAxis(this.plateThickness, this[i, j].line.Center(), 2, false));
                                    sidePlanes2Extended.Add(this[i, j].ePl90Offset.MovePlanebyAxis(this.depth*2, this[i, j].line.Center(), 2, true));
                                    sidePlanes2Extended.Add(this[i, j].foPl[0].MovePlanebyAxis(this.plateThickness, this[i, j].line.Center(), 2, false));
                                    sidePlanes2Extended.Add(this[po[0], po[1]].foPl[1].MovePlanebyAxis(plateThickness));
                                    sidePlanes2Extended.Add(this[i, j].fcPl_foPl);


                                    Polyline p4B = PolylineUtil.PolylineFromPlanes(this[i, j].endPl1, sidePlanes2);//Top Removal - End
                                    Polyline p4B_Extended = PolylineUtil.PolylineFromPlanes(this[i, j].endPl1.MovePlanebyAxis(0.00,this[i,j].line.Center(),2,false), sidePlanes2Extended);//Top Removal - End //3
                                    cutters0.Add(p4B_Extended);
                                    //p4B_Extended.Bake();
                                    //p2.Bake();
                                    //p3.Bake();
                                    //p4.Bake();


                                    this[i, j].profiles.Add(p4B);
                                    this[i, j].profiles.Add(p3B);

                      


                                }





                                this[i, j].profiles.Add(p0);
                                this[i, j].profiles.Add(p1);
                                //End profile


                            }
                            else if (this[i, j].isNexor == 1)
                            {

                                List<Plane> sidePlanesNaked = new List<Plane>();
                                sidePlanesNaked.AddRange(this[i, j].fcPl);
                                sidePlanesNaked.Add(this[i, j].ePl90Offset);
                                sidePlanesNaked.Add(this[i, j].foPl[0]);
                                sidePlanesNaked.Add(this[i, j].fcPl_foPl);

                                Plane planeA = (this[i, j].endPl0.Origin.Z < z) ? ground : this[i, j].endPl0;
                                Plane planeB = (this[i, j].endPl1.Origin.Z < z) ? ground : this[i, j].endPl1;

                                if (this[i, j].endPl0.Origin.Z < z) this[i, j].endPl0 = ground;
                                if (this[i, j].endPl1.Origin.Z < z) this[i, j].endPl1 = ground;




                                Polyline p0 = PolylineUtil.PolylineFromPlanes(planeA, sidePlanesNaked);
                                Polyline p1 = PolylineUtil.PolylineFromPlanes(planeB, sidePlanesNaked);

                                this[i, j].profiles.Add(p0);
                                this[i, j].profiles.Add(p1);
                            }
                            else if (this[i, j].isNexor == -1)
                            {
                                List<Plane> sidePlanesNaked = new List<Plane>();

                                sidePlanesNaked.Add(this[i, j].ePl90Offset);
                                sidePlanesNaked.Add(this[i, (j + 1).Wrap(fe.Length)].fcPl[2]);
                                sidePlanesNaked.Add(this[i, (j + 1).Wrap(fe.Length)].fcPl[1]);
                                sidePlanesNaked.Add(this[i, (j + 1).Wrap(fe.Length)].fcPl[0]);
                                sidePlanesNaked.Add(this[i, j].fcPl_foPl);
                                sidePlanesNaked.AddRange(this[i, (j - 1).Wrap(fe.Length)].fcPl);


                                this[i, j].endPl0 = this[i, (j + 1).Wrap(fe.Length)].fcPl[0];
                                this[i, j].endPl1 = this[i, (j - 1).Wrap(fe.Length)].fcPl[0];



                                Polyline p0 = PolylineUtil.PolylineFromPlanes(this[i, j].fcPl[0], sidePlanesNaked);
                                Polyline p1 = PolylineUtil.PolylineFromPlanes(this[i, j].foPl[0], sidePlanesNaked);
                                //p0.Bake();
                                //p1.Bake();
                                this[i, j].profiles.Add(p0);
                                this[i, j].profiles.Add(p1);
                                //p0.Bake();
                                //p1.Bake();
                            }

                            this[i, j].CNC_Cuts.AddRange(cutters0,new Grasshopper.Kernel.Data.GH_Path(i,j));

                        }
                }




                foreach (var n in this._nexors)
                    if (n.isNexor != 0)
                    {
                        var path = new Grasshopper.Kernel.Data.GH_Path(n.idNested[0], n.idNested[1]);
                        if (n.profiles.Count > 0)
                        {
                            dt.AddRange(n.profiles, path);
                        }
                    }


            } catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            return dt;

        }


     
        public DataTree<Line> GetDowels(double radius = 0.075, int count=8)
        {
            DataTree<Line> dt = new DataTree<Line>();

            for (int i = 0; i < this._nexors.Count; i++)
            {
                if (this._nexors[i].isNexor != 0)
                {
                    var path = new GH_Path(i);
                    Point3d c = (this._nexors[i].endPl0.Origin + this._nexors[i].endPl1.Origin) * 0.5;

                    int dir = (c.DistanceToSquared(this._nexors[i].endPl0.Origin + this._nexors[i].endPl0.ZAxis) < c.DistanceToSquared(this._nexors[i].endPl0.Origin - this._nexors[i].endPl0.ZAxis)) ? -1 : 1;

                    Plane plane = this._nexors[i].endPl0.ChangeOrigin(PlaneUtil.LinePlane(this._nexors[i].pipe.line, this._nexors[i].endPl0));
                    Circle circle = new Circle(plane, radius);
                    Curve curve = circle.ToNurbsCurve();
                    curve.DivideByCount(count, true, out Point3d[] pts);

                    foreach (Point3d p in pts)
                    {
                        Line line = new Line(p, this._nexors[i].endPl0.ZAxis * dir* 0.15);
                        dt.Add(line, path);
                        //line.Bake();
                    }

                    plane = this._nexors[i].endPl1.ChangeOrigin(PlaneUtil.LinePlane(this._nexors[i].pipe.line, this._nexors[i].endPl1));
                    circle = new Circle(plane, radius);
                    curve = circle.ToNurbsCurve();
                    curve.DivideByCount(count, true, out Point3d[] pts1);



                    dir = (c.DistanceToSquared(this._nexors[i].endPl1.Origin + this._nexors[i].endPl1.ZAxis) < c.DistanceToSquared(this._nexors[i].endPl1.Origin - this._nexors[i].endPl1.ZAxis)) ? -1 : 1;


                    foreach (Point3d p in pts1)
                    {
                       
                        Line line = new Line(p, this._nexors[i].endPl1.ZAxis *dir*0.15);
                        dt.Add(line, path);
                        //line.Bake();
                    }

                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddCircle(circle);


                }
            }
            return dt;
        }

        public DataTree<Polyline> Panels(NGonsCore.MeshProps p)
        {
            //Draw panels
            DataTree<Polyline> dtPlates = new DataTree<Polyline>();
            for (int i = 0; i < p.M._countF(); i++)
            {

                Plane panel0 = p.fPl[i];
                Plane panel1 = p.fPl[i].MovePlanebyAxis(-plateThickness);
                Plane panel2 = panel1.MovePlanebyAxis(plateThickness*10);
                List<Plane> sidePlanes = new List<Plane>();


                for (int j = 0; j < p.M._countE(i); j++)
                {

                    if (this[i, j].isNexor != 0)
                    {
                        sidePlanes.Add(this[i, j].fcPl[0]);
                    }
                    else
                    {

                        int[] oppo = p.M._OppositeFE(i, j);

                        Plane sidePlane = Plane.Unset;

                        if (oppo[0] != -1)
                        {
                            Plane oppoPlane = this[oppo[0], oppo[1]].ePl;

                            Plane ePl0 = oppoPlane;
                            Plane ePl1 = new Plane(oppoPlane.Origin, oppoPlane.YAxis, oppoPlane.ZAxis);
                            Plane left = p.fPl[i];
                            Plane right = p.fPl[oppo[0]];

                            Point3d originRight = PlaneUtil.PlanePlanePlane(ePl0, ePl1, left);
                            Point3d originLeft = PlaneUtil.PlanePlanePlane(ePl0, ePl1, right);
                            sidePlane = p.fePl[i][j].ChangeOrigin(originRight);
                        }
                        else
                        {
                            sidePlane = p.fePl[i][j];
                        }



                        sidePlane = sidePlane.MovePlanebyAxis(offset0 * scale * extend, panel0.Origin);
                        sidePlanes.Add(sidePlane);

                    }


                }



                    Polyline panelOutline0 = PolylineUtil.PolylineFromPlanes(panel0, sidePlanes);
                Polyline panelOutline1 = PolylineUtil.PolylineFromPlanes(panel1, sidePlanes);
                Polyline panelOutline2 = PolylineUtil.PolylineFromPlanes(panel2, sidePlanes);
                dtPlates.Add(panelOutline0, new Grasshopper.Kernel.Data.GH_Path(i));
                dtPlates.Add(panelOutline1, new Grasshopper.Kernel.Data.GH_Path(i));

                for (int j = 0; j < p.M._countE(i); j++)
                {
                    int[] op = p.M._OppositeFE(i,j);
                    if (this[i, j].isNexor != 0)
                    {
                        this[i, j].CNCtopProfileCuts.Add(panelOutline1);
                        this[i, j].CNCtopProfileCuts.Add(panelOutline2);
                    }

                    if(op[0]!= -1)
                    {
                        this[op[0], op[1]].CNCtopProfileCuts.Add(panelOutline1);
                        this[op[0], op[1]].CNCtopProfileCuts.Add(panelOutline2);
                    }
                }


            }



            return dtPlates;

            List<Line> pipes = new List<Line>();
            for (int i = 0; i < p.M._countF(); i++)
            {
                int[] fe = p.M._fe(i);
                for (int j = 0; j < p.M._countE(i); j++)
                {

                    if (this[i, j].isNexor != 0 && !p.M.IsNaked(fe[j]))
                    {
                        Line line = this[i, j].line;
                        line.Transform(Transform.Translation(this[i, j].ePl.YAxis * -movePipeAxis));
                        line = new Line(PlaneUtil.LinePlane(line, this[i, j].endPl0), PlaneUtil.LinePlane(line, this[i, j].endPl1));
                        line.Extend(0.2, 0.2);
                        pipes.Add(line);
                    }

                }
            }
        }

        /// <summary>
        ///For center axis I need three planes:
        ///A - Edge plane
        ///B - Bottom Plane
        ///C - Section Plane
        ///Also two points
        ///And radius
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DataTree<Polyline> RoundBeams(NGonsCore.MeshProps p, double radius = 0.075)
        {
            var dt = new DataTree<Polyline>();


            for (int i = 0; i < p.M._countF(); i++)
            {

                int[] fe = p.M._fe(i);
                for (int j = 0; j < p.M._countE(i); j++)
                {

                    if (this[i, j].isNexor != 0)
                    {
                        Plane edgePlane = this[i, j].ePl;
                        Plane sectionPlane = new Plane(this[i, j].ePl.Origin, this[i, j].ePl.YAxis, this[i, j].ePl.ZAxis);
                        Plane bottomPlane = this[i, j].ePl90Offset;

                        //for getting the points

                        Point3d p1 = PlaneUtil.PlanePlanePlane(this[i, j].foPl[0], this[i, j].foPl[1].MovePlanebyAxis(this.plateThickness), sectionPlane);
                        Point3d p0 = PlaneUtil.PlanePlanePlane(this[i, j].fcPl[2], this[i, j].fcPl[1].MovePlanebyAxis(this.plateThickness), sectionPlane);

                        Point3d pc = PlaneUtil.PlanePlanePlane(this[i, j].fcPl[0], this[i, j].fcPl_foPl, sectionPlane);
                        Point3d po = PlaneUtil.PlanePlanePlane(this[i, j].foPl[2], this[i, j].fcPl_foPl, sectionPlane);
                        //pc.Bake();
                        //po.Bake();

                        //if (i == 20) {

                        Line line = this[i, j].line;
                        Circle circle = CircleUtil.GetCircleCenter(edgePlane, bottomPlane, sectionPlane, new List<Point3d> { p0, p1 }, radius);
                        Vector3d v = circle.Center - line.From;
                        line.Transform(Transform.Translation(v));
                        //Rhino.RhinoDoc.ActiveDoc.Objects.AddCircle(circle);



                        //Rhino.RhinoApp.WriteLine(p.M.IsNaked(p.M._meshE(i, j)).ToString());



                        int[] on = (p.M.IsNaked(fe[j])) ? new int[] { i, (j + 1).Wrap(fe.Length) } : p.M._OppositeFE(i, j, 1);
                        int[] op = (p.M.IsNaked(fe[j])) ? new int[] { i, (j - 1).Wrap(fe.Length) } : p.M._OppositeFE(i, j, -1);

                        bool onFlag = this[on[0], on[1]].isNexor == 0;
                        bool opFlag = this[op[0], op[1]].isNexor == 0;

                        if (this[on[0], on[1]].isNexor == 0) on = p.M._OppositeFE(on[0], on[1]);
                        if (this[op[0], op[1]].isNexor == 0) op = p.M._OppositeFE(op[0], op[1]);

                        Plane pl0 = onFlag ? this[on[0], on[1]].foPl[0] : this[on[0], on[1]].fcPl[2];
                        Plane pl1 = opFlag ? this[op[0], op[1]].foPl[0] : this[op[0], op[1]].fcPl[2];

                        pl0 = this[i, j].endPl0;
                        pl1 = this[i, j].endPl1;

                        if (this[on[0], on[1]].isNexor != 0 && this[op[0], op[1]].isNexor != 0)
                        {
                            line = new Line(PlaneUtil.LinePlane(line, this[i, j].endPl0), PlaneUtil.LinePlane(line, this[i, j].endPl1));
                            line.Extend(0.1, 0.1);

                            //line.Transform(Transform.Scale(line.Center(), 2));
                            this[i, j].pipe = NGonsCore.PipeUtil.CreatePipe(
                                line,
                                this[i, j].id, sectionPlane,
                                radius,
                                pl0.MovePlanebyAxis(0.01,this[i, j].line.Center(),2,false),
                                pl1.MovePlanebyAxis(0.01, this[i, j].line.Center(), 2, false),
                                10,-1,true);




                            // Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(pipe.meshloft);
                            //Rhino.RhinoDoc.ActiveDoc.Objects.AddBrep(this[i, j].pipe.breploft);
                        }


                        //}


                    }




                }

            }




            return dt;
        }


        public DataTree<Polyline> Cuts()
        {
            DataTree<Polyline> dt = new DataTree<Polyline>();



            //Create end cuts
            for (int i = 0; i < this._nexors.Count; i++)
            {
                if (this._nexors[i].isNexor != 0)
                {
                    var path = new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1, this.id_[i].Item2);

                    //1. Get bisector planes
                    List<Plane> sidePlanes = new List<Plane>()
                    {
                        this._nexors[i].fcPl[2],
                        this._nexors[i].endPl0,
                         this._nexors[i].foPl[0],
                          this._nexors[i].endPl1
                    };

                    Polyline outline = PolylineUtil.PolylineFromPlanes(this._nexors[i].ePl90Offset, sidePlanes);
                    Plane[] bisectors = outline.PolylineBisectors();
                    Plane higherPlane = (this._nexors[i].fcPl[1].Origin.DistanceToSquared(this._nexors[i].ePl90Offset.Origin) > this._nexors[i].foPl[1].Origin.DistanceToSquared(this._nexors[i].ePl90Offset.Origin)) ? this._nexors[i].fcPl[1] : this._nexors[i].foPl[1];
                    higherPlane = higherPlane.MovePlanebyAxis(this.plateThickness * 3);
                    Plane lowerPlane = this._nexors[i].ePl90Offset.MovePlanebyAxis(-this.plateThickness * 3);
                    Polyline outline1 = PolylineUtil.PolylineFromPlanes(higherPlane, sidePlanes);
                    Plane[] bisectors1 = outline1.PolylineBisectors();

                    for (int j = 0; j < bisectors.Length; j++)
                    {
                        bisectors[j] = new Plane(
                            (bisectors[j].Origin + bisectors1[j].Origin) * 0.5,
                            (bisectors[j].XAxis + bisectors1[j].XAxis) * 0.5,
                            bisectors[j].Origin - bisectors1[j].Origin
                             );
                    }


                    //Create V-Shape cuts
                    Point3d nexorCenter = this._nexors[i].line.Center();


                    sidePlanes = new List<Plane>()  {
                        this._nexors[i].endPl0.MovePlanebyAxis(0.2,nexorCenter),
                        this._nexors[i].fcPl[2].MovePlanebyAxis(0.1,nexorCenter,2,false),
                        bisectors[0],
                        this._nexors[i].endPl0,
                        bisectors[1],
                        this._nexors[i].foPl[0].MovePlanebyAxis(0.1,nexorCenter,2,false)
                        ,  };
                    Polyline endCuts0A = PolylineUtil.PolylineFromPlanes(lowerPlane, sidePlanes, true);
                    Polyline endCuts0B = PolylineUtil.PolylineFromPlanes(higherPlane, sidePlanes, true);
                    sidePlanes[0] = sidePlanes[0].MovePlanebyAxis(-0.2 * 2, nexorCenter, 2, false);
                    Polyline endCuts0A_ = PolylineUtil.PolylineFromPlanes(lowerPlane, sidePlanes, true);
                    Polyline endCuts0B_ = PolylineUtil.PolylineFromPlanes(higherPlane, sidePlanes, true);

                    //endCuts0A_.Bake();
                    //endCuts0B_.Bake();

                    sidePlanes = new List<Plane>() {
                        this._nexors[i].endPl1.MovePlanebyAxis(0.2,nexorCenter),
                         this._nexors[i].foPl[0].MovePlanebyAxis(0.1,nexorCenter,2,false),
                        bisectors[2],
                        this._nexors[i].endPl1,
                        bisectors[3],
                         this._nexors[i].fcPl[2].MovePlanebyAxis(0.1,nexorCenter,2,false)
                    };

                    Polyline endCuts1A = PolylineUtil.PolylineFromPlanes(lowerPlane, sidePlanes, true);
                    Polyline endCuts1B = PolylineUtil.PolylineFromPlanes(higherPlane, sidePlanes, true);
                    sidePlanes[0] = sidePlanes[0].MovePlanebyAxis(-0.2 * 2, nexorCenter, 2, false);
                    Polyline endCuts1A_ = PolylineUtil.PolylineFromPlanes(lowerPlane, sidePlanes, true);
                    Polyline endCuts1B_ = PolylineUtil.PolylineFromPlanes(higherPlane, sidePlanes, true);


                    //endCuts1A_.Bake();
                    //endCuts1B_.Bake();




                    this._nexors[i].CNCendBladeSawCuts0.Add(endCuts0A_);
                    this._nexors[i].CNCendBladeSawCuts0.Add(endCuts0B_);
                    this._nexors[i].CNCendBladeSawCuts1.Add(endCuts1A_);
                    this._nexors[i].CNCendBladeSawCuts1.Add(endCuts1B_);


                    this._nexors[i].CNCendBladeSawCuts0Negative.Add(endCuts0A);
                    this._nexors[i].CNCendBladeSawCuts0Negative.Add(endCuts0B);
                    this._nexors[i].CNCendBladeSawCuts1Negative.Add(endCuts1A);
                    this._nexors[i].CNCendBladeSawCuts1Negative.Add(endCuts1B);


                    dt.AddRange(this._nexors[i].CNCendBladeSawCuts0, path);
                    dt.AddRange(this._nexors[i].CNCendBladeSawCuts1, path);

                   



           


                    //if (this._nexors[i].isNexor == -1)
                    //{
                    //    endCuts0A.Bake();
                    //    endCuts0B.Bake();
                    //    endCuts1A.Bake();
                    //    endCuts1B.Bake();
                    //}
                }
            }


            //Collect neighbour cuts
            for (int i = 0; i < this._nexors.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1, this.id_[i].Item2);

                int[] adjE0S = this._nexors[i].adjE0S;
                int[] adjE1S = this._nexors[i].adjE1S;

                if (this._nexors[i].isNexor == 1)
                {


                    if (adjE0S[0] != -1) dt.AddRange(this[adjE0S[0], adjE0S[1]].CNCendBladeSawCuts0Negative, path);
                    if (adjE1S[0] != -1) dt.AddRange(this[adjE1S[0], adjE1S[1]].CNCendBladeSawCuts1Negative, path);

                    //Bottom cut
                    Point3d c = this._nexors[i].line.Center();
                    List<Plane> sidePlanes = new List<Plane>() { this._nexors[i].endPl0.MovePlanebyAxis(0.2,c,2,false), this._nexors[i].fcPl[2].MovePlanebyAxis(0.1, c, 2, false), this._nexors[i].endPl1.MovePlanebyAxis(0.2, c, 2, false),  this._nexors[i].foPl[0].MovePlanebyAxis(0.1, c, 2, false) };
                    Polyline bottomCut0 = PolylineUtil.PolylineFromPlanes(this._nexors[i].ePl90Offset, sidePlanes);
                    Polyline bottomCut1 = PolylineUtil.PolylineFromPlanes(this._nexors[i].ePl90Offset.MovePlanebyAxis(0.1, c, 2, false), sidePlanes);
                    //dt.Add(bottomCut0,path);
                    //dt.Add(bottomCut1, path);

                    dt.AddRange(this._nexors[i].CNCtopProfileCuts, path);
                    //bottomCut0.Bake();
                    //bottomCut1.Bake();
                }

                if (this._nexors[i].isNexor == -1)
                {

                    dt.AddRange(this._nexors[i].CNCendBladeSawCuts0Negative, new Grasshopper.Kernel.Data.GH_Path(adjE0S[0], adjE0S[1]));
                    dt.AddRange(this._nexors[i].CNCendBladeSawCuts1Negative, new Grasshopper.Kernel.Data.GH_Path(adjE1S[0], adjE1S[1]));

                    //Bottom cut
                    Point3d c = this._nexors[i].line.Center();
                    List<Plane> sidePlanes = new List<Plane>() { this._nexors[i].endPl0.MovePlanebyAxis(0.2, c, 2, false), this._nexors[i].fcPl[2].MovePlanebyAxis(0.1, c, 2, false), this._nexors[i].endPl1.MovePlanebyAxis(0.2, c, 2, false), this._nexors[i].foPl[0].MovePlanebyAxis(0.1, c, 2, false) };
                    Polyline bottomCut0 = PolylineUtil.PolylineFromPlanes(this._nexors[i].ePl90Offset, sidePlanes);
                    Polyline bottomCut1 = PolylineUtil.PolylineFromPlanes(this._nexors[i].ePl90Offset.MovePlanebyAxis(0.1, c, 2, false), sidePlanes);
                   //dt.Add(bottomCut0, path);
                  //dt.Add(bottomCut1, path);


                    dt.AddRange(this._nexors[i].CNCtopProfileCuts, path);

                    //instead of path ?????????????????????

                    //Rhino.RhinoApp.WriteLine(String.Format("adj {0} - {1}, {2}-{3} , {4}-{5}",
                    //    adjE0S[0],
                    //    adjE0S[1],
                    //    adjE1S[0],
                    //    adjE1S[1],
                    //    this._nexors[i].idNested [0],
                    //    this._nexors[i].idNested[1]
                    //    ));
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddBrep(this._nexors[i].pipe.breploft);

                }






            }



            for (int i = 0; i < this._nexors.Count; i++)
            {
                if (this._nexors[i].isNexor != 0)
                {
                    var path = new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1, this.id_[i].Item2);


                    Point3d c = this._nexors[i].pipe.line.Center();
      

                    List<Plane> sidePlanes = new List<Plane>()
                    {
                        this._nexors[i].fcPl[0].MovePlanebyAxis(0.01,c,2,false),
                        this._nexors[i].endPl0.MovePlanebyAxis(0.01,c,2,false),
                         this._nexors[i].foPl[2].MovePlanebyAxis(0.01,c,2,false),
                         this._nexors[i].endPl1.MovePlanebyAxis(0.01,c,2,false),
                    };

                    Polyline angledCut0 = PolylineUtil.PolylineFromPlanes(this._nexors[i].fcPl_foPl, sidePlanes);
                    Polyline angledCut1 = PolylineUtil.PolylineFromPlanes(this._nexors[i].fcPl_foPl.MovePlanebyAxis(0.1, c, 2, false), sidePlanes);


                    //dt.Add(angledCut0, path);
                    //dt.Add(angledCut1, path);
                    //bottomCut0.Bake();
                    //bottomCut1.Bake();


                }

            }


                    return dt;
        }


        //Output Mehthods

        public DataTree<Polyline> GetCNC_Cuts()
        {
            DataTree<Polyline> dt = new DataTree<Polyline>();

            for (int i = 0; i < this._nexors.Count; i++)
            {
                if (this._nexors[i].isNexor != 0)
                {
                           dt.MergeTree(this._nexors[i].CNC_Cuts);
                }
            }
            return dt;
        }

        public DataTree<Line> GetNexorLines(int naked = 1)
        {
            DataTree<Line> dt = new DataTree<Line>();

            for (int i = 0; i < this._nexors.Count; i++)
            {
                if (this._nexors[i].isNexor == naked)
                {
                    Line line = this._nexors[i].line;
                    dt.Add(line, new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1));
                }
            }
            return dt;
        }



        public DataTree<Mesh> GetNexorPipes()
        {
            DataTree<Mesh> dt = new DataTree<Mesh>();

            for (int i = 0; i < this._nexors.Count; i++)
                if (this._nexors[i].isNexor != 0)
                {
                    if (this._nexors[i].pipe != null)
                        dt.Add(this._nexors[i].pipe.meshloft, new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1, this.id_[i].Item2));

                }

            return dt;
        }

        public DataTree<Brep> GetNexorPipesBreps()
        {
            DataTree<Brep> dt = new DataTree<Brep>();

            for (int i = 0; i < this._nexors.Count; i++)
                if (this._nexors[i].isNexor != 0)
                {
                    if (this._nexors[i].pipe != null)
                        dt.Add(this._nexors[i].pipe.breploft, new Grasshopper.Kernel.Data.GH_Path(this.id_[i].Item1, this.id_[i].Item2));

                }

            return dt;
        }


        public DataTree<Line> GetNexorEccentricities()
        {

            DataTree<Line> dt = new DataTree<Line>();

            int counter = 0;
            foreach (var n in this._nexors)
            {
                if (n.isNexor!=0)
                    dt.AddRange(n.ecc, new Grasshopper.Kernel.Data.GH_Path(this.id_[counter].Item1, this.id_[counter].Item2));
                counter++;
            }
            return dt;
        }

        public Tuple<List<Line>, List<Line>, List<double>, List<double>> GetNexorPairs(List<int[]> _FEFlatten)
        {

            List<Line> l0 = new List<Line>();
            List<Line> l1 = new List<Line>();
            List<double> t0 = new List<double>();
            List<double> t1 = new List<double>();



            for (int i = 0; i < this._nexors.Count; i++)
            {



                if (this._nexors[i].isNexor!=0)
                {

                    for (int k = 0; k < this._nexors[i].ecc.Count; k++)
                    {

                        Line la = this._nexors[i].line;
                        int neiID = this._nexors[i].eccID[k];
                        neiID = this._id[new Tuple<int, int>(_FEFlatten[neiID][0], _FEFlatten[neiID][1])];
                        Line lb = this._nexors[neiID].line;
                        Line ecc = this._nexors[i].ecc[k];

                        l0.Add(la);
                        l1.Add(lb);

                        t0.Add(la.ClosestParameter(ecc.From));
                        t1.Add(lb.ClosestParameter(ecc.To));



                        //t0.Add(nexors[i][j].eccT0[k]);
                        //t1.Add(nexors[i][j].eccT1[k]);
                    }

                }


            }

            return new Tuple<List<Line>, List<Line>, List<double>, List<double>>(l0, l1, t0, t1);
        }

        public DataTree<Plane> GetNexorPlanes()
        {
            DataTree<Plane> dt = new DataTree<Plane>();



            foreach (var n in this._nexors)
                if (n.isNexor!=0)
                {

                    var path = new Grasshopper.Kernel.Data.GH_Path(n.idNested[0], n.idNested[1]);



                    dt.AddRange(n.foPl, path);
                    dt.Add(n.ePl, path);
                    dt.Add(n.fcPl_foPl, path);
                    dt.AddRange(n.fcPl, path);

                    dt.Add(n.ePl90Offset, path);
                  

               

                    dt.Add(n.endPl0, path);
                    dt.Add(n.endPl1, path);
                }
            return dt;
        }
    }
}
