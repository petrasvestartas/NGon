using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using NGonsCore;
using NGonsCore.Clipper;
using ObjParser;
using Rhino.Geometry;


namespace NGonGh.Utils {
    public class BoundaryFirstFlatenning : GH_Component {

        public BoundaryFirstFlatenning() 
          : base("BoundaryFirstFlatenning", "BoundaryFirstFlatenning",
              "BoundaryFirstFlatenning", "NGon",
             "Utilities Mesh") {
            }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddGenericParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Path","P","Path of ngon folder, only needed if component gives you error message", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("Mesh3D", "M3D", "Mesh3D", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh2D", "M2D", "Mesh2D", GH_ParamAccess.item);
            
        }


        protected override void SolveInstance(IGH_DataAccess DA) {

            Mesh m = new Mesh();
            DA.GetData(0, ref m);

            string NGonPath = "";
            DA.GetData(1, ref NGonPath);


            bool isOriented;
            bool hasBoundary;

            Mesh M = m.DuplicateMesh();
            M.Faces.ConvertQuadsToTriangles();

            if (M.IsValid && M.IsManifold(true, out isOriented, out hasBoundary)) {


                //write obj file
                var objInput = new ObjParser.Obj();


                var assembly = System.Reflection.Assembly.GetAssembly(typeof(ObjParser.Obj)).Location;
                string assemblyPath = System.IO.Path.GetDirectoryName(assembly);
                //Print(theDirectory);


                //string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                //string assemblyPath = System.IO.Path.GetDirectoryName(assemblyLocation);

                if (NGonPath != null) {
                    if (NGonPath != "") {
                        assemblyPath = NGonPath;
                    }
                }
                string tempfilepath = assemblyPath + @"\windows\in.obj";

                if (System.IO.File.Exists(tempfilepath)) {
                    System.IO.File.Delete(tempfilepath);
                }

                // string tempfilepath = @"C:\Users\petra\New folder\test.obj";
                string[] headers = new string[] { "ObjParser" };

                List<string> objFile = new List<string>();

                foreach (Point3f p in M.Vertices) {
                    objFile.Add("v" + " " + p.X.ToString() + " " + p.Y.ToString() + " " + p.Z.ToString());
                }

                foreach (MeshFace mf in M.Faces) {
                    objFile.Add("f" + " " + (mf.A + 1).ToString() + " " + (mf.B + 1).ToString() + " " + (mf.C + 1).ToString());
                }



                objInput.LoadObj(objFile.ToArray());
                objInput.WriteObjFile(tempfilepath, headers);



                string filename = assemblyPath + @"\windows\bff-command-line.exe";
                string workingDirectory = assemblyPath + @"\windows\";


                var proc = new System.Diagnostics.Process {
                    StartInfo = new System.Diagnostics.ProcessStartInfo {
                        FileName = filename,
                        Arguments = "in.obj out.obj",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDirectory
                    }
                };

                proc.Start();

                proc.WaitForExit();


                //Outputs
                Mesh mesh3D = new Mesh();
                Mesh mesh2D = new Mesh();
                List<Point3d> points3D = new List<Point3d>();
                List<Point3d> points2D = new List<Point3d>();
                List<MeshFace> faces3D = new List<MeshFace>();
                List<MeshFace> faces2D = new List<MeshFace>();

                // Initialize
                var obj = new ObjParser.Obj();

                // Read Wavefront OBJ file
                //obj.LoadObj(@"C:\libs\windows\out.obj");
                obj.LoadObj(assemblyPath + @"\windows\out.obj");

                //Rhino.RhinoApp.WriteLine(assemblyPath + @"\windows\out.obj");


                foreach (ObjParser.Types.Vertex v in obj.VertexList) {
                    mesh3D.Vertices.Add(new Point3d(v.X, v.Y, v.Z));
                }

                foreach (ObjParser.Types.TextureVertex v in obj.TextureList) {
                    mesh2D.Vertices.Add(new Point3d(v.X, v.Y, 0));
                }


                foreach (ObjParser.Types.Face f in obj.FaceList) {

                    string[] lineData = f.ToString().Split(' ');
                    string[] v0 = lineData[1].Split('/');
                    string[] v1 = lineData[2].Split('/');
                    string[] v2 = lineData[3].Split('/');

                    MeshFace mf3D = new MeshFace(Convert.ToInt32(v0[0]) - 1, Convert.ToInt32(v1[0]) - 1, Convert.ToInt32(v2[0]) - 1);
                    MeshFace mf2D = new MeshFace(Convert.ToInt32(v0[1]) - 1, Convert.ToInt32(v1[1]) - 1, Convert.ToInt32(v2[1]) - 1);
                    mesh3D.Faces.AddFace(mf3D);
                    mesh2D.Faces.AddFace(mf2D);
                }


                mesh2D.RebuildNormals();
                mesh3D.RebuildNormals();

                BoundingBox bbox = mesh2D.GetBoundingBox(false);
                Vector3d vec = M.Vertices[0] - mesh3D.Vertices[0];
                mesh3D.Translate(vec);
                //mesh2D.Transform(Transform.Translation(bbox.PointAt(0.5, 0, 0) - bbox.PointAt(0, 0, 0)));

                //      DA.SetData(0, mesh3D);
                //      DA.SetData(1, mesh2D);

                DA.SetData(0, mesh3D);
                DA.SetData(1, mesh2D);


                //M2D = mesh2D;
                //M3D = mesh3D;


            }

        }


        public void processLine(string line, ref List<Point3d> points3D, ref List<Point3d> points2D, ref List<MeshFace> faces3D, ref List<MeshFace> faces2D) {


            string[] lineData = line.Split(' ');
            string type = lineData[0];

            switch (type) {

                case ("v")://3d point
                Point3d p3D = new Point3d(Convert.ToDouble(lineData[1]), Convert.ToDouble(lineData[2]), Convert.ToDouble(lineData[3]));
                points3D.Add(p3D);
                break;

                case ("vt")://2d point
                Point3d p2D = new Point3d(Convert.ToDouble(lineData[1]), Convert.ToDouble(lineData[2]), 0);
                points2D.Add(p2D);
                break;

                case ("f"):

                string[] v0 = lineData[1].Split('/');
                string[] v1 = lineData[2].Split('/');
                string[] v2 = lineData[3].Split('/');

                MeshFace mf3D = new MeshFace(Convert.ToInt32(v0[0]) - 1, Convert.ToInt32(v1[0]) - 1, Convert.ToInt32(v2[0]) - 1);
                MeshFace mf2D = new MeshFace(Convert.ToInt32(v0[1]) - 1, Convert.ToInt32(v1[1]) - 1, Convert.ToInt32(v2[1]) - 1);
                faces3D.Add(mf3D);
                faces2D.Add(mf2D);

                break;
            }
        }

        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resources.boundaryfirstflatenning;
            }
        }

        public override GH_Exposure Exposure {
            get { return GH_Exposure.tertiary; }
        }

        public override Guid ComponentGuid {
            get { return new Guid("55f1321a-d5e1-4c3f-aedb-bd89ce58a178"); }
        }
    }
}

