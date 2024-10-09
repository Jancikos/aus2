using FRI.AUS2.Libs;
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

namespace FRI.AUS2.StuctureTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExampleStructure _exampleStructure;

        public MainWindow()
        {   
            InitializeComponent();

            _exampleStructure = new ExampleStructure();
        }

        private void _mnitem_Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"ExampleStructure items count: {_exampleStructure.ItemsCount++}", Title);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}