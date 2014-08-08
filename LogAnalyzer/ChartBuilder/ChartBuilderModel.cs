using LogAnalyzer.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogAnalyzer {
    public class ChartBuilderModel : INotifyPropertyChanged {
        private string _Title;
        public string Title {
            get { return _Title; }
            set {
                _Title = value;
                NotifyPropertyChanged();
            }
        }

        public ChartBuilderModel() {
            this.SeriesToAdd = new ObservableCollection<CustomSeries>();
        }

        private bool _IsSegmentTextFileEnabled;
        public bool IsSegmentTextFileEnabled {
            get { return _IsSegmentTextFileEnabled; }
            set {
                _IsSegmentTextFileEnabled = value;
                NotifyPropertyChanged();
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
        

        ///Set in the ChartBuilder.xaml.cs for data binding purposes
        public List<string> FilterEvents { get; set; }

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


        private int _MaxNumberOfWindows = 10;
        public int MaxNumberOfWindows {
            get { return _MaxNumberOfWindows; }
            set {
                _MaxNumberOfWindows = value;
                NotifyPropertyChanged();
            }
        }

        public XElement ToXml() {
            XElement chartModel = new XElement("ChartBuilder");
            chartModel.Add(new XAttribute("MaxNumberOfWindows", this.MaxNumberOfWindows));
            chartModel.Add(new XAttribute("EndEvent", this.EndEvent ?? ""));
            chartModel.Add(new XAttribute("StartEvent", this.StartEvent ?? ""));
            chartModel.Add(new XAttribute("IsSegmentTextFileEnabled", this.IsSegmentTextFileEnabled));
            chartModel.Add(new XAttribute("Title", this.Title ?? ""));
            XElement series = new XElement("SeriesToAdd");
            foreach(var s in this.SeriesToAdd){
                series.Add(s.ToXml());
            }
            chartModel.Add(series);

            return chartModel;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation

        internal static ChartBuilderModel FromXml(XElement xml, List<RegexFilter> filters) {
            ChartBuilderModel model = new ChartBuilderModel();
            model.MaxNumberOfWindows = int.Parse(xml.Attribute("MaxNumberOfWindows").Value);
            model.EndEvent = xml.Attribute("EndEvent").Value;
            model.StartEvent = xml.Attribute("StartEvent").Value;
            model.IsSegmentTextFileEnabled = bool.Parse(xml.Attribute("IsSegmentTextFileEnabled").Value);
            model.Title = xml.Attribute("Title").Value;
            foreach (var s in xml.Element("SeriesToAdd").Elements()) {
                model.SeriesToAdd.Add(CustomSeries.FromXml(s, filters));
            }

            return model;
        }
    }
}
