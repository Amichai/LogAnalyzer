using AST.ParseRules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AST {
    public class ParserLibrary {
        public ParserLibrary() {
            this.Rules = new ObservableCollection<RegexParseRule>();
        }
        public ObservableCollection<RegexParseRule> Rules { get; set; }

        internal XElement ToXml() {
            XElement root = new XElement("ParserLibrary");
            foreach (var r in this.Rules) {
                root.Add(r.ToXml());
            }
            return root;
        }

        internal static ParserLibrary FromXml(XElement root) {
            ParserLibrary toReturn = new ParserLibrary();
            foreach(var r in root.Elements("ParseRule")) {
                toReturn.Rules.Add(RegexParseRule.FromXml(r));
            }
            return toReturn;
        }
    }
}
