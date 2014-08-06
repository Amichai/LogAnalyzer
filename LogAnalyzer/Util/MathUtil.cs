using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LogAnalyzer.Util {
    public static class MathUtil {
        public static double Dist(this Vector v1, Vector v2) {
            return Math.Sqrt((v2.X - v1.X).Sqrd() + (v2.Y - v1.Y).Sqrd());
        }

        public static double Sqrd(this double d) {
            return d * d;
        }
    }
}
