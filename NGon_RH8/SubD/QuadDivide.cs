using System;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NGonCore;
using Rhino.Geometry;
using System.Linq;
using System.Drawing;

namespace NGon_RH8.SubD {
    public class QuadDivide : GH_Component_NGon {
        /// <summary>
        /// Initializes a new instance of the HexDivide class.
        /// </summary>
        public QuadDivide()
          : base("QuadDivide", "QuadDivide",
              "Divide NurbsSurface into quads",
               "Subdivide") {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("U", "U", "Divide in first direction", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V", "V", "Divide in second direction", GH_ParamAccess.item);
            //pManager.AddBrepParameter("Brep", "B", "Brep for measuring distance", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Min", "A", "Min distance for color mapping", GH_ParamAccess.item, 0);
            //pManager.AddNumberParameter("Max", "B", "Max distance for color mapping", GH_ParamAccess.item, 1);
            //pManager.AddNumberParameter("Shift", "S", "Shift", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Rebuild", "R", "Rebuild surface", GH_ParamAccess.item, -1);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            //pManager[3].Optional = true;
            //pManager[4].Optional = true;
            //pManager[5].Optional = true;
            //pManager[6].Optional = true;
            pManager[3].Optional = true;

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Dist", "D", "Distance to brep", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA) {
            try {
                Brep brep = new Brep();
                double U = 2;
                int V = 2;
                DA.GetData(1, ref U);
                bool flag2 = DA.GetData(2, ref V);

                if (DA.GetData(0, ref brep)) {
                    if (brep.Faces.Count == 1 && flag2) {

                        Surface S = brep.Faces[0].ToNurbsSurface();
                        //DA.GetData(0, ref S);


                        Brep b = new Brep();
                        double min = 0;
                        double max = 1;
                        double shift = 0.5;
                        int rebuild = 9;


                        bool flag = false;
                        //bool flag = DA.GetData(3, ref b);
                        // DA.GetData(4, ref min);
                        //DA.GetData(5, ref max);
                        //DA.GetData(6, ref shift);
                        DA.GetData(3, ref rebuild);
                        if (rebuild > 0)
                            S = S.Rebuild(3, 3, rebuild, rebuild);


                        if (U <= 0)
                            U = 1;

                        if (V <= 0)
                            V = 1;

                        S.SetDomain(0, new Interval(0, 1));
                        S.SetDomain(1, new Interval(0, 1));


                        if (flag) {
                            double[] distances = new double[0];
                            Mesh mesh = NGonCore.MeshCreate.QuadMeshDistanceFromBrep(S, Math.Max(2, (int)U), Math.Max(2, (int)V), b, min, max, shift, ref distances);


                            DA.SetData(0, mesh, DA.Iteration);
                            DA.SetDataList(1, distances);
                            base.Message = (String.Format("Min {0} \n Min {1} \n Av {2}", Math.Round(distances.Min(), 2), Math.Round(distances.Max(), 2), Math.Round(distances.Average(), 2)));

                        } else {
                            Mesh mesh = NGonCore.MeshCreate.QuadMesh(S, Math.Max(2, (int)U), Math.Max(2, (int)V));

                            brep.ClosestPoint(mesh.Vertices[0], out Point3d cp, out ComponentIndex ci, out double s, out double t, 1000000000, out Vector3d normal);
                            if ((((Point3d)mesh.Vertices[0]) + normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1) > (((Point3d)mesh.Vertices[0]) - normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1))
                                mesh.Flip(true, true, true);
                            DA.SetData(0, mesh);
                            this.PreparePreview(mesh, DA.Iteration);
                        }
                        this.Message = "Division U V ";
                    } else {


                        if (!explode && foo) {//&& !flag2


                            int[][] feDivisions = null;
                            if (U == 0)
                                U = 1000;
                            int[] edgeDivisions = brep.EdgeDivisions(U, ref feDivisions);
                            //Rhino.RhinoApp.WriteLine(feDivisions.Length.ToString());
                            Mesh mesh = new Mesh();

                            for (int i = 0; i < brep.Faces.Count; i++) {
                                Surface surface = brep.Faces[i].ToNurbsSurface();
                                //Rhino.RhinoApp.WriteLine(feDivisions[i][0].ToString() +" " + feDivisions[i][1].ToString());
                                int UDvisions = (foo) ? feDivisions[i][0] : Math.Max(2, (int)U);
                                int VDvisions = (foo) ? feDivisions[i][1] : Math.Max(2, (int)V);
                                Mesh meshTemp = NGonCore.MeshCreate.QuadMesh(surface, UDvisions, VDvisions, false, false, false);
                                mesh.Append(meshTemp);
                            }

                            mesh.Clean();
                            if (mesh.SolidOrientation() == -1)
                                mesh.Flip(true, true, true);
                            //mesh.WeldUsingRTree(0.001);
                            mesh.Ngons.Clear();


                            brep.ClosestPoint(mesh.Vertices[0], out Point3d cp, out ComponentIndex ci, out double s, out double t, 1000000000, out Vector3d normal);
                            if ((((Point3d)mesh.Vertices[0]) + normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1) > (((Point3d)mesh.Vertices[0]) - normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1))
                                mesh.Flip(true, true, true);
                            DA.SetData(0, mesh);
                            this.PreparePreview(mesh, DA.Iteration);

                            this.Message = "Division U Dist";




                        } else {

                            Mesh mesh = new Mesh();
                            for (int i = 0; i < brep.Faces.Count; i++) {


                                //Reparametricize
                                Surface S = brep.Faces[i].ToNurbsSurface();
                                S.SetDomain(0, new Interval(0, 1));
                                S.SetDomain(1, new Interval(0, 1));
                                Mesh m = NGonCore.MeshCreate.QuadMesh(S, Math.Max(2, (int)U), (int)Math.Max(2, V), false, false, false);
                                mesh.Append(m);
                            }


                            brep.ClosestPoint(mesh.Vertices[0], out Point3d cp, out ComponentIndex ci, out double s, out double t, 1000000000, out Vector3d normal);
                            if ((((Point3d)mesh.Vertices[0]) + normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1) > (((Point3d)mesh.Vertices[0]) - normal * 1).DistanceToSquared(((Point3d)mesh.Vertices[0]) + mesh.Normals[0] * 1))
                                mesh.Flip(true, true, true);
                            this.PreparePreview(mesh, DA.Iteration);
                            DA.SetData(0, mesh);

                            this.Message = "Division UV Explode" ;

                        }



                    }
                }
            }catch(Exception e) {
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


        public static Mesh QuadMeshDistanceFromBrep(Surface surface, int num, int num2, Brep target, double min, double max, double shift, ref double[] distances) {

            Mesh mesh = new Mesh();

            surface.SetDomain(0, new Interval(0.0, 1.0));
            surface.SetDomain(1, new Interval(0.0, 1.0));

            double num5 = 1.0 / (double)num;
            double num6 = 1.0 / (double)num2;

            int v = 0;
            int f = 0;

            for (int i = 0; i < num; i++) {
                for (int j = 0; j < num2; j++) {
                    Point3d pa = surface.PointAt((double)i * num5, (double)j * num6);
                    Point3d pb = surface.PointAt((double)(i + 1) * num5, (double)j * num6);
                    Point3d pc = surface.PointAt((double)(i + 1) * num5, (double)(j + 1) * num6);
                    Point3d pd = surface.PointAt((double)i * num5, (double)(j + 1) * num6);

                    mesh.Vertices.Add(pa);
                    mesh.Vertices.Add(pb);
                    mesh.Vertices.Add(pc);
                    mesh.Vertices.Add(pd);

                    mesh.Faces.AddFace(v + 2, v + 1, v + 0);
                    mesh.Faces.AddFace(v + 3, v + 2, v + 0);

                    MeshNgon ngon = MeshNgon.Create(new[] { v + 0, v + 1, v + 2, v + 3 }, new[] { f + 0, f + 1 });
                    mesh.Ngons.AddNgon(ngon);
                    v += 4;
                    f += 2;
                }
            }

            mesh.Vertices.CombineIdentical(true, true);
            mesh.FaceNormals.ComputeFaceNormals();
            mesh.Normals.ComputeNormals();
            mesh.Compact();



            //Assign colors


            distances = new double[mesh.Vertices.Count];

            for (int i = 0; i < mesh.Vertices.Count; i++) {

                double dist = target.ClosestPoint(mesh.Vertices[i]).DistanceTo(mesh.Vertices[i]);
                dist -= shift;
                distances[i] = dist;
                dist = MathUtil.Constrain(dist, min, max);

                int temp = MathUtil.MapInt(dist, min, max, 0, 255);


                //mesh.VertexColors.Add( Color.FromArgb(255, 255-temp,temp));
                mesh.VertexColors.Add( Color.FromArgb(255-temp, 255-temp, 255-temp));

                //mesh.VertexColors.Add(Color.FromArgb(0,rAverage, gAverage, bAverage));
            }



            return mesh;
        }


        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.grid;
            }
        }


        public override Guid ComponentGuid {
            get { return new Guid("f8b558c9-c00b-4bf1-b2b4-8140a5ee1f08"); }
        }
    }
}