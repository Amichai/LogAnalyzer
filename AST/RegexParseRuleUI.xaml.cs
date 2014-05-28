using AST.ParseRules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace AST {
    /// <summary>
    /// Interaction logic for RegexParseRule.xaml
    /// </summary>
    public partial class RegexParseRuleUI : UserControl, INotifyPropertyChanged {
        public RegexParseRuleUI() {
            InitializeComponent();
        }

        public RegexParseRule ParseRule {
            get { return (RegexParseRule)GetValue(ParseRuleProperty); }
            set { SetValue(ParseRuleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParseRule.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParseRuleProperty =
            DependencyProperty.Register("ParseRule", typeof(RegexParseRule), typeof(RegexParseRuleUI), new PropertyMetadata(default(RegexParseRule)));

        


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
