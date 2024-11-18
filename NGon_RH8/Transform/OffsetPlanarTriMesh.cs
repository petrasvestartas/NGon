using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;

namespace NGon_RH8.Modifications
{
    public class OffsetTriPlanarMesh : GH_Component_NGon
    {
        public OffsetTriPlanarMesh()
          : base("OffsetTriangleMesh", "OffsetTriangleMesh",
              "OffsetTriangleMesh mesh",
              "Transform")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Dist", "D", "Offset dist", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Chamfer", "C", "Chamfer dist", GH_ParamAccess.item, 0.001);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polylines", "P0", "Outlines", GH_ParamAccess.list);
            pManager.AddCurveParameter("Polylines", "P1", "Outlines", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "S", "Edge planes", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "P", "P0 planes", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double d = 0.01;
            DA.GetData(1, ref d);

            double c = 0.001;
            DA.GetData(2, ref c);

            Tuple<Polyline[], Polyline[], Plane[], Plane[]> offset = NGonCore.MeshUtilSimple.OffsetTriangleMesh(m, d, c);

            List < Mesh > loftedMesh = new List<Mesh>();

            for(int i = 0; i < m.Faces.Count; i++) {

                List<Polyline> polylinesD = new List<Polyline>();
                polylinesD.AddRange(offset.Item1);
                polylinesD.AddRange(offset.Item2);
   
                this.PreparePreview(new Mesh(), DA.Iteration+i, polylinesD,false);
              
            }

            

            //DA.SetDataList(0, loftedMesh);
            DA.SetDataList(0, offset.Item1);
            DA.SetDataList(1, offset.Item2);
            DA.SetDataList(2, offset.Item3);
            DA.SetDataList(3, offset.Item4);




        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.OffsetWithChamfer;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("b30e676d-d93d-466a-85b5-89958c6e5591"); }
        }
    }
}