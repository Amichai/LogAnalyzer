using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using BarSeries = OxyPlot.Series.BarSeries;
using ColumnSeries = OxyPlot.Series.ColumnSeries;

namespace LogAnalyzer {
    public class Histogram {

        private double binSize, lowerBound;
        private double upperBound {
            get {
                return lowerBound + binSize * binCount;
            }
        }
        private double range {
            get {
                return upperBound - lowerBound;
            }
        }

        int binCount;
        public Histogram(double binSize, double lowerBound, int binCount) {
            this.binSize = binSize;
            this.lowerBound = lowerBound;
            this.binCount = binCount;
            this.valCount = Enumerable.Range(0, binCount).ToDictionary(i => lowerBound + i * binSize, i => (int)0);
        }
        private Dictionary<double, int> valCount = new Dictionary<double, int>();

        internal void Add(double dist) {
            if (dist < lowerBound || dist > upperBound) {
                return;
            }
            foreach (var i in valCount.ToList()) {
                if (dist < i.Key + this.binSize) {
                    valCount[i.Key]++;
                    break;
                }
            }
        }

        internal void Show() {
            Window w = new Window();
            PlotView view = new PlotView();

            var model = new OxyPlot.PlotModel();
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis { ItemsSource = valCount, LabelField = "Key" });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis() {
                MinimumPadding = 0, AbsoluteMinimum = 0,
                Position = OxyPlot.Axes.AxisPosition.Left,
            });
            view.Model = model;
            w.Content = view;
            ColumnSeries s = new ColumnSeries();
            
            //s.ItemsSource = valCount.Select(i => i.Value) ;
            s.ItemsSource = valCount;
            s.ValueField = "Value";
            
            model.Series.Add(s);
            w.Show();
        }
    }
}
