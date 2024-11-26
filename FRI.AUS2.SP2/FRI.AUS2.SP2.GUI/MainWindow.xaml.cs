using FRI.AUS2.SP2.Libs;
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

namespace FRI.AUS2.SP2.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SP2Backend _backend;
        public MainWindow()
        {
            _backend = new SP2Backend();

            InitializeComponent();
        }

        #region UI Event Handlers
        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _mnitem_Clear_Click(object sender, RoutedEventArgs e)
        {
            //_backend.ClearData();
            //RerenderTables();
        }

        private void _mnitem_Generate_Click(object sender, RoutedEventArgs e)
        {
            //var form = new GeoItemsGenerationFormWindow();

            //form.ShowDialog();

            //if (form.DialogResult == false)
            //{
            //    return;
            //}

            //_backend.GenerateData(form.ParcelsCount, form.PropertiesCount, form.PropertiesOverlap, form.Seed, form.DoublePrecision);

            MessageBox.Show("Data vygenerované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

            //RerenderTables();
        }
    }
    #endregion
}