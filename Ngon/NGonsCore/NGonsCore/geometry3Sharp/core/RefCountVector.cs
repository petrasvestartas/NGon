﻿namespace NGonsCore.geometry3Sharp.core
{

    // this class allows you to keep track of refences to indices,
    // with a free list so unreferenced indices can be re-used.
    //
    // the enumerator iterates over valid indices
    //
    public class RefCountVector : System.Collections.IEnumerable
    {
        public static readonly short invalid = -1;


        DVector<short> ref_counts;
        DVector<int> free_indices;
        int used_count;

        public RefCountVector()
        {
            ref_counts = new DVector<short>();
            free_indices = new DVector<int>();
            used_count = 0;
        }

        public RefCountVector(RefCountVector copy)
        {
            ref_counts = new DVector<short>(copy.ref_counts);
            free_indices = new DVector<int>(copy.free_indices);
            used_count = copy.used_count;
        }

        public DVector<short> RawRefCounts {
            get { return ref_counts; }
        }

        public bool Empty {
            get { return used_count == 0; }
        }
        public int Count {
            get { return used_count; }
        }
        public int Max_index {
            get { return ref_counts.Size; }
        }
        public bool Is_dense {
            get { return free_indices.Length == 0; }
        }


        public bool IsValid(int index) {
            return ( index >= 0 && index < ref_counts.Size && ref_counts[index] > 0 );
        }
        public bool IsValidUnsafe(int index) {
            return ref_counts[index] > 0;
        }


        public int RefCount(int index) {
            int n = ref_counts[index];
            return (n == invalid) ? 0 : n;
        }


        public int Allocate() {
            used_count++;
            if (free_indices.empty) {
                ref_counts.push_back(1);
                return ref_counts.Size - 1;
            } else {
                int iFree = free_indices.back;
                free_indices.pop_back();
                ref_counts[iFree] = 1;
                return iFree;
            }
        }



        public int Increment(int index, short increment = 1) {
            Util.gDevAssert( IsValid(index)  );
            ref_counts[index] += increment;
            return ref_counts[index];       
        }

        public void Decrement(int index, short decrement = 1) {
            Util.gDevAssert( IsValid(index) );
            ref_counts[index] -= decrement;
            Util.gDevAssert(ref_counts[index] >= 0);
            if (ref_counts[index] == 0) {
                free_indices.push_back(index);
                ref_counts[index] = invalid;
                used_count--;
            }
        }


        // [RMS] really should not use this!!
        public void Set_Unsafe(int index, short count)
        {
            ref_counts[index] = count;
        }

        // todo:
        //   insert
        //   remove
        //   clear


        public void Rebuild_free_list()
        {
            free_indices = new DVector<int>();
            used_count = 0;

            int N = ref_counts.Length;
            for ( int i = 0; i < N; ++i ) {
                if (ref_counts[i] > 0)
                    used_count++;
                else
                    free_indices.Add(i);
            }
        }




        public System.Collections.IEnumerator GetEnumerator()
        {
            int nIndex = 0;
            int nLast = Max_index;

            // skip leading empties
            while (nIndex != nLast && ref_counts[nIndex] <= 0)
                nIndex++;

            while (nIndex != nLast) {
                yield return nIndex;

                if (nIndex != nLast)
                    nIndex++;
                while (nIndex != nLast && ref_counts[nIndex] <= 0)
                    nIndex++;
            }
        }



        public string Debug_print()
        {
            string s = string.Format("size {0} used {1} free_size {2}\n", ref_counts.Size, used_count, free_indices.Size);
            for (int i = 0; i < ref_counts.Size; ++i)
                s += string.Format("{0}:{1} ", i, ref_counts[i]);
            s += "\nfree:\n";
            for (int i = 0; i < free_indices.Size; ++i)
                s += free_indices[i].ToString() + " ";
            return s;
        }


	}
}

