using LogAnalyzer.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LogAnalyzer {
    public class LogLine : INotifyPropertyChanged {
        public LogLine(string val) {
            this.Value = val;
            this.HighlightedText = "";
            this.Highlights = new ObservableCollection<Highlight>();
        }
        public string Value { get; private set; }
        public string HighlightedText { get; private set; }

        public override string ToString() {
            return Value;
        }

        private Size MeasureString(string candidate) {
            int trailingWhiteSpaces = countTrailingWhitespaces(candidate);

            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Courier"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                12,
                Brushes.Black);

            return new Size(formattedText.Width + trailingWhiteSpaces * spaceWidth, formattedText.Height);
        }


        private ObservableCollection<Highlight> _Highlights;
        public ObservableCollection<Highlight> Highlights {
            get { return _Highlights; }
            set {
                _Highlights = value;
                NotifyPropertyChanged();
            }
        }
        private double spaceWidth = 3.286666666666667;

        private int countTrailingWhitespaces(string val) {
            int count = 0;
            for (int i = val.Length - 1; i >= 0; i--) {
                if (char.IsWhiteSpace(val[i])) {
                    count++;
                } else {
                    return count;
                }
            }
            return count;
        }


        private Highlight getHighlight(string prefix, string val, ref double offset) {
            var p = this.MeasureString(prefix);
            var s = this.MeasureString(val);
            var width = s.Width;
            var height = s.Height;
            var padding = p.Width;
            
            var h = new Highlight() {
                Left = padding - offset,
                Height = height, 
                Width = width
            };
            offset = padding + width;
            return h;
        }

        public void ClearAllHighlights() {
            this.Highlights.Clear();
        }

        public bool Highlight(IFilter filter) {
            var inspection = filter.InspectionString(this.Value);
            if (!string.IsNullOrWhiteSpace(inspection)) {
                return this.Highlight(inspection);
            } return true;
        }

        public bool Highlight(string toHighlight) {
            try {
                double offset = 0;
                int startIdx = this.Value.IndexOf(toHighlight);
                var prefix = string.Concat(this.Value.Take(startIdx));
                var h = getHighlight(prefix, toHighlight, ref offset);
                //Debug.Print("{0}, prefix: {1}, left: {2}, width: {3}, offset: {4}", m.Value, prefix, h.Left, h.Width, currentOffset);
                this.Highlights.Add(h);
                //this.HighlightedText = r.Match(this.Value).Value;
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        ////public bool UpdateHighlight(string regex) {
        ////    try {
        ////        this.Highlights.Clear();
        ////        Regex r = new Regex(regex);
        ////        double currentOffset = 0;
        ////        foreach (Match m in r.Matches(this.Value)) {
        ////            var prefix = string.Concat(this.Value.Take(m.Index));
        ////            var h = getHighlight(prefix, m.Value, ref currentOffset);
        ////            //Debug.Print("{0}, prefix: {1}, left: {2}, width: {3}, offset: {4}", m.Value, prefix, h.Left, h.Width, currentOffset);
        ////            this.Highlights.Add(h);
        ////        }
        ////        //this.HighlightedText = r.Match(this.Value).Value;
        ////        return true;
        ////    } catch (Exception ex){
        ////        return false;
        ////    }
        ////}

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
