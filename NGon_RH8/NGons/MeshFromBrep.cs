using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.NGons {
    public class MeshFromBrep : GH_Component_NGon {
        public MeshFromBrep()
            : base("From Brep", "From Brep",
                "Create Mesh from polygonal brep",
                "NGons") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Brep", "B", "Polygonal Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weld", "W", "Weld", GH_ParamAccess.item, -1);

            pManager[1].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Polylines", "P", "Outlines of brep faces", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
           Brep brep = new Brep();
            bool flag = DA.GetData(0, ref brep);

            double weld = 0.001;
            DA.GetData(1, ref weld);


            Polyline[] outlines = new Polyline[brep.Faces.Count];

            try
            {
                Mesh mesh = brep.MeshFromPolygonalBrep(weld, ref outlines);

                DA.SetData(0, mesh);
                DA.SetDataList(1, outlines);
                this.PreparePreview(mesh, DA.Iteration);
            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }


        }



        public Mesh FromPolylines(IEnumerable<Polyline> nurbsCurves, double weld) {

            //Create Mesh and get face
            Mesh mesh = new Mesh();
            int v = 0;
            int f = 0;

            foreach (Polyline polyline in nurbsCurves) {

                switch (polyline.Count - 1) {

                    case (6):
                        polyline.RemoveAt(polyline.Count - 1);
                        mesh.Vertices.AddVertices(polyline);
                        mesh.Faces.AddFace(v + 2, v + 1, v + 0);
                        mesh.Faces.AddFace(v + 4, v + 3, v + 2);
                        mesh.Faces.AddFace(v + 5, v + 2, v + 0);
                        mesh.Faces.AddFace(v + 4, v + 2, v + 5);
                        mesh.Ngons.AddNgon(MeshNgon.Create(new[] { v, v + 1, v + 2, v + 3, v + 4, v + 5 },
                            new[] { f + 0, f + 1, f + 2, f + 3 }));
                        v += 6;
                        f += 4;
                        break;


                    case (4):
                        polyline.RemoveAt(polyline.Count - 1);
                        mesh.Vertices.AddVertices(polyline);
                        mesh.Faces.AddFace(2 + v, 1 + v, 0 + v);
                        mesh.Faces.AddFace(3 + v, 2 + v, 0 + v);
                        mesh.Ngons.AddNgon(MeshNgon.Create(new[] { v, 1 + v, 2 + v, 3 + v }, new[] { 0 + f, 1 + f }));
                        v += 4;
                        f += 2;
                        break;

                    case (3):
                        polyline.RemoveAt(polyline.Count - 1);
                        mesh.Vertices.AddVertices(polyline);
                        mesh.Faces.AddFace(2 + v, 1 + v, 0 + v);
                        mesh.Ngons.AddNgon(MeshNgon.Create(new[] { v, 1 + v, 2 + v }, new[] { 0 }));
                        v += 3;
                        f += 1;
                        break;

                    default:
                        Mesh temp = Mesh.CreateFromClosedPolyline(polyline);

                        int[] tempV = new int[temp.Vertices.Count];
                        for (int i = 0; i < temp.Vertices.Count; i++)
                            tempV[i] = v + i;

                        int[] tempF = new int[temp.Faces.Count];
                        for (int i = 0; i < temp.Faces.Count; i++)
                            tempF[i] = f + i;

                        mesh.Append(temp);
                        mesh.Ngons.AddNgon(MeshNgon.Create(tempV, tempF));

                        v += temp.Vertices.Count;
                        f += temp.Faces.Count;



                        break;
                }
            }

            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            mesh.Compact();

            if (weld > 0) {
                mesh.WeldUsingRTree(0.001);
                mesh.Vertices.CombineIdentical(true, true);
                mesh.Vertices.CullUnused();
                mesh.Weld(3.14159265358979);
                mesh.UnifyNormals();
                mesh.FaceNormals.ComputeFaceNormals();
                mesh.Normals.ComputeNormals();
                mesh.Compact();
            }


            return mesh;

        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.frombrep;



        public override Guid ComponentGuid => new Guid("{92044ffc-0168-4ee5-9af7-b278aa048d25}");
    }
}