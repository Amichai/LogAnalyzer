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
using System.IO;

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

        private List<int> lineCountsPerFile = new List<int>();

        public List<LogLine> Lines {
            get {
                var lines = new List<LogLine>();
                this.lineCountsPerFile = new List<int>();
                foreach (var file in session.Files) {
                    var l = this.readAllLines(file);
                    this.lineCountsPerFile.Add(l.Count());
                    lines.AddRange(l);
                }
                return lines;
            }
        }

        public void SetSession(Session session) {
            this.session = session;
            this.Model = session.ChartModel;
            this.filters = this.session.Filters;
            this.Model.FilterEvents = this.filters.Where(i => i.FilterType == FilterType.Event).Select(i => i.Name).ToList();
            this.NotifyPropertyChanged("FilterEvents");
            this.startFilterEvent.SelectedValue = this.Model.StartEvent;
            this.endFilterEvent.SelectedValue = this.Model.EndEvent;
        }


        private List<LogLine> readAllLines(string path) {
            if (!System.IO.File.Exists(path)) {
                return new List<LogLine>();
            }
            return System.IO.File.ReadAllLines(path).Select(i => new LogLine(i)).ToList();
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
            //saveImage(model);
        }

        private static void saveImage(PlotModel model) {
            BitmapSource bitmap = PngExporter.ExportToBitmap(model, 1000, 1000, OxyColors.White);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            Guid photoID = System.Guid.NewGuid();
            var dir = @"\\files\Trivial\Amichai\ValidatorData\RobotPaths";
            String photolocation = System.IO.Path.Combine(dir, photoID.ToString() + ".jpg");  //file name 

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var filestream = new FileStream(photolocation, FileMode.Create))
                encoder.Save(filestream);
        }

        private string getFilename(int abosluteLineNumber) {
            int currentSum = 0;
            for (int i = 0; i < this.lineCountsPerFile.Count(); i++) {
                int inspectionSum = currentSum + this.lineCountsPerFile[i];
                if (inspectionSum > abosluteLineNumber) {
                    return this.session.Files[i] + " line: " + (abosluteLineNumber - currentSum); ;
                }
                currentSum += inspectionSum;
            }
            throw new Exception("Line number to big");
        }

        private IEnumerable<List<LogLine>> SegmentText(RegexFilter start, RegexFilter end) {
            List<LogLine> toReturn = new List<LogLine>();
            bool started = false;
            int lineNumber = 0;
            foreach (var l in this.Lines) {
                if (started) {
                    toReturn.Add(l);
                }
                if (start.Val(l) as bool? == true) {
                    started = true;
                }
                if (end.Val(l) as bool? == true) {
                    this.Model.Title = this.getFilename(lineNumber);
                    yield return toReturn;
                    toReturn = new List<LogLine>();
                    started = false;
                }
                lineNumber++;

            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e) {
            //this.Model.Title = string.Join(", ", this.session.Files);
            if (!this.Model.IsSegmentTextFileEnabled) {
                this.showPlot(this.Lines);
            } else {
                var startFilter = this.filters.Where(i => i.Name == this.Model.StartEvent).Single();
                var endFilter = this.filters.Where(i => i.Name == this.Model.EndEvent).Single();
                int plotCounter = 0;
                foreach (var lines in this.SegmentText(startFilter, endFilter).Take(this.Model.MaxNumberOfWindows)) {
                    if (lines.Count() == 0) {
                        continue;
                    }
                    //this.Model.Title = string.Join(", ", this.session.Files) + " " + (plotCounter++).ToString();
                    this.showPlot(lines);
                }
            }
            
            //if (string.IsNullOrWhiteSpace(this.Title)) {
            //    this.Title = this.SelectedXAxis + ", " + this.SelectedYAxis;
            //}
        }

        private void AddSeries_Click(object sender, RoutedEventArgs e) {
            var s = new CustomSeries(this.filters);
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
