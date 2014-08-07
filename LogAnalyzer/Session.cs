using LogAnalyzer.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogAnalyzer {
    public class Session {
        public Session() {
            this.Filters = new List<RegexFilter>();
            this.Timestamp = DateTime.Now;
        }

        public string Filepath { get; set; }
        public List<RegexFilter> Filters { get; set; }
        public DateTime Timestamp { get; set; }
        public int LinesToShow { get; set; }
        public int StartLine { get; set; }
        public string DateString {
            get {
                return this.Timestamp.ToShortDateString();
            }
        }

        public string FiltersString {
            get {
                return string.Concat(this.Filters.Select(i => i.Regex + ", "));
            }
        }

        public XElement ToXml() {
            XElement s = new XElement("Session");
            XElement filters = new XElement("Filters");
            foreach (var f in this.Filters) {
                filters.Add(f.ToXml());
            }
            s.Add(filters);
            s.Add(new XAttribute("LogFilepath", this.Filepath));
            s.Add(new XAttribute("DateTime", this.Timestamp));
            s.Add(new XAttribute("LinesToShow", this.LinesToShow));
            s.Add(new XAttribute("StartLine", this.StartLine));


            return s;
        }

        internal static Session Parse(XElement s) {
            Session toReturn = new Session();
            toReturn.Filepath = s.Attribute("LogFilepath").Value;
            toReturn.Timestamp = DateTime.Parse(s.Attribute("DateTime").Value);
            toReturn.LinesToShow = int.Parse(s.Attribute("LinesToShow").Value);
            if (s.Attribute("StartLine") != null) {
                toReturn.StartLine = int.Parse(s.Attribute("StartLine").Value);
            }

            foreach (var f in s.Element("Filters").Elements()) {
                var parsed = RegexFilter.FromXml(f);
                toReturn.Filters.Add(parsed);
            }
            return toReturn;
        }
    }
}
