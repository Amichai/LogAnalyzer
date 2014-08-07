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
using System.Xml.Linq;

namespace LogAnalyzer {
    /// <summary>
    /// Interaction logic for ChartBuilder.xaml
    /// </summary>
    public partial class ChartBuilder : UserControl, INotifyPropertyChanged {
        public ChartBuilder() {
            InitializeComponent();
            this.SeriesToAdd = new ObservableCollection<CustomSeries>();
            SeriesSelector.Remove.Subscribe(i => {
                this.SeriesToAdd.Remove(i);
            });
            this.SeriesToAdd.Add(new CustomSeries());
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
            this.NotifyPropertyChanged("FilterEvents");
            foreach (var s in this.SeriesToAdd) {
                s.SetDataSources(f);
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

        private bool _SegmentTextFile;
        public bool SegmentTextFile {
            get { return _SegmentTextFile; }
            set {
                _SegmentTextFile = value;
                NotifyPropertyChanged();
            }
        }

        //could be used:
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

        private ObservableCollection<CustomSeries> _SeriesToAdd;
        public ObservableCollection<CustomSeries> SeriesToAdd {
            get { return _SeriesToAdd; }
            set {
                _SeriesToAdd = value;
                NotifyPropertyChanged();
            }
        }

        ///TODO: add a checkbox or some other method of specifying a datetime axis


        public List<string> FilterEvents {
            get {
                if (this.filters == null) {
                    return null;
                }
                return this.filters.Where(i => i.FilterType == FilterType.Event).Select(i => i.Name).ToList(); 
            }
        }

        private string _StartEvent;
        public string StartEvent {
            get { return _StartEvent; }
            set {
                _StartEvent = value;
                NotifyPropertyChanged();
            }
        }

        private string _EndEvent;
        public string EndEvent {
            get { return _EndEvent; }
            set {
                _EndEvent = value;
                NotifyPropertyChanged();
            }
        }

        private void showPlot(List<LogLine> lines) {
            Window w = new Window();
            var view = new PlotView();
            var model = new OxyPlot.PlotModel() {
                Title = this.Title,
            };
            view.Model = model;
            w.Content = view;
            foreach (var s in this.SeriesToAdd) {
                model.Series.Add(s.GetSeries(lines));
            }
            w.Show();
        }

        private IEnumerable<List<LogLine>> SegmentText(RegexFilter start, RegexFilter end) {
            List<LogLine> toReturn = new List<LogLine>();
            bool started = false;
            foreach (var l in this.Lines) {
                if (started) {
                    toReturn.Add(l);
                }
                if (start.Val(l) as bool? == true) {
                    started = true;
                }
                if (end.Val(l) as bool? == true) {
                    yield return toReturn;
                    toReturn = new List<LogLine>();
                    started = false;
                }
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e) {

            if (!this.SegmentTextFile) {
                this.showPlot(this.Lines);
            } else {
                var startFilter = this.filters.Where(i => i.Name == this.StartEvent).Single();
                var endFilter = this.filters.Where(i => i.Name == this.EndEvent).Single();
                foreach (var lines in this.SegmentText(startFilter, endFilter)) {
                    this.showPlot(lines);
                }
            }
            //if (string.IsNullOrWhiteSpace(this.Title)) {
            //    this.Title = this.SelectedXAxis + ", " + this.SelectedYAxis;
            //}


        }

        private List<LogLine> Lines;

        internal void SetLines(List<LogLine> lines) {
            this.Lines = lines;
        }

        private void AddSeries_Click(object sender, RoutedEventArgs e) {
            var s = new CustomSeries();
            s.SetDataSources(this.filters);
            this.SeriesToAdd.Add(s);
        }

        internal void Remove(CustomSeries s) {
            this.SeriesToAdd.Remove(s);
        }

        internal XElement ToXml() {
            return new XElement("ChartBuilder");
        }
    }
    
}
