using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore.Graphs {
    public class GraphBuilder {
        private DirectedNode m_Graph;

        private SortedList<int, int> m_NodeIndex;

        private List<Point3d[]> m_Levels;//this

        private int[] m_NodeLevel;

        private double m_Radius;

        private double m_Height;

        private int m_NodeCount;

        private List<Line> m_Lines;

        private List<int> m_LinesIndex;

        public double Height {
            get {
                return this.m_Height;
            }
            set {
                this.m_Height = value;
            }
        }

        private SortedList<int, int> Ids {
            get {
                return this.m_NodeIndex;
            }
        }

        //private Point3d[] Level(int index) {
        //    get
        //    {
        //        return this.m_Levels[index];
        //    }
        //    set
        //    {
        //        this.m_Levels[index] = value;
        //    }
        //}

        //private Point3d[] this[int index] {
        //    get
        //    {
        //        return this.m_Levels[index];
        //    }
        //    set
        //    {
        //        this.m_Levels[index] = value;
        //    }
        //}



        private int LevelCount {
            get {
                return this.m_Levels.Count;
            }
        }

        //private Point3d PointAt(int level, int index) {
        //    get
        //    {
        //        return this.m_Levels[level][index];
        //    }
        //    set
        //    {
        //        this.m_Levels[level][index] = value;
        //    }
        //}


        private Point3d this[int level, int index] {
            get
            {
                return this.m_Levels[level][index];
            }
            set
            {
                this.m_Levels[level][index] = value;
            }
        }

        //private Point3d PointAt(int index) {
        //    get
        //    {
        //        return this.m_Levels[this.m_NodeLevel[index]][index];
        //    }
        //    set
        //    {
        //        this.m_Levels[this.m_NodeLevel[index]][index] = value;
        //    }
        //}

        private Point3d this[int index] {
            get {
                return this.m_Levels[this.m_NodeLevel[index]][index];
            }
            set {
                this.m_Levels[this.m_NodeLevel[index]][index] = value;
            }
        }

        public double Radius {
            get {
                return this.m_Radius;
            }
            set {
                this.m_Radius = value;
            }
        }

        public GraphBuilder() {
            this.m_Graph = null;
            this.m_NodeIndex = new SortedList<int, int>();
            this.m_Levels = new List<Point3d[]>();
            this.m_Radius = 10;
            this.m_Height = 1;
            this.m_NodeCount = 1;
            this.m_Lines = new List<Line>();
            this.m_LinesIndex = new List<int>();
        }

        private void AddLevel() {
            this.m_Levels.Add(this.ConstructLevel());
        }

        public SortedList<int, Polyline> Build(DirectedNode Graph) {
            this.m_Levels.Clear();
            this.m_Lines.Clear();
            this.m_LinesIndex.Clear();
            this.m_NodeIndex.Clear();
            this.m_Graph = Graph;
            this.m_NodeCount = this.TotalNodeCount(this.m_Graph);
            this.m_NodeLevel = MathUtil.CreateArray(this.m_NodeCount, 0);
            this.AddLevel();
            List<DirectedNode> directedNodes = new List<DirectedNode>()
            {
                this.m_Graph
            };
            while (directedNodes.Count > 0) {
                List<DirectedNode> directedNodes1 = new List<DirectedNode>();
                int count = checked(directedNodes.Count - 1);
                for (int i = 0; i <= count; i = checked(i + 1)) {
                    directedNodes1.AddRange(this.BuildSplitNode(directedNodes[i]));
                }
                directedNodes = directedNodes1;
            }
            for (int j = checked(this.m_NodeIndex.Keys.Count - 1); j >= 0; j = checked(j + -1)) {
                if (this.m_NodeIndex.Keys[j] < 0) {
                    this.BuildMergeNode(this.m_Graph.FindNode(this.m_NodeIndex.Keys[j]), j == 0);
                }
            }
            SortedList<int, Polyline> nums = new SortedList<int, Polyline>();
            SortedList<int, List<Line>> lines = new SortedList<int, List<Line>>();
            int num = checked(this.m_LinesIndex.Count - 1);
            for (int k = 0; k <= num; k = checked(k + 1)) {
                lines[this.m_LinesIndex[k]] = new List<Line>();
            }
            int count1 = checked(this.m_Lines.Count - 1);
            for (int l = 0; l <= count1; l = checked(l + 1)) {
                lines[this.m_LinesIndex[l]].Add(this.m_Lines[l]);
            }
            int num1 = checked(lines.Keys.Count - 1);
            for (int m = 0; m <= num1; m = checked(m + 1)) {
                List<Line> item = lines[lines.Keys[m]];
                Polyline polyline = new Polyline();
                Line line = item[0];
                polyline.Add(line.From);
                int count2 = checked(item.Count - 1);
                for (int n = 0; n <= count2; n = checked(n + 1)) {
                    line = item[n];
                    polyline.Add(line.To);
                }
                nums[lines.Keys[m]] = polyline;
            }
            return nums;
        }

        private void BuildMergeNode(DirectedNode Node, bool LastOne) {
            int item = this.m_NodeIndex[Node.ID];
            if (Node != null) {
                this.MoveUp(item, checked(this.m_Levels.Count - 1));
                Point3d pointAt = this[item];
                this.AddLevel();
                this.MoveUp(item, checked(this.m_Levels.Count - 1));
                Point3d point3d = this[item];
                int mNodeLevel = this.m_NodeLevel[item];
                int count = checked(Node.Parents.Count - 1);
                for (int i = 0; i <= count; i = checked(i + 1)) {
                    DirectedNode directedNode = Node.Parents[i];
                    int num = this.m_NodeIndex[directedNode.ID];
                    Point3d pointAt1 = this[num];
                    this.MoveUp(num, checked(this.m_Levels.Count - 1));
                    Point3d point3d1 = this[num];
                    this.m_Lines.Add(new Line(pointAt1, point3d1));
                    this.m_Lines.Add(new Line(point3d1, point3d));
                    this.m_LinesIndex.Add(directedNode.ID);
                    this.m_LinesIndex.Add(directedNode.ID);
                }
                if (LastOne) {
                    Point3d pointAt2 = this[item];
                    this.AddLevel();
                    this.MoveUp(item, checked(this.m_NodeLevel[item] + 1));
                    this.m_Lines.Add(new Line(pointAt2, this[item]));
                    this.m_LinesIndex.Add(Node.ID);
                }
            }
        }

        private List<DirectedNode> BuildSplitNode(DirectedNode Node) {
            int item = this.m_NodeIndex[Node.ID];
            List<DirectedNode> directedNodes = new List<DirectedNode>();
            this.AddLevel();
            Point3d pointAt = this[item];
            this.MoveUp(item, checked(this.m_Levels.Count - 1));
            Point3d point3d = this[item];
            this.m_Lines.Add(new Line(pointAt, point3d));
            this.m_LinesIndex.Add(Node.ID);
            int count = checked(Node.Children.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                if (Node.Children[i].ID >= 0) {
                    DirectedNode directedNode = Node.Children[i];
                    int num = this.m_NodeIndex[directedNode.ID];
                    this.MoveUp(num, checked(this.m_Levels.Count - 1));
                    this.m_Lines.Add(new Line(point3d, this[num]));
                    this.m_LinesIndex.Add(Node.Children[i].ID);
                    directedNodes.Add(Node.Children[i]);
                }
            }
            return directedNodes;
        }

        private Point3d[] ConstructLevel() {
            Point3d[] point3dArray = new Point3d[checked(checked(this.m_NodeCount - 1) + 1)];
            int mNodeCount = checked(this.m_NodeCount - 1);
            for (int i = 0; i <= mNodeCount; i = checked(i + 1)) {
                Point3d point3d = new Point3d(this.m_Radius, 0, (double)this.m_Levels.Count * this.Height);
                double num = 6.28318530717959 * ((double)i / (double)this.m_NodeCount);
                point3d.Transform(Transform.Rotation(num, Vector3d.ZAxis, Point3d.Origin));
                point3dArray[i] = point3d;
            }
            return point3dArray;
        }

        private void CountAllUniqueChildren(ref DirectedNode tree, ref int count, ref HashSet<int> counted) {
            if (!counted.Contains(tree.ID)) {
                counted.Add(tree.ID);
                count = checked(count + 1);
                int num = checked(tree.Children.Count - 1);
                for (int i = 0; i <= num; i = checked(i + 1)) {
                    List<DirectedNode> children = tree.Children;
                    List<DirectedNode> directedNodes = children;
                    int num1 = i;
                    int num2 = num1;
                    DirectedNode item = children[num1];
                    this.CountAllUniqueChildren(ref item, ref count, ref counted);
                    directedNodes[num2] = item;
                }
            }
        }

        private void MoveUp(int PointIdx, int ToLevel) {
            this.m_NodeLevel[PointIdx] = ToLevel;
        }

        private int TotalNodeCount(DirectedNode Tree) {
            int num = 0;
            HashSet<int> nums = new HashSet<int>();
            this.CountAllUniqueChildren(ref Tree, ref num, ref nums);
            List<int> nums1 = new List<int>();
            nums1.AddRange(nums.ToList<int>());
            nums1.Sort();
            int count = checked(nums1.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                this.m_NodeIndex.Add(nums1[i], i);
            }
            return num;
        }

    }
}
