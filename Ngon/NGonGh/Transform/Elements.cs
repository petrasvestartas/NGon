using System;
using System.Collections.Generic;
using NGonsCore;
using Rhino.Geometry;

namespace SubD.Modifications {


    public class Panel {

        public Polyline[] contourNoJoints = new Polyline[2];
        public Polyline[] contour = new Polyline[2];

        public Plane plane = Plane.Unset;
        public Plane planeOffset0 = Plane.Unset;
        public Plane planeOffset1 = Plane.Unset;

        public Plane planeRot = Plane.Unset;
        public Plane planeRotOffset0 = Plane.Unset;
        public Plane planeRotOffset1 = Plane.Unset;
        public Plane planeEdge = Plane.Unset;

        public Plane planeF0 = Plane.Unset;
        public Plane planeF1 = Plane.Unset;



        public List<Plane> edgePlanes = new List<Plane>();
        public string id;
        public List<Curve> text = new List<Curve>();


        public List<Polyline> cuts = new List<Polyline>();



        public Panel(Plane plane) {
            this.plane = plane;
            this.contourNoJoints = new Polyline[] { new Polyline(), new Polyline() };
            this.contour = new Polyline[] { new Polyline(), new Polyline() };
        }

        public Panel(Polyline p0, Polyline p1) {
            this.contourNoJoints = new Polyline[] { new Polyline(p0), new Polyline(p1) };
            this.contour = new Polyline[] { new Polyline(p0), new Polyline(p1) };
            this.planeOffset0 = p0.plane();
            this.planeOffset1 = p1.plane();
            this.plane = new Plane((planeOffset0.Origin + planeOffset1.Origin) * 0.5, planeOffset0.XAxis, planeOffset0.YAxis);
        }

        public Polyline MidContour() {
            return PolylineUtil.tweenPolylines(contour[0],contour[1]);
        }

        public void ChangeJoint(double extend = 10, int type = 0) {

            if (contour[0].Count > 23) {

                for(int i = 0; i < contour.Length; i++) {

                    Polyline newContour = new Polyline();

                    for(int j = 5; j < 23; j++) {
                        newContour.Add(contour[i][j]);
                    }

                    

                    //extend top part
                    newContour[0] +=(newContour[0] - newContour[1]).UnitVector() * extend;
                    newContour[newContour.Count-1] +=  (newContour[newContour.Count - 1] - newContour[newContour.Count - 2]).UnitVector() * extend;
                    newContour.Close();

                    contour[i] = newContour;



                }


            }


        }

        public void CreateCut(int segmentID, Plane jointPlane0, Plane jointPlane1, double length,ref Panel jointPanel) {//, ref Panel neiPanel,) {

            double e = 0;


            Polyline cut = new Polyline();

            Line segment0 = contourNoJoints[0].SegmentAt(segmentID);
            Line segment1 = contourNoJoints[1].SegmentAt(segmentID);

            //Intersect plate edge line and joint plane offsets
            Point3d edgePoint00 = PlaneUtil.LinePlane(segment0, jointPlane0);
            Point3d edgePoint01 = PlaneUtil.LinePlane(segment0, jointPlane1);
            Point3d edgePoint10 = PlaneUtil.LinePlane(segment1, jointPlane0);
            Point3d edgePoint11 = PlaneUtil.LinePlane(segment1, jointPlane1);

            //Get direction of a cut
            Vector3d v00 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane0);
            Vector3d v01 = PlaneUtil.PlanePlaneVec(planeOffset0, jointPlane1);
            Vector3d v10 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane0);
            Vector3d v11 = PlaneUtil.PlanePlaneVec(planeOffset1, jointPlane1);



       
  

            bool moveFlag = (segment0.PointAt(0.5) + v00).DistanceTo(planeOffset0.Origin) < (segment0.PointAt(0.5) - v00).DistanceTo(planeOffset0.Origin);

            //Moved Points
            Point3d innerPoint00 = (moveFlag) ? edgePoint00 + (v00 * length) : edgePoint00 - (v00 * length);
            Point3d innerPoint01 = (moveFlag) ? edgePoint01 + (v01 * length) : edgePoint01 - (v01 * length);
            Point3d innerPoint10 = (moveFlag) ? edgePoint10 + (v10 * length) : edgePoint10 - (v10 * length);
            Point3d innerPoint11 = (moveFlag) ? edgePoint11 + (v11 * length) : edgePoint11 - (v11 * length);
            Point3d innerPointCenter = (innerPoint00 + innerPoint01 + innerPoint10 + innerPoint11) * 0.25;
            Plane perpPlane = new Plane(innerPointCenter, jointPlane0.Normal, Vector3d.CrossProduct(planeOffset0.Normal,v00));



