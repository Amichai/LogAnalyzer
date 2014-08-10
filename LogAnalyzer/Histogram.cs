using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Util;

using BarSeries = OxyPlot.Series.BarSeries;
using ColumnSeries = OxyPlot.Series.ColumnSeries;
using System.Diagnostics;

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
        private string title;

        int binCount;
        public Histogram(double binSize, double lowerBound, int binCount, string title) {
            this.title = title;
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
                    //int index = (int)Math.Floor((dist - lowerBound) * binCount / (this.range)) + 1;
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
            model.Title = this.title;
            w.Content = view;
            ColumnSeries s = new ColumnSeries();
            
            //s.ItemsSource = valCount.Select(i => i.Value) ;
            s.ItemsSource = valCount;
            s.ValueField = "Value";
            
            model.Series.Add(s);

            w.Show();
        }

        public void Save() {
            var model = new OxyPlot.PlotModel();
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis { ItemsSource = valCount, LabelField = "Key" });
            model.Axes.Add(new OxyPlot.Axes.LinearAxis() {
                MinimumPadding = 0, AbsoluteMinimum = 0,
                Position = OxyPlot.Axes.AxisPosition.Left,
            });
            model.Title = this.title;
            ColumnSeries s = new ColumnSeries();

            //s.ItemsSource = valCount.Select(i => i.Value) ;
            s.ItemsSource = valCount;
            s.ValueField = "Value";

            model.Series.Add(s);
            var dir = @"\\files\Trivial\Amichai\ValidatorData\positionHistograms";

            model.Save(dir, 2000, 1000, this.title);
            Process.Start(dir);
        }
    }
}
