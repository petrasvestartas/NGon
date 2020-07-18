using System;
using System.Collections.Generic;
using NGonsCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using System.Threading.Tasks;

namespace NGonGh.Modifications
{
    public class ExtrudeMeshEdges : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the ExtrudeMeshEdges class.
        /// </summary>
        public ExtrudeMeshEdges()
          : base("Extrude Edges", "Extrude Edges",
              "Extrude NGon edges by normals",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to Extrude", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D", "Extrusion dist", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Cap", "C", "Cap holes", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Merge", "M", "Extrude all edges instead of individual ngons", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("BothSides", "B", "Extrude both sides", GH_ParamAccess.item, false);
            //pManager.AddIntegerParameter("Planar","P","Perform planarization after extrusion", GH_ParamAccess.item,0 );

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            //pManager[5].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to Extrude", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double d = 1;
            DA.GetData(1, ref d);

            bool cap = false;
            bool merge = false;
            bool bothsides = false;
           // int iterations = 0;
            DA.GetData(2, ref cap);
            DA.GetData(3, ref merge);
            DA.GetData(4, ref bothsides);

                Mesh[] mesh = m.ExtrudeMesh((float)(d), merge, cap, bothsides);
            if (d == 0) {
                PreparePreview(m, DA.Iteration);
                DA.SetData(0, m);
            } else {
                Mesh temp = new Mesh();
                foreach (var t in mesh)
                    temp.Append(t);
                PreparePreview(temp, DA.Iteration);
            DA.SetDataList(0, mesh);
            }


        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.EXTRUDEEDGES;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2d04e1f8-fb27-4e4a-8ff8-01b8f56ca1f4"); }
        }
    }
}