using System;
using System.Collections.Generic;
using NGonsCore;
using Rhino.Geometry;

namespace NGonsCore {


    public  struct JointsVDAInputs{

        public int meshEdge;
        public int divisions;
        public double length;
        public double height;
        public double thickness;
        public double custom;
        public Vector3d insertion;
        public double addExtend;
        public double cutExtend;

        public  JointsVDAInputs(int meshEdge = -1, int divisions = 1, double length = 60, double height = 100, double thickness = 3, double custom = -20, Vector3d insertion = default(Vector3d), double addExtend = 0, double cutExtend = 5) {
            this.meshEdge = meshEdge;
            this.divisions = divisions;
            this.length = length;
            this.height = height;
            this.thickness = thickness;
            this.custom = custom;
            this.insertion = insertion;
            this.addExtend = addExtend;
            this.cutExtend = cutExtend;
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
