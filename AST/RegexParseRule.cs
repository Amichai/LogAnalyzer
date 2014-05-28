using AST.ParseRules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AST {
    public class RegexParseRule : IParse, INotifyPropertyChanged {
        public RegexParseRule() {
            this.Name = "";
        }
        private string _NodeCapture;
        public string NodeCapture {
            get { return _NodeCapture; }
            set {
                _NodeCapture = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TestResult");
            }
        }

        private string _ChildCapture;
        public string ChildCapture {
            get { return _ChildCapture; }
            set {
                _ChildCapture = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TestResult");
            }
        }

        private string _TestCase;
        public string TestCase {
            get { return _TestCase; }
            set {
                _TestCase = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TestResult");
            }
        }

        public string TestResult {
            get {
                string inspectionVal;
                string nodeVal;
                List<string> children;
                if (this.InspectionCapture == null || NodeCapture == null) {
                    return "";
                }
                try {
                    this.Parse(this.TestCase, out inspectionVal, out nodeVal, out children);
                } catch (Exception ex) {
                    return ex.Message;
                }

                return string.Format("Inspection: {0}, Node: {1}, Children: {2}", inspectionVal, nodeVal, string.Join(", ", children));
            }
        }

        private string _InspectionCapture;
        public string InspectionCapture {
            get { return _InspectionCapture; }
            set {
                _InspectionCapture = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("TestResult");
            }
        }

        private string _Name;
        public string Name {
            get { return _Name; }
            set {
                _Name = value;
                NotifyPropertyChanged();
            }
        }

        public void Parse(string text, out string inspection, out string node, out List<string> children) {
            Regex r = new Regex(InspectionCapture);
            inspection = r.Match(text).ToString();
            r = new Regex(NodeCapture);
            node = r.Match(inspection).ToString();
            r = new Regex(ChildCapture);
            children = new List<string>();
            foreach (var m in r.Matches(inspection)) {
                children.Add(m.ToString());
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

        internal XElement ToXml() {
            XElement root = new XElement("ParseRule");
            root.Add(new XAttribute("Name", this.Name));
            root.Add(new XAttribute("InspectionCapture", this.InspectionCapture));
            root.Add(new XAttribute("NodeCapture", this.NodeCapture));
            root.Add(new XAttribute("ChildCapture", this.ChildCapture));
            root.Add(new XAttribute("TestCase", this.TestCase));
            return root;
        }

        internal static RegexParseRule FromXml(XElement root) {
            RegexParseRule toReturn = new RegexParseRule();
            toReturn.Name = root.Attribute("Name").Value;
            toReturn.InspectionCapture = root.Attribute("InspectionCapture").Value;
            toReturn.NodeCapture = root.Attribute("NodeCapture").Value;
            toReturn.ChildCapture = root.Attribute("ChildCapture").Value;
            toReturn.TestCase = root.Attribute("TestCase").Value;

            return toReturn;
           

        }
    }
}
