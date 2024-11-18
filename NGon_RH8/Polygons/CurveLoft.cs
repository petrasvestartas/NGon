using Grasshopper.Kernel;
using NGon_RH8;
using NGonCore;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NGon_RH8.SubD
{
    public class CurveLoft : GH_Component_NGon
    {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public CurveLoft()
          : base("CurveLoft", "CurveLoft",
              "Loft curves into a quad mesh. " +
                "If V paremeter is not supplied, " +
                "default division in V direction is based on C0 input:" +
                " end points for a curve" +
                "/ control points for a polyline",
               "Polygon")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("CurveFirst", "C0", "Curves", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves", GH_ParamAccess.list);
            pManager.AddIntegerParameter("DivisionsU", "U", "Divisions in first direction", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("DivisionsV", "V", "Divisions in second direction", GH_ParamAccess.item, 0);
            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddIntegerParameter("D", "D", "D", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //If it is a polyline and if there the same ammount of points then take control points
            Curve CFirst = NGonCore.Clipper.DataAccessHelper.Fetch<Curve>(DA, "CurveFirst");


            List<Curve> C = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(DA, "Curves");
            int U = NGonCore.Clipper.DataAccessHelper.Fetch<int>(DA, "DivisionsU");
            int V = NGonCore.Clipper.DataAccessHelper.Fetch<int>(DA, "DivisionsV");
            if (CFirst != null)
            {
                C.Insert(0, CFirst);
            }

            int divisions = -1;
            var meshes = NGonCore.MeshCreate.MeshLoftMultiple(C, U, V, ref divisions);
            DA.SetData(1, divisions);

            Mesh mesh = new Mesh();
            foreach (var m in meshes)
                mesh.Append(m);
            mesh.Clean();

            //DA.SetData(0, mesh);
            DA.SetDataList(0, meshes);



            base.PreparePreview(mesh, DA.Iteration, mesh.GetPolylines().ToList(), false, mesh.Vertices.ToPoint3dArray().ToList());
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.curveloft;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("f8b111c9-c00b-4bf1-b2b4-8140a5ee1f08"); }
        }
    }
}