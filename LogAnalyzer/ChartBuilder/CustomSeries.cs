using LogAnalyzer.Filters;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace LogAnalyzer {
    public class CustomSeries : INotifyPropertyChanged {
        public CustomSeries() {
            this.XAxis = new ObservableCollection<string>();
            this.YAxis = new ObservableCollection<string>();
            this.ChangeColor();
        }

        private ObservableCollection<string> _XAxis;
        public ObservableCollection<string> XAxis {
            get { return _XAxis; }
            set {
                _XAxis = value;
                NotifyPropertyChanged();
            }
        }

        private ComboBoxItem _ChartType;
        public ComboBoxItem ChartType {
            get { return _ChartType; }
            set {
                _ChartType = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> _YAxis;
        public ObservableCollection<string> YAxis {
            get { return _YAxis; }
            set {
                _YAxis = value;
                NotifyPropertyChanged();
            }
        }


        private List<RegexFilter> filters;

        private string _SelectedXAxis;
        public string SelectedXAxis {
            get { return _SelectedXAxis; }
            set {
                _SelectedXAxis = value;
                NotifyPropertyChanged();
            }
        }

        private string _SelectedYAxis;
        public string SelectedYAxis {
            get { return _SelectedYAxis; }
            set {
                _SelectedYAxis = value;
                NotifyPropertyChanged();
            }
        }

        private string _FilterText;
        public string FilterText {
            get { return _FilterText; }
            set {
                _FilterText = value;
                NotifyPropertyChanged();
            }
        }

        private OxyColor oxySeriesColor {
            get {
                Color r = (this.SeriesColor as SolidColorBrush).Color;
                return OxyColor.FromArgb(r.A, r.R, r.G, r.B);
            }
        }

        private static List<Brush> seriesColors = new List<Brush>() { Brushes.Red, Brushes.Blue, Brushes.Black, Brushes.Green, Brushes.Purple, Brushes.Yellow, Brushes.Orange };
        private static int brushIndex;

        private bool _Interpolate;
        public bool Interpolate {
            get { return _Interpolate; }
            set {
                _Interpolate = value;
                NotifyPropertyChanged();
            }
        }

        public XYAxisSeries GetSeries(List<LogLine> lines) {
            XYAxisSeries scatterSeries;
            if (this.ChartType.Content as string == "Scatter") {
                scatterSeries = new ScatterSeries() {
                    MarkerFill = this.oxySeriesColor,
                    MarkerStrokeThickness = this.Thickness,
                };
            } else {
                scatterSeries = new LineSeries() {
                    Color = this.oxySeriesColor,
                    MarkerSize = this.Thickness,
                    CanTrackerInterpolatePoints = this.Interpolate,
                };
            }

            Dictionary<RegexFilter, object> lastVals = new Dictionary<RegexFilter, object>();

            List<RegexFilter> toDisplay = new List<RegexFilter>() {
                this.filters.Where(i => i.Name == this.SelectedXAxis).Single(),
                this.filters.Where(i => i.Name == this.SelectedYAxis).Single(),
            };

            ///TODO: multiple series. Select a range of lines to process.


            //int counter = 0;
            //foreach (var f in toDisplay) {
            //    string title = f.Name;
            //    if (f.FilterType == FilterType.Time) {
            //        model.Axes.Add(new OxyPlot.Axes.DateTimeAxis() {
            //            Title = title,
            //            Position = counter++ == 0 ? OxyPlot.Axes.AxisPosition.Bottom : OxyPlot.Axes.AxisPosition.Left
            //        });
            //    } else {
            //        model.Axes.Add(new OxyPlot.Axes.LinearAxis() {
            //            Title = title,
            //            Position = counter++ == 0 ? OxyPlot.Axes.AxisPosition.Bottom : OxyPlot.Axes.AxisPosition.Left
            //        });
            //    }
            //}

            ////TODO: Can we parse a custom string to get a derived property?
            //var valsTest = this.getValues(toDisplay).ToList();
            var targetFilterCount = toDisplay.Count();
            foreach (var l in lines) {
                for (int axisIndex = 0; axisIndex < targetFilterCount; axisIndex++) {
                    var filter = toDisplay[axisIndex];
                    var v = (filter as RegexFilter).Val(l.Value);
                    if (v != null) {
                        if (axisIndex == 1 && !string.IsNullOrWhiteSpace(this.FilterText) && !l.Value.Contains(this.FilterText)) {
                            continue;
                        }

                        lastVals[filter] = v;
                    }
                    if (lastVals.Where(i => i.Value != null).Count() == targetFilterCount) {
                        DataPoint pt;
                        if ((lastVals.First().Key as RegexFilter).FilterType == FilterType.Time) {
                            pt = FilterExtensions.GetDataPoint(lastVals);
                        } else {
                            var x = (double)lastVals.First().Value;
                            var y = (double)lastVals.Last().Value;
                            pt = new DataPoint(x, y);
                        }
                        if (scatterSeries is LineSeries) {
                            (scatterSeries as LineSeries).Points.Add(pt);
                        } else {
                            ScatterPoint p = new ScatterPoint() {
                                X = pt.X,
                                Y = pt.Y,
                            };
                            (scatterSeries as ScatterSeries).Points.Add(p);
                        }
                        foreach (var key in lastVals.Keys.ToList()) {
                            lastVals[key] = null;
                        }
                    }
                }
            }
            return scatterSeries;
        }

        internal void SetDataSources(List<RegexFilter> f) {
            this.filters = f;
            this.XAxis.Clear();
            this.YAxis.Clear();
            foreach (var filter in this.filters) {
                this.XAxis.Add(filter.Name);
                this.YAxis.Add(filter.Name);
            }
        }

        private double _Thickness = .5;
        public double Thickness {
            get { return _Thickness; }
            set {
                _Thickness = value;
                NotifyPropertyChanged();
            }
        }
        private Brush _SeriesColor;
        public Brush SeriesColor {
            get { return _SeriesColor; }
            set {
                _SeriesColor = value;
                NotifyPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation

        internal void ChangeColor() {
            this.SeriesColor = seriesColors[brushIndex++ % seriesColors.Count()];
        }

        internal XElement ToXml() {
            XElement series = new XElement("Series");
            series.Add(new XAttribute("Thickness", this.Thickness));
            series.Add(new XAttribute("SeriesColor", this.SeriesColor));
            series.Add(new XAttribute("SelectedXAxis", this.SelectedXAxis ?? ""));
            series.Add(new XAttribute("SelectedYAxis", this.SelectedYAxis ?? ""));
            series.Add(new XAttribute("FilterText", this.FilterText ?? ""));
            series.Add(new XAttribute("ChartType", this.ChartType.Content as string  ?? ""));
            return series;
        }
    }
}
