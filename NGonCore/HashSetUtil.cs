using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {


    //Method 1 DataEntry
    public struct DataEntry : IEquatable<DataEntry> {
        public int A;
        public int B;
        public int C;
        public int D;

        public DataEntry(int a, int b, int c, int d) {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public override bool Equals(object obj) {
            return (obj is DataEntry other) && Equals(other);
        }

        public bool Equals(DataEntry other) {
            return other.A == A && other.B == B && other.C == C && other.D == D;
        }

        public override int GetHashCode() {
            // this hashcode algorithm is recommended by some books
            // the numbers are arbitrarily constructed

            unchecked {
                var hashCode = -1117161837;
                hashCode = hashCode * -1521734295 + A;
                hashCode = hashCode * -1521734295 + B;
                hashCode = hashCode * -1521734295 + C;
                hashCode = hashCode * -1521734295 + D;
                return hashCode;
            }
        }
    }
    public static class HashSetTest {
        public static void ProcessDataWithCustomizedType(IEnumerable<List<int>> inputs) {
            var hashset = new HashSet<DataEntry>();

            foreach (var it in inputs) {
                var entry = new DataEntry(it[0], it[1], it[2], it[3]);
                if (hashset.Add(entry)) {
                    // first visit

                    // do something
                } else {
                    // already exists in the hashset

                    // do something
                }
            }
        }
    }




    //Method 2 Ilist Comparer

    public class ListComparer<T> : IEqualityComparer<IList<T>> {
        private IEqualityComparer<T> _comparer;
        public ListComparer(IEqualityComparer<T> comparer = null) {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(IList<T> list1, IList<T> list2) {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            return list1.SequenceEqual(list2);
        }

        public int GetHashCode(IList<T> obj) {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                foreach (T x in obj) {
                    hash = hash * 23 + _comparer.GetHashCode(x);
                }
                return hash;
            }
        }
    }










}
