using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleCropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }

        private void ButtonGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            //xPath.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#4e74ba");
        }

        private void ButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            //xPath.Fill = Brushes.CornflowerBlue;
        }

        private void ButtonGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //StartUpGrid.Visibility = Visibility.Collapsed;
        }
    }
}