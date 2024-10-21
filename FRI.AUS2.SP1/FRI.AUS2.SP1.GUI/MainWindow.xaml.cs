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

namespace FRI.AUS2.SP1.GUI
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

        #region UI Event Handlers
        private void _mnitem_Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    #endregion
}