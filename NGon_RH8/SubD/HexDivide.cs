using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;

namespace NGon_RH8.SubD {
    public class HexDivide : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public HexDivide()
          : base("HexDivide", "HexDivide",
              "Divide nurbs surface into hexagons",
               "Subdivide") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("U", "U", "Divide in first direction", GH_ParamAccess.item,10);
            pManager.AddIntegerParameter("V", "V", "Divide in second direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("Parameter", "T", "Scale hexagon", GH_ParamAccess.item, 0.25);
            pManager.AddIntegerParameter("Boundary", "B", "Boundary Ngons", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Swap", "S", "Swap direction of the surface from UV to VU, RhinoCommon Transpose", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Rebuild", "R", "Rebuild Surface", GH_ParamAccess.item, -1);
            pManager.AddBooleanParameter("Quadrangulate", "Q", "Split hexagons into two quads", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("DistA", "A", "Vertical length", GH_ParamAccess.list);
            pManager.AddNumberParameter("DistB", "B", "Horizontal length", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                double U = 10;
            int V = 10;
            double a = 0;
            DA.GetData(1, ref U);
           bool flag2 =  DA.GetData(2, ref V);
            DA.GetData(3, ref a);

            Brep brep = new Brep();



                int T = 0;
                int r = -1;
                bool q = true;

                bool swap = true;


                DA.GetData(4, ref T);
                DA.GetData(5, ref swap);
                DA.GetData(6, ref r);
                DA.GetData(7, ref q);


                bool f = false;

                if (T == 0 || T == 1)
                    f = true;


                if (T != 0 && T != 1) {
                    T = 0;
                }

                if (DA.GetData(0, ref brep)) {

                if ( brep.Faces.Count == 1 && !brep.Faces[0].IsClosed(0) && !brep.Faces[0].IsClosed(1) ) {

                    Surface S = brep.Faces[0].ToNurbsSurface();



                    Surface tempS = S;

                    if (swap) {
                        S = tempS.Transpose(true);

                    } else {
                        S = tempS.Reverse(0, true);
                    }

                    if (r > 0)
                        S = S.Rebuild(3, 3, r, r);


                    if (U <= 0)
                        U = 1;

                    if (V <= 0)
                        V = 1;

                    S.SetDomain(0, new Interval(0, 1));
                    S.SetDomain(1, new Interval(0, 1));

                    List<double> distanceA = new List<double>();
                    List<double> distanceB = new List<double>();

                       
                        Mesh mesh = new Mesh();
                        if (a == 1)
                        {
                            //Rhino.RhinoApp.WriteLine("Hi");
                            Surface ss = S.ToNurbsSurface();
                            //ss.Reverse(0, true);

                            if (V % 2 == 1)
                            {
                                ss.Reverse(1, true);

                                mesh = NGonCore.MeshCreate.DiamondMesh(ss, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), 0, true);
                                mesh.Ngons.Clear();
                                mesh.Flip(true, true, true);
                            }
                            else
                            {
                                mesh = MeshCreate.HexMesh(S, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), a, T, ref distanceA, ref distanceB, f, q);
                                //ss.Reverse(1, true);
                                ////ss=ss.Transpose();
                                //mesh = NGonCore.MeshCreate.DiamondMesh(ss, (int)MathUtil.Constrain(U-1-1, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), 0, true);
                                //mesh.Ngons.Clear();
                                //mesh.Flip(true, true, true);
                            }
                        }else if (a==0)
                        {
                            mesh = NGonCore.MeshCreate.DiamondMesh(S, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), 0, true);
                        }
                        else
                        {
                            mesh = MeshCreate.HexMesh(S, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), a, T, ref distanceA, ref distanceB, f, q);
                        }

                    //base.Message = (String.Format("Min {0} {1} \n Max {2} {3} \n Av {4} {5} \n Total {6}",
                    //    Math.Round(distanceA.Min(), 2),
                    //    Math.Round(distanceB.Min(), 2),
                    //    Math.Round(distanceA.Max(), 2),
                    //    Math.Round(distanceB.Max(), 2),
                    //    Math.Round(distanceA.Average(), 2),
                    //Math.Round(distanceB.Average(), 2),
                    //mesh.Ngons.Count

                    //    ));


                        mesh.Compact();
                        mesh.Vertices.CombineIdentical(true, true);
                        mesh.Vertices.CullUnused();

                        if (mesh.Ngons.Count > 0)
                            mesh.UnifyNormalsNGons();
                        else
                            mesh.UnifyNormals();


                        mesh.Weld(3.14159265358979);
                        mesh.FaceNormals.ComputeFaceNormals();
                        mesh.Normals.ComputeNormals();

                        if (mesh.SolidOrientation() == -1)
                            mesh.Flip(true, true, true);

                        DA.SetData(0, mesh);
                    this.PreparePreview(mesh, DA.Iteration);
                    DA.SetDataList(1, distanceA);
                    DA.SetDataList(2, distanceB);
                        this.Message = "Division UV ";
                        

                    } else  {

                        if (!explode)
                        {//&& !flag2// && a!=1 
                            int[][] feDivisions = null;
                            if ((int)U == 0)
                                U = 10;
                            Rhino.RhinoApp.WriteLine("Hi");
                            if (!flag2 && foo) {
                                int[] edgeDivisions = brep.EdgeDivisions(U, ref feDivisions);
                                this.Message = "Division U Dist";
                                //Rhino.RhinoApp.WriteLine("Hi");
                            } else {
                                this.Message = "Division UV Explode";
                            }
                            
                            List<Polyline> plinesClothed = new List<Polyline>();
                            List<Polyline> plinesNaked = new List<Polyline>();
                            //Rhino.RhinoApp.WriteLine(U.ToString());
                            MeshCreate.HexOrDiamondGrid(brep, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), ref plinesClothed, ref plinesNaked, a, true, feDivisions);
                            plinesClothed.AddRange(plinesNaked);
                            Mesh mesh = MeshCreate.MeshFromPolylines(plinesClothed, 0);
                            this.PreparePreview(mesh, DA.Iteration);

                            brep.ClosestPoint(mesh.Vertices[0], out Point3d cp, out ComponentIndex ci, out double s, out double t, 1000000000, out Vector3d normal);
                            if ((((Point3d)mesh.Vertices[0]) + normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1) > (((Point3d)mesh.Vertices[0]) - normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1))
                                mesh.Flip(true, true, true);
                            DA.SetData(0, mesh);

                         
                        } else {

                            Mesh mesh = new Mesh();
                            for (int i = 0; i < brep.Faces.Count; i++) {


                                //Reparametricize
                                Surface S = brep.Faces[i].ToNurbsSurface();
                                S.SetDomain(0, new Interval(0, 1));
                                S.SetDomain(1, new Interval(0, 1));
                                List<double> distanceA = new List<double>();
                                List<double> distanceB = new List<double>();
                                Mesh m = new Mesh();
                                
                                //if (a == 1)
                                //{
                                //    //Rhino.RhinoApp.WriteLine("Hrfi");
                                //    //Rhino.RhinoApp.WriteLine("Hi");
                                //    Surface ss = S.ToNurbsSurface();
                                //    ss.Reverse(1, true);
                                //    m = NGonCore.MeshCreate.DiamondMesh(ss, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), 0, true);
                                //    m.Ngons.Clear();
                                //    m.Flip(true, true, true);
                                //}
                               if (a == 0)
                                {
                                    m = NGonCore.MeshCreate.DiamondMesh(S, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), 0, true);
                                }
                                else
                                {
                                    m = MeshCreate.HexMesh(S, (int)MathUtil.Constrain(U, 2, 1000000000), (int)MathUtil.Constrain(V, 2, 1000000000), a, T, ref distanceA, ref distanceB, f, q);
                                }

                                mesh.Append(m);
                            }




