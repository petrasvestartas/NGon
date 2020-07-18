using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonsCore.Graphs {

    public struct GraphEdge : IComparable<GraphEdge> {
        private int _From;

        private int _To;

        public int From {
            get {
                return this._From;
            }
            set {
                this._From = value;
            }
        }

        public int To {
            get {
                return this._To;
            }
            set {
                this._To = value;
            }
        }

        public GraphEdge(int From, int To) {
            this = new GraphEdge() {
                _From = From,
                _To = To
            };
        }

        public int CompareTo(GraphEdge other) {
            int num;
            int from = this.From;
            if (from < other.From) {
                num = -1;
            } else if (from <= other.From) {
                int to = this.To;
                if (to >= other.To) {
                    num = (to <= other.To ? 0 : 1);
                } else {
                    num = -1;
                }
            } else {
                num = 1;
            }
            return num;
        }

        public static GraphEdge Empty() {
            return new GraphEdge(-1, -1);
        }

        public void Flip() {
            int from = this.From;
            this.From = this.To;
            this.To = this.From;
        }

        public int GetOtherEnd(int ThisEnd) {
            int to;
            if (this.From != ThisEnd) {
                to = (this.To != ThisEnd ? -1 : this.From);
            } else {
                to = this.To;
            }
            return to;
        }

        public bool IsCycle() {
            bool flag;
            flag = (!(this.IsValid() & this.From == this.To) ? false : true);
            return flag;
        }

        public object IsEmpty() {
            return this == GraphEdge.Empty();
        }

        public bool IsSimilarTo(GraphEdge Other) {
            bool flag;
            if (this != Other) {
                Other.Flip();
                flag = (this != Other ? false : true);
            } else {
                flag = true;
            }
            return flag;
        }

        public bool IsValid() {
            bool flag;
            if (this.From != -1) {
                flag = (this.To != -1 ? true : false);
            } else {
                flag = false;
            }
            return flag;
        }

        internal void OnVertexRemoved(int VertexIndex) {
            if (this.From == VertexIndex) {
                this.From = -1;
            }
            if (this.To == VertexIndex) {
                this.To = -1;
            }
            if (this.From > VertexIndex) {
                this.From = checked(this.From - 1);
            }
            if (this.To > VertexIndex) {
                this.To = checked(this.To - 1);
            }
        }

        public static bool operator ==(GraphEdge A, GraphEdge B) {
            bool flag;
            flag = (!(A.From == B.From & A.To == B.To) ? false : true);
            return flag;
        }

        public static bool operator !=(GraphEdge A, GraphEdge B) {
            return !(A == B);
        }

        public void Orient() {
            int num = Math.Min(this.From, this.To);
            int num1 = Math.Max(this.From, this.To);
            this.From = num;
            this.To = num1;
        }

        public override string ToString() {
            return string.Concat("GraphEdge ", this.From.ToString(), "-", this.To.ToString());
        }
    }
}