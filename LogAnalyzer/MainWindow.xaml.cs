﻿using LogAnalyzer.Filters;
using Microsoft.Win32;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LogAnalyzer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            ///TODO: add a progress bar for the update button
            ///Highlight text on a background thread
            InitializeComponent();
            this.LinesToShow = Properties.Settings.Default.LinesToShow;
            this.Filepath = Properties.Settings.Default.LastFilepath;
            this.Filters = new ObservableCollection<IFilter>();
            
            this.loadSessions();
            this.loadFilters(this.Sessions.First());
        }

        ///TODO: allow users to work with many files simultaneously

        private int _LinesToShow;
        public int LinesToShow {
            get { return _LinesToShow; }
            set {
                _LinesToShow = value;
                Properties.Settings.Default.LinesToShow = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        private string _Filepath;
        public string Filepath {
            get { return _Filepath; }
            set {
                _Filepath = value;
                NotifyPropertyChanged();
                if (!System.IO.File.Exists(value)) {
                    return;
                }
                this.Lines = System.IO.File.ReadAllLines(this.Filepath).Select(i => new LogLine(i)).ToList();
                this.setUILines();
                Properties.Settings.Default.LastFilepath = this.Filepath;
                Properties.Settings.Default.Save();
            }
        }

        private ObservableCollection<IFilter> _Filters;
        public ObservableCollection<IFilter> Filters {
            get { return _Filters; }
            set {
                _Filters = value;
                NotifyPropertyChanged();
            }
        }

        private void updateHighlights() {
            var r = new RegexFilter(FilterType.Event, "name") {
                Regex = this.Regex
            };
            this.Lines.ForEach(i => i.ClearAllHighlights());
            this.Lines.ForEach(i => i.Highlight(r));
        }

        private string _Regex;
        public string Regex {
            get { return _Regex; }
            set {
                _Regex = value;
                //this.Lines.ForEach(i => i.UpdateHighlight(value));
                //var t = this.Lines;
                //this.Lines = null;
                //this.Lines = t;
                NotifyPropertyChanged();
            }
        }

        private XElement filtersElement() {
            XElement root = new XElement("Filters");
            foreach (var f in this.Filters) {
                root.Add(f.ToXml());
            }
            return root;
        }

        private string _DateTimeRegex;
        public string DateTimeRegex {
            get { return _DateTimeRegex; }
            set {
                _DateTimeRegex = value;
                NotifyPropertyChanged();
            }
        }

        private List<LogLine> _Lines;
        public List<LogLine> Lines {
            get { return _Lines; }
            set {
                _Lines = value;
                NotifyPropertyChanged();
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

        private void Open_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            this.Filepath = ofd.FileName;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            this.Regex = this.regText.Text;
        }

        private List<LogLine> _UILines;
        public List<LogLine> UILines {
            get { return _UILines; }
            set {
                _UILines = value;
                NotifyPropertyChanged();
            }
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e) {
            this.Filters.Add(new RegexFilter(FilterType.Event, "name") {
                Regex = this.Regex
            });
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            var filter = (sender as TextBox).Tag as RegexFilter;
            this.Regex = filter.Regex;
        }

        private void ListView_Selected(object sender, RoutedEventArgs e) {
            var lv = (sender as ListView);
            var selected = lv.SelectedItem ;
            if (selected == null) {
                var filters = (lv.ItemsSource as ObservableCollection<IFilter>);
                if (filters.Count() == 0) {
                    return;
                }
                selected = filters.First();
            }
            this.Regex = (selected as RegexFilter).Regex;
        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            var l = this.Lines;
            this.Lines = null;
            this.Lines = l;
            this.updateHighlights();
        }

        private void loadFilters(XElement root) {
            foreach (var f in root.Elements()) {
                var r = RegexFilter.FromXml(f);
                this.Filters.Add(r);
            }
        }

        private void PrintResults_Click(object sender, RoutedEventArgs e) {
            foreach (var l in this.Lines) {
                foreach (var f in this.Filters) {
                    Debug.Print(f.InspectionString(l.Value));
                }
            }
        }

        private class rollingAverage {
            public int Count { get; set; }
            public rollingAverage(int count) {
                this.Count = count;
                this.lastVals = new Queue<bool>();
            }

            public void Add(bool b) {
                this.lastVals.Enqueue(b);
                if (lastVals.Count() > this.Count) {
                    this.lastVals.Dequeue();
                }
            }

            public double Average() {
                var c = this.lastVals.Where(i => i).Count();
                if (c == 0) {
                    return 0;
                }
                return c / (double)this.lastVals.Count();
            }

            private Queue<bool> lastVals;
        }

        private void Chart_Click(object sender, RoutedEventArgs e) {
            Window w = new Window();
            var view = new PlotView();
            var model = new OxyPlot.PlotModel("% Is Charging");
            view.Model = model;
            w.Content = view;
            var scatterSeries = new OxyPlot.Series.LineSeries();
            scatterSeries.Color = OxyPlot.OxyColors.Black;
            scatterSeries.MarkerSize = .3;

            
            Dictionary<IFilter, object> lastVals = new Dictionary<IFilter, object>();
            var targetFilterCount = this.Filters.Where(i => i.ToDisplay).Count();
            foreach (var l in this.Lines) {
                foreach (var filter in this.Filters.Where(i => i.ToDisplay)) {
                    var v = (filter as RegexFilter).Vals(l.Value);
                    if (v != null) {
                        lastVals[filter] = v;
                    }
                    if (lastVals.Where(i => i.Value != null).Count() == targetFilterCount) {
                        var pt = FilterExtensions.GetDataPoint(lastVals);
                        scatterSeries.Points.Add(pt);
                        foreach (var key in lastVals.Keys.ToList()) {
                            lastVals[key] = null;
                        }
                    }
                }
            }

            model.Series.Add(scatterSeries);
            w.Show();
        }

        private void RefreshUILines_Click(object sender, RoutedEventArgs e) {
            setUILines();
        }

        private void setUILines() {
            this.UILines = this.Lines.Take(this.LinesToShow).ToList();
        }

        private ObservableCollection<Session> _Sessions;
        public ObservableCollection<Session> Sessions {
            get { return _Sessions; }
            set {
                _Sessions = value;
                NotifyPropertyChanged();
            }
        }

        private void loadSessions() {
            this.Sessions = new ObservableCollection<Session>();
            XElement root = XElement.Load(SESSION_FILE);
            foreach (var s in root.Elements()) {
                this.Sessions.Add(Session.Parse(s));
            }
        }

        private string SESSION_FILE = @"..\..\History\Session.xml"; 

        private void Window_Closing(object sender, CancelEventArgs e) {
        }

        private void saveSessions() {
            
            XElement root = XElement.Load(SESSION_FILE);
            root.RemoveAll();
            foreach (var s in this.Sessions) {
                root.Add(s.ToXml());
            }
            root.Save(SESSION_FILE);
        }

        private void DeleteSession_Click(object sender, RoutedEventArgs e) {
            var session = (sender as Button).Tag as Session;
            this.Sessions.Remove(session);
            this.saveSessions();
        }

        private void ImportSession_Click(object sender, RoutedEventArgs e) {
            var session = (sender as Button).Tag as Session;
            this.LinesToShow = session.LinesToShow;
            this.Filepath = session.Filepath;
            loadFilters(session);
        }

        private void loadFilters(Session session) {
            this.Filters.Clear();
            foreach (var f in session.Filters) {
                this.Filters.Add(f);
            }
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e) {
            Session newSession = new Session();
            newSession.Filepath = this.Filepath;
            newSession.Filters = this.Filters.Cast<RegexFilter>().ToList();
            newSession.Timestamp = DateTime.Now;
            newSession.LinesToShow = this.LinesToShow;
            this.Sessions.Add(newSession);
            this.saveSessions();
        }
    }
}
