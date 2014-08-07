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
            SeriesSelector.Remove.Subscribe(i => {
                this.SeriesToAdd.Remove(i);
            });
            this.Model = new ChartBuilderModel();
            this.SeriesToAdd.Add(new CustomSeries());
        }

        private ObservableCollection<CustomSeries> SeriesToAdd {
            get {
                return this.Model.SeriesToAdd;
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

        private ChartBuilderModel _Model;
        public ChartBuilderModel Model {
            get { return _Model; }
            set {
                _Model = value;
                NotifyPropertyChanged();
            }
        }

        private List<RegexFilter> filters;

        private Session session;

        public void SetSession(Session session) {
            this.session = session;
            //this.Model = session.ChartModel;
            this.filters = this.session.Filters;
            this.Model.FilterEvents = this.filters.Where(i => i.FilterType == FilterType.Event).Select(i => i.Name).ToList();
            this.NotifyPropertyChanged("FilterEvents");
            foreach (var s in this.SeriesToAdd) {
                s.SetDataSources(this.filters);
            }
            this.Lines = new List<LogLine>();
            foreach (var file in session.Files) {
                var lines = System.IO.File.ReadAllLines(file).Select(i => new LogLine(i)).ToList();
                this.Lines.AddRange(lines);
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

        private void showPlot(List<LogLine> lines) {
            Window w = new Window();
            var view = new PlotView();
            var model = new OxyPlot.PlotModel() {
                Title = this.Model.Title,
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
            if (!this.Model.IsSegmentTextFileEnabled) {
                this.showPlot(this.Lines);
            } else {
                var startFilter = this.filters.Where(i => i.Name == this.Model.StartEvent).Single();
                var endFilter = this.filters.Where(i => i.Name == this.Model.EndEvent).Single();
                foreach (var lines in this.SegmentText(startFilter, endFilter).Take(this.Model.MaxNumberOfWindows)) {
                    if (lines.Count() == 0) {
                        continue;
                    }
                    this.showPlot(lines);
                }
            }
            //if (string.IsNullOrWhiteSpace(this.Title)) {
            //    this.Title = this.SelectedXAxis + ", " + this.SelectedYAxis;
            //}
        }


        private List<LogLine> Lines;

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
