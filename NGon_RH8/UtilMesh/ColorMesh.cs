using System;
using System.Collections.Generic;
using NGonCore;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace NGon_RH8.Utils
{
    public class ColorMesh : GH_Component
    {

        public ColorMesh()
          : base("ColorMesh", "ColorMesh",
              "ColorMesh", "NGon",
             "Utilities Mesh")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddColourParameter("Color","C","Change mesh colors by assigning vertex colors", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            var colors = new List<System.Drawing.Color>();
            bool flag = DA.GetDataList(1, colors);




            //Colors
            Mesh mColored = m.DuplicateMesh();
            mColored.VertexColors.Clear();
            mColored.Unweld(0, true);

            //base.Message = mColored.Faces.Count.ToString() + " " + mColored.VertexColors.Count.ToString();

            if (m.Faces.Count == colors.Count) {


                mColored.VertexColors.CreateMonotoneMesh(System.Drawing.Color.FromArgb(200, 200, 200));

                for (int i = 0; i < mColored.Faces.Count; i++) {
                    mColored.VertexColors.SetColor(mColored.Faces[i], colors[i]);
                }
              
            }else if (m.Ngons.Count > 0 && m.Ngons.Count == colors.Count) {

            

                Mesh explodedMesh = new Mesh();
                IEnumerable<MeshNgon> ngonFaces = m.GetNgonAndFacesEnumerable();

                var ngonsColors = new System.Drawing.Color[m.Faces.Count];

                mColored.VertexColors.CreateMonotoneMesh(System.Drawing.Color.FromArgb(200, 200, 200));
                //base.Message = m.Faces.Count + " " + mColored.Faces.Count;

                //for (int i = 0; i < m.Ngons.Count; i++) {
                //    uint[] f = m.Ngons[i].FaceIndexList();
                //    for (int j = 0; j < f.Length; j++) {
                //        mColored.VertexColors.SetColor((int)f[j], colors[i]);
                //    }
                //}

                int count = 0;
                foreach (MeshNgon ngon in ngonFaces) {
                    int[] meshFaceList = ngon.FaceIndexList().Select(j => unchecked((int)j)).ToArray();
                    Mesh NGonFaceMesh = m.DuplicateMesh().Faces.ExtractFaces(meshFaceList);
                    NGonFaceMesh.VertexColors.CreateMonotoneMesh(colors[count++]);
                    explodedMesh.Append(NGonFaceMesh);
                }

                mColored = explodedMesh;
                explodedMesh.RebuildNormals();

            } else if(colors.Count>0) {
                mColored.VertexColors.CreateMonotoneMesh(colors[0]);
            } else {
                //mColored.VertexColors.CreateMonotoneMesh(System.Drawing.Color.FromArgb(200, 200, 200));
            }

           
        
            DA.SetData(0,mColored);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ColorMesh;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("55f1321a-d5e1-4c3f-aedb-bd27ce11a513"); }
        }
    }
}