﻿using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.solvers
{
    public interface IMatrix
    {
        int Rows { get; }
        int Columns { get; }
        Index2i Size { get; }

        void Set(int r, int c, double value);

        double this[int r, int c] { get; set; }
    }
}
