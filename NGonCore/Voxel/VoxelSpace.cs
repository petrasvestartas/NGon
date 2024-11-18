using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NGonCore.Voxel {


    public class VoxelSpace<T> {
        private SortedList<VoxelAddress, T> _dict;

        public IList<VoxelAddress> AddressBook {
            get {
                return this._dict.Keys;
            }
        }

        public int Count {
            get {
                return this._dict.Count;
            }
        }

        public T this[int X, int Y, int Z] {
            get {
                T t;
                T t1 = default(T);
                t = (!this._dict.TryGetValue(VoxelAddress.Create(X, Y, Z), out t1) ? default(T) : t1);
                return t;
            }
        }

        public T this[VoxelAddress This] {
            get {
                T t;
                T t1 = default(T);
                t = (!this._dict.TryGetValue(This, out t1) ? default(T) : t1);
                return t;
            }
            set {
                this._dict[This] = value;
            }
        }

        public T this[int Index] {
            get {
                return this._dict[this._dict.Keys[Index]];
            }
            set {
                this._dict[this._dict.Keys[Index]] = value;
            }
        }

        public VoxelSpace() {
            this._dict = new SortedList<VoxelAddress, T>();
        }

        public void Add(T Value, VoxelAddress At) {
            this[At] = Value;
        }

        public void AddRange(IEnumerable<T> Values, IEnumerable<VoxelAddress> At) {
            int num = checked(Values.Count<T>() - 1);
            for (int i = 0; i <= num; i = checked(i + 1)) {
                this.Add(Values.ElementAtOrDefault<T>(i), At.ElementAtOrDefault<VoxelAddress>(i));
            }
        }

        public void Clear() {
            this._dict.Clear();
        }

        public T[] CollectValues(IEnumerable<VoxelAddress> From, ref bool[] Collected ) {
            T[] valueAt = new T[checked(checked(From.Count<VoxelAddress>() - 1) + 1)];
            if (Collected == null) {
                int num = checked(From.Count<VoxelAddress>() - 1);
                for (int i = 0; i <= num; i = checked(i + 1)) {
                    valueAt[i] = this[From.ElementAtOrDefault<VoxelAddress>(i)];
                }
            } else {
                int num1 = checked(From.Count<VoxelAddress>() - 1);
                for (int j = 0; j <= num1; j = checked(j + 1)) {
                    if (this.Contains(From.ElementAtOrDefault<VoxelAddress>(j))) {
                        Collected[j] = true;
                        valueAt[j] = this[From.ElementAtOrDefault<VoxelAddress>(j)];
                    }
                }
            }
            return valueAt;
        }

        public bool Contains(VoxelAddress This) {
            return this._dict.ContainsKey(This);
        }
   

        public Mesh CreateMesh() {

            Mesh polyMesh = new Mesh();
            Mesh polyMesh1 = new Mesh();
            polyMesh1.Vertices.Add(new Point3d(0, 0, 0));
            polyMesh1.Vertices.Add(new Point3d(1, 0, 0));
            polyMesh1.Vertices.Add(new Point3d(1, 1, 0));
            polyMesh1.Vertices.Add(new Point3d(0, 1, 0));
            polyMesh1.Vertices.Add(new Point3d(0, 0, 1));
            polyMesh1.Vertices.Add(new Point3d(1, 0, 1));
            polyMesh1.Vertices.Add(new Point3d(1, 1, 1));
            polyMesh1.Vertices.Add(new Point3d(0, 1, 1));

            MeshFace[] polyFace = new MeshFace[] {
                new MeshFace( 0, 3, 2, 1 ),
                new MeshFace( 4, 0, 1, 5 ),
                new MeshFace( 7, 3, 0, 4 ),
                new MeshFace( 1, 2, 6, 5 ),
                new MeshFace( 2, 3, 7, 6 ),
                new MeshFace( 6, 7, 4, 5 )
            };

  
            for (int i = 0; i < this.Count; i++) {

                VoxelAddress item = this._dict.Keys[i];
                Mesh polyMesh2 = polyMesh1.DuplicateMesh();
                polyMesh2.Transform(Transform.Translation(new Vector3d((double)item.X, (double)item.Y, (double)item.Z)));

                VoxelAddress[] faceNeighbors = this.GetFaceNeighbors(item);
                bool[] flagArray = new bool[faceNeighbors.Length];
                this.CollectValues(faceNeighbors, ref flagArray);


                for (int j = 0; j < faceNeighbors.Length; j++) 
                    if (!flagArray[j]) 
                        polyMesh2.Faces.AddFace(polyFace[j]);

                polyMesh.Append(polyMesh2);

            }//for i


            return polyMesh;
        }

        public VoxelAddress[] GetAllNeighbors(VoxelAddress This) {
            int num = 0;
            VoxelAddress[] voxelAddressArray = new VoxelAddress[26];
            int num1 = -1;
            do {
                int num2 = -1;
                do {
                    int num3 = -1;
                    do {
                        if (num1 == 0 & num2 == 0 & num3 == 0) {
                            VoxelAddress @this = This + VoxelAddress.Create(num3, num2, num1);
                            voxelAddressArray[num] = @this;
                            num = checked(num + 1);
                        }
                        num3 = checked(num3 + 1);
                    }
                    while (num3 <= 1);
                    num2 = checked(num2 + 1);
                }
                while (num2 <= 1);
                num1 = checked(num1 + 1);
            }
            while (num1 <= 1);
            return voxelAddressArray;
        }

        public VoxelAddress[] GetFaceNeighbors(VoxelAddress This) {
            int num = 2;
            VoxelAddress[] voxelAddressArray = new VoxelAddress[6];
            int num1 = -1;
            do {
                VoxelAddress @this = This + VoxelAddress.Create(num1, 0, 0);
                voxelAddressArray[num] = @this;
                num = checked(num + 1);
                num1 = checked(num1 + 2);
            }
            while (num1 <= 1);
            num = 1;
            int num2 = -1;
            do {
                VoxelAddress voxelAddress = This + VoxelAddress.Create(0, num2, 0);
                voxelAddressArray[num] = voxelAddress;
                num = checked(num + 3);
                num2 = checked(num2 + 2);
            }
            while (num2 <= 1);
            num = 0;
            int num3 = -1;
            do {
                VoxelAddress this1 = This + VoxelAddress.Create(0, 0, num3);
                voxelAddressArray[num] = this1;
                num = checked(num + 5);
                num3 = checked(num3 + 2);
            }
            while (num3 <= 1);
            return voxelAddressArray;
        }

        public void RemoveAt(VoxelAddress This) {
            this._dict.Remove(This);
        }
    }
}
