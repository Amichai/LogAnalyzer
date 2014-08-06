using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ResultStore {
    public class Experiment {
        private static string FILE_PATH = @"..\..\..\ResultStore\ResultStore.xml";
        public Experiment() {
            this.Created = DateTime.Now;
            ResultStore.Trial.ValueUpdated += (s, e) => {
                this.save();
            };
        }

        public string Name { get; private set; }
        public DateTime Created { get; set; }

        public static Experiment FromXml(XElement xml) {
            Experiment e = new Experiment();
            e.Name = xml.Attribute("Name").Value;
            e.Created = DateTime.Parse(xml.Attribute("Created").Value);
            foreach (var t in xml.Elements()) {
                var trial = ResultStore.Trial.FromXml(t);
                e.Trials.Add(trial);
            }
            return e;
        }

        public XElement ToXml() {
            XElement toReturn = new XElement("Experiments");
            XElement experiment = new XElement("Experiment");
            experiment.Add(new XAttribute("Name", this.Name));
            experiment.Add(new XAttribute("Created", this.Created));
            foreach (var t in this.Trials) {
                experiment.Add(t.ToXml());
            }
            toReturn.Add(experiment);
            return toReturn;
        }

        public static Experiment Get(string name) {
            XElement xml = XElement.Load(FILE_PATH);
            int count = 0;
            var matched = xml.Elements().Where(i => i.Attribute("Name").Value == name);
            count = matched.Count();
            if (count > 1) {
                throw new Exception("More than one experiment with the same name");
            } else if (count == 1) {
                return Experiment.FromXml(matched.Single());
            } else {
                Experiment toReturn = new Experiment();
                toReturn.Name = name;
                xml.Add(toReturn.ToXml());
                xml.Save(FILE_PATH);
                return toReturn;
            }
        }

        public List<Trial> Trials = new List<Trial>();

        private void save() {
            this.ToXml().Save(FILE_PATH);
        }

        public Trial Trial(DateTime dateTime) {
            var matched = this.Trials.Where(i => i.Timestamp == dateTime);
            var count = matched.Count();
            if (count == 0) {
                Trial toReturn = new Trial(dateTime);
                this.Trials.Add(toReturn);
                this.save();
                return toReturn;
            } if (count == 1) {
                return matched.Single();
            } else {
                throw new Exception("More than one trial with the same name");
            }
        }
    }
}
