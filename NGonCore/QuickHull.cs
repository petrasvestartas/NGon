//using PolyMesh_Core.Geometry;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    //public class QuickHull {
    //    private PolyMesh _mesh;

    //    private List<Plane> _faceplanes;

    //    private List<double> _facedenoms;

    //    private List<double[]> _faceequations;

    //    private bool[] _used;

    //    private double _tolerance;

    //    private int _removed;

    //    private Random rnd;

    //    private List<Plane> FacePlanes {
    //        get {
    //            return this._faceplanes;
    //        }
    //        set {
    //            this._faceplanes = value;
    //        }
    //    }

    //    public PolyMesh Mesh {
    //        get {
    //            return this._mesh;
    //        }
    //        set {
    //            this._mesh = value;
    //        }
    //    }

    //    public QuickHull(IEnumerable<Point3d> Points) {
    //        this._mesh = null;
    //        this._faceplanes = new List<Plane>();
    //        this._facedenoms = new List<double>();
    //        this._faceequations = new List<double[]>();
    //        this._removed = 0;
    //        this.rnd = new Random(123);
    //        this.Mesh = new PolyMesh();
    //        this.Mesh.Vertices.AddRange(Points);
    //        this._used = new bool[checked(checked(Points.Count<Point3d>() - 1) + 1)];
    //    }
       
    //    private bool AddFacesFromVertex(int Index) {
    //        List<PolyMesh_Core.Geometry.TopologyEdge> topologyEdges = this.Mesh.GetTopologyEdges();
    //        List<PolyFace> polyFaces = new List<PolyFace>();
    //        int count = checked(topologyEdges.Count - 1);
    //        for (int i = 0; i <= count; i++) {
    //            if (topologyEdges[i].IsNaked) {
    //                int[] to = new int[3];
    //                PolyMesh_Core.Geometry.TopologyEdge item = topologyEdges[i];
    //                to[0] = item.To;
    //                item = topologyEdges[i];
    //                to[1] = item.From;
    //                to[2] = Index;
    //                polyFaces.Add(new PolyFace(to));
    //            }
    //        }
    //        int num = this.Mesh.Faces.Count;
    //        this.Mesh.Faces.AddRange(polyFaces);
    //        this.ComputePlanes(num);
    //        return true;
    //    }
        
    //    private void ComputePlanes(int From = 0) {
    //        int count = checked(this.Mesh.Faces.Count - 1);
    //        for (int i = From; i <= count; i++) {
    //            this.FacePlanes.Add(new Plane(this.Mesh.Faces.GetCenter(i), this.Mesh.Faces.GetNormal(i)));
    //            this._faceequations.Add(this.FacePlanes[i].GetPlaneEquation());
    //            this._facedenoms.Add(this.GetDenominator(this._faceequations[i]));
    //        }
    //    }

    //    private void FlagVertices() {
    //        int count = checked(this.Mesh.Vertices.Count - 1);
    //        for (int i = 0; i <= count; i++) {
    //            if (!this._used[i]) {
    //                bool flag = true;
    //                int num = checked(this.Mesh.Faces.Count - 1);
    //                int num1 = 0;
    //                while (num1 <= num) {
    //                    if (this.PlaneToPoint(num1, i) <= this._tolerance) {
    //                        num1++;
    //                    } else {
    //                        flag = false;
    //                        break;
    //                    }
    //                }
    //                if (flag) {
    //                    this._used[i] = true;
    //                    this._removed = checked(this._removed + 1);
    //                }
    //            }
    //        }
    //    }

    //    private double GetDenominator(double[] Equation) {
    //        return 1 / Math.Sqrt(Math.Pow(Equation[0], 2) + Math.Pow(Equation[1], 2) + Math.Pow(Equation[2], 2));
    //    }

    //    private int PickFurthers(int Index) {
    //        double[] item = this._faceequations[Index];
    //        double num = this._facedenoms[Index];
    //        double num1 = 0;
    //        int num2 = -1;
    //        int count = checked(this.Mesh.Vertices.Count - 1);
    //        for (int i = 0; i <= count; i++) {
    //            if (!this._used[i]) {
    //                double point = this.PlaneToPoint(item, num, this.Mesh.Vertices[i]);
    //                if (point > num1) {
    //                    num1 = point;
    //                    num2 = i;
    //                }
    //            }
    //        }
    //        return num2;
    //    }

    //    private int PickFurthers(Plane P) {
    //        double[] planeEquation = P.GetPlaneEquation();
    //        double denominator = this.GetDenominator(planeEquation);
    //        double num = 0;
    //        int num1 = -1;
    //        int count = checked(this.Mesh.Vertices.Count - 1);
    //        for (int i = 0; i <= count; i++) {
    //            if (!this._used[i]) {
    //                double point = this.PlaneToPoint(planeEquation, denominator, this.Mesh.Vertices[i]);
    //                if (point > num) {
    //                    num = point;
    //                    num1 = i;
    //                }
    //            }
    //        }
    //        return num1;
    //    }

    //    private bool PickTetrahedron() {
    //        bool flag;
    //        int[] numArray = new int[3];
    //        int count = checked(this.Mesh.Vertices.Count * 3);
    //        while (true) {
    //            if (count > 0) {
    //                int num = 0;
    //                do {
    //                    numArray[num] = this.rnd.Next(0, this.Mesh.Vertices.Count);
    //                    num++;
    //                }
    //                while (num <= 2);
    //                Plane plane = new Plane(this.Mesh.Vertices[numArray[0]], this.Mesh.Vertices[numArray[1]], this.Mesh.Vertices[numArray[2]]);
    //                int num1 = this.PickFurthers(plane);
    //                if (num1 == -1) {
    //                    count--;
    //                } else {
    //                    this.Mesh.Faces.Add(new PolyFace(new int[] { numArray[2], numArray[1], numArray[0] }));
    //                    this.Mesh.Faces.Add(new PolyFace(new int[] { num1, numArray[0], numArray[1] }));
    //                    this.Mesh.Faces.Add(new PolyFace(new int[] { num1, numArray[1], numArray[2] }));
    //                    this.Mesh.Faces.Add(new PolyFace(new int[] { num1, numArray[2], numArray[0] }));
    //                    this.ComputePlanes(0);
    //                    int length = checked((int)numArray.Length - 1);
    //                    for (int i = 0; i <= length; i++) {
    //                        this._used[numArray[i]] = true;
    //                    }
    //                    this._used[num1] = true;
    //                    this._removed = checked(this._removed + 4);
    //                    flag = true;
    //                    break;
    //                }
    //            } else {
    //                flag = false;
    //                break;
    //            }
    //        }
    //        return flag;
    //    }

    //    private double PlaneToPoint(double[] Equation, double Denominator, Point3d P) {
    //        return (Equation[0] * P.X + Equation[1] * P.Y + Equation[2] * P.Z + Equation[3]) * Denominator;
    //    }

    //    private double PlaneToPoint(int FaceIndex, Point3d P) {
    //        return (this._faceequations[FaceIndex][0] * P.X + this._faceequations[FaceIndex][1] * P.Y + this._faceequations[FaceIndex][2] * P.Z + this._faceequations[FaceIndex][3]) * this._facedenoms[FaceIndex];
    //    }

    //    private double PlaneToPoint(int FaceIndex, int PointIndex) {
    //        double item = this._faceequations[FaceIndex][0];
    //        Point3d point3d = this.Mesh.Vertices[PointIndex];
    //        double x = item * point3d.X;
    //        double num = this._faceequations[FaceIndex][1];
    //        point3d = this.Mesh.Vertices[PointIndex];
    //        double y = x + num * point3d.Y;
    //        double item1 = this._faceequations[FaceIndex][2];
    //        point3d = this.Mesh.Vertices[PointIndex];
    //        return (y + item1 * point3d.Z + this._faceequations[FaceIndex][3]) * this._facedenoms[FaceIndex];
    //    }

    //    private bool RemoveVisibleFaces(int Index) {
    //        Point3d item = this.Mesh.Vertices[Index];
    //        List<int> nums = new List<int>();
    //        int count = checked(this.FacePlanes.Count - 1);
    //        for (int i = 0; i <= count; i++) {
    //            if (this.PlaneToPoint(i, item) < -this._tolerance) {
    //                nums.Add(i);
    //            }
    //        }
    //        List<PolyFace> polyFaces = new List<PolyFace>();
    //        List<double> nums1 = new List<double>();
    //        List<double[]> numArrays = new List<double[]>();
    //        List<Plane> planes = new List<Plane>();
    //        int num = checked(nums.Count - 1);
    //        for (int j = 0; j <= num; j++) {
    //            polyFaces.Add(this.Mesh.Faces[nums[j]]);
    //            nums1.Add(this._facedenoms[nums[j]]);
    //            numArrays.Add(this._faceequations[nums[j]]);
    //            planes.Add(this._faceplanes[nums[j]]);
    //        }
    //        this.Mesh.Faces.Clear();
    //        this._facedenoms.Clear();
    //        this._faceequations.Clear();
    //        this._faceplanes.Clear();
    //        this.Mesh.Faces.AddRange(polyFaces);
    //        this._facedenoms.AddRange(nums1);
    //        this._faceequations.AddRange(numArrays);
    //        this._faceplanes.AddRange(planes);
    //        return true;
    //    }

    //    public bool Solve(double Tolerance = 0, int seed = 123) {
    //        bool flag;
    //        this._tolerance = Tolerance;
    //        if (this.Mesh.Vertices.Count >= 4) {
    //            this.rnd = new Random(seed);
    //            this.PickTetrahedron();
    //            if (this.Mesh.Vertices.Count != 4) {
    //                while (this._removed < this.Mesh.Vertices.Count) {
    //                    int count = checked(this.Mesh.Faces.Count - 1);
    //                    int num = 0;
    //                    while (num <= count) {
    //                        if (this._removed != this.Mesh.Vertices.Count) {
    //                            int num1 = this.PickFurthers(this.FacePlanes[num]);
    //                            if (num1 != -1) {
    //                                this.RemoveVisibleFaces(num1);
    //                                //this.AddFacesFromVertex(num1);
    //                                this._used[num1] = true;
    //                                this._removed = checked(this._removed + 1);
    //                                this.FlagVertices();
    //                                num = 0;
    //                            } else if (num == checked(this.Mesh.Faces.Count - 1)) {
    //                                flag = false;
    //                                return flag;
    //                            }
    //                            num++;
    //                        } else {
    //                            flag = true;
    //                            return flag;
    //                        }
    //                    }
    //                }
    //                flag = true;
    //            } else {
    //                flag = true;
    //            }
    //        } else {
    //            flag = false;
    //        }
    //        return flag;
    //    }

    //}
}
