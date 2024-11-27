using FRI.AUS2.SP2.Libs;
using FRI.AUS2.SP2.Libs.Models;
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
        private Uri DefaultFilesFolder = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\SP2\");
     
        SP2Backend _backend;
        public MainWindow()
        {
            _backend = new SP2Backend(1000, DefaultFilesFolder);

            InitializeComponent();
        }

        private void _renderCustomer(Customer customer)
        {
            _frm_Display.Customer = customer;
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

        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void _btn_FindById_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nájdené!" + _txtbx_FindId.Value, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void _btn_FindByEcv_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nájdené!" + _txtbx_FindEcv.Value, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _btn_Update_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aktualizované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void _btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Zatial neimplementovane!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    #endregion
}