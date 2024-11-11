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
using FRI.AUS2.SP1.GUI.Controls;
using FRI.AUS2.SP1.GUI.Windows;
using FRI.AUS2.SP1.Libs;
using FRI.AUS2.SP1.Libs.Models;
using Microsoft.Win32;

namespace FRI.AUS2.SP1.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SP1Backend _backend;

        private Uri DefaultExportFolder = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\export");

        public MainWindow()
        {
            InitializeComponent();

            _backend = new SP1Backend();

            _initializePropertiesManagment();
            _initializeParcelsManagment();
            _initializeCombinedItemsManagment();
        }

        private void _initializePropertiesManagment()
        {
            _initializeGeoItemsManagmentActions(_mng_Properties, _backend.AddProperty, (item) => _backend.DeleteProperty((Property)item));

            _mng_Properties.EditAction = (item) =>
            {
                var property = (Property)item;

                var form = new GeoItemFormWindow()
                {
                    Number = property.StreetNumber,
                    Description = property.Description,
                    PosA = property.PositionA ?? throw new Exception("Position A is null"),
                    PosB = property.PositionB ?? throw new Exception("Position B is null"),
                };

                form.ShowDialog();

                if (form.DialogResult == false)
                {
                    return;
                }

                _backend.EditProperty(
                    property,
                    new Property()
                    {
                        StreetNumber = form.Number,
                        Description = form.Description,
                        PositionA = form.PosA,
                        PositionB = form.PosB
                    }
                );

                MessageBox.Show($"Nehnuteľnosť upravená!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                RerenderTables();
            };

            // setup table orogin items source
            _mng_Properties.GetTableAllItemsSource = () => _backend.Properties;
            _mng_Properties.GetTableFilteredByPointItemsSource = _backend.FindProperties;

            // setup table columns
            _mng_Properties.AddTableColumn("Sup. č.", "StreetNumber");
            _mng_Properties.AddTableColumn("Popis", "Description");
            _mng_Properties.AddTableColumn("Pozícia A", "PositionA");
            _mng_Properties.AddTableColumn("Pozícia B", "PositionB");
            _mng_Properties.AddTableColumn("P. parc.", "ParcelsCount");

            // render table
            _mng_Properties.RerenderTable();
        }

        private void _initializeParcelsManagment()
        {
            _initializeGeoItemsManagmentActions(_mng_Parcels, _backend.AddParcel, (item) => _backend.DeleteParcel((Parcel)item));

            _mng_Parcels.EditAction = (item) =>
            {
                var parcel = (Parcel)item;

                var form = new GeoItemFormWindow()
                {
                    Number = parcel.Number,
                    Description = parcel.Description,
                    PosA = parcel.PositionA ?? throw new Exception("Position A is null"),
                    PosB = parcel.PositionB ?? throw new Exception("Position B is null"),
                };

                form.ShowDialog();

                if (form.DialogResult == false)
                {
                    return;
                }

                _backend.EditParcel(
                    parcel,
                    new Parcel()
                    {
                        Number = form.Number,
                        Description = form.Description,
                        PositionA = form.PosA,
                        PositionB = form.PosB
                    }
                );

                MessageBox.Show($"Parcela upravená!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                RerenderTables();
            };


            // setup table orogin items source
            _mng_Parcels.GetTableAllItemsSource = () => _backend.Parcels;
            _mng_Parcels.GetTableFilteredByPointItemsSource = _backend.FindParcels;

            // setup table columns
            _mng_Parcels.AddTableColumn("Čislo", "Number");
            _mng_Parcels.AddTableColumn("Popis", "Description");
            _mng_Parcels.AddTableColumn("Pozícia A", "PositionA");
            _mng_Parcels.AddTableColumn("Pozícia B", "PositionB");
            _mng_Parcels.AddTableColumn("P. nehn.", "PropertiesCount");

            // render table
            _mng_Parcels.RerenderTable();
        }

        private void _initializeCombinedItemsManagment()
        {
            // setup table orogin items source
            _mng_CombinedItems.GetTableAllItemsSource = () => _backend.Combined;
            _mng_CombinedItems.GetTableFilteredByRectangleItemsSource = _backend.FindCombined;
            _mng_CombinedItems.FilterMode = ItemsFilterMode.Rectangle;

            // setup visibility
            _mng_CombinedItems.ManageButtonsVisibility = Visibility.Collapsed;

            // setup table columns
            _mng_CombinedItems.AddTableColumn("Pozícia", "Position");
            _mng_CombinedItems.AddTableColumn("Data", "Item.Data");
        }
        
        private void _initializeGeoItemsManagmentActions(GeoItemsManagement mngItems, Action<int, string, GpsPoint, GpsPoint> addItemAction, Action<object> deleteItemAction)
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

                MessageBox.Show($"{mngItems.Title} pridaná!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                RerenderTables();
            };

            mngItems.DeleteAction = (item) =>
            {
                deleteItemAction(item);

                MessageBox.Show($"{mngItems.Title} odstránená!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                RerenderTables();
            };
        }

        public void RerenderTables()
        {
            _mng_Properties.RerenderTable();
            _mng_Parcels.RerenderTable();
            _mng_CombinedItems.RerenderTable();
        }

        #region UI Event Handlers
        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _mnitem_Clear_Click(object sender, RoutedEventArgs e)
        {
            _backend.ClearData();
            RerenderTables();
        }

        private void _mnitem_Generate_Click(object sender, RoutedEventArgs e)
        {
            var form = new GeoItemsGenerationFormWindow();

            form.ShowDialog();

            if (form.DialogResult == false)
            {
                return;
            }

            _backend.GenerateData(form.ParcelsCount, form.PropertiesCount, form.PropertiesOverlap, form.Seed, form.DoublePrecision);

            MessageBox.Show("Data vygenerované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

            RerenderTables();
        }

        private void _mnitem_Export_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DefaultExportFolder.LocalPath))
            {
                Directory.CreateDirectory(DefaultExportFolder.LocalPath);
            }

            var folderDialog = new OpenFolderDialog
            {
                Title = "Vyberte priečinok pre export dát",
                InitialDirectory = DefaultExportFolder.LocalPath
            };

            if (folderDialog.ShowDialog() != true)
            {
                MessageBox.Show($"Export dát zrušený!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFolder = new Uri(folderDialog.FolderName);
            var parcelsPath = saveFolder.LocalPath + @"\parcels.csv";
            var propertiesPath = saveFolder.LocalPath + @"\properties.csv";

            if (File.Exists(parcelsPath) || File.Exists(propertiesPath))
            {
                if (MessageBoxResult.No == MessageBox.Show($"Aktuálny export prepíše už existujúci export! Chcete pokračovať?", Title, MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    return;
                } 
            }

            var (propertiesCsv, parcelsCsv) = _backend.ExportData();
            File.WriteAllText(parcelsPath, Parcel.GetCsvHeader() + Environment.NewLine, Encoding.UTF8);
            File.WriteAllText(propertiesPath, Property.GetCsvHeader() + Environment.NewLine , Encoding.UTF8);

            File.AppendAllLines(parcelsPath, parcelsCsv, Encoding.UTF8);
            File.AppendAllLines(propertiesPath, propertiesCsv, Encoding.UTF8);

            MessageBox.Show($"Data exportované do {saveFolder.LocalPath}", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _mnitem_Import_Click(object sender, RoutedEventArgs e)
        {
            var importFolderDialog = new OpenFolderDialog
            {
                Title = "Vyberte priečinok s exportovanými dátami",
                InitialDirectory = DefaultExportFolder.LocalPath
            };

            if (importFolderDialog.ShowDialog() != true)
            {
                MessageBox.Show($"Import dát zrušený!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var importFolder = new Uri(importFolderDialog.FolderName);
            if (!Directory.Exists(importFolder.LocalPath))
            {
                MessageBox.Show($"Import priečinok neexistuje!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var parcelsPath = importFolder.LocalPath + @"\parcels.csv";
            var propertiesPath = importFolder.LocalPath + @"\properties.csv";

            if (!File.Exists(parcelsPath))
            {
                MessageBox.Show($"Súbor {parcelsPath} neexistuje!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!File.Exists(propertiesPath))
            {
                MessageBox.Show($"Súbor {propertiesPath} neexistuje!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var parcelsCsv = File.ReadAllLines(parcelsPath, Encoding.UTF8).Skip(1).ToArray();
            var propertiesCsv = File.ReadAllLines(propertiesPath, Encoding.UTF8).Skip(1).ToArray();

            _backend.ImportData(propertiesCsv, parcelsCsv);

            MessageBox.Show($"Data importované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

            RerenderTables();
        }
    }
    #endregion
}