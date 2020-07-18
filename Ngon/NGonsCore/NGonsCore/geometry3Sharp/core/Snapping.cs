using System;

namespace NGonsCore.geometry3Sharp.core
{
    public class Snapping
    {

        public static double SnapToIncrement(double fValue, double fIncrement)
        {
            if (!math.MathUtil.IsFinite(fValue))
                return 0;
            double sign = Math.Sign(fValue);
            fValue = Math.Abs(fValue);
            int nInc = (int)(fValue / fIncrement);
            double fRem = fValue % fIncrement;
            if (fRem > fIncrement / 2)
                ++nInc;
            return sign * (double)nInc * fIncrement;
        }




        public static double SnapToNearbyIncrement(double fValue, double fIncrement, double fTolerance)
        {
            double snapped = SnapToIncrement(fValue, fIncrement);
            if (Math.Abs(snapped - fValue) < fTolerance)
                return snapped;
            return fValue;
        }

    }
}
