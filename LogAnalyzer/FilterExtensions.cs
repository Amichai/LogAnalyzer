using LogAnalyzer.Filters;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer {
    public static class FilterExtensions {
        public static object Val(this RegexFilter f, LogLine l) {
            return f.Val(l.Value);
        }

        public static object Val(this RegexFilter f, string l) {
            if (!string.IsNullOrWhiteSpace(f.LineContains) && !l.Contains(f.LineContains)) {
                return null;
            }
            var inspection = f.InspectionString(l);
            switch (f.FilterType) {
                case FilterType.Boolean:
                    break;
                case FilterType.Event:
                    return f.Result<bool>(inspection, i => {
                        return !string.IsNullOrWhiteSpace(inspection);
                    });
                case FilterType.Numeric:
                    var val = f.Result<double?>(inspection, i => {
                        if (string.IsNullOrWhiteSpace(i)) {
                            return null;
                        }
                        return double.Parse(i.Split(' ', ':').Last());
                    });
                    return val;
                case FilterType.Time:
                    return f.Result<DateTime?>(inspection, i => {
                        if (string.IsNullOrWhiteSpace(i)) {
                            return null;
                        }
                        return DateTime.Parse(i.Split(',')[0]);
                    });
            }
            throw new Exception("Unhandled type");
        }

        public static DataPoint GetDataPoint(Dictionary<RegexFilter, object> vals) {
            var x = OxyPlot.Axes.DateTimeAxis.ToDouble((DateTime)vals.First().Value);
            var y = (double)vals.Last().Value;
            return new DataPoint(x, y);

        }
    }
}