            innerPoint00 = perpPlane.RayPlane(edgePoint00, v00);
            innerPoint01 = perpPlane.RayPlane(edgePoint01, v01);
            innerPoint10 = perpPlane.RayPlane(edgePoint10, v10);
            innerPoint11 = perpPlane.RayPlane(edgePoint11, v11);

            //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(perpPlane, 50, 50));
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddRectangle(new Rectangle3d(jointPlane0, 50, 50));


            //Middle points and projection to plane
            Point3d innerPointMid00 = (moveFlag) ? edgePoint00 + (v00 * length * 0.5) : edgePoint00 - (v00 * length*0.5);
            Point3d innerPointMid01 = (moveFlag) ? edgePoint01 + (v01 * length * 0.5) : edgePoint01 - (v01 * length * 0.5);
            Point3d innerPointMid10 = (moveFlag) ? edgePoint10 + (v10 * length * 0.5) : edgePoint10 - (v10 * length * 0.5);
            Point3d innerPointMid11 = (moveFlag) ? edgePoint11 + (v11 * length * 0.5) : edgePoint11 - (v11 * length * 0.5);
            Point3d innerPointMid = (innerPointMid00 + innerPointMid01 + innerPointMid10 + innerPointMid11) * 0.25;
            Plane perpPlaneMid = new Plane(innerPointMid, jointPlane0.Normal, perpPlane.YAxis);

            innerPointMid00 = perpPlaneMid.RayPlane(edgePoint00, v00);
            innerPointMid01 = perpPlaneMid.RayPlane(edgePoint01, v01);
            innerPointMid10 = perpPlaneMid.RayPlane(edgePoint10, v10);
            innerPointMid11 = perpPlaneMid.RayPlane(edgePoint11, v11);


            //It is not closest point because the connection is not perpendicular to two adjacent plates
            //It might be close to perpendicular but not possible

            Polyline cut0 = new Polyline(new Point3d[] { edgePoint00, innerPointMid00+ v00*e, innerPointMid01 + v01 * e, edgePoint01 });//perpPlane.ClosestPoint
            Polyline cut1 = new Polyline(new Point3d[] { edgePoint10, innerPointMid10 + v10 * e, innerPointMid11 + v11 * e, edgePoint11 });
            this.cuts.Add(cut0);
            this.cuts.Add(cut1);

            Polyline cutNei0 = new Polyline(new Point3d[] { innerPoint00, innerPointMid00 + v00 *-e, innerPointMid10 + v01*-e , innerPoint10 });//perpPlane.ClosestPoint
            Polyline cutNei1 = new Polyline(new Point3d[] { innerPoint01, innerPointMid01 + v10 * -e, innerPointMid11 + v11 * -e, innerPoint11 });
            jointPanel.cuts.Add(cutNei0);
            jointPanel.cuts.Add(cutNei1);

            contour[0].InsertPolyline(cut0);
            contour[1].InsertPolyline(cut1);

            jointPanel.contour[0].InsertPolyline(cutNei0);
            jointPanel.contour[1].InsertPolyline(cutNei1);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut0);
            //Rhino.RhinoDoc.ActiveDoc.Objects.AddPolyline(cut1);
        }

   






    }

    public class PanelGroup {

        List<Panel> panels = new List<Panel>();

        public PanelGroup(List<Polyline> contourNoJoints) {

            for (int i = 0; i < contourNoJoints.Count; i+=2) {
                panels.Add(new Panel(contourNoJoints[i], contourNoJoints[i+1]));
            }

        }

        public PanelGroup(Plane[] planes) {

            for (int i = 0; i < planes.Length; i++) {
                panels.Add(new Panel(planes[i]));
            }

        }
    }




    public static class ElementsUtil{














        public static JointReciprocal[] OrientJointsByBaseTile(this JointReciprocal joint, Plane[] planes) {

            JointReciprocal[] joints = new JointReciprocal[planes.Length];

            for(int i = 0; i < joints.Length; i++) {
                JointReciprocal jointCopy =  joint.Duplicate();
                jointCopy.Orient(jointCopy.pl, planes[i]);
                joints[i] = jointCopy;
            }

            return joints;
        }

        public static JointReciprocal[][] OrientJointsByBaseTile(this JointReciprocal joint, Plane[] p0, Plane[] p1) {

            JointReciprocal[][] joints = new JointReciprocal[p0.Length][];

            for (int i = 0; i < joints.Length; i++) {
                JointReciprocal jointCopy0 = joint.Duplicate();
                jointCopy0.Orient(jointCopy0.pl, p0[i]);
                JointReciprocal jointCopy1 = joint.Duplicate();
                jointCopy1.Orient(jointCopy1.pl, p1[i]);
                joints[i] = new JointReciprocal[] { jointCopy0, jointCopy1 };
            }

            return joints;
        }

        public static Polyline[][][] GetJointsGeo (this ElementReciprocal[] elements) {

            Polyline[][][] outlines = new Polyline[elements.Length][][];

            for (int i = 0; i < elements.Length; i++) {
                Polyline[][] o0 = elements[i]._joints.GetJointsOutlines();
                Polyline[][] o1 = elements[i]._jointsNei.GetJointsOutlines();
                outlines[i] = new Polyline[o0.Length + o1.Length][];
                Array.Copy(o0, outlines[i], o0.Length);
                Array.Copy(o1, 0, outlines[i], o0.Length, o1.Length);
            }

            return outlines;

        }

        public static Polyline[][] GetJointsOutlines(this JointReciprocal[] joints) {

            Polyline[][] outlines = new Polyline[joints.Length][];

            for (int i = 0; i < joints.Length; i++) {
                outlines[i] = joints[i]._pln;
            }

            return outlines;
        }

        public static Polyline[][] GetJointsOutlines(this List<JointReciprocal> joints) {

            Polyline[][] outlines = new Polyline[joints.Count][];

            for (int i = 0; i < joints.Count; i++) {
       
                outlines[i] = joints[i]._pln;
            }

            return outlines;
        }



    }


    public class ElementReciprocal {

        public JointReciprocal[] _joints = new JointReciprocal[0];
        public JointReciprocal[] _jointsFemale = new JointReciprocal[0];
        public List<JointReciprocal> _jointsNei = new List<JointReciprocal>();

        public Polyline[] _polylines;

        public ElementReciprocal(Polyline[] polylines) {
            this._polylines = new Polyline[polylines.Length];

            for (int i = 0; i < polylines.Length;i++) {
                this._polylines[i] = new Polyline(polylines[i]);
            }

     
        }

        public ElementReciprocal(Polyline[] polylines, JointReciprocal[] joints, JointReciprocal[] jointsFemale, List<JointReciprocal> jointsNei) : this(polylines) {

            _joints = new JointReciprocal[joints.Length];

            for (int i = 0; i < joints.Length; i++)
                _joints[i] = (joints[i].Duplicate());

            _jointsFemale = new JointReciprocal[jointsFemale.Length];

            for (int i = 0; i < jointsFemale.Length; i++)
                _jointsFemale[i] = (jointsFemale[i].Duplicate());

            _jointsNei = new List<JointReciprocal>(jointsNei.Count);

            for (int i = 0; i < _jointsNei.Count; i++)
                jointsNei[i] = (jointsNei[i].Duplicate());

        }
        
        public void AddReverseJoint(JointReciprocal joint) {
            var j = joint.Duplicate();
            var p = new Polyline[j._pln.Length];
            for(int i = 0; i < j._pln.Length; i += 2) {
                p[i]=(j._pln[i + 1]);
                p[i+1] = (j._pln[i ]);
            }
            j._pln = p;
            this._jointsNei.Add(j);
        }




        public ElementReciprocal Duplicate() {
            return new ElementReciprocal(_polylines,_jointsFemale , _joints,_jointsNei);
        }

      


    }
    public class JointReciprocal {

        public Polyline[] _pln;
        public Plane pl = Plane.WorldXY;

        public JointReciprocal(Polyline[] pln) {

            this._pln = new Polyline[pln.Length];

            for(int i = 0; i < pln.Length;i++)
                this._pln[i]=(new Polyline(pln[i]));
        }

        public JointReciprocal(List<Curve> c) {
            this._pln = new Polyline[c.Count];

            for (int i = 0; i < _pln.Length; i++) { 
                Polyline p = new Polyline();
                if(c[i].TryGetPolyline(out p))
                    this._pln[i]=(new Polyline(p));

            }
        }

        public void Orient(Plane s, Plane t) {
            for (int i = 0; i < _pln.Length; i++) {
                _pln[i].Transform(Transform.PlaneToPlane(s, t));
            }
        }

        public JointReciprocal OrientCopy(Plane s, Plane t) {
            var r = new JointReciprocal(_pln);
            r.Orient(s, t);
            return r;
        }

        public JointReciprocal Duplicate() {
            return new JointReciprocal(_pln);
        }


    }
}
