using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGonCore {
    public static class IntervalUtil {
        public static Interval ToInterval(double n) {
            return new Interval(-n*0.5,n*0.5);
        }
    }
}
