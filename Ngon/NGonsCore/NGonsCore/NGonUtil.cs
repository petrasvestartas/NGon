using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Collections;

namespace NGonsCore
{
    public static class NGonsUtil
    {

        /// <summary>
        /// Get Ngon Count
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>Oppo
        public static int _countF(this Mesh M)
        {
            return M.Ngons.Count;
        }

        /// <summary>
        /// Get Ngon Vertex and Edge Count
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static int _countE(this Mesh M, int F)
        {
            return M.Ngons[F].BoundaryVertexCount;
        }

        /// <summary>
        /// Get Ngon Vertex and Edge Count
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static int _countV(this Mesh M, int F)
        {
            return M.Ngons[F].BoundaryVertexCount;
        }

        /// <summary>
        /// Get NGon Boundaries as topological vertex ID array
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static int[] _v(this Mesh M, int F, bool topologyVertices = true)
        {

            if (topologyVertices)
                return MeshUtil.VertexTopoVertex(M, M.Ngons[F].BoundaryVertexIndexList());

            uint[] vUint = M.Ngons[F].BoundaryVertexIndexList();
            int[] vInt = new int[vUint.Length];
            for (int i = 0; i < vUint.Length; i++)
                vInt[i] = (int)vUint[i];

            return vInt;
        }


        /// <summary>
        /// Get NGons Boundaries as topological vertex ID array
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static int[][] _v(this Mesh M)
        {
            int[][] boundaries = new int[M.Ngons.Count][];

            for (int i = 0; i < M.Ngons.Count; i++)
                boundaries[i] = M._v(i);

            return boundaries;
        }

        public static int[] _fv(this Mesh M, int F, bool topo)
        {

            uint[] uintv = M.Ngons[F].BoundaryVertexIndexList();
            int[] v = new int[uintv.Length];
            for (int i = 0; i < uintv.Length; i++)
            {
                v[i] = (topo) ? (int)uintv[i] : M.TopologyVertices.TopologyVertexIndex((int)uintv[i]);
            }

            return v;

        }


        /// <summary>
        /// Get Ngon Edge vertices
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        public static int[] _e(this Mesh M, int F, int E, int next = 0)
        {

            var v = M.Ngons[F].BoundaryVertexIndexList();

            int v0 = (int)v[MathUtil.Wrap(E + 0 + next, v.Length)];
            int v1 = (int)v[MathUtil.Wrap(E + 1 + next, v.Length)];

            return new int[] { v0, v1 };
        }



        public static int _meshE(this Mesh M, int F, int E, int next = 0)
        {

            var v = M.Ngons[F].BoundaryVertexIndexList();

            int v0 = (int)v[MathUtil.Wrap(E + 0 + next, v.Length)];
            int v1 = (int)v[MathUtil.Wrap(E + 1 + next, v.Length)];

            v0 = M.TopologyVertices.TopologyVertexIndex(v0);
            v1 = M.TopologyVertices.TopologyVertexIndex(v1);

            int meshEdge = M.TopologyEdges.GetEdgeIndex(v0, v1);

            return meshEdge;
        }


        /// <summary>
        /// Get NGons Face Edges, as mesh edge indices
        /// </summary>
        /// <param name="M"></param>
        /// <param name="nGonTV"></param>
        /// <returns></returns>
        public static int[] _fe(this Mesh M, int F)
        {


            int n = M.Ngons[F].BoundaryVertexCount;
            int[] fe = new int[n];
            int[] v = M._v(F);


            for (int j = 0; j < n - 1; j++)
                fe[j] = M.TopologyEdges.GetEdgeIndex(v[j], v[j + 1]);

            int tempEId = M.TopologyEdges.GetEdgeIndex(v[0], v[n - 1]);
            fe[n - 1] = tempEId;


            return fe;
        }


        /// <summary>
        /// Get NGons Faces Edges, as mesh edge indices
        /// </summary>
        /// <param name=","></param>
        /// <param name="nGonTV"></param>
        /// <returns></returns>
        public static int[][] _FE(this Mesh M, int[][] nGonTV)
        {

            int[][] nGonE = new int[M.Ngons.Count][];

            for (int i = 0; i < M.Ngons.Count; i++)
            {
                nGonE[i] = M._fe(i);
            }

            return nGonE;
        }


