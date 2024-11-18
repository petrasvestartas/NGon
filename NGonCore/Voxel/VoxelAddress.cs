using System;
using System.Collections;
using System.Collections.Generic;
using Rhino.Geometry;

namespace NGonCore.Voxel {

    public struct VoxelAddress : IComparable<VoxelAddress> {

        private int _x;
        private int _y;
        private int _z;

        public static VoxelAddress Unset;


    public int this[int Coordinate] {
            get {
                int x;
                switch (Coordinate) {
                    case 0: {
                            x = this.X;
                            break;
                        }
                    case 1: {
                            x = this.Y;
                            break;
                        }
                    case 2: {
                            x = this.Z;
                            break;
                        }
                    default: {
                            x = 2147483647;
                            break;
                        }
                }
                return x;
            }

            set {
                switch (Coordinate) {
                    case 0: {
                            this.X = value;
                            return;
                        }
                    case 1: {
                            this.Y = value;
                            return;
                        }
                    case 2: {
                            this.Z = value;
                            return;
                        }
                    default: {
                            return;
                        }
                }
            }

        }



        public int X {
            get {
                return this._x;
            }
            set {
                this._x = value;
            }
        }

        public int Y {
            get {
                return this._y;
            }
            set {
                this._y = value;
            }
        }

        public int Z {
            get {
                return this._z;
            }
            set {
                this._z = value;
            }
        }

        static VoxelAddress() {
            VoxelAddress.Unset = new VoxelAddress(-2147483647, -2147483647, -2147483647);
        }

        public VoxelAddress(int X, int Y, int Z) {
            this = new VoxelAddress() {
                _x = X,
                _y = Y,
                _z = Z
            };
        }

        public int CompareTo(VoxelAddress other) {
            int num;
            int num1 = 0;
            while (true) {
                if (this[num1] < other[num1]) {
                    num = -1;
                    break;
                } else if (this[num1] <= other[num1]) {
                    num1 = checked(num1 + 1);
                    if (num1 > 2) {
                        num = 0;
                        break;
                    }
                } else {
                    num = 1;
                    break;
                }
            }
            return num;
        }

        public static VoxelAddress Create(int X, int Y, int Z) {
            return new VoxelAddress(X, Y, Z);
        }

        public static VoxelAddress operator +(VoxelAddress A, VoxelAddress B) {
            return new VoxelAddress(checked(A.X + B.X), checked(A.Y + B.Y), checked(A.Z + B.Z));
        }

        public static VoxelAddress operator /(VoxelAddress A, double V) {
            double v = 1 / V;
            return new VoxelAddress(checked((int)Math.Round((double)A.X * v)), checked((int)Math.Round((double)A.Y * v)), checked((int)Math.Round((double)A.Z * v)));
        }

        public static bool operator ==(VoxelAddress A, VoxelAddress B) {
            bool flag;
            flag = (!(A.X == B.X & A.Y == B.Y & A.Z == B.Z) ? false : true);
            return flag;
        }

        public static bool operator !=(VoxelAddress A, VoxelAddress B) {
            return !(A == B);
        }

        public static VoxelAddress operator *(VoxelAddress A, double V) {
            return new VoxelAddress(checked((int)Math.Round((double)A.X * V)), checked((int)Math.Round((double)A.Y * V)), checked((int)Math.Round((double)A.Z * V)));
        }

        public static VoxelAddress operator -(VoxelAddress A, VoxelAddress B) {
            return new VoxelAddress(checked(A.X - B.X), checked(A.Y - B.Y), checked(A.Z - B.Z));
        }

        public override string ToString() {
            return string.Concat(new string[] { "VoxelAddress <", (this.X).ToString(), ", ",(this.Y).ToString(), ", ", (this.Z).ToString(), ">" });
        }
    }

}
