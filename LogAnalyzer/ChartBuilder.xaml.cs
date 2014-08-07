using LogAnalyzer.Filters;
using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ScatterSeries = OxyPlot.Series.ScatterSeries;
using LineSeries = OxyPlot.Series.LineSeries;
using XYAxisSeries = OxyPlot.Series.XYAxisSeries;
using ScatterPoint = OxyPlot.Series.ScatterPoint;

namespace LogAnalyzer {
    /// <summary>
    /// Interaction logic for ChartBuilder.xaml
    /// </summary>
    public partial class ChartBuilder : UserControl, INotifyPropertyChanged {
        public ChartBuilder() {
            InitializeComponent();
            this.XAxis = new ObservableCollection<string>();
            this.YAxis = new ObservableCollection<string>();
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

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation

        private List<RegexFilter> filters;

        internal void SetDataSources(List<RegexFilter> f) {
            this.filters = f;
            this.XAxis.Clear();
            this.YAxis.Clear();
            foreach (var filter in this.filters) {
                this.XAxis.Add(filter.Name);
                this.YAxis.Add(filter.Name);
            }
        }

        private string _Title;
        public string Title {
            get { return _Title; }
            set {
                _Title = value;
                NotifyPropertyChanged();
            }
        }

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

        //WORKING - should be used
        Dictionary<RegexFilter, double?> dict = new Dictionary<RegexFilter, double?>();
        private IEnumerable<Dictionary<RegexFilter, double?>> getValues(List<RegexFilter> filters) {
            int filterCount = filters.Count();
            if (dict.Count() == 0 && filterCount != 0) {
                foreach (var f in filters) {
                    dict[f] = null;
                }
            }
            foreach (var l in this.Lines) {
                foreach (var f in filters) {
                    var v = (double?)(f as RegexFilter).Val(l.Value);
                    if (v == null) {
                        continue;
                    }
                    dict[f] = v;


                    if (!dict.Any(i => i.Value == null)) {
                        yield return dict;
                        dict = new Dictionary<RegexFilter, double?>();
                        foreach (var f2 in filters) {
                            dict[f2] = null;
                        }
                    }
                }
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e) {
            Window w = new Window();
            var view = new PlotView();
            var model = new OxyPlot.PlotModel() {
                Title = this.Title,
            };
            view.Model = model;
            w.Content = view;
            XYAxisSeries scatterSeries;
            if (this.ChartType.Content as string == "Scatter") {
                scatterSeries = new ScatterSeries() {
                    MarkerFill = OxyColors.Blue,
                    MarkerStrokeThickness = .3
                };
            } else {
                scatterSeries = new LineSeries() {
                    Color = OxyColors.Blue,
                    MarkerSize = .3,
                };
            }

            if (string.IsNullOrWhiteSpace(this.Title)) {
                this.Title = this.SelectedXAxis + ", " + this.SelectedYAxis;
            }

            model.Title = this.Title;

            Dictionary<RegexFilter, object> lastVals = new Dictionary<RegexFilter, object>();

            List<RegexFilter> toDisplay = new List<RegexFilter>() {
                this.filters.Where(i => i.Name == this.SelectedXAxis).Single(),
                this.filters.Where(i => i.Name == this.SelectedYAxis).Single(),
            };

            ///TODO: multiple series. Select a range of lines to process.


            int counter = 0;
            foreach (var f in toDisplay) {
                string title = f.Name;
                if (f.FilterType == FilterType.Time) {
                    model.Axes.Add(new OxyPlot.Axes.DateTimeAxis() {
                        Title = title,
                        Position = counter++ == 0 ? OxyPlot.Axes.AxisPosition.Bottom : OxyPlot.Axes.AxisPosition.Left
                    });
                } else {
                    model.Axes.Add(new OxyPlot.Axes.LinearAxis() {
                        Title = title,
                        Position = counter++ == 0 ? OxyPlot.Axes.AxisPosition.Bottom : OxyPlot.Axes.AxisPosition.Left
                    });
                }
            }

            ////TODO: Can we parse a custom string to get a derived property?
            //var valsTest = this.getValues(toDisplay).ToList();
            var targetFilterCount = toDisplay.Count();
            foreach (var l in this.Lines) {
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
                                Y = pt.Y
                            };
                            (scatterSeries as ScatterSeries).Points.Add(p);
                        }
                        foreach (var key in lastVals.Keys.ToList()) {
                            lastVals[key] = null;
                        }
                    }
                }
            }

            model.Series.Add(scatterSeries);
            w.Show();
        }



        private List<LogLine> Lines;

        internal void SetLines(List<LogLine> lines) {
            this.Lines = lines;
        }
    }
    
}
