using Microsoft.Win32;

using System.IO;
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
        private string _selectedDirectory = string.Empty;
        readonly string[] _supportedExtentions = { ".jpg", ".png", ".bmp", ".tiff" };

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
            AddDirectoryIcon.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#4e74ba");
        }

        private void ButtonGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            AddDirectoryIcon.Foreground = Brushes.CornflowerBlue;
        }

        private void ButtonGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Select Folder";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Multiselect = false; 

            if (dialog.ShowDialog() == true)
            {
                _selectedDirectory = dialog.FolderName;
                ShowFirstImageOfDirectory();
                AddDirectoryIconGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowFirstImageOfDirectory()
        {
            var filePaths = Directory.EnumerateFiles(_selectedDirectory, "*.*", SearchOption.AllDirectories);
            string? firstImagePath = filePaths.FirstOrDefault(s => _supportedExtentions.Any(x => s.ToLower().EndsWith(x)));
            if (firstImagePath == null)
            {
                return;
            }

            PART_CropViewer.ImageSource = new BitmapImage(new Uri(firstImagePath, UriKind.Absolute));
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            // 동작 코드 추가


            AddDirectoryIconGrid.Visibility = Visibility.Visible;
        }

        private void CropImages()
        {
            var filePaths = Directory.EnumerateFiles(_selectedDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => _supportedExtentions.Any(x => s.ToLower().EndsWith(x)));
        }
    }
}