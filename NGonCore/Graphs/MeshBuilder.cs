//using PolyMesh_Core.Geometry;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore.Graphs {
    public class MeshBuilder {

        private List<Polyline> Polylines;

        private List<int> Thickness;

        private List<int> FacesCounts;

        private SortedList<GraphEdge, int> SrtThick;

        private SortedList<GraphEdge, int> SrtFaces;

        private SortedList<GraphEdge, Circle[]> SrtCircles;

        private bool[] WrongVertex;

        private SortedList<Tuple<int, int, int>, Point3d[]> SrtPoints;

        private SortedList<int, Point3d> SrtEnds;

        private double Proportion;

        public MeshBuilder() {
            this.Polylines = new List<Polyline>();
            this.Thickness = new List<int>();
            this.FacesCounts = new List<int>();
            this.SrtThick = new SortedList<GraphEdge, int>();
            this.SrtFaces = new SortedList<GraphEdge, int>();
            this.SrtCircles = new SortedList<GraphEdge, Circle[]>();
            this.SrtPoints = new SortedList<Tuple<int, int, int>, Point3d[]>();
            this.SrtEnds = new SortedList<int, Point3d>();
            this.Proportion = 0.1;
        }
        /*
                private PolyMesh BuildEndStrut(GraphEdge Edge) {
                    PolyMesh polyMesh = new PolyMesh();
                    Circle[] item = this.SrtCircles[Edge];
                    int num = this.SrtThick[Edge];
                    if (item[0].Radius != 0) {
                        Point3d[] point3dArray = this.SrtPoints[new Tuple<int, int, int>(Edge.From, Edge.To, 0)];
                        Point3d[][] point3dArray1 = this.CreateCirclePoints(item[0], item[1], item[2], num, point3dArray);
                        polyMesh.Append(this.MeshPoints(point3dArray1[0], point3dArray1[1]));
                        polyMesh.Append(this.MeshPoints(point3dArray1[1], point3dArray1[2]));
                        Point3d point3d = this.SrtEnds[Edge.From];
                        Point3d item1 = this.SrtEnds[Edge.To];
                        Point3d[] point3dArray2 = new Point3d[checked(checked((int)point3dArray1[0].Length - 1) + 1)];
                        Line line = new Line(point3d, item1);
                        Point3d point3d1 = line.ClosestPoint(point3dArray1[2][0], false);
                        int length = checked((int)point3dArray2.Length - 1);
                        for (int i = 0; i <= length; i = checked(i + 1)) {
                            point3dArray2[i] = point3dArray1[2][i] + (item1 - point3d1);
                        }
                        polyMesh.Append(this.MeshPoints(point3dArray1[2], point3dArray2));
                    } else {
                        Point3d[] item2 = this.SrtPoints[new Tuple<int, int, int>(Edge.From, Edge.To, 1)];
                        Point3d[][] point3dArray3 = this.CreateCirclePoints(item[5], item[4], item[3], num, item2);
                        polyMesh.Append(this.MeshPoints(point3dArray3[0], point3dArray3[1]));
                        polyMesh.Append(this.MeshPoints(point3dArray3[1], point3dArray3[2]));
                        Point3d point3d2 = this.SrtEnds[Edge.From];
                        Point3d item3 = this.SrtEnds[Edge.To];
                        Point3d[] point3dArray4 = new Point3d[checked(checked((int)point3dArray3[0].Length - 1) + 1)];
                        Line line1 = new Line(point3d2, item3);
                        Point3d point3d3 = line1.ClosestPoint(point3dArray3[2][0], false);
                        int length1 = checked((int)point3dArray4.Length - 1);
                        for (int j = 0; j <= length1; j = checked(j + 1)) {
                            point3dArray4[j] = point3dArray3[2][j] + (point3d2 - point3d3);
                        }
                        polyMesh.Append(this.MeshPoints(point3dArray3[2], point3dArray4));
                    }
                    return polyMesh;
                }
                */

        private Mesh BuildEndStrut(GraphEdge Edge) {
            Mesh polyMesh = new Mesh();
            Circle[] item = this.SrtCircles[Edge];
            int num = this.SrtThick[Edge];
            if (item[0].Radius != 0) {
                Point3d[] point3dArray = this.SrtPoints[new Tuple<int, int, int>(Edge.From, Edge.To, 0)];
                Point3d[][] point3dArray1 = this.CreateCirclePoints(item[0], item[1], item[2], num, point3dArray);
                polyMesh.Append(this.MeshPoints(point3dArray1[0], point3dArray1[1]));
                polyMesh.Append(this.MeshPoints(point3dArray1[1], point3dArray1[2]));
                Point3d point3d = this.SrtEnds[Edge.From];
                Point3d item1 = this.SrtEnds[Edge.To];
                Point3d[] point3dArray2 = new Point3d[checked(checked((int)point3dArray1[0].Length - 1) + 1)];
                Line line = new Line(point3d, item1);
                Point3d point3d1 = line.ClosestPoint(point3dArray1[2][0], false);
                int length = checked((int)point3dArray2.Length - 1);
                for (int i = 0; i <= length; i = checked(i + 1)) {
                    point3dArray2[i] = point3dArray1[2][i] + (item1 - point3d1);
                }
                polyMesh.Append(this.MeshPoints(point3dArray1[2], point3dArray2));
            } else {
                Point3d[] item2 = this.SrtPoints[new Tuple<int, int, int>(Edge.From, Edge.To, 1)];
                Point3d[][] point3dArray3 = this.CreateCirclePoints(item[5], item[4], item[3], num, item2);
                polyMesh.Append(this.MeshPoints(point3dArray3[0], point3dArray3[1]));
                polyMesh.Append(this.MeshPoints(point3dArray3[1], point3dArray3[2]));
                Point3d point3d2 = this.SrtEnds[Edge.From];
                Point3d item3 = this.SrtEnds[Edge.To];
                Point3d[] point3dArray4 = new Point3d[checked(checked((int)point3dArray3[0].Length - 1) + 1)];
                Line line1 = new Line(point3d2, item3);
                Point3d point3d3 = line1.ClosestPoint(point3dArray3[2][0], false);
                int length1 = checked((int)point3dArray4.Length - 1);
                for (int j = 0; j <= length1; j = checked(j + 1)) {
                    point3dArray4[j] = point3dArray3[2][j] + (point3d2 - point3d3);
                }
                polyMesh.Append(this.MeshPoints(point3dArray3[2], point3dArray4));
            }
            return polyMesh;
        }

        private SimpleGraph<Point3d> BuildGraph(List<Polyline> Polylines) {
            List<Polyline>.Enumerator enumerator = new List<Polyline>.Enumerator();
            SimpleGraph<Point3d> simpleGraph = new SimpleGraph<Point3d>(null);
            PointCloud pointCloud = new PointCloud();
            pointCloud.Add(Polylines[0][0]);
            simpleGraph.AddVertex(Polylines[0][0]);
            int num = 0;
            try {
                enumerator = Polylines.GetEnumerator();
                while (enumerator.MoveNext()) {
                    Polyline current = enumerator.Current;
                    GraphEdge count = new GraphEdge();
                    int num1 = pointCloud.ClosestPoint(current[0]);
                    Point3d location = pointCloud[num1].Location;
                    double num2 = location.DistanceTo(current[0]);
                    count.From = num1;
                    if (num2 > Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) {
                        pointCloud.Add(current[0]);
                        simpleGraph.AddVertex(current[0]);
                        count.From = checked(simpleGraph.Count - 1);
                    }
                    num1 = pointCloud.ClosestPoint(current[checked(current.Count - 1)]);
                    count.To = num1;
                    if (pointCloud[num1].Location.DistanceTo(current[checked(current.Count - 1)]) > Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance) {
                        pointCloud.Add(current[checked(current.Count - 1)]);
                        simpleGraph.AddVertex(current[checked(current.Count - 1)]);
                        count.To = checked(simpleGraph.Count - 1);
                    }
                    count.Orient();
                    this.SrtThick[count] = this.Thickness[num];
                    this.SrtFaces[count] = this.FacesCounts[num];
                    simpleGraph.Edges.Add(count);
                    num = checked(num + 1);
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
            return simpleGraph;
        }

        //public PolyMesh BuildMesh(List<Polyline> Polylines, List<int> Strips, List<int> Faces, double Proportion) {
        //    int count;
        //    List<int>.Enumerator enumerator = new List<int>.Enumerator();
        //    HashSet<GraphEdge>.Enumerator enumerator1 = new HashSet<GraphEdge>.Enumerator();
        //    PolyMesh polyMesh = new PolyMesh();
        //    this.Polylines.Clear();
        //    this.Thickness.Clear();
        //    this.FacesCounts.Clear();
        //    this.SrtThick.Clear();
        //    this.SrtFaces.Clear();
        //    this.SrtCircles.Clear();
        //    this.SrtPoints.Clear();
        //    this.SrtEnds.Clear();
        //    this.Proportion = Proportion;
        //    this.Polylines.AddRange(Polylines);
        //    this.FacesCounts.AddRange(Faces);
        //    this.Thickness.AddRange(Strips);
        //    int num = checked(this.FacesCounts.Count - 2);
        //    for (int i = 1; i <= num; i = checked(i + 1)) {
        //        List<int> facesCounts = this.FacesCounts;
        //        List<int> nums = facesCounts;
        //        int num1 = i;
        //        count = num1;
        //        facesCounts[num1] = checked(nums[count] - 6);
        //    }
        //    SimpleGraph<Point3d> simpleGraph = this.BuildGraph(Polylines);
        //    this.WrongVertex = new bool[checked(checked(simpleGraph.Count - 1) + 1)];
        //    List<int>[] connectivityMap = simpleGraph.GetConnectivityMap();
        //    count = checked(this.SrtThick.Count - 1);
        //    for (int j = 0; j <= count; j = checked(j + 1)) {
        //        Circle[] circleArray = new Circle[6];
        //        this.SrtCircles[this.SrtThick.Keys[j]] = circleArray;
        //    }
        //    int count1 = checked(simpleGraph.Count - 1);
        //    for (int k = 0; k <= count1; k = checked(k + 1)) {
        //        List<int> vertex = connectivityMap[k];
        //        if (vertex.Count == 1) {
        //            this.SrtEnds[k] = simpleGraph[k];
        //            this.SrtEnds[vertex[0]] = simpleGraph[vertex[0]];
        //        } else if (vertex.Count == 3) {
        //            List<Point3d> point3ds = new List<Point3d>();
        //            List<Vector3d> vector3ds = new List<Vector3d>();
        //            List<int> nums1 = new List<int>();
        //            try {
        //                enumerator = vertex.GetEnumerator();
        //                while (enumerator.MoveNext()) {
        //                    int current = enumerator.Current;
        //                    Point3d point3d = simpleGraph[current];
        //                    point3d.Transform(Transform.Scale(simpleGraph[k], Proportion));
        //                    vector3ds.Add(simpleGraph[k] - point3d);
        //                    point3ds.Add(point3d);
        //                    GraphEdge graphEdge = new GraphEdge(current, k);
        //                    graphEdge.Orient();
        //                    nums1.Add(this.SrtThick[graphEdge]);
        //                }
        //            } finally {
        //                ((IDisposable)enumerator).Dispose();
        //            }
        //            List<Circle> circles = this.ConstructTangentCircles(point3ds, PointUtil.InscribedCircle(point3ds));
        //            List<Circle> circles1 = this.SecondaryCircles(point3ds, vector3ds, circles);
        //            List<Circle> circles2 = this.TertiaryCircles(point3ds, simpleGraph[k], circles);
        //            int count2 = checked(circles.Count - 1);
        //            for (int l = 0; l <= count2; l = checked(l + 1)) {
        //                GraphEdge item = new GraphEdge(k, vertex[l]);
        //                item.Orient();
        //                if (k != item.From) {
        //                    this.SrtCircles[item][5] = circles[l];
        //                    this.SrtCircles[item][4] = circles1[l];
        //                    this.SrtCircles[item][3] = circles2[l];
        //                } else {
        //                    this.SrtCircles[item][0] = circles[l];
        //                    this.SrtCircles[item][1] = circles1[l];
        //                    this.SrtCircles[item][2] = circles2[l];
        //                }
        //            }
        //            int[] numArray = null;
        //            List<Circle> circles3 = this.OrientPrimary(circles, nums1, ref numArray);
        //            List<int> nums2 = new List<int>();
        //            int length = checked((int)numArray.Length - 1);
        //            for (int m = 0; m <= length; m = checked(m + 1)) {
        //                nums2.Add(nums1[numArray[m]]);
        //                int item1 = vertex[numArray[m]];
        //                GraphEdge graphEdge1 = new GraphEdge(item1, k);
        //                graphEdge1.Orient();
        //                if (item1 != graphEdge1.From) {
        //                    this.SrtCircles[graphEdge1][0] = circles3[m];
        //                } else {
        //                    this.SrtCircles[graphEdge1][5] = circles3[m];
        //                }
        //            }
        //            List<Point3d> point3ds1 = null;
        //            List<Point3d> point3ds2 = null;
        //            List<Point3d> point3ds3 = null;
        //            PolyMesh polyMesh1 = this.MeshConnector(circles3, nums2, ref point3ds1, ref point3ds2, ref point3ds3);
        //            int length1 = checked((int)numArray.Length - 1);
        //            for (int n = 0; n <= length1; n = checked(n + 1)) {
        //                int item2 = vertex[numArray[n]];
        //                GraphEdge array = new GraphEdge(item2, k);
        //                array.Orient();
        //                if (item2 != array.From) {
        //                    switch (n) {
        //                        case 0: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds3.ToArray();
        //                                break;
        //                            }
        //                        case 1: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds2.ToArray();
        //                                break;
        //                            }
        //                        case 2: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds1.ToArray();
        //                                break;
        //                            }
        //                    }
        //                } else {
        //                    switch (n) {
        //                        case 0: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds3.ToArray();
        //                                break;
        //                            }
        //                        case 1: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds2.ToArray();
        //                                break;
        //                            }
        //                        case 2: {
        //                                this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds1.ToArray();
        //                                break;
        //                            }
        //                    }
        //                }
        //            }
        //            polyMesh.Append(polyMesh1);
        //        }
        //    }
        //    try {
        //        enumerator1 = simpleGraph.Edges.GetEnumerator();
        //        while (enumerator1.MoveNext()) {
        //            GraphEdge current1 = enumerator1.Current;
        //            if (!(connectivityMap[current1.From].Count == 3 & connectivityMap[current1.To].Count == 3)) {
        //                if (!(connectivityMap[current1.From].Count == 1 & connectivityMap[current1.To].Count == 3 | connectivityMap[current1.From].Count == 3 & connectivityMap[current1.To].Count == 1)) {
        //                    continue;
        //                }
        //                polyMesh.Append(this.BuildEndStrut(current1));
        //            } else {
        //                polyMesh.Append(this.BuildStrut(current1));
        //            }
        //        }
        //    } finally {
        //        ((IDisposable)enumerator1).Dispose();
        //    }
        //    polyMesh.Weld(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 123);
        //    polyMesh.UnifyFaceNormals();
        //    return polyMesh;
        //}

        public Mesh BuildMesh(List<Polyline> Polylines, List<int> Strips, List<int> Faces, double Proportion) {
            int count;
            List<int>.Enumerator enumerator = new List<int>.Enumerator();
            HashSet<GraphEdge>.Enumerator enumerator1 = new HashSet<GraphEdge>.Enumerator();
            Mesh polyMesh = new Mesh();
            this.Polylines.Clear();
            this.Thickness.Clear();
            this.FacesCounts.Clear();
            this.SrtThick.Clear();
            this.SrtFaces.Clear();
            this.SrtCircles.Clear();
            this.SrtPoints.Clear();
            this.SrtEnds.Clear();
            this.Proportion = Proportion;
            this.Polylines.AddRange(Polylines);
            this.FacesCounts.AddRange(Faces);
            this.Thickness.AddRange(Strips);
            int num = checked(this.FacesCounts.Count - 2);
            for (int i = 1; i <= num; i = checked(i + 1)) {
                List<int> facesCounts = this.FacesCounts;
                List<int> nums = facesCounts;
                int num1 = i;
                count = num1;
                facesCounts[num1] = checked(nums[count] - 6);
            }
            SimpleGraph<Point3d> simpleGraph = this.BuildGraph(Polylines);
            this.WrongVertex = new bool[checked(checked(simpleGraph.Count - 1) + 1)];
            List<int>[] connectivityMap = simpleGraph.GetConnectivityMap();
            count = checked(this.SrtThick.Count - 1);
            for (int j = 0; j <= count; j = checked(j + 1)) {
                Circle[] circleArray = new Circle[6];
                this.SrtCircles[this.SrtThick.Keys[j]] = circleArray;
            }
            int count1 = checked(simpleGraph.Count - 1);
            for (int k = 0; k <= count1; k = checked(k + 1)) {
                List<int> vertex = connectivityMap[k];
                if (vertex.Count == 1) {
                    this.SrtEnds[k] = simpleGraph[k];
                    this.SrtEnds[vertex[0]] = simpleGraph[vertex[0]];
                } else if (vertex.Count == 3) {
                    List<Point3d> point3ds = new List<Point3d>();
                    List<Vector3d> vector3ds = new List<Vector3d>();
                    List<int> nums1 = new List<int>();
                    try {
                        enumerator = vertex.GetEnumerator();
                        while (enumerator.MoveNext()) {
                            int current = enumerator.Current;
                            Point3d point3d = simpleGraph[current];
                            point3d.Transform(Transform.Scale(simpleGraph[k], Proportion));
                            vector3ds.Add(simpleGraph[k] - point3d);
                            point3ds.Add(point3d);
                            GraphEdge graphEdge = new GraphEdge(current, k);
                            graphEdge.Orient();
                            nums1.Add(this.SrtThick[graphEdge]);
                        }
                    } finally {
                        ((IDisposable)enumerator).Dispose();
                    }
                    List<Circle> circles = this.ConstructTangentCircles(point3ds, PointUtil.InscribedCircle(point3ds));
                    List<Circle> circles1 = this.SecondaryCircles(point3ds, vector3ds, circles);
                    List<Circle> circles2 = this.TertiaryCircles(point3ds, simpleGraph[k], circles);
                    int count2 = checked(circles.Count - 1);
                    for (int l = 0; l <= count2; l = checked(l + 1)) {
                        GraphEdge item = new GraphEdge(k, vertex[l]);
                        item.Orient();
                        if (k != item.From) {
                            this.SrtCircles[item][5] = circles[l];
                            this.SrtCircles[item][4] = circles1[l];
                            this.SrtCircles[item][3] = circles2[l];
                        } else {
                            this.SrtCircles[item][0] = circles[l];
                            this.SrtCircles[item][1] = circles1[l];
                            this.SrtCircles[item][2] = circles2[l];
                        }
                    }
                    int[] numArray = null;
                    List<Circle> circles3 = this.OrientPrimary(circles, nums1, ref numArray);
                    List<int> nums2 = new List<int>();
                    int length = checked((int)numArray.Length - 1);
                    for (int m = 0; m <= length; m = checked(m + 1)) {
                        nums2.Add(nums1[numArray[m]]);
                        int item1 = vertex[numArray[m]];
                        GraphEdge graphEdge1 = new GraphEdge(item1, k);
                        graphEdge1.Orient();
                        if (item1 != graphEdge1.From) {
                            this.SrtCircles[graphEdge1][0] = circles3[m];
                        } else {
                            this.SrtCircles[graphEdge1][5] = circles3[m];
                        }
                    }
                    List<Point3d> point3ds1 = null;
                    List<Point3d> point3ds2 = null;
                    List<Point3d> point3ds3 = null;
                    Mesh polyMesh1 = this.MeshConnector(circles3, nums2, ref point3ds1, ref point3ds2, ref point3ds3);
                    int length1 = checked((int)numArray.Length - 1);
                    for (int n = 0; n <= length1; n = checked(n + 1)) {
                        int item2 = vertex[numArray[n]];
                        GraphEdge array = new GraphEdge(item2, k);
                        array.Orient();
                        if (item2 != array.From) {
                            switch (n) {
                                case 0: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds3.ToArray();
                                        break;
                                    }
                                case 1: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds2.ToArray();
                                        break;
                                    }
                                case 2: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 0)] = point3ds1.ToArray();
                                        break;
                                    }
                            }
                        } else {
                            switch (n) {
                                case 0: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds3.ToArray();
                                        break;
                                    }
                                case 1: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds2.ToArray();
                                        break;
                                    }
                                case 2: {
                                        this.SrtPoints[new Tuple<int, int, int>(array.From, array.To, 1)] = point3ds1.ToArray();
                                        break;
                                    }
                            }
                        }
                    }
                    polyMesh.Append(polyMesh1);
                }
            }
            try {
                enumerator1 = simpleGraph.Edges.GetEnumerator();
                while (enumerator1.MoveNext()) {
                    GraphEdge current1 = enumerator1.Current;
                    if (!(connectivityMap[current1.From].Count == 3 & connectivityMap[current1.To].Count == 3)) {
                        if (!(connectivityMap[current1.From].Count == 1 & connectivityMap[current1.To].Count == 3 | connectivityMap[current1.From].Count == 3 & connectivityMap[current1.To].Count == 1)) {
                            continue;
                        }
                        polyMesh.Append(this.BuildEndStrut(current1));
                    } else {
                        polyMesh.Append(this.BuildStrut(current1));
                    }
                }
            } finally {
                ((IDisposable)enumerator1).Dispose();
            }

            polyMesh.Clean();
            //polyMesh.Weld(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, 123);
            //polyMesh.UnifyFaceNormals();
            return polyMesh;
        }


        //private PolyMesh BuildStrut(GraphEdge Edge) {
        //    Circle[] item = this.SrtCircles[Edge];
        //    int num = this.SrtThick[Edge];
        //    Tuple<int, int, int> tuple = new Tuple<int, int, int>(Edge.From, Edge.To, 0);
        //    Tuple<int, int, int> tuple1 = new Tuple<int, int, int>(Edge.From, Edge.To, 1);
        //    Point3d[][] point3dArray = this.CreateCirclePoints(item[0], item[1], item[2], num, this.SrtPoints[tuple]);
        //    Point3d[][] point3dArray1 = this.CreateCirclePoints(item[5], item[4], item[3], num, this.SrtPoints[tuple1]);
        //    PolyMesh polyMesh = new PolyMesh();
        //    polyMesh.Append(this.MeshPoints(point3dArray[0], point3dArray[1]));
        //    polyMesh.Append(this.MeshPoints(point3dArray[1], point3dArray[2]));
        //    polyMesh.Append(this.MeshPoints(point3dArray1[0], point3dArray1[1]));
        //    polyMesh.Append(this.MeshPoints(point3dArray1[1], point3dArray1[2]));
        //    Point3d[][] point3dArray2 = PointUtil.InterpolatePointArrays(point3dArray[2], this.FixOrientation(point3dArray[2], point3dArray1[2]), this.SrtFaces[Edge]);
        //    int length = checked((int)point3dArray2.Length - 2);
        //    for (int i = 0; i <= length; i = checked(i + 1)) {
        //        polyMesh.Append(this.MeshPoints(point3dArray2[i], point3dArray2[checked(i + 1)]));
        //    }
        //    return polyMesh;
        //}

        private Mesh BuildStrut(GraphEdge Edge) {
            Circle[] item = this.SrtCircles[Edge];
            int num = this.SrtThick[Edge];
            Tuple<int, int, int> tuple = new Tuple<int, int, int>(Edge.From, Edge.To, 0);
            Tuple<int, int, int> tuple1 = new Tuple<int, int, int>(Edge.From, Edge.To, 1);
            Point3d[][] point3dArray = this.CreateCirclePoints(item[0], item[1], item[2], num, this.SrtPoints[tuple]);
            Point3d[][] point3dArray1 = this.CreateCirclePoints(item[5], item[4], item[3], num, this.SrtPoints[tuple1]);
            Mesh polyMesh = new Mesh();
            polyMesh.Append(this.MeshPoints(point3dArray[0], point3dArray[1]));
            polyMesh.Append(this.MeshPoints(point3dArray[1], point3dArray[2]));
            polyMesh.Append(this.MeshPoints(point3dArray1[0], point3dArray1[1]));
            polyMesh.Append(this.MeshPoints(point3dArray1[1], point3dArray1[2]));
            Point3d[][] point3dArray2 = PointUtil.InterpolatePointArrays(point3dArray[2], this.FixOrientation(point3dArray[2], point3dArray1[2]), this.SrtFaces[Edge]);
            int length = checked((int)point3dArray2.Length - 2);
            for (int i = 0; i <= length; i = checked(i + 1)) {
                polyMesh.Append(this.MeshPoints(point3dArray2[i], point3dArray2[checked(i + 1)]));
            }
            return polyMesh;
        }

        private List<Circle> ConstructTangentCircles(List<Point3d> Triangle, Circle InCircle) {
            List<Circle> circles = new List<Circle>();
            int count = checked(Triangle.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                double num = Vector3d.VectorAngle(Triangle[(checked(checked(i + 1) + 3)) % 3] - Triangle[i], Triangle[(checked(checked(i - 1) + 3)) % 3] - Triangle[i]) / 2;
                double radius = InCircle.Radius;
                Point3d center = InCircle.Center;
                double num1 = center.DistanceTo(Triangle[i]);
                Vector3d vector3d = InCircle.Center - Triangle[i];
                vector3d.Unitize();
                vector3d = vector3d * (num1 - InCircle.Radius);
                Point3d item = Triangle[i] + vector3d;
                double num2 = Math.Tan(num) * (num1 - radius);
                circles.Add(new Circle(new Plane(item, vector3d), num2));
            }
            return circles;
        }

        private Point3d[][] CreateCirclePoints(Circle CGuide, Circle CMiddle, Circle CLast, int Strings, Point3d[] GuidePoints) {
            Plane plane;
            Circle[] cGuide = new Circle[] { CGuide, CMiddle, CLast };
            Point3d[][] guidePoints = new Point3d[3][];
            int num = 0;
            do {
                Point3d[] point3dArray = new Point3d[checked(checked(Strings - 1) + 1)];
                guidePoints[num] = point3dArray;
                num = checked(num + 1);
            }
            while (num <= 2);
            guidePoints[0] = GuidePoints;
            int num1 = 0;
            do {
                int num2 = num1;
                int num3 = checked(num1 + 1);
                Point3d[] point3dArray1 = guidePoints[num2];
                Circle circle = cGuide[num2];
                Circle circle1 = cGuide[num3];
                Transform transform = Transform.PlaneToPlane(circle.Plane, circle1.Plane);
                Transform transform1 = Transform.Scale(circle1.Center, circle1.Radius / circle.Radius);
                double[] numArray = new double[checked(checked(checked(Strings * 2) - 1) + 1)];
                int[] numArray1 = new int[checked(checked(checked(Strings * 2) - 1) + 1)];
                int strings = checked(checked(Strings * 2) - 1);
                for (int i = 0; i <= strings; i = checked(i + 1)) {
                    plane = circle1.Plane;
                    Vector3d zAxis = plane.ZAxis;
                    plane = circle1.Plane;
                    Transform transform2 = Transform.Rotation((double)i / (double)(checked(Strings * 2)) * 3.14159265358979 * 2, zAxis, plane.Origin);
                    double num4 = 0;
                    int length = checked((int)point3dArray1.Length - 1);
                    for (int j = 0; j <= length; j = checked(j + 1)) {
                        Point3d point3d = point3dArray1[j];
                        point3d.Transform(transform);
                        point3d.Transform(transform1);
                        point3d.Transform(transform2);
                        num4 += point3d.DistanceTo(point3dArray1[j]);
                    }
                    numArray[i] = num4;
                    numArray1[i] = i;
                }
                Array.Sort<double, int>(numArray, numArray1);
                plane = circle1.Plane;
                Vector3d vector3d = plane.ZAxis;
                plane = circle1.Plane;
                Transform transform3 = Transform.Rotation((double)numArray1[0] / (double)(checked(Strings * 2)) * 3.14159265358979 * 2, vector3d, plane.Origin);
                int length1 = checked((int)point3dArray1.Length - 1);
                for (int k = 0; k <= length1; k = checked(k + 1)) {
                    Point3d point3d1 = point3dArray1[k];
                    point3d1.Transform(transform);
                    point3d1.Transform(transform1);
                    point3d1.Transform(transform3);
                    guidePoints[num3][k] = point3d1;
                }
                num1 = checked(num1 + 1);
            }
            while (num1 <= 1);
            return guidePoints;
        }



        //??
        private Point3d[] FixOrientation(Point3d[] Guide, Point3d[] ToOrient) {
            int cnt = ToOrient.Length;

            Point3d[] Arev = new Point3d[ToOrient.Length - 1 + 1];
            int idx = 0;

            for (int i = ToOrient.Length - 1; i >= 0; i += -1) {
                Arev[idx] = ToOrient[i];
                idx += 1;
            }

            double[] Asum = new double[cnt - 1 + 1];
            int[] Aind = new int[cnt - 1 + 1];

            double[] ArevSum = new double[cnt - 1 + 1];
            int[] ArevInd = new int[cnt - 1 + 1];

            for (int i = 0; i <= cnt - 1; i += 1) {
                for (int j = 0; j <= cnt - 1; j += 1) {
                    Asum[i] += ToOrient[(j + i + cnt) % cnt].DistanceTo(Guide[j]);
                    Aind[i] = i;
                }
            }

            for (int i = 0; i <= cnt - 1; i += 1) {
                for (int j = 0; j <= cnt - 1; j += 1) {
                    ArevSum[i] += Arev[(j + i + cnt) % cnt].DistanceTo(Guide[j]);
                    ArevInd[i] = i;
                }
            }

            Array.Sort(Asum, Aind);
            Array.Sort(ArevSum, ArevInd);

            Point3d[] AFinale = new Point3d[cnt - 1 + 1];

            if (Asum[0] < ArevSum[0]) {
                for (int i = 0; i <= cnt - 1; i += 1)
                    AFinale[i] = ToOrient[(i + Aind[0] + cnt) % cnt];
            } else
                for (int i = 0; i <= cnt - 1; i += 1)
                    AFinale[i] = Arev[(i + ArevInd[0] + cnt) % cnt];

            return AFinale;
        }












        //private PolyMesh MeshConnector(List<Circle> Circles, List<int> Values, ref List<Point3d> LargeCircleDivision, ref List<Point3d> MiddleCircleDivision, ref List<Point3d> SmallCircleDivision) {
        //    Circle item;
        //    int num = 0;
        //    do {
        //        Circle circle = Circles[num];
        //        circle.Reverse();
        //        Circles[num] = circle;
        //        num = checked(num + 1);
        //    }
        //    while (num <= 1);
        //    PolyMesh polyMesh = new PolyMesh();
        //    PolyMesh polyMesh1 = new PolyMesh();
        //    List<Point3d> point3ds = new List<Point3d>();
        //    List<Point3d> point3ds1 = new List<Point3d>();
        //    List<Point3d> point3ds2 = new List<Point3d>();
        //    int item1 = Values[0];
        //    for (int i = 0; i <= item1; i = checked(i + 1)) {
        //        PolyVertexList vertices = polyMesh.Vertices;
        //        item = Circles[0];
        //        vertices.Add(item.PointAt((double)i / (double)Values[0] * 3.14159265358979 * 2));
        //        if (i < Values[0]) {
        //            item = Circles[0];
        //            point3ds2.Add(item.PointAt((double)i / (double)Values[0] * 3.14159265358979 * 2));
        //        }
        //    }
        //    int num1 = Values[0];
        //    for (int j = 0; j <= num1; j = checked(j + 1)) {
        //        PolyVertexList polyVertexList = polyMesh.Vertices;
        //        item = Circles[2];
        //        polyVertexList.Add(item.PointAt((double)j / (double)Values[0] * 3.14159265358979));
        //    }
        //    int item2 = checked(Values[0] - 1);
        //    for (int k = 0; k <= item2; k = checked(k + 1)) {
        //        polyMesh.Faces.Add((IEnumerable<int>)(new int[] { k, checked(checked(k + Values[0]) + 1), checked(checked(k + Values[0]) + 2), checked(k + 1) }));
        //    }
        //    int num2 = Values[1];
        //    for (int l = 0; l <= num2; l = checked(l + 1)) {
        //        PolyVertexList vertices1 = polyMesh1.Vertices;
        //        item = Circles[1];
        //        vertices1.Add(item.PointAt((double)l / (double)Values[1] * 3.14159265358979 * 2));
        //        if (l < Values[1]) {
        //            item = Circles[1];
        //            point3ds1.Add(item.PointAt((double)l / (double)Values[1] * 3.14159265358979 * 2));
        //        }
        //    }
        //    int item3 = Values[1];
        //    for (int m = 0; m <= item3; m = checked(m + 1)) {
        //        PolyVertexList polyVertexList1 = polyMesh1.Vertices;
        //        item = Circles[2];
        //        polyVertexList1.Add(item.PointAt((double)m / (double)Values[1] * 3.14159265358979 + 3.14159265358979));
        //    }
        //    int num3 = checked(Values[0] - 1);
        //    for (int n = 0; n <= num3; n = checked(n + 1)) {
        //        item = Circles[2];
        //        point3ds.Add(item.PointAt((double)n / (double)Values[0] * 3.14159265358979));
        //    }
        //    int item4 = checked(Values[1] - 1);
        //    for (int o = 0; o <= item4; o = checked(o + 1)) {
        //        item = Circles[2];
        //        point3ds.Add(item.PointAt((double)o / (double)Values[1] * 3.14159265358979 + 3.14159265358979));
        //    }
        //    int num4 = checked(Values[1] - 1);
        //    for (int p = 0; p <= num4; p = checked(p + 1)) {
        //        polyMesh1.Faces.Add((IEnumerable<int>)(new int[] { p, checked(checked(p + Values[1]) + 1), checked(checked(p + Values[1]) + 2), checked(p + 1) }));
        //    }
        //    polyMesh.Append(polyMesh1);
        //    LargeCircleDivision = point3ds;
        //    MiddleCircleDivision = point3ds1;
        //    SmallCircleDivision = point3ds2;
        //    return polyMesh;
        //}

        private Mesh MeshConnector(List<Circle> Circles, List<int> Values, ref List<Point3d> LargeCircleDivision, ref List<Point3d> MiddleCircleDivision, ref List<Point3d> SmallCircleDivision) {
            Circle item;
            int num = 0;
            do {
                Circle circle = Circles[num];
                circle.Reverse();
                Circles[num] = circle;
                num = checked(num + 1);
            }
            while (num <= 1);
            Mesh polyMesh = new Mesh();
            Mesh polyMesh1 = new Mesh();
            List<Point3d> point3ds = new List<Point3d>();
            List<Point3d> point3ds1 = new List<Point3d>();
            List<Point3d> point3ds2 = new List<Point3d>();
            int item1 = Values[0];
            for (int i = 0; i <= item1; i = checked(i + 1)) {
                Rhino.Geometry.Collections.MeshVertexList vertices = polyMesh.Vertices;
                item = Circles[0];
                vertices.Add(item.PointAt((double)i / (double)Values[0] * 3.14159265358979 * 2));
                if (i < Values[0]) {
                    item = Circles[0];
                    point3ds2.Add(item.PointAt((double)i / (double)Values[0] * 3.14159265358979 * 2));
                }
            }
            int num1 = Values[0];
            for (int j = 0; j <= num1; j = checked(j + 1)) {
                Rhino.Geometry.Collections.MeshVertexList polyVertexList = polyMesh.Vertices;
                item = Circles[2];
                polyVertexList.Add(item.PointAt((double)j / (double)Values[0] * 3.14159265358979));
            }
            int item2 = checked(Values[0] - 1);
            for (int k = 0; k <= item2; k = checked(k + 1)) {
                polyMesh.AddFace((IEnumerable<int>)(new int[] { k, checked(checked(k + Values[0]) + 1), checked(checked(k + Values[0]) + 2), checked(k + 1) }));
            }
            int num2 = Values[1];
            for (int l = 0; l <= num2; l = checked(l + 1)) {
                Rhino.Geometry.Collections.MeshVertexList vertices1 = polyMesh1.Vertices;
                item = Circles[1];
                vertices1.Add(item.PointAt((double)l / (double)Values[1] * 3.14159265358979 * 2));
                if (l < Values[1]) {
                    item = Circles[1];
                    point3ds1.Add(item.PointAt((double)l / (double)Values[1] * 3.14159265358979 * 2));
                }
            }
            int item3 = Values[1];
            for (int m = 0; m <= item3; m = checked(m + 1)) {
                Rhino.Geometry.Collections.MeshVertexList polyVertexList1 = polyMesh1.Vertices;
                item = Circles[2];
                polyVertexList1.Add(item.PointAt((double)m / (double)Values[1] * 3.14159265358979 + 3.14159265358979));
            }
            int num3 = checked(Values[0] - 1);
            for (int n = 0; n <= num3; n = checked(n + 1)) {
                item = Circles[2];
                point3ds.Add(item.PointAt((double)n / (double)Values[0] * 3.14159265358979));
            }
            int item4 = checked(Values[1] - 1);
            for (int o = 0; o <= item4; o = checked(o + 1)) {
                item = Circles[2];
                point3ds.Add(item.PointAt((double)o / (double)Values[1] * 3.14159265358979 + 3.14159265358979));
            }
            int num4 = checked(Values[1] - 1);
            for (int p = 0; p <= num4; p = checked(p + 1)) {
                polyMesh1.AddFace((IEnumerable<int>)(new int[] { p, checked(checked(p + Values[1]) + 1), checked(checked(p + Values[1]) + 2), checked(p + 1) }));
            }
            polyMesh.Append(polyMesh1);
            LargeCircleDivision = point3ds;
            MiddleCircleDivision = point3ds1;
            SmallCircleDivision = point3ds2;
            return polyMesh;
        }

        //private PolyMesh MeshPoints(Point3d[] PointsDown, Point3d[] PointsUp) {
        //    PolyMesh polyMesh = new PolyMesh();
        //    polyMesh.Vertices.AddRange(PointsDown);
        //    polyMesh.Vertices.AddRange(PointsUp);
        //    int length = checked((int)PointsDown.Length - 1);
        //    for (int i = 0; i <= length; i = checked(i + 1)) {
        //        int num = i;
        //        int length1 = (checked(checked(i + 1) + (int)PointsDown.Length)) % (int)PointsDown.Length;
        //        int num1 = checked(i + (int)PointsDown.Length);
        //        int length2 = checked((checked(checked(num1 + 1) + (int)PointsDown.Length)) % (int)PointsDown.Length + (int)PointsDown.Length);
        //        polyMesh.Faces.Add((IEnumerable<int>)(new int[] { num, length1, length2, num1 }));
        //    }
        //    return polyMesh;
        //}

        private Mesh MeshPoints(Point3d[] PointsDown, Point3d[] PointsUp) {
            Mesh polyMesh = new Mesh();
            polyMesh.Vertices.AddVertices(PointsDown);
            polyMesh.Vertices.AddVertices(PointsUp);
            int length = checked((int)PointsDown.Length - 1);
            for (int i = 0; i <= length; i = checked(i + 1)) {
                int num = i;
                int length1 = (checked(checked(i + 1) + (int)PointsDown.Length)) % (int)PointsDown.Length;
                int num1 = checked(i + (int)PointsDown.Length);
                int length2 = checked((checked(checked(num1 + 1) + (int)PointsDown.Length)) % (int)PointsDown.Length + (int)PointsDown.Length);
                polyMesh.AddFace((IEnumerable<int>)(new int[] { num, length1, length2, num1 }));
            }
            return polyMesh;
        }

        private List<Circle> OrientPrimary(List<Circle> Circles, List<int> Values, ref int[] NewOrder) {
            int[] numArray = new int[3];
            int count = checked(Circles.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                numArray[i] = i;
            }
            Array.Sort<int, int>(Values.ToArray(), numArray);
            Circle[] item = new Circle[3];
            int length = checked((int)item.Length - 1);
            for (int j = 0; j <= length; j = checked(j + 1)) {
                item[j] = Circles[numArray[j]];
            }
            NewOrder = numArray;
            Circle circle = item[2];
            Circle circle1 = item[1];
            Circle circle2 = item[0];
            double num = 0;
            circle.ClosestParameter(item[0].Center, out num);
            num -= 1.5707963267949;
            Plane plane = circle.Plane;
            Vector3d zAxis = plane.ZAxis;
            plane = circle.Plane;
            circle.Transform(Transform.Rotation(num, zAxis, plane.Origin));
            circle1.ClosestParameter(circle.Center, out num);
            num += 3.14159265358979;
            plane = circle1.Plane;
            Vector3d vector3d = plane.ZAxis;
            plane = circle1.Plane;
            circle1.Transform(Transform.Rotation(num, vector3d, plane.Origin));
            circle2.ClosestParameter(circle.Center, out num);
            num += 3.14159265358979;
            plane = circle2.Plane;
            Vector3d zAxis1 = plane.ZAxis;
            plane = circle2.Plane;
            circle2.Transform(Transform.Rotation(num, zAxis1, plane.Origin));
            return (new Circle[] { circle2, circle1, circle }).ToList<Circle>();
        }

        private List<Circle> SecondaryCircles(List<Point3d> Triangle, List<Vector3d> TriVec, List<Circle> PrimaryCircles) {
            List<Circle> circles = new List<Circle>();
            int count = checked(Triangle.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                Circle item = PrimaryCircles[i];
                Vector3d center = item.Center - Triangle[i];
                Vector3d vector3d = TriVec[i];
                center.Unitize();
                vector3d.Unitize();
                Point3d radius = Triangle[i];
                item = PrimaryCircles[i];
                radius -= ((vector3d * item.Radius) * 2);
                Plane plane = new Plane(radius, center + vector3d);
                item = PrimaryCircles[i];
                circles.Add(new Circle(plane, item.Radius));
            }
            return circles;
        }

        private List<Circle> TertiaryCircles(List<Point3d> Triangle, Point3d Tip, List<Circle> PrimaryCircles) {
            List<Circle> circles = new List<Circle>();
            int count = checked(Triangle.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                Point3d item = Triangle[i];
                Vector3d vector3d = Triangle[i] - Tip;
                vector3d.Unitize();
                Circle circle = PrimaryCircles[i];
                item = item + ((vector3d * circle.Radius) * 4);
                Plane plane = new Plane(item, Tip - item);
                circle = PrimaryCircles[i];
                circles.Add(new Circle(plane, circle.Radius));
            }
            return circles;
        }


    }
}