        public static List<int> _FEFlattenV(this Mesh M)
        {
            List<int> v = new List<int>();

            int counter = 0;
            for (int i = 0; i < M._countF(); i++)
                for (int j = 0; j < M._countE(i); j++)
                    v.Add(counter++);

            return v;
        }

        /// <summary>
        /// //face, edge - flatten id of faces edge (duplicates)
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static int[][] _FEFlatten(this Mesh M)
        {

            int[][] FE_ = new int[M._countF()][];//face, edge - flatten id


            int counter = 0;
            for (int i = 0; i < M._countF(); i++)
            {
                FE_[i] = new int[M._countE(i)];

                for (int j = 0; j < M._countE(i); j++)
                    FE_[i][j] = counter++;
            }

            return FE_;
        }

        /// <summary>
        /// flatten id of faces edge (duplicates) - face, edge 
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static List<int[]> _FlattenFE(this Mesh M)
        {

            List<int[]> _FE = new List<int[]>(); //flatten id - face, edge

            for (int i = 0; i < M._countF(); i++)
                for (int j = 0; j < M._countE(i); j++)
                    _FE.Add(new int[] { i, j });

            return _FE;
        }

        /// <summary>
        /// edge adjacency: current edge - next face edge, prev face edge, opposite face edge
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static List<int[]>[] _eehalf(this Mesh M)
        {


            var eehalf = new List<int[]>[M._countF()];

            var FE_ = M._FEFlatten();

            for (int i = 0; i < M._countF(); i++)
            {

                var edges = new List<int[]>();

                for (int j = 0; j < M._countE(i); j++)
                {

                    int eA = FE_[i][j];
                    int eBNext = FE_[i].Next(j);
                    int eBPrev = FE_[i].Prev(j);

                    int[] oppo = M._OppositeFE(i, j);
                    if (oppo[0] != -1)
                    {//not a boundary
                        int eBOppo = FE_[oppo[0]][oppo[1]];
                        edges.Add(new int[] { eA, eBOppo });
                        //if(i == 0 && j == 0) Print(eA.ToString() + " " + eBNext.ToString() + " " + eBPrev.ToString() + " " + eBOppo.ToString());
                    }

                    //next / prev
                    edges.Add(new int[] { eA, eBNext });
                    edges.Add(new int[] { eA, eBPrev });


                }

                eehalf[i] = edges;
            }

            return eehalf;
        }

        /// <summary>
        /// NGons By MeshEdge
        /// </summary>
        /// <param name="M"></param>
        /// <param name="MeshEdge"></param>
        /// <returns></returns>
        public static int[] _ef(this Mesh M, int MeshEdge)
        {

            //Get connected mesh faces
            int[] connectedFaces = M.TopologyEdges.GetConnectedFaces(MeshEdge);
            int[] connectedNGons = new int[connectedFaces.Length];

            for (int i = 0; i < connectedFaces.Length; i++)
                connectedNGons[i] = M.Ngons.NgonIndexFromFaceIndex(connectedFaces[i]);

            return connectedNGons;
        }

        /// <summary>
        /// Get edge face adjacency, boundaries would have 1 face
        /// id array corresponds to M.EAll method order
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static int[][] _EF(this Mesh M)
        {
            Dictionary<int, int> eAll = M._EAll();

            int[][] ef = new int[eAll.Count][];

            int i = 0;
            foreach (KeyValuePair<int, int> meshEdge_FlatListOfID in eAll)
                ef[i++] = M._ef(meshEdge_FlatListOfID.Key);

            return ef;
        }

        /// <summary>
        /// array of tuples - all edges
        /// array - number of connected faces
        /// tuple - face, local edge
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static Tuple<int, int>[][] _EFE(this Mesh M)


        {

            Dictionary<int, int> eAll = M._EAll();
            Tuple<int, int>[][] efe = new Tuple<int, int>[eAll.Count][];
            int i = 0;

            //we have mesh edge and face edge
            foreach (KeyValuePair<int, int> meshEdge_FlatListOfID in eAll)
            {

                int meshedge = meshEdge_FlatListOfID.Key;
                int[] faces = M._ef(meshEdge_FlatListOfID.Key);
                efe[i] = new Tuple<int, int>[faces.Length];

                for (int j = 0; j < faces.Length; j++)
                {
                    int[] fe = M._fe(faces[j]);
                    int e = Array.IndexOf(fe, meshedge);
                    efe[i][j] = new Tuple<int, int>(faces[j], e);
                }

                i++;
            }


            return efe;
        }

        /// <summary>
        /// Get edge face adjacency, boundaries would have 1 face
        /// Dictionary contains mesh edges 
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static Dictionary<int, int[]> _EFDict(this Mesh M)
        {
            Dictionary<int, int> eAll = M._EAll();

            Dictionary<int, int[]> ef = new Dictionary<int, int[]>();

            foreach (KeyValuePair<int, int> meshEdge_FlatListOfID in eAll)
                ef.Add(meshEdge_FlatListOfID.Key, M._ef(meshEdge_FlatListOfID.Key));

            return ef;
        }

        public static int[] _vf(this Mesh M, int MeshVertex)
        {


            int[] connectedNGons = new int[0];

            int tv = M.TopologyVertices.TopologyVertexIndex(MeshVertex);


            int[] connectedFaces = M.TopologyVertices.ConnectedFaces(tv);

            HashSet<int> connectedNGonsHash = new HashSet<int>();
            foreach (int j in connectedFaces)
                connectedNGonsHash.Add(M.Ngons.NgonIndexFromFaceIndex(j));

            connectedNGons = connectedNGonsHash.ToArray();


            return connectedNGons;
        }

        public static int[] _ve(this Mesh M, int MeshVertex)
        {

            int tv = M.TopologyVertices.TopologyVertexIndex(MeshVertex);
            M.TopologyVertices.SortEdges(tv);

            int[] connectedEdges = M.TopologyVertices.ConnectedEdges(tv);

            HashSet<int> connectedNGonsEdgesHash = new HashSet<int>();

            foreach (int j in connectedEdges)
                if (!M.TopologyEdges.IsNgonInterior(j))
                    connectedNGonsEdgesHash.Add(j);



            return connectedNGonsEdgesHash.ToArray();

        }

        /// <summary>
        /// Get opposite Ngon, its local edge and meshe edge
        /// </summary>
        /// <param name="M"></param>
        /// <param name="f - ngon id"></param>
        /// <param name="e - local edge 0, 1, 2 - n"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static int[] _OppositeFE(this Mesh M, int F, int E, int next = 0)
        {

            //Get mesh edge from mesh face e

            int eWrapped = MathUtil.Wrap(E, M.Ngons[F].BoundaryVertexCount);

            int me = M._fe(F)[eWrapped];



            //Get connected mesh faces
            int[] connectedFaces = M.TopologyEdges.GetConnectedFaces(me);

            int oppositeFace = -1;


            if (connectedFaces.Length == 2)
            {
                int f0 = M.Ngons.NgonIndexFromFaceIndex(connectedFaces[0]);
                int f1 = M.Ngons.NgonIndexFromFaceIndex(connectedFaces[1]);
                oppositeFace = (F == f0) ? f1 : f0;
            }

            int oppositeEdge = -1;

            if (oppositeFace != -1)
            {
            
                int[] oppositeFE = M._fe(oppositeFace);

                oppositeEdge = Array.IndexOf(oppositeFE, me);
                oppositeEdge = MathUtil.Wrap(oppositeEdge + next, oppositeFE.Length);

            }

            int meOppo = (oppositeFace == -1) ? -1:  M._fe(oppositeFace)[oppositeEdge];

            return new int[] { oppositeFace, oppositeEdge,me, meOppo };//me

        }




        /// <summary>
        /// Find Sharing Mesh Edge
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F0"></param>
        /// <param name="F1"></param>
        /// <returns></returns>
        public static int _CommonEdge(this Mesh M, int F0, int F1)
        {

            int[] e0 = M._fe(F0);
            int[] e1 = M._fe(F1);

            var common = e0.Intersect(e1);

            if (common.Count() > 0)
                return common.First();
            else
                return -1;
        }

        /// <summary>
        /// Find Sharing Mesh Edges
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F0"></param>
        /// <param name="F1"></param>
        /// <returns></returns>
        public static int[] _CommonEdges(this Mesh M, int F0, int F1)
        {

            int[] e0 = M._fe(F0);
            int[] e1 = M._fe(F1);

            var common = e0.Intersect(e1);

            return common.ToArray();
        }

        //////////////////////////////////Geometry//////////////////////////////////



        /// <summary>
        /// Get NGone Edge Line by Face and Edge
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        public static Line _Line(this Mesh M, int F, int E)
        {

            int eWrapped = MathUtil.Wrap(E, M.Ngons[F].BoundaryVertexCount);
            int[] v = M._e(F, eWrapped);
            return new Line(M.Vertices[v[0]], M.Vertices[v[1]]);
        }


        public static Line _Line(this Mesh M, int[] FE)
        {
            return _Line(M, FE[0], FE[1]);
        }


        /// <summary>
        /// Get NGon Polyline from Mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Polyline _Polyline(this Mesh M, int F)
        {

            int[] v = M._v(F);
            Polyline p = new Polyline();

            for (int j = 0; j < v.Length; j++)
                p.Add(M.Vertices[v[j]]);

            p.Close();

            return p;
        }



        /// <summary>
        /// Get NGons as Polylines from Mesh
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static Polyline[] _Polylines(this Mesh M)
        {

            Polyline[] polylines = new Polyline[M.Ngons.Count];

            for (int i = 0; i < M.Ngons.Count; i++)
            {
                polylines[i] = M._Polyline(i);
            }

            return polylines;
        }

        /// <summary>
        /// Get Ngon Center
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="ngon"></param>
        /// <returns></returns>
        public static Point3d _FCenter(this Mesh M, int F)
        {

            uint[] vertices = M.Ngons.GetNgon(F).BoundaryVertexIndexList();

            Polyline outline = new Polyline();
            foreach (int v in vertices)
            {
                outline.Add(M.Vertices[v]);
            }

            return PolylineUtil.CenterPoint(outline);
        }

        /// <summary>
        /// Get Ngon points
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static Point3d[] _Points(this Mesh M, int F)
        {
            uint[] v = M.Ngons[F].BoundaryVertexIndexList();
            Point3d[] p = new Point3d[v.Length];

            for (int i = 0; i < v.Length; i++)
                p[i] = M.Vertices[(int)v[i]];

            return p;
        }


        /// <summary>
        /// Get Ngon Plane
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Plane _Plane(this Mesh M, int F)
        {
            return new Plane(M._FCenter(F), M._Normal(F));
        }

        /// <summary>
        /// Get Ngon Planes
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static Plane[] _FPlanes(this Mesh M)
        {
            Plane[] p = new Plane[M.Ngons.Count];
            for (int i = 0; i < M.Ngons.Count; i++)
                //Plane.FitPlaneToPoints(pts[i], out p[i]);
                p[i] = M._Plane(i);
            return p;
        }


        /// <summary>
        /// Get Ngon normal
        /// </summary>
        /// <param name="M"></param>
        /// <param name="F"></param>
        /// <returns></returns>
        public static Vector3d _Normal(this Mesh M, int F)
        {

            Vector3d vector3d = new Vector3d();

            uint[] faces = M.Ngons[F].FaceIndexList();

            for (int i = 0; i < faces.Length; i++)
                vector3d += M.FaceNormals[(int)faces[i]];

            vector3d.Unitize();

            return vector3d;
        }

        ///// <summary>
        ///// Get Mesh Topology vertices in all NGons (HashSet)
        ///// </summary>
        ///// <param name="mesh"></param>
        ///// <returns></returns>
        //public static int[] _VTopoAll(this Mesh M) {

        //    HashSet<int> allNGonTV = new HashSet<int>();


        //    for (int i = 0; i < M.Ngons.Count; i++) {
        //        uint[] meshV = M.Ngons[i].BoundaryVertexIndexList();
        //        int[] meshTv = _VtoVTopo(M, meshV);

        //        foreach (int j in meshTv)
        //            allNGonTV.Add(j);
        //    }
        //    return allNGonTV.ToArray();
        //}

        /// <summary>
        /// Get Mesh vertices in all NGons (HashSet)
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Dictionary<int, int> _VAll(this Mesh M, bool topo = false)
        {

            HashSet<int> allNGonV = new HashSet<int>();

            for (int i = 0; i < M.Ngons.Count; i++)
            {
                uint[] meshV = M.Ngons[i].BoundaryVertexIndexList();
                foreach (uint j in meshV)
                {

                    if (!topo)
                    {
                        allNGonV.Add((int)j);
                    }
                    else
                    {
                        allNGonV.Add(M.TopologyVertices.TopologyVertexIndex((int)j));
                    }

                }
            }

            Dictionary<int, int> meshVertexNGonVertex = new Dictionary<int, int>();

            int counter = 0;
            foreach (int i in allNGonV)
            {
                meshVertexNGonVertex.Add(i, counter++);
            }

            return meshVertexNGonVertex;
        }

        /// <summary>
        /// Dicionary of all mesh edges, first int - meshes Edge. second int - flattend id list
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static Dictionary<int, int> _EAll(this Mesh M)
        {

            Dictionary<int, int> allE = new Dictionary<int, int>();
            HashSet<int> hash = new HashSet<int>();
            int counter = 0;

            try
            {
                for (int i = 0; i < M.Ngons.Count; i++)
                {
                    int n = M.Ngons[i].BoundaryVertexCount;

                    int[] fv = M._fv(i, true);


                    for (int j = 0; j < n - 1; j++)
                    {
                        int et = M.TopologyEdges.GetEdgeIndex(fv[j], fv[j + 1]);

                        if (hash.Add(et))
                            allE.Add(et, counter++);

                        //if (!allE.ContainsKey(et))
                        //    allE.Add(et, counter++);

                    }

                    int e = M.TopologyEdges.GetEdgeIndex(fv[0], fv[n - 1]);

                    if (hash.Add(e))
                        allE.Add(e, counter++);

                    //if (!allE.ContainsKey(e)) {
                    //    allE.Add(e, counter++);
                    //}

                }
            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.ToString());
            }
            return allE;



        }

        /// <summary>
        /// Get outlines of faces edges
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static List<Line> _EFLinesAll(this Mesh M)
        {

            var faceLines = new List<Line>();

            for (int i = 0; i < M._countF(); i++)
            {
                int[] fv = M._fv(i, false);

                for (int j = 0; j < fv.Length; j++)
                {
                    faceLines.Add(new Line(M.Vertices[fv[j]], M.Vertices[fv.Next(j)]));
                }

            }

            return faceLines;

        }

        public static Line[][] _EFLines(this Mesh M)
        {

            var faceLines = new Line[M._countF()][];

            for (int i = 0; i < M._countF(); i++)
            {
                int[] fv = M._fv(i, false);
                faceLines[i] = new Line[fv.Length];

                for (int j = 0; j < fv.Length; j++)
                {
                    faceLines[i][j] = new Line(M.Vertices[fv[j]], M.Vertices[fv.Next(j)]);
                }

            }

            return faceLines;

        }


        /// <summary>
        /// Get outlines of faces edges, where the end vertex is the center of ngon
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        public static List<Polyline> _EFPolylinesAll(this Mesh M)
        {

            var facePolylines = new List<Polyline>();

            for (int i = 0; i < M._countF(); i++)
            {
                int[] fv = M._fv(i, false);
                Point3d c = M._FCenter(i);
                for (int j = 0; j < fv.Length; j++)
                {
                    facePolylines.Add(new Polyline() { M.Vertices[fv[j]], M.Vertices[fv.Next(j)], c, M.Vertices[fv[j]] });
                }

            }

            return facePolylines;
        }




        /// <summary>
        /// Convert mesh vertex ID array to mesh topological vertex ID array
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vertexIDs"></param>
        /// <returns></returns>
        public static int[] _VtoVTopo(this Mesh mesh, uint[] vertexIDs)
        {
            int[] topoVertices = new int[vertexIDs.Length];

            for (int i = 0; i < vertexIDs.Length; i++)
                topoVertices[i] = mesh.TopologyVertices.TopologyVertexIndex((int)vertexIDs[i]);

            return topoVertices;
        }




        /// <summary>
        /// Get NGons as Polylines from Mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Polyline[] _FPolylines(this Mesh M)
        {
            Polyline[] polylines = new Polyline[M.Ngons.Count];


            for (int i = 0; i < M.Ngons.Count; i++)
            {
                uint[] boundaries = M.Ngons[i].BoundaryVertexIndexList();

                Polyline p = new Polyline();

                for (int j = 0; j < boundaries.Length; j++)
                    p.Add(M.Vertices[(int)boundaries[j]]);

                p.Add(M.Vertices[(int)boundaries[0]]);
                polylines[i] = p;
            }

            return polylines;
        }

        /// <summary>
        /// Get NGons edges as Lines from Mesh Face
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Line[] _flines(this Mesh M, int i)
        {


            uint[] boundaries = M.Ngons[i].BoundaryVertexIndexList();
            Line[] lines = new Line[boundaries.Length];


            for (int j = 0; j < boundaries.Length; j++)
            {
                Point3d p0 = M.Vertices[(int)boundaries[j]];
                Point3d p1 = M.Vertices[(int)boundaries[(j + 1) % boundaries.Length]];
                lines[j] = new Line(p0, p1);
            }

            return lines;
        }

        public static MeshProps _Planes(this Mesh M)
        {
            return new MeshProps(M);
        }

    }


    public class MeshProps


    {

        public Mesh M = new Mesh();

        //Planes
        public Plane[] fPl = new Plane[0];


        public Plane[][] fePl = new Plane[0][];
        public Plane[][] fePl90 = new Plane[0][];//90 corresponds to face plane orientation
        public Plane[][] feOffset = new Plane[0][];

        public Plane[] ePl = new Plane[0];
        public Plane[] ePl90 = new Plane[0];//90 corresponds to face plane orientation
        public Plane[] ePlOffset = new Plane[0];

        public Tuple<int, int>[][] efe = new Tuple<int, int>[0][];//edge face localE

        // Nexors Edge properties
        public Line[][] EFLines;
        public Dictionary<int, int> edgeColors;
        public int[][] FEFlatten;
        public List<int[]> _FlattenFE;



        public MeshProps(Mesh M)
        {

            this.M = M.DuplicateMesh();
            //Rhino.RhinoApp.WriteLine(this.M.Vertices.ToString());
            
            //Planes
            fPl = M._FPlanes();
            fePl = new Plane[M._countF()][];
            fePl90 = new Plane[M._countF()][];

            for (int i = 0; i < M._countF(); i++)
            {

                Line[] lines = M._flines(i);
                fePl[i] = new Plane[lines.Length];
                fePl90[i] = new Plane[lines.Length];

                Point3d p = fPl[i].Origin;

                for (int j = 0; j < lines.Length; j++)
                {
                    Point3d origin = lines[j].Center();
                    Vector3d xAxis = lines[j].Direction;
                    Vector3d yAxis = fPl[i].ZAxis;

                    Vector3d yAxis90 = Vector3d.CrossProduct(yAxis, xAxis);

                    //if (i == 10)
                    //{
                    if ((origin + yAxis90 * 0.1).DistanceToSquared(p) > (origin - yAxis90 * 0.1).DistanceToSquared(p))
                    {
                        //(origin + yAxis90 * 0.1).Bake();
                        //(origin - yAxis90 * 0.1).Bake();
                        //(p ).Bake();

                        xAxis.Reverse();
                        //Line line = new Line(origin, origin + xAxis * 0.1);
                        //line.Bake();
                        //(origin + xAxis * 0.1).Bake();
                        //(origin ).Bake();
                        yAxis90 = Vector3d.CrossProduct(yAxis, xAxis);
                        //yAxis.Reverse();
                    }
                    //}


                    fePl[i][j] = new Plane(origin, xAxis, yAxis);
                    fePl90[i][j] = new Plane(origin, xAxis, yAxis90);


                    //if (i == 10)
                    //{
                    //    p.Bake();
                    //    fePl[i][j].MovePlanebyAxis(0.1).Bake();
                    //}



                }

            }

            efe = M._EFE();
            ePl = new Plane[efe.Length];
            ePl90 = new Plane[efe.Length];

            for (int i = 0; i < efe.Length; i++)
            {


                if (efe[i].Length == 1)
                {
                    int f0 = efe[i][0].Item1;
                    int e0 = efe[i][0].Item2;
                    ePl[i] = fePl[f0][e0];
                    ePl90[i] = fePl90[f0][e0];
                }

                else if (efe[i].Length == 2)
                {


                    int f0 = efe[i][0].Item1;
                    int e0 = efe[i][0].Item2;
                    Plane p0 = fePl90[f0][e0];

                    int f1 = efe[i][1].Item1;
                    int e1 = efe[i][1].Item2;
                    Plane p1 = fePl90[f1][e1];

                    Point3d origin = p0.Origin;
                    Vector3d xAxis = p0.XAxis;
                    Vector3d zAxis = (p0.ZAxis + p1.ZAxis).UnitVector();
                    Vector3d yAxis = Vector3d.CrossProduct(zAxis, xAxis);

                    ePl[i] = new Plane(origin, xAxis, zAxis);
                    ePl90[i] = new Plane(origin, xAxis, yAxis);


                    //Reassign values of planes to match bisectors
                    //fePl90[f0][e0] = ePl90[i];
                    //fePl90[f1][e1] = ePl90[i];

                    //fePl[f0][e0] = ePl[i];
                    //fePl[f1][e1] = ePl[i];

                }
            }


            //Nexor Edge properties
            this.EFLines = M._EFLines();
            this.edgeColors = Algo.Graph._GraphColorHalfEdgesDict(M);
            this.FEFlatten = M._FEFlatten();
            this._FlattenFE = M._FlattenFE();



        }

        public Line[][] NexorTranslateLines(ref NGonsCore.Nexorades.Nexors nexors, double[][] D_, bool scale = false)
        {
            return NexorTranslateLines(this.EFLines, ref nexors, this.M, this.edgeColors, this.FEFlatten, D_, scale);
        }


        public Line[][] NexorTranslateLines(Line[][] EFLines, ref NGonsCore.Nexorades.Nexors nexors, Mesh M, Dictionary<int, int> edgeColors, int[][] FEFlatten, double[][] D_, bool scale = false)
        {

            double[][] D = new double[EFLines.Length][];

            Line[][] EFLinesCopy = new Line[EFLines.Length][];

            for (int i = 0; i < EFLines.Length; i++)
            {
                EFLinesCopy[i] = new Line[EFLines[i].Length];
                for(int j = 0; j < EFLines[i].Length; j++)
                    EFLinesCopy[i][j] = EFLines[i][j];
            }


            D = D_;

            //Translate lines
            for (int i = 0; i < M._countF(); i++)
            {

                Point3d c = M._FCenter(i);

                for (int j = 0; j < FEFlatten[i].Length; j++)
                {

                    Vector3d v0 = EFLinesCopy[i].next(j).Direction.UnitVector();
                    Vector3d v1 = -EFLinesCopy[i].prev(j).Direction.UnitVector();
                    Vector3d v = (v0 + v1) * 0.5;


                    int id = FEFlatten[i][j];

                    if (edgeColors[id] == 0)
                    {
                        if (scale)
                            EFLinesCopy[i][j].Transform(Transform.Scale(c, Math.Abs(D[i][j])));
                        else
                            EFLinesCopy[i][j].Transform(Transform.Translation(v * D[i][j]));

                    }


                }

            }

            
            for (int i = 0; i < M._countF(); i++)    {

                for (int j = 0; j < M._countE(i); j++) {

                    int id = FEFlatten[i][j];

                    int[] op = M._OppositeFE(i, (j), -1);
                    int[] on = M._OppositeFE(i, (j), 1);

                    if (edgeColors[id] == 0)
                    {

                        Line line = EFLinesCopy[i][j];

                        //Next line to interesect
                        int[] no = M._OppositeFE(i, (j + 1).Wrap(M._countE(i)));
                        int[] pairNO = (no[0] != -1) ? new int[] { no[0], no[1] } : new int[] { i, (j + 1).Wrap(EFLinesCopy[i].Length) };
                        Line lineNO = EFLinesCopy[pairNO[0]][pairNO[1]];
                        double[] tn;
                        Line cpNext = line.LineLineCP(lineNO, out tn);
                        EFLinesCopy[i][j] = new Line(EFLinesCopy[i][j].From, cpNext.From);


                        //Prev line to intersect
                        int[] po = M._OppositeFE(i, (j - 1).Wrap(M._countE(i)));
                        int[] pairPO = (po[0] != -1) ? new int[] { po[0], po[1] } : new int[] { i, (j - 1).Wrap(EFLinesCopy[i].Length) };
                        Line linePO = EFLinesCopy[pairPO[0]][pairPO[1]];
                        double[] tp;
                        Line cpPrev = line.LineLineCP(linePO, out tp);
                        EFLinesCopy[i][j] = new Line(cpPrev.From, EFLinesCopy[i][j].To);


                        

                        //Nexor properties
                        nexors[i,j].line = EFLinesCopy[i][j];
                        nexors[i,j].isNexor = 1;
                        nexors[pairNO[0],pairNO[1]].isNexor = (M._OppositeFE(pairNO[0], pairNO[1])[0]==-1) ?-1 : 1;//enable naked edges
                        nexors[pairPO[0],pairPO[1]].isNexor = (M._OppositeFE(pairPO[0], pairPO[1])[0] == -1) ? -1 : 1;//enable naked edges
                        nexors[i,j].AddEcc(FEFlatten[pairNO[0]][pairNO[1]], tn[0], tn[1], cpNext);
                        nexors[i,j].AddEcc(FEFlatten[pairPO[0]][pairPO[1]], tp[0], tp[1], cpPrev);

                        //End nexors
                        nexors[i, j].adjE0 = pairNO;
                        nexors[i, j].adjE1 = pairPO;
                        //Side nexors

                        nexors[i, j].adjE0S = op;//(op[0] == -1)  ? new int[] { i, (j - 1).Wrap(EFLinesCopy[i].Length) } : 
                        nexors[i, j].adjE1S =  on;//(on[0] == -1) ?  new int[] { i, (j + 1).Wrap(EFLinesCopy[i].Length) } :

                    }   else if (op[0] == -1)
                    {
                        nexors[i, j].isNexor = -1;
                        nexors[i, j].adjE0S = new int[] { i, (j + 1).Wrap(EFLinesCopy[i].Length)};
                        nexors[i, j].adjE1S = new int[] { i, (j - 1).Wrap(EFLinesCopy[i].Length)};
                    }    
        }

    }
  

            return EFLinesCopy;
        }





        public void OffsetPlanes_fePl(double d, bool rotated = false)
        {
            feOffset = new Plane[this.fePl.Length][];

            for (int i = 0; i < this.fePl.Length; i++)
            {
                feOffset[i] = new Plane[this.fePl[i].Length];
                for (int j = 0; j < this.fePl[i].Length; j++)
                    feOffset[i][j] = (rotated) ? this.fePl[i][j].MovePlanebyAxis(d) : this.fePl90[i][j].MovePlanebyAxis(d);
            }

        }

        public void OffsetPlanes_ePl(double d, bool rotated = false)
        {
            ePlOffset = new Plane[this.ePl.Length];

            for (int i = 0; i < this.ePl.Length; i++)
            {
                for (int j = 0; j < this.fePl[i].Length; j++)
                    ePlOffset[i] = (rotated) ? this.ePl[i].MovePlanebyAxis(d) : this.ePl90[i].MovePlanebyAxis(d);
            }

        }




    }

}
