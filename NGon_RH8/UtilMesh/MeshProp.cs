using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using NGonCore;
using NGonCore.Clipper;
using ObjParser;
using Rhino.Geometry;


namespace NGon_RH8.Utils {
    public class MeshProp : GH_Component {

        public MeshProp() 
          : base("MeshProp", "MeshProp",
              "MeshProp", "NGon",
             "Reciprocal") {
            }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset","O","Offset Planar Mesh",GH_ParamAccess.item,0);
            pManager.AddBooleanParameter("Flip", "F", "Flip",GH_ParamAccess.item,false);
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("Mesh Properties","Prop","Mesh Properties such as face, edge planes, indexing", GH_ParamAccess.item);
            pManager.AddPlaneParameter("FacePlanes", "FPl", "Ngon face planes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("FaceEdgePlanes", "FEPL", "Ngon face planes oriented to ngon edge", GH_ParamAccess.tree);
            //pManager.AddPlaneParameter("FaceEdgePlanes90", "FEPL90", "Ngon face planes oriented to ngon edge, rotated 90 deg about mesh edge", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("EdgePlanes", "EPL", "Edge planes is an average of adjacent FaceEdgePlanes", GH_ParamAccess.tree);
            //pManager.AddPlaneParameter("EdgePlanes90", "EPL90", "Edge planes is an average of adjacent FaceEdgePlanes,rotated 90 deg about mesh edge", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            double dist = DA.Fetch<double>("Offset");
            bool flip = DA.Fetch<bool>("Flip");

          
            try
            {


                if (dist != 0) {


                    m = m.FaceFaceOffset(dist);
                    //Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(m);
                }

                Mesh M = NGonCore.MeshCreate.MeshFromPolylines(m.GetFacePolylinesArray(), 0.01);


                //var bfs = NGonCore.Graphs.UndirectedGraphBfsRhino.MeshBFS(m, "0");

                //var plines = m.UnifyWinding(bfs.Item2[0].ToArray());
                //m = MeshCreate.MeshFromPolylines(plines, 0.01, -1);


                DA.SetData(4, M);




                MeshProps mp = M._Planes();




                DA.SetData(0, mp);
                DA.SetDataTree(1, GrasshopperUtil.IE(mp.fPl,-1));
                DA.SetDataTree(2, GrasshopperUtil.IE2(mp.fePl));
               // DA.SetDataTree(3, GrasshopperUtil.IE2(mp.fePl90));
                DA.SetDataTree(3, GrasshopperUtil.IE(mp.ePl));
                //DA.SetDataTree(5, GrasshopperUtil.IE(mp.ePl90));
                
            }
            catch(Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            
        }



        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.MeshProperties;
            }
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        public override Guid ComponentGuid {
            get { return new Guid("55f1321a-d5e1-4c3f-aedb-bd89ce58a897"); }
        }
    }
}

