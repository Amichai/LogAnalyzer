using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;
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

namespace LogAnalyzer {
    /// <summary>
    /// Interaction logic for SeriesSelector.xaml
    /// </summary>
    public partial class SeriesSelector : UserControl {
        public SeriesSelector() {
            InitializeComponent();
        }

        private void Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            ((sender as Rectangle).Tag as CustomSeries).ChangeColor();
        }

        public static Subject<CustomSeries> Remove = new Subject<CustomSeries>();

        private void Delete_Click(object sender, RoutedEventArgs e) {
            var s = ((sender as Button).Tag as CustomSeries);
            Remove.OnNext(s);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            this.xAxisComboBox.SelectedIndex = 0;
            this.yAxisComboBox.SelectedIndex = 1;
            this.chartTypeComboBox.SelectedIndex = 0;
        } 
    }
}