                            mesh.Compact();
                            mesh.Vertices.CombineIdentical(true, true);
                            mesh.Vertices.CullUnused();

                            if (mesh.Ngons.Count > 0)
                                mesh.UnifyNormalsNGons();
                            else
                                mesh.UnifyNormals();


                            mesh.Weld(3.14159265358979);
                            mesh.FaceNormals.ComputeFaceNormals();
                            mesh.Normals.ComputeNormals();

                            if (mesh.SolidOrientation() == -1)
                                mesh.Flip(true, true, true);


                            brep.ClosestPoint(mesh.Vertices[0], out Point3d cp, out ComponentIndex ci, out double s, out double t, 1000000000, out Vector3d normal);
                            if( (((Point3d)mesh.Vertices[0])+normal*1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1) > (((Point3d)mesh.Vertices[0]) - normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1))
                                mesh.Flip(true, true, true);

                            this.PreparePreview(mesh, DA.Iteration);
                            DA.SetData(0, mesh);
                            this.Message = "Division UV Explode";
                        }

                    }


            }

            }catch (Exception e) {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
        }




        bool foo = true;

        bool explode = false;

        public override bool Write(GH_IWriter writer) {
            writer.SetBoolean("Divisions", this.foo);
            writer.SetBoolean("Explode", this.explode);
            return base.Write(writer);

        }

        public override bool Read(GH_IReader reader) {
            this.foo = false;
            this.explode = true;
            reader.TryGetBoolean("Divisions", ref this.foo);
            reader.TryGetBoolean("Explode", ref this.explode);
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalComponentMenuItems(menu);

            var fooToggle = Menu_AppendItem(menu, "Divisions", FooHandler, true, this.foo);
            fooToggle.ToolTipText = "Divisions";


            var explodeToggle = Menu_AppendItem(menu, "Explode", ExplodeHandler, true, this.explode);
            explodeToggle.ToolTipText = "Explode";
        }

        protected void FooHandler(object sender, EventArgs e) {
            this.foo = !this.foo;

            this.ExpireSolution(true);
        }

        protected void ExplodeHandler(object sender, EventArgs e) {
            this.explode = !this.explode;

            this.ExpireSolution(true);
        }



        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.hex;
            }
        }

        public override Guid ComponentGuid {
            get { return new Guid("f8b558c9-c00b-4bf1-b2b4-8110a5ee8f02"); }
        }
    }
}