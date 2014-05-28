using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AST {
    public class TreeNode : INotifyPropertyChanged {
        public TreeNode(string val) {
            Debug.Print("node initialized: " + val);
            this.TextValue = val;
            this.Value = val;
            this.Children = new List<TreeNode>();
            this.parseResult();
        }

        private string _TextValue;
        public string TextValue {
            get { return _TextValue; }
            set {
                _TextValue = value;
                NotifyPropertyChanged();
            }
        }

        private string _Value;
        public string Value {
            get { return _Value; }
            set {
                _Value = value;
                NotifyPropertyChanged();
            }
        }

        private string _Type;
        public string Type {
            get { return _Type; }
            set {
                _Type = value;
                NotifyPropertyChanged();
            }
        }

        private string _AsString;
        public string AsString {
            get { return _AsString; }
            set {
                _AsString = value;
                NotifyPropertyChanged();
            }
        }

        public static ParserLibrary lib;

        private bool stringRemainder(string full, string substring, out string prefix, out string suffix) {
            int fullLength = full.Count();
            int subLength = substring.Count();
            if (fullLength == subLength) {
                prefix = "";
                suffix = "";
                return true;
            }
            var index = full.IndexOf(substring);
            if (index == -1) {
                throw new Exception("Substring not found");
            }
            prefix = string.Concat(full.Take(index));
            suffix = string.Concat(full.Skip(subLength + index));
            return false;           
        }

        private void parseResult() {
            foreach (var n in lib.Rules) {
                string inspectionVal, nodeVal;
                List<string> children;
                n.Parse(this.TextValue, out inspectionVal, out nodeVal, out children);
                if (string.IsNullOrWhiteSpace(nodeVal)) {
                    continue;
                }

                TreeNode newNode = new TreeNode(nodeVal);
                newNode.Children = children.Select(i => new TreeNode(i)).ToList();
                string prefix, suffix;
                stringRemainder(this.TextValue, nodeVal, out prefix, out suffix);
                if (!string.IsNullOrWhiteSpace(prefix)) {
                    this.Children.Add(new TreeNode(prefix));
                }
                this.Children.Add(newNode);
                if (!string.IsNullOrWhiteSpace(suffix)) {
                    this.Children.Add(new TreeNode(suffix));
                }
                break;
            }
            this.AsString = string.Format("Text: {0}, children: {1}, val: {2}", this.TextValue, this.Children.Count(), this.Value);
            NotifyPropertyChanged("Children");
        }

        private List<TreeNode> _Children;
        public List<TreeNode> Children {
            get { return _Children; }
            set {
                _Children = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasChildren {
            get {
                return this.Children.Count() > 0;
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
            XElement node = new XElement("Node");
            node.Add(new XAttribute("Value", this.Value));
            node.Add(new XAttribute("TextValue", this.TextValue));
            XElement children = new XElement("Children");
            foreach (var c in this.Children) {
                children.Add(c.ToXml());
            }
            if (this.HasChildren) {
                node.Add(children);
            }
            return node;
        }

        internal static TreeNode FromXml(XElement root) {
            var value = root.Attribute("Value").Value;
            var textVal = root.Attribute("TextValue").Value;
            TreeNode toReturn = new TreeNode(textVal);
            toReturn.Value = value;
            if (root.Element("Children") == null) {
                return toReturn;
            }

            foreach (var c in root.Element("Children").Elements()) {
                var p = TreeNode.FromXml(c);
                if (p == null) {
                    continue;
                }
                toReturn.Children.Add(p);
            }
            return toReturn;

        }

        internal void Parse() {
            this.parseResult();
        }
    }
}
