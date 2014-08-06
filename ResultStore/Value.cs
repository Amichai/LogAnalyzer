using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResultStore {
    public class Value {
        public Value(string key, object value, string type = null, string description = "") {
            this.Key = key;
            this.Val = value;
            if (type == null) {
                this.Type = value.GetType().Name;
            } else {
                this.Type = type;
            }
            
            this.Description = description;
        }

        public string Key { get; set; }
        public object Val { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public XElement ToXml() {
            XElement value = new XElement("Value");
            value.Add(new XAttribute("Key", this.Key));
            value.Add(new XAttribute("Val", this.Val));
            value.Add(new XAttribute("Type", this.Type));
            value.Add(new XAttribute("Description", this.Description));
            return value;
        }

        internal static Value FromXml(XElement v) {
            var k = v.Attribute("Key").Value;
            var t = v.Attribute("Type").Value;
            var val = v.Attribute("Val").Value;
            var d = v.Attribute("Description").Value;
            Value toReturn = new Value(k, val, t, d);

            return toReturn;
        }
    }
}
