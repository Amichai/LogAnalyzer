using LogAnalyzer.Filters;
using Microsoft.Win32;
using OxyPlot.Series;
using OxyPlot.Wpf;
using ResultStore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using LogAnalyzer.Util;

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
            this.StartLine = Properties.Settings.Default.StartLine;
            this.loadSessions();
            this.CurrentSession = this.Sessions.First();
            this.Filepath = CurrentSession.Files.First();
        }

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

        private int _StartLine;
        public int StartLine {
            get { return _StartLine; }
            set {
                _StartLine = value;
                Properties.Settings.Default.StartLine = value;
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
            }
        }

        private void updateHighlights(RegexFilter r) {
            this.Lines.ForEach(i => i.ClearAllHighlights());
            this.Lines.ForEach(i => i.Highlight(r));
        }

        private void updateHighlights() {
            var r = new RegexFilter(FilterType.Event, "name") {
                Regex = this.Regex
            };
            this.updateHighlights(r);
        }

        private string _Regex;
        public string Regex {
            get { return _Regex; }
            set {
                _Regex = value;
                NotifyPropertyChanged();
            }
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
                NotifyPropertyChanged("LineCount");
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
            var f = new RegexFilter(FilterType.Event, "name") {
                Regex = this.Regex
            };
            this.CurrentSession.Filters.Add(f);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            var filter = (sender as TextBox).Tag as RegexFilter;
            this.Regex = filter.Regex;
        }

        ///TODO: persist chart builder settings in the session file
        private void ListView_Selected(object sender, RoutedEventArgs e) {
            var lv = (sender as ListView);
            var selected = lv.SelectedItem ;
            if (selected == null) {
                var filters = (lv.ItemsSource as ObservableCollection<RegexFilter>);
                if (filters.Count() == 0) {
                    return;
                }
                selected = filters.First();
            }
            this.Regex = (selected as RegexFilter).Regex;
        }

        public int LineCount {
            get {
                if (this.Lines == null) {
                    return 0;
                }
                return this.Lines.Count();
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            this.updateHighlights();
        }

        private IEnumerable<string> getAllMatchedLines() {
            foreach (var l in this.Lines) {
                foreach (var f in this.CurrentSession.Filters) {
                    var r = f.InspectionString(l.Value);
                    if (string.IsNullOrWhiteSpace(r)) {
                        continue;
                    }
                    yield return r;
                    
                }
            }
        }

        private void PrintResults_Click(object sender, RoutedEventArgs e) {
            foreach (var r in this.getAllMatchedLines()) {
                Debug.Print(r);
            }
        }

        private void RefreshUILines_Click(object sender, RoutedEventArgs e) {
            setUILines();
        }

        private void setUILines() {
            this.UILines = this.Lines.Skip(this.StartLine).Take(this.LinesToShow).ToList();
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
                this.Sessions.Add(Session.FromXml(s));
            }
        }

        private string SESSION_FILE = @"..\..\History\Session.xml"; 

        private void Window_Closing(object sender, CancelEventArgs e) {
        }

        private void saveSessions() {
            XElement newXml = new XElement("Sessions");
            foreach (var s in this.Sessions) {
                newXml.Add(s.ToXml());
            }
            newXml.Save(SESSION_FILE);
        }

        private void DeleteSession_Click(object sender, RoutedEventArgs e) {
            var a = MessageBox.Show("Are you sure you want to delete this session?", "Delete", MessageBoxButton.OKCancel); ///Give a session name or identifier

            if (a != MessageBoxResult.OK) {
                return;
            }

            var session = (sender as Button).Tag as Session;
            this.Sessions.Remove(session);
            this.saveSessions();
        }

        private Session _CurrentSession;
        public Session CurrentSession {
            get { return _CurrentSession; }
            set {
                _CurrentSession = value;
                NotifyPropertyChanged();
                //TODO: invert this relationship so that chart builder is part of the session state
                this.ChartBuilder.SetSession(this.CurrentSession);
            }
        }

        private void ImportSession_Click(object sender, RoutedEventArgs e) {
            var session = (sender as Button).Tag as Session;
            this.CurrentSession = session;
            this.LinesToShow = session.LinesToShow;
            this.StartLine = session.StartLine;
            this.Filepath = session.Files.First();
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e) {
            //Session newSession = new Session();
            //newSession.Filepath = this.Filepath;
            //newSession.Filters = this.Filters.ToList();
            //newSession.Timestamp = DateTime.Now;
            //newSession.LinesToShow = this.LinesToShow;
            //newSession.StartLine = this.StartLine;
            ///TODO: not working yet. Get rid of the locals filters list. Bind to current session only.
            this.Sessions.Add(this.CurrentSession);
            this.saveSessions();
        }

        private void Export_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".txt";
            sfd.ShowDialog();
            var filepath = sfd.FileName;
            using (var w = new StreamWriter(filepath, false)) {
                foreach (var r in this.getAllMatchedLines()) {
                    w.WriteLine(r);
                }
                w.Flush();
            }
            Process.Start(filepath);
        }

        //TODO: advanced search functionality with line numbers

        private void SaveChanges_Click(object sender, RoutedEventArgs e) {
            this.saveSessions();
        }

        private void TestRegexFilter_Click(object sender, RoutedEventArgs e) {
            RegexFilter f = (sender as Button).Tag as RegexFilter;
            this.updateHighlights(f);
        }

        private void DeleteFilter_Click(object sender, RoutedEventArgs e) {
            RegexFilter f = (sender as Button).Tag as RegexFilter;
            this.CurrentSession.Filters.Remove(f);
        }

        private CustomAnalysis customAnalysis;

        ///TODO: Functionality for selecting a subset of the file
        private void Custom_Click(object sender, RoutedEventArgs e) {
            customAnalysis = new CustomAnalysis(this.ChartBuilder.Lines, this.CurrentSession.Filters.ToList());
            //customAnalysis.custom2();

            customAnalysis.DistanceHistogram(this.CurrentSession.Files.ToList());
        }

        ///TODO: BUg: added files are not available in the chartbuilder
        private void Add_Click(object sender, RoutedEventArgs e) {
            this.CurrentSession.Files.Add(this.Filepath);
        }

        private void DeleteSessionFile_Click(object sender, RoutedEventArgs e) {
            var file = (sender as Button).Tag as string;
            this.CurrentSession.Files.Remove(file);
        }

        private void ListBox_Selected(object sender, RoutedEventArgs e) {
            this.Filepath = this.sessionFiles.SelectedValue as string;
        }

        private void TextBlock_PreviewMouseDown_1(object sender, RoutedEventArgs e) {
            var t = ((sender as TextBlock).Tag as Session);
            t.IsInEditMode = !t.IsInEditMode;
        }
        private void SaveSessionName_Click_1(object sender, RoutedEventArgs e) {
            this.saveSessions();
            ((sender as Button).Tag as Session).IsInEditMode = false;
        }
    }
}
