using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace NGonCore.Graphs {
    public class DirectedNode {
        private int m_id;

        private double m_value;

        private List<DirectedNode> m_parent;

        private List<DirectedNode> m_children;

        public List<DirectedNode> Children {
            get {
                return this.m_children;
            }
            set {
                this.m_children = value;
            }
        }

        public int ID {
            get {
                return this.m_id;
            }
            set {
                this.m_id = value;
            }
        }

        public List<DirectedNode> Parents {
            get {
                return this.m_parent;
            }
            set {
                this.m_parent = value;
            }
        }

        public double Value {
            get {
                return this.m_value;
            }
            set {
                this.m_value = value;
            }
        }

        public DirectedNode() {
            this.m_parent = new List<DirectedNode>();
            this.m_children = new List<DirectedNode>();
        }

        public DirectedNode(int ID = 0, double Value = 0) {
            this.m_parent = new List<DirectedNode>();
            this.m_children = new List<DirectedNode>();
            this.m_id = ID;
            this.m_value = Value;
        }

        private static void Accumulate(DirectedNode Graph, int LastID, int Addition) {
            int count = checked(Graph.Children.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                if (Graph.Children[i].ID != LastID) {
                    Graph.Children[i].Value = Math.Max(Graph.Children[i].Value, Graph.Value + 6 + (double)Addition);
                } else {
                    Graph.Children[i].Value = Math.Max(Graph.Children[i].Value, Graph.Value + 3 + (double)Addition);
                }
                DirectedNode.Accumulate(Graph.Children[i], LastID, Addition);
            }
        }

        public void AddChild(DirectedNode Child) {
            if (Child != this) {
                this.m_children.Add(Child);
                Child.Parents.Add(this);
            }
        }

        public void AddParent(DirectedNode Parent) {
            if (Parent != null) {
                this.m_parent.Add(Parent);
                Parent.Children.Add(this);
            }
        }

        public static List<double> CleanupTreeInstructions(List<double> Instructions) {
            List<double> nums = new List<double>();
            int count = checked(Instructions.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                if (Instructions[i] <= 1) {
                    nums.Add(Instructions[i]);
                } else {
                    nums.Add(Instructions[i] - Instructions[i] % 1);
                    if (Instructions[i] % 1 > 0) {
                        nums.Add(Instructions[i] % 1);
                    }
                }
            }
            return nums;
        }

        public bool CollapseGraph(DirectedNode.CollapseMethod Collapse = 0, double[] CollapseValues = null) {
            if (CollapseValues != null) {
                DirectedNode.CollapseMethod collapse = Collapse;
                if (collapse == DirectedNode.CollapseMethod.LowestHighest || collapse == DirectedNode.CollapseMethod.LowestLowest) {
                    List<DirectedNode> leafs = this.GetLeafs();
                    int num = -1;
                    while (leafs.Count > 1) {
                        int[] numArray = new int[checked(checked(leafs.Count - 1) + 1)];
                        double[] collapseValues = new double[checked(checked(leafs.Count - 1) + 1)];
                        int count = checked(leafs.Count - 1);
                        for (int i = 0; i <= count; i = checked(i + 1)) {
                            numArray[i] = i;
                            collapseValues[i] = CollapseValues[i];
                        }
                        Array.Sort<double, int>(collapseValues, numArray);
                        DirectedNode item = leafs[numArray[0]];
                        DirectedNode directedNode = null;
                        if (Collapse == DirectedNode.CollapseMethod.LowestLowest) {
                            directedNode = leafs[numArray[1]];
                        } else if (Collapse == DirectedNode.CollapseMethod.LowestHighest) {
                            directedNode = leafs[numArray[checked((int)numArray.Length - 1)]];
                        }
                        DirectedNode directedNode1 = new DirectedNode(num, item.Value + directedNode.Value);
                        num = checked(num - 1);
                        item.AddChild(directedNode1);
                        directedNode.AddChild(directedNode1);
                        leafs.Clear();
                        leafs = this.GetLeafs();
                        List<DirectedNode> directedNodes = new List<DirectedNode>();
                        HashSet<int> nums = new HashSet<int>();
                        int count1 = checked(leafs.Count - 1);
                        for (int j = 0; j <= count1; j = checked(j + 1)) {
                            if (!nums.Contains(leafs[j].ID)) {
                                directedNodes.Add(leafs[j]);
                                nums.Add(leafs[j].ID);
                            }
                        }
                        leafs = directedNodes;
                    }
                }
            } else {
                DirectedNode.CollapseMethod collapseMethod = Collapse;
                if (collapseMethod == DirectedNode.CollapseMethod.LowestHighest || collapseMethod == DirectedNode.CollapseMethod.LowestLowest) {
                    List<DirectedNode> leafs1 = this.GetLeafs();
                    int num1 = -1;
                    while (leafs1.Count > 1) {
                        int[] numArray1 = new int[checked(checked(leafs1.Count - 1) + 1)];
                        double[] value = new double[checked(checked(leafs1.Count - 1) + 1)];
                        int count2 = checked(leafs1.Count - 1);
                        for (int k = 0; k <= count2; k = checked(k + 1)) {
                            numArray1[k] = k;
                            value[k] = leafs1[k].Value;
                        }
                        Array.Sort<double, int>(value, numArray1);
                        DirectedNode item1 = leafs1[numArray1[0]];
                        DirectedNode item2 = null;
                        if (Collapse == DirectedNode.CollapseMethod.LowestLowest) {
                            item2 = leafs1[numArray1[1]];
                        } else if (Collapse == DirectedNode.CollapseMethod.LowestHighest) {
                            item2 = leafs1[numArray1[checked((int)numArray1.Length - 1)]];
                        }
                        DirectedNode directedNode2 = new DirectedNode(num1, item1.Value + item2.Value);
                        num1 = checked(num1 - 1);
                        item1.AddChild(directedNode2);
                        item2.AddChild(directedNode2);
                        leafs1.Clear();
                        leafs1 = this.GetLeafs();
                        List<DirectedNode> directedNodes1 = new List<DirectedNode>();
                        HashSet<int> nums1 = new HashSet<int>();
                        int num2 = checked(leafs1.Count - 1);
                        for (int l = 0; l <= num2; l = checked(l + 1)) {
                            if (!nums1.Contains(leafs1[l].ID)) {
                                directedNodes1.Add(leafs1[l]);
                                nums1.Add(leafs1[l].ID);
                            }
                        }
                        leafs1 = directedNodes1;
                    }
                }
            }
            return true;
        }

        public static void ComputeMeshCounts(DirectedNode Graph, int AddFaces = 0) {
            Graph.Value = 4;
            List<DirectedNode> leafs = Graph.GetLeafs();
            leafs[0].Value = 3;
            DirectedNode.Accumulate(Graph, leafs[0].ID, AddFaces);
            DirectedNode.CorrectValues(Graph, leafs[0].ID);
            leafs[0].Value = 3;
            SortedList<int, DirectedNode> nums = new SortedList<int, DirectedNode>();
            DirectedNode.FlatTree(Graph, ref nums);
            int count = checked(nums.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                DirectedNode item = nums[nums.Keys[i]];
                item.Value = item.Value + (double)AddFaces;
            }
        }

        private static void Correction(DirectedNode Graph) {
            if (Graph.ID == 0) {
                Graph.Value = 4;
                return;
            }
            int num = 0;
            int count = checked(Graph.Parents.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                num = checked((int)Math.Round(Math.Max((double)num, Graph.Parents[i].Value)));
            }
            int count1 = checked(Graph.Parents.Count - 1);
            for (int j = 0; j <= count1; j = checked(j + 1)) {
                Graph.Parents[j].Value = (double)num - Graph.Parents[j].Value + 6;
                DirectedNode.Correction(Graph.Parents[j]);
            }
        }

        private static void CorrectValues(DirectedNode Graph, int LastID) {
            SortedList<int, DirectedNode> nums = new SortedList<int, DirectedNode>();
            DirectedNode.FlatTree(Graph, ref nums);
            DirectedNode.Correction(nums[LastID]);
        }

        public DirectedNode FindNode(int ID) {
            return this.FindNode(this, ID);
        }

        private DirectedNode FindNode(DirectedNode T, int ID) {
            DirectedNode t;
            if (T.ID != ID) {
                int count = checked(T.Children.Count - 1);
                int num = 0;
                while (num <= count) {
                    DirectedNode directedNode = this.FindNode(T.Children[num], ID);
                    if (directedNode == null) {
                        num = checked(num + 1);
                    } else {
                        t = directedNode;
                        return t;
                    }
                }
                t = null;
            } else {
                t = T;
            }
            return t;
        }

        public static void FlatTree(DirectedNode G, ref SortedList<int, DirectedNode> Srt) {
            List<DirectedNode>.Enumerator enumerator = new List<DirectedNode>.Enumerator();
            Srt[G.ID] = G;
            try {
                enumerator = G.Children.GetEnumerator();
                while (enumerator.MoveNext()) {
                    DirectedNode.FlatTree(enumerator.Current, ref Srt);
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
        }

        public static void GetAllNodes(ref DirectedNode Node, SortedList<int, DirectedNode> SortedByID) {
            SortedByID[Node.ID] = Node;
            int count = checked(Node.Children.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                List<DirectedNode> children = Node.Children;
                int num = i;
                int num1 = num;
                DirectedNode item = children[num];
                DirectedNode.GetAllNodes(ref item, SortedByID);
                children[num1] = item;
            }
        }

        private void GetLeafNodes(ref List<DirectedNode> Nodes) {
            if (this.IsLeaf()) {
                Nodes.Add(this);
                return;
            }
            int count = checked(this.Children.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                this.Children[i].GetLeafNodes(ref Nodes);
            }
        }

        public List<DirectedNode> GetLeafs() {
            List<DirectedNode> directedNodes = new List<DirectedNode>();
            this.GetLeafNodes(ref directedNodes);
            return directedNodes;
        }

        public void GrowGraph(IEnumerable<double> Instructions) {
            List<DirectedNode> leafs = this.GetLeafs();
            int num = 0;
            int num1 = 1;
            int num2 = checked(Instructions.Count<double>() - 1);
            for (int i = 0; i <= num2; i = checked(i + 1)) {
                double num3 = Instructions.ElementAtOrDefault<double>(i);
                double num4 = num3;
                if (num4 == 0) {
                    leafs.Clear();
                    leafs = this.GetLeafs();
                    num = 0;
                } else if (num4 >= 1) {
                    leafs.Clear();
                    leafs = this.GetLeafs();
                    num = checked((int)Math.Round(((double)num + num3 + (double)leafs.Count) % (double)leafs.Count));
                } else {
                    DirectedNode item = leafs[num];
                    item.AddChild(new DirectedNode(num1, num3 * item.Value));
                    num1 = checked(num1 + 1);
                    item.AddChild(new DirectedNode(num1, (1 - num3) * item.Value));
                    num1 = checked(num1 + 1);
                    num = checked(num + 1);
                }
                if (num > checked(leafs.Count - 1)) {
                    leafs.Clear();
                    leafs = this.GetLeafs();
                    num = 0;
                }
            }
        }

        public bool IsLeaf() {
            return this.m_children.Count == 0;
        }

        public bool IsRoot() {
            return this.m_parent.Count == 0;
        }

        public static void OptimizeSplits(DirectedNode Graph, int MinimalValue, bool Minimize, ref double[] CollapseValues) {
            List<DirectedNode>.Enumerator enumerator = new List<DirectedNode>.Enumerator();
            SortedList<int, DirectedNode> nums = new SortedList<int, DirectedNode>();
            DirectedNode.FlatTree(Graph, ref nums);
            List<DirectedNode> directedNodes = new List<DirectedNode>(nums.Values);
            double[] numArray = new double[checked(checked(directedNodes.Count - 1) + 1)];
            DirectedNode[] item = new DirectedNode[checked(checked(directedNodes.Count - 1) + 1)];
            double num = double.MaxValue;
            int count = checked(directedNodes.Count - 1);
            for (int i = 0; i <= count; i = checked(i + 1)) {
                item[directedNodes[i].ID] = directedNodes[i];
                num = Math.Min(num, directedNodes[i].Value);
            }
            double[] value = new double[checked(checked(directedNodes.Count - 1) + 1)];
            double[] value1 = new double[checked(checked(directedNodes.Count - 1) + 1)];
            double[] numArray1 = new double[checked(checked(directedNodes.Count - 1) + 1)];
            int length = checked((int)item.Length - 1);
            for (int j = 0; j <= length; j = checked(j + 1)) {
                value[j] = item[j].Value;
                value1[j] = -1;
                numArray1[j] = -1;
            }
            double[] numArray2 = new double[checked(checked((int)value.Length - 1) + 1)];
            int[] numArray3 = new int[checked(checked((int)value.Length - 1) + 1)];
            int length1 = checked((int)value.Length - 1);
            for (int k = 0; k <= length1; k = checked(k + 1)) {
                numArray[k] = value[k];
                numArray2[k] = value[k];
                numArray3[k] = k;
            }
            CollapseValues = numArray;
            Array.Sort<double, int>(numArray2, numArray3);
            if (!Minimize) {
                int num1 = checked((int)value.Length - 1);
                for (int l = 0; l <= num1; l = checked(l + 1)) {
                    value[l] = Math.Round(value[l] / num);
                    item[l].Value = value[l];
                }
            } else {
                int length2 = checked((int)value.Length - 1);
                for (int m = 0; m <= length2; m = checked(m + 1)) {
                    value[m] = 1;
                    item[m].Value = value[m];
                }
            }
            int num2 = checked((int)numArray3.Length - 1);
            for (int n = 0; n <= num2; n = checked(n + 1)) {
                int num3 = numArray3[n];
                DirectedNode directedNode = item[num3];
                if (num3 != 0) {
                    DirectedNode item1 = directedNode.Parents[0];
                    if (value1[num3] != -1 & numArray1[num3] != -1) {
                        item[num3].Value = value1[directedNode.ID] + numArray1[directedNode.ID];
                        value[num3] = item[num3].Value;
                    }
                    if (value1[item1.ID] == -1) {
                        value1[item1.ID] = directedNode.Value;
                    } else if (numArray1[item1.ID] == -1) {
                        numArray1[item1.ID] = directedNode.Value;
                    }
                } else {
                    directedNode.Value = value[directedNode.Children[0].ID] + value[directedNode.Children[1].ID];
                }
            }
            try {
                enumerator = directedNodes.GetEnumerator();
                while (enumerator.MoveNext()) {
                    DirectedNode current = enumerator.Current;
                    current.Value = current.Value * (double)MinimalValue;
                }
            } finally {
                ((IDisposable)enumerator).Dispose();
            }
        }

        public override string ToString() {
            return "TreeNode";
        }

        public string ToSuperString() {
            return this.ToSuperString(0);
        }

        private string ToSuperString(int Level) {
            string str = new string(Conversions.ToCharArrayRankOne(""));
            int level = checked(Level - 1);
            for (int i = 0; i <= level; i = checked(i + 1)) {
                str = string.Concat(str, ". ");
            }
            string[] strArrays = new string[] { str, "<ID:", Conversions.ToString(this.ID), ", Value:", null, null };
            double value = this.Value;
            strArrays[4] = value.ToString(CultureInfo.InvariantCulture);
            strArrays[5] = ">";
            str = string.Concat(strArrays);
            int count = checked(this.Children.Count - 1);
            for (int j = 0; j <= count; j = checked(j + 1)) {
                str = string.Concat(str, "\r\n", this.Children[j].ToSuperString(checked(Level + 1)));
            }
            return str;
        }

        public enum CollapseMethod {
            Invalid = -1,
            LowestHighest = 0,
            LowestLowest = 1
        }


    }
}
