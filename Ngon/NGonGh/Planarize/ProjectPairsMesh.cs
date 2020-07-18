using System;
using Grasshopper.Kernel;
using NGonsCore;
using Rhino.Geometry;

namespace SubD.Planarize
{



    public class ProjectPairsMesh : GH_Component
    {


        /// <summary>
        /// Initializes a new instance of the Planarize class.
        /// </summary>
        public ProjectPairsMesh()
            : base("ProjectPairsMesh", "ProjectPairsMesh",
                "ProjectPairsMesh",
                "SubD", "Planarize")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh ", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "For animations, add slider 0.00 to 1.00 to interpolate", GH_ParamAccess.item, 0);
            pManager.AddMeshParameter("Mesh", "T", "Mesh ", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh ", GH_ParamAccess.list);
            //  pManager.AddCurveParameter("Polylines", "P", "Projected NGons to average planes", GH_ParamAccess.list);
            // pManager.AddIntegerParameter("Id", "I", "Vertex Id in original mesh", GH_ParamAccess.tree);
        }




        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();


            double scale = 1;
            DA.GetData(1, ref scale);

            Mesh target = new Mesh();


            if (DA.GetData(0, ref mesh) && DA.GetData(2, ref target))
            {
                var m = ProjectPairsMeshToTarget(mesh,scale,target);
                DA.SetDataList(0, m);
            }
        }


        public static Mesh[] ProjectPairsMeshToTarget( Mesh mesh, double value, Mesh target) {

            int n = (int)(mesh.Ngons.Count * 0.5);
            Polyline[][] p = new Polyline[n][];
            Plane[] pl = mesh.GetNgonPlanes();
            Mesh[] m = new Mesh[n];


            uint[][] id = mesh.GetNGonsBoundaries();

            for (int i = 0; i < n; i++) {

                p[i] = new Polyline[] { new Polyline(), new Polyline() };

                //Vector3d vec = Vector3d.ZAxis;

                for (int j = 0; j < id[i].Length; j++) {
                    Line temp = new Line(mesh.Vertices[(int)id[i][j]], mesh.Vertices[(int)id[i + n][j]]);
                    temp.Transform(Transform.Scale(temp.PointAt(0.5), 100000));

                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i], out double t1);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i + n], out double t2);

                    Point3d[] pt= Rhino.Geometry.Intersect.Intersection.MeshLine(target, temp, out int[] faceIds);

                    Point3d ptA = pt[0];


                    Point3d tempPt = mesh.ClosestPoint(pt[0]);
                    double distance = tempPt.DistanceTo(pt[0]);
                    if (pt.Length > 1)
                    {
                        for (int k = 1; k < pt.Length; k++) {
                            //Measure distance between shell mesh and intersection point
                            Point3d tempPt2 = mesh.ClosestPoint(pt[k]);
                            double dist = tempPt2.DistanceTo(pt[k]);
                            if (dist < distance) {
                                ptA = pt[k];
                                distance = dist;
                            }
                        }
                    }

                    double t3 = temp.ClosestParameter(ptA);

                    

                    Point3d a = temp.PointAt(t2);
                    Point3d b = temp.PointAt(MathUtil.Lerp(t1, t3, value));
                    p[i][0].Add(a);
                    p[i][1].Add(b);

                    //Vector3d tempVec = Vector3d.Subtract((Vector3d)a, (Vector3d)b);
                    //tempVec.Unitize();
                    //vec += tempVec;
                }

                //vec.Unitize();

                //Point3d origin = Point3d.Origin;
                //origin += p[i][1][0];
                //origin += p[i][1][1];
                //origin += p[i][1][3];
                //origin += p[i][1][4];

                //if (origin != Point3d.Origin)
                //    origin /= 4;

                //Point3d cp = target.ClosestPoint(origin);
                //vec = Vector3d.Subtract((Vector3d)origin, (Vector3d)cp);


                //pl[i] = new Plane(p[i][1].CenterPoint(), vec);//p[i][1].plane();
                //pl[i] = new Plane(origin, vec);//p[i][1].plane();
                //for (int j = 0; j < id[i].Length; j++) {

                //    Line temp = new Line(p[i][0][j], p[i][1][j]);

                //    Rhino.Geometry.Intersect.Intersection.LinePlane(temp, pl[i], out double t11);
                //    p[i][1][j] = temp.PointAt(t11);
                //}



                m[i] = new Mesh();
                m[i].Vertices.AddVertices(p[i][0].ToArray());
                m[i].Vertices.AddVertices(p[i][1].ToArray());

                m[i].Faces.AddFace(2, 1, 0);
                m[i].Faces.AddFace(4, 3, 2);
                m[i].Faces.AddFace(5, 2, 0);
                m[i].Faces.AddFace(4, 2, 5);
                m[i].Ngons.AddNgon(MeshNgon.Create(new[] { 0, 1, 2, 3, 4, 5 }, new[] { 0, 1, 2, 3 }));

                m[i].Faces.AddFace(2 + 6, 1 + 6, 0 + 6);
                m[i].Faces.AddFace(4 + 6, 3 + 6, 2 + 6);
                m[i].Faces.AddFace(5 + 6, 2 + 6, 0 + 6);
                m[i].Faces.AddFace(4 + 6, 2 + 6, 5 + 6);
                m[i].Ngons.AddNgon(MeshNgon.Create(new[] { 0 + 6, 1 + 6, 2 + 6, 3 + 6, 4 + 6, 5 + 6 }, new[] { 4, 5, 6, 7 }));

                for (int j = 0; j < p[i][0].Count - 1; j++)
                            m[i].Faces.AddFace(j, j + 1, p[i][0].Count+ j + 1, p[i][0].Count + j);
                m[i].Faces.AddFace(p[i][0].Count - 1, 0, p[i][0].Count , p[i][0].Count * 2 - 1);



            }

            return m;
        }


        public static Vector3d AverageNormal( Polyline p) {
            //PolyFace item = this[index];
            int len = p.Count - 1;
            Vector3d vector3d = new Vector3d();
            int count = checked(len - 1);

            for (int i = 0; i <= count; i++) {
                int num = (checked(checked(i - 1) + len)) % len;
                int item1 = (checked(checked(i + 1) + len)) % len;
                Point3d point3d = p[num];
                Point3d point3d1 = p[item1];
                Point3d item2 = p[i];
                vector3d = vector3d + Vector3d.CrossProduct(new Vector3d(item2 - point3d), new Vector3d(point3d1 - item2));
            }

            if (vector3d.X == 0 & vector3d.Y == 0 & vector3d.Z == 0)
                vector3d.Unitize();

            return vector3d;
        }




        protected override System.Drawing.Bitmap Icon { get; } = Properties.Resources.projectToMesh;

        public override Guid ComponentGuid { get; } = new Guid("ed86ecf4-801b-40e5-b153-1ecfdcdf1563");
    }
}