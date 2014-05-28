using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AST {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            this.AppStatePath =@"..\..\AppState.xml";
            var root = XElement.Load(AppStatePath);
            this.Library = new ParserLibrary();
            this.Library = ParserLibrary.FromXml(root.Element("ParserLibrary"));
            TreeNode.lib = this.Library;
            this.RootNode = new TreeNode(root.Element("InputText").Value);
            this.tree.ItemsSource = new List<TreeNode>() { this.RootNode };

            //this.RootNode = new TreeNode("abcdefgh");
        }

        private ParserLibrary _Library;
        public ParserLibrary Library {
            get { return _Library; }
            set {
                _Library = value;
                NotifyPropertyChanged();
            }
        }

        private TreeNode _RootNode;
        public TreeNode RootNode {
            get { return _RootNode; }
            set {
                _RootNode = value;
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

        private void Add_Click(object sender, RoutedEventArgs e) {
            this.Library.Rules.Add(new RegexParseRule());
        }

        public string AppStatePath { get; set; }

        private void Window_Closing_1(object sender, CancelEventArgs e) {
            //save();
        }

        private void save() {
            XElement root = new XElement("AppState");
            root.Add(new XElement("InputText", this.RootNode.TextValue));
            root.Add(this.Library.ToXml());
            root.Save(this.AppStatePath);
        }

        private void Save_Click_1(object sender, RoutedEventArgs e) {
            this.save();
        }

        private void Parse_Click_1(object sender, RoutedEventArgs e) {
            this.RootNode.Parse();
        }
    }
}
