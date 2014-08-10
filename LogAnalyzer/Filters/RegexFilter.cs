using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogAnalyzer.Filters {
    public class RegexFilter {
        public RegexFilter(FilterType t, string name) {
            this.FilterType = t;
            this.Name = name;
        }
        public string Regex { get; set; }
        public string InspectionString(string line) {
            if (!string.IsNullOrWhiteSpace(this.LineContains) && !line.Contains(this.LineContains)) {
                return "";
            }
            Regex r = new Regex(this.Regex);
            foreach (Match m in r.Matches(line)) {
                return m.Value;
            }
            return "";
        }

        public override string ToString() {
            return string.Format("{0} - {1}", Name, this.Regex);
        }

        public string Name { get; set; }

        public string LineContains { get; set; }

        public T Result<T>(string inspectionString, Func<string, T> parser) {
            return parser(inspectionString);
        }

        public XElement ToXml() {
            XElement root = new XElement("RegexFilter");
            root.Add(new XAttribute("Name", this.Name));
            root.Add(new XAttribute("Regex", this.Regex));
            root.Add(new XAttribute("Type", this.FilterType.ToString()));
            root.Add(new XAttribute("LineContains", this.LineContains ?? ""));

            return root;
        }

        public static RegexFilter FromXml(XElement xml) {
            var name = xml.Attribute("Name").Value;
            var type =  xml.Attribute("Type").Value;
            var t = (FilterType)Enum.Parse(typeof(FilterType), type);
            var f = new RegexFilter(t, name);
            f.Regex= xml.Attribute("Regex").Value;
            f.LineContains = xml.Attribute("LineContains").Value;
            return f;
        }

        public FilterType FilterType { get; set; }

    }

    public enum FilterType { Time, Numeric, Event, Boolean };
}
