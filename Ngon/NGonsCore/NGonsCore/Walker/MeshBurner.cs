using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore.Walker {
    public static class MeshBurner {

        public static List<Mesh> MeshBurnerSolver(Mesh M) {

            if (M.IsValid) {
                if (M.Ngons.Count > 0)
                    return meshBurnerNgons(M);
                else
                    return meshBurner(M);
            } else return new List<Mesh>() { M};


        }

        //Return the face idices of any face with a naked vertex
        public static List<int> GetNakedFaceIDs(Mesh mesh) {

            List<int> nakedFaces = new List<int>();

            //Get naked vertieces
            bool[] nPts = mesh.GetNakedEdgePointStatus();
            List<int> nIDs = new List<int>();

            for (int i = 0; i < nPts.Length; i++) {
                if (nPts[i] == true)
                    nIDs.Add(i);
            }//for i

            for (int i = 0; i < mesh.Faces.Count; i++) {

                //Get face vertices
                MeshFace f = mesh.Faces[i];
                List<int> vts = new List<int>() { f.A, f.B, f.C };

                if (f.IsQuad)
                    vts.Add(f.D);

                //Check if they are naked
                bool naked = false;
                foreach (int vt in vts) {
                    if (nPts[vt] == true)
                        naked = true;
                }

                if (naked)
                    nakedFaces.Add(i);

            }//for i


            return nakedFaces;

        }

        public static List<int> GetNakedNGonIDs(Mesh mesh) {

            List<int> nakedFaces = new List<int>();

            //Get naked vertices
            bool[] nPts = mesh.GetNakedEdgePointStatus();
            List<int> nIDs = new List<int>();

            for (int i = 0; i < nPts.Length; i++) {
                if (nPts[i] == true)
                    nIDs.Add(i);
            }//for i

            for (int i = 0; i < mesh.Ngons.Count; i++) {



                //Get face vertices
                //MeshFace f = mesh.Faces[i];
                //List < int> vts = new List<int>(){f.A,f.B,f.C};
                uint[] vts = mesh.Ngons[i].BoundaryVertexIndexList();

                //if(f.IsQuad)
                // vts.Add(f.D);

                //Check if they are naked
                bool naked = false;
                foreach (int vt in vts) {
                    if (nPts[(int)vt] == true)
                        naked = true;
                }

                if (naked)
                    nakedFaces.Add(i);

            }//for i


            return nakedFaces;

        }


        public  static List<Mesh> meshBurnerNgons(Mesh m) {
            //Discretize a mesh using a grassire algorithm

            Mesh mesh = m.DuplicateMesh();

            List<Mesh> burnFronts = new List<Mesh>();


            int i = 0;
            while (mesh.Faces.Count > 0 && i < 10) {

                Polyline[] polylines = mesh.GetPolylines();

                uint[][] nf = mesh.GetFacesInNGons();


                //Add the burn perimeter to the mesh
                List<int> nfIDs = GetNakedNGonIDs(mesh);


                Polyline[] bmPolys0 = new Polyline[nfIDs.Count];
                Polyline[] bmPolys1 = new Polyline[polylines.Length - nfIDs.Count];

                int counter0 = 0;
                int counter1 = 0;
                for (int j = 0; j < polylines.Length; j++) {
                    if (nfIDs.Contains(j)) {
                        bmPolys0[counter0++] = polylines[j];
                    } else {
                        bmPolys1[counter1++] = polylines[j];
                    }
                }
                Mesh bm0 = NGonsCore.MeshCreate.MeshFromPolylines(bmPolys0, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                Mesh bm1 = NGonsCore.MeshCreate.MeshFromPolylines(bmPolys1, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                // bm0.Clean();
                // bm1.Clean();
                //bm0.UnifyNormals();
                //bm0.RebuildNormals();

                Mesh[] meshes = bm0.SplitDisjointPieces();
                foreach (Mesh mm in meshes)
                    mm.Clean();

                mesh = bm1.DuplicateMesh();





                burnFronts.AddRange(meshes);
                // Print(mesh.Ngons.Count.ToString());
                i++;
            }


            return burnFronts;
        }




        public static List<Mesh>  meshBurner(Mesh m) {
            //Discretize a mesh using a grassire algorithm

            Mesh mesh = m.DuplicateMesh();

            List<Mesh> burnFronts = new List<Mesh>();

            int i = 0;
            while (mesh.Faces.Count > 0 && i < 100) {
          
                //Make furn front mesh and add vertices to it
                Mesh bm = new Mesh();
                bm.Vertices.AddVertices(mesh.Vertices.ToPoint3fArray());

                //Add the burn permeter to the mesh
                List<int> nfIDs = GetNakedFaceIDs(mesh);
                foreach (int f in nfIDs)
                    bm.Faces.AddFace(mesh.Faces[f]);

                //Compact and append to ouput list
                bm.Compact();
                bm.Normals.ComputeNormals();
                burnFronts.Add(bm);

                //delete burned faces
                mesh.Faces.DeleteFaces(nfIDs);

                i++;
            }


            return burnFronts;
        }


    }
}
