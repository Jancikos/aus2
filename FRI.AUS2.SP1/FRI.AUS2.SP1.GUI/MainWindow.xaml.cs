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
using FRI.AUS2.SP1.GUI.Controls;
using FRI.AUS2.SP1.GUI.Windows;
using FRI.AUS2.SP1.Libs;
using FRI.AUS2.SP1.Libs.Models;

namespace FRI.AUS2.SP1.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SP1Backend _backend;

        public MainWindow()
        {
            InitializeComponent();

            _backend = new SP1Backend();

            _initializePropertiesManagment();
        }

        private void _initializePropertiesManagment()
        {
            _initializePropertiesManagmentActions(_mng_Properties, _backend.AddProperty, _backend.GenerateProperties);

            // setup table orogin items source
            _mng_Properties.GetTableAllItemsSource = () => _backend.Properties;

            // setup table columns
            _mng_Properties.AddTableColumn("Sup. č.", "StreetNumber");
            _mng_Properties.AddTableColumn("Popis", "Description");
            _mng_Properties.AddTableColumn("Pozícia A", "PositionA");
            _mng_Properties.AddTableColumn("Pozícia B", "PositionB");
        }
        
        private void _initializePropertiesManagmentActions(GeoItemsManagement mngItems, Action<int, string, GpsPoint, GpsPoint> addItemAction, Action<int, int, string, (int, int), (int, int), (int, int), (int, int), (int, int)> generateItemsAction)
        {
            mngItems.InsertAction = () =>
            {
                var form = new GeoItemFormWindow();

                form.ShowDialog();

                if (form.DialogResult == false)
                {
                    return;
                }

                addItemAction(
                    form.Number,
                    form.Description,
                    form.PosA,
                    form.PosB
                );

                mngItems.RerenderTable();

                MessageBox.Show($"{mngItems.Title} pridaná!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            };

            mngItems.GenerateAction = () =>
            {
                var form = new GeoItemGenerationFormWindow();

                form.ShowDialog();

                if (form.DialogResult == false)
                {
                    return;
                }

                generateItemsAction(
                    form.Count,
                    form.Seed,
                    form.DescriptionPrefix,
                    form.StreetNumber,
                    form.PosA_X,
                    form.PosA_Y,
                    form.PosB_X,
                    form.PosB_Y
                );

                mngItems.RerenderTable();

                MessageBox.Show($"{mngItems.Title} vygenerované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            };
        }


        #region UI Event Handlers
        private void _mnitem_Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Test", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    #endregion
}