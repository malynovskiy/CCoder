using CCoder.Models;
using CCoder.Views;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CCoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CodeTextBox.Highlighter = HighlightingManager.Instance.Highlighters["CBaseSyntax"];
        }

        private void onTextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            CodeTextBox.Text = FileTextBox.Text;
        }

        private void onSaveClicked(object sender, RoutedEventArgs e)
        {
            MainView view = (MainView)DataContext;
            if (view == null)
                return;

            // updating Document.Text (from ModernTextBox) before doing Save command
            ref Document doc = ref view.getCurrentDocument();
            if (doc != null && CodeTextBox.Text != null)
                doc.Text = CodeTextBox.Text;
        }
    }
}
