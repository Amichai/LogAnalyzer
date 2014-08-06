using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResultStore {
    public class Trial {
        public Trial(DateTime time) {
            this.Timestamp = time;
        }
        internal XElement ToXml() {
            XElement toReturn = new XElement("Trial");
            toReturn.Add(new XAttribute("Timestamp", this.Timestamp));
            foreach (var v in this.Values) {
                toReturn.Add(v.ToXml());
            }
            return toReturn;
        }

        public static Trial FromXml(XElement xml) {
            DateTime time = DateTime.Parse(xml.Attribute("Timestamp").Value);
            var t = new Trial(time);
            foreach (var v in xml.Elements()) {
                t.Values.Add(ResultStore.Value.FromXml(v));
            }
            return t;
        }

        public DateTime Timestamp { get; private set; }

        public readonly List<Value> Values = new List<Value>();

        public static event EventHandler ValueUpdated;
        private void OnValueUpdated() {
            var handler = ValueUpdated;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        public void SetValue(string key, object value, string type, string description = "") {
            var matched = this.Values.Where(i => i.Key == key);
            if (matched.Count() == 0) {
                this.Values.Add(new Value(key, value, type, description));
                this.OnValueUpdated();
            } else if (matched.Count() == 1) {
                var m = matched.Single();
                m.Val = value;
                m.Type = type;
                m.Description = description;
                this.OnValueUpdated();
            } else {
                throw new Exception("More than one value with this key");
            }
        }
    }
}
