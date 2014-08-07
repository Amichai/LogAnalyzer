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
    public class Session : INotifyPropertyChanged {
        public Session() {
            this.Filters = new List<RegexFilter>();
            this.Timestamp = DateTime.Now;
            this.Files = new ObservableCollection<string>();
        }

        public List<RegexFilter> Filters { get; set; }
        public DateTime Timestamp { get; set; }
        public int LinesToShow { get; set; }
        public int StartLine { get; set; }
        public string DateString {
            get {
                return this.Timestamp.ToShortDateString();
            }
        }

        private ObservableCollection<string> _Files;
        public ObservableCollection<string> Files {
            get { return _Files; }
            set {
                _Files = value;
                NotifyPropertyChanged();
            }
        }

        public string Filepath {
            get {
                return this.Files.FirstOrDefault();
            }
        }

        public string FiltersString {
            get {
                return string.Concat(this.Filters.Select(i => i.Regex + ", "));
            }
        }

        //public ChartBuilderModel ChartModel { get; set; }

        public XElement ToXml() {
            XElement s = new XElement("Session");
            XElement filters = new XElement("Filters");
            foreach (var f in this.Filters) {
                filters.Add(f.ToXml());
            }
            s.Add(filters);
            s.Add(new XAttribute("DateTime", this.Timestamp));
            s.Add(new XAttribute("LinesToShow", this.LinesToShow));
            s.Add(new XAttribute("StartLine", this.StartLine));
            s.Add(this.filesXml());

            return s;
        }

        private XElement filesXml() {
            XElement files = new XElement("Files");
            foreach (var f in this.Files) {
                XElement file = new XElement("File");
                file.Add(new XAttribute("Path", f));
                files.Add(file);
            }
            return files;
        }

        internal static Session FromXml(XElement s) {
            Session toReturn = new Session();
            toReturn.Timestamp = DateTime.Parse(s.Attribute("DateTime").Value);
            toReturn.LinesToShow = int.Parse(s.Attribute("LinesToShow").Value);
            if (s.Attribute("StartLine") != null) {
                toReturn.StartLine = int.Parse(s.Attribute("StartLine").Value);
            }

            foreach (var f in s.Element("Filters").Elements()) {
                var parsed = RegexFilter.FromXml(f);
                toReturn.Filters.Add(parsed);
            }

            foreach (var file in s.Element("Files").Elements()) {
                toReturn.Files.Add(file.Attribute("Path").Value);
            }
            return toReturn;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation
    }
}
