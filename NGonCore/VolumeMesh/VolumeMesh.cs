using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore
{
    public class VolumeMesh
    {
        private PlaneMeshList _meshes;

        public VolumeMesh()
        {
        }

        public void AddMesh(PlaneMesh planeMesh)
        {
            this._meshes.Add(planeMesh);
        }

        public void AddMesh(Mesh mesh, Plane[] facePlanes, int ID = -1, int[] curIDmf = null, int[] adjIDm = null, int[] adjIDmf = null)
        {
            this._meshes.Add(new PlaneMesh(mesh, facePlanes,ID, curIDmf, adjIDm, adjIDmf));
        }


        //Plane[][] _meshesPlanes;
        //int[][] _adjID; // id of adjancent meshes
        //int[][] _F;//id of faces that are connected to other meshes
        //int[][] _adjIDF;//if of adjacent meshes faces

        //public VolumeMesh(int NumOfMeshes)
        //{
        //    _meshes = new Mesh[NumOfMeshes];
        //    _meshesPlanes = new Plane[NumOfMeshes][];
        //    _adjID = new int[NumOfMeshes][];
        //    _F = new int[NumOfMeshes][];
        //    _adjIDF = new int[NumOfMeshes][];
        //}

        //public void Add(Mesh mesh, Plane[] meshPlanes, int id, int[] f, int[] adjID, int[] adjIDF)
        //{
        //    this._meshes[id] = mesh;
        //    this._meshesPlanes[id] = meshPlanes;
        //    this._F[id] = f;
        //    this._adjID[id] = adjID;
        //    this._adjIDF[id] = adjIDF;
        //}



    }
}
