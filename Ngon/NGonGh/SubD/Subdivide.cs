using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SubD.SubD {
    public class Subdivide : GH_Component {

        public Rhino.Geometry.SubD.InteriorCreaseOption[] Crease = new[]
        {
            Rhino.Geometry.SubD.InteriorCreaseOption.Unset,
            Rhino.Geometry.SubD.InteriorCreaseOption.AtMeshCrease,
            Rhino.Geometry.SubD.InteriorCreaseOption.AtMeshEdge,
            Rhino.Geometry.SubD.InteriorCreaseOption.None
        };

        public Rhino.Geometry.SubD.SubDType[] type = new[]
        {
            Rhino.Geometry.SubD.SubDType.Unset,
            Rhino.Geometry.SubD.SubDType.Custom,
            Rhino.Geometry.SubD.SubDType.CustomQuad,
            Rhino.Geometry.SubD.SubDType.CustomTri,
            Rhino.Geometry.SubD.SubDType.TriLoopWarren,
            Rhino.Geometry.SubD.SubDType.QuadCatmullClark
        };

        public Rhino.Geometry.DistancingMode[] DistanceModes = new[]
        {
            Rhino.Geometry.DistancingMode.Linear,
            Rhino.Geometry.DistancingMode.LinearFromEnd,
            Rhino.Geometry.DistancingMode.Ratio,
            Rhino.Geometry.DistancingMode.RatioFromEnd,
            Rhino.Geometry.DistancingMode.Undefined
        };

        public Subdivide()
          : base("Subdivide", "Subdivide",
              "Description",
              "SubD", "Sub") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("N", "N", "N", GH_ParamAccess.item,1);
            pManager.AddIntegerParameter("N", "N", "N", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("N", "N", "N", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("N", "N", "N", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Distance", "D", "Chamfer Distance", GH_ParamAccess.item, 1);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            //Input
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            int n = 0;
            DA.GetData(1, ref n);
            n %= 4;

            int divisions = 0;
            DA.GetData(2, ref divisions);

            int t = 0;
            DA.GetData(3, ref t);
            n %= 6;

            int d = 0;
            DA.GetData(4, ref d);
            n %= 5;

            double dist = 1;
            DA.GetData(5, ref dist);

            //Solution


            Rhino.Geometry.SubD sub = Rhino.Geometry.SubD.CreateFromMesh(mesh, Crease[n]);
            //for (int i = 0; i < sub.Vertices.Count; i++)
            //{
            //    sub.ChamferVertex(i, DistanceModes[d],dist);
            //}
            //


          

            //Output
            DA.SetData(0,sub.ToLimitSurfaceMesh(divisions,type[t]));

        }


        protected override System.Drawing.Bitmap Icon {
            get {

                return null;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("aee5165f-a4d1-42bc-b582-45fc33d2e806"); }
        }
    }
}