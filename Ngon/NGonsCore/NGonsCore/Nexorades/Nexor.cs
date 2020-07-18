using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
using Rhino.Geometry;

namespace NGonsCore.Nexorades
{
    public class Nexor
    {

        public int id = -1;
        public int[] idNested = new int[] {-1,-1 };//edge id and face id
        public Line line = Line.Unset;
        public double translation = 0;

        //Adjacencies

        public int[] adjE0 = new int[] { -1, -1 };//End0
        public int[] adjE1 = new int[] { -1, -1 };//End1      
        public int[] adjE0S = new int[] { -1, -1 };//End0Side        
        public int[] adjE1S = new int[] { -1, -1 };//End1Side

        //Additional properties
        public List<Line> ecc = new List<Line>();
        public List<int> eccID = new List<int>();
        public List<double> eccT0 = new List<double>();
        public List<double> eccT1 = new List<double>();
        public int isNexor = 0;//0 not nexorade, 1 nexorade, -1 boundary

        //Planes
        public Plane endPl0 = Plane.Unset;
        public Plane endPl1 = Plane.Unset;
        public Plane midPl0 = Plane.Unset;
        public Plane midPl1 = Plane.Unset;

        public Plane ePl = Plane.Unset;
        public Plane ePl90 = Plane.Unset;
        public Plane ePl90Offset = Plane.Unset;

        public List<Plane> fcPl = new List<Plane>();//current face plane
        public List<Plane> foPl = new List<Plane>();//opposite face plane

        public Plane fcPl_foPl = Plane.Unset;//average plane

        public Point3d oppositeP = Point3d.Unset;

        //Profiles
        public List<Polyline>profiles = new List<Polyline>();

        //For a pipe
        //public Line pipe;

        public Pipe pipe;
        public List<Polyline> CNCendBladeSawCuts0 = new List<Polyline>();
        public List<Polyline> CNCendBladeSawCuts1 = new List<Polyline>();

        public List<Polyline> CNCendBladeSawCuts0Negative = new List<Polyline>();
        public List<Polyline> CNCendBladeSawCuts1Negative = new List<Polyline>();

        public List<Polyline> CNCsideBladeSawCuts = new List<Polyline>();
        public List<Polyline> CNCtopProfileCuts = new List<Polyline>();
        public List<Polyline> CNCbottomProfileCuts = new List<Polyline>();
        public List<Polyline> CNCindexing = new List<Polyline>();

        public DataTree<Polyline> CNC_Cuts = new DataTree<Polyline>();


        public Nexor(int id, int i, int j, Line line )
        {
            this.id = id;
            this.idNested = new int[] { i, j };
            this.line = line;
        }

        public Nexor Duplicate()
        {
            Nexor n = new Nexor(this.id, this.idNested[0], this.idNested[1], this.line);
            try
            {


                //adj
                n.adjE0 = new int[] { adjE0[0], adjE0[1] };//End0
                n.adjE1 = new int[] { adjE1[0], adjE1[1] };//End1      
                n.adjE0S = new int[] { adjE0S[0], adjE0S[1] };//End0Side        
                n.adjE1S = new int[] { adjE1S[0], adjE1S[1] };//End1Side

                //Additional properties
                foreach (var o in ecc) n.ecc.Add(o);
                foreach (var o in eccID) n.eccID.Add(o);
                foreach (var o in eccT0) n.eccT0.Add(o);
                foreach (var o in eccT1) n.eccT1.Add(o);
                n.isNexor = isNexor;

                //Planes
                n.endPl0 = new Plane(endPl0);
                n.endPl1 = new Plane(endPl1);
                n.midPl0 = new Plane(midPl0);
                n.midPl1 = new Plane(midPl1);

                n.ePl = new Plane(ePl);
                n.ePl90 = new Plane(ePl90);
                n.ePl90Offset = new Plane(ePl90Offset);

                foreach (var o in fcPl) n.fcPl.Add(new Plane(o));
                foreach (var o in foPl) n.foPl.Add(new Plane(o));

                n.fcPl_foPl = new Plane(fcPl_foPl);

                n.oppositeP = new Point3d(oppositeP);

                //Profiles
                foreach (var o in profiles) n.profiles.Add(o.Duplicate());


                //For a pipe
                if (n.pipe != null) n.pipe = pipe.Duplicate();


                foreach (var o in CNCendBladeSawCuts0) n.CNCendBladeSawCuts0.Add(o.Duplicate());
                foreach (var o in CNCendBladeSawCuts1) n.CNCendBladeSawCuts1.Add(o.Duplicate());
                foreach (var o in CNCsideBladeSawCuts) n.CNCsideBladeSawCuts.Add(o.Duplicate());
                foreach (var o in CNCtopProfileCuts) n.CNCtopProfileCuts.Add(o.Duplicate());
                foreach (var o in CNCbottomProfileCuts) n.CNCbottomProfileCuts.Add(o.Duplicate());
                foreach (var o in CNCindexing) n.CNCindexing.Add(o.Duplicate());

                n.CNC_Cuts = new DataTree<Polyline>(this.CNC_Cuts);

            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }

            return n;

        }

    

        public void AddEcc(int ID, double t0, double t1, Line line)
        {
            this.eccID.Add(ID);
            this.eccT0.Add(t0);
            this.eccT1.Add(t1);
            this.ecc.Add(line);
        }

        public override string ToString()
        {
            return String.Format("Nexor: {0}, Neigbours {1}", this.id, eccID.Count);
        }

    }

}
