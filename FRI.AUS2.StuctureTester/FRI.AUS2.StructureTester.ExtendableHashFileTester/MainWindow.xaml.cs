using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.ExtendableHashFileTester.Models;
using FRI.AUS2.StructureTester.ExtendableHashFileTester.Utils;
using FRI.AUS2.StructureTester.HeapFileTester.Models;
using FRI.AUS2.StructureTester.HeapFileTester.Utils;

namespace FRI.AUS2.StructureTester.ExtendableHashFileTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Uri DefaultFilesFolder = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\ExtendableHashFileTester\");

        private ExtendableHashFile<HeapData> _structure;

        public MainWindow()
        {
            InitializeComponent();

            _structure = new ExtendableHashFile<HeapData>(500, new(DefaultFilesFolder.LocalPath + "ExtendableHashFileTester.bin"));

            _frm_Insert.OnIdChanged = _setSelectedId;
            _frm_Insert.InitilizeDefaultValues();

            _frm_HeapBlocks.OnItemDoubleClicked += (address, data) => {
                _setSelectedId(data.Id);
            };

            _initOperationsGenerator();

            _updateStructureStats();
        }

        private void _setSelectedId(int id)
        {
            _frm_FindFilter.Id = id;
            _frm_DeleteFilter.Id = id;

            _txtbx_Hash.Value = id.ToString();
            _btn_HashToAddress.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void _initOperationsGenerator()
        {
            _frm_OperationsGenerator.Count = 100;
            _frm_OperationsGenerator.OpereationRatioInsertDuplicate = 0;
            _frm_OperationsGenerator.OpereationRatioFind = 0;
            _frm_OperationsGenerator.OpereationRatioFindSpecific = 1;
            _frm_OperationsGenerator.OpereationRatioDelete = 0;
            _frm_OperationsGenerator.OpereationRatioDeleteSpecific = 0;

            _frm_OperationsGenerator.InitializeForm(new ExtendableHashFileOperationsGenerator(_structure));
            _frm_OperationsGenerator.RunTest += (sender, e) => {
                var operatiosnGenerator = new ExtendableHashFileOperationsGenerator(_structure);

                _frm_OperationsGenerator.InitializeGenerator(operatiosnGenerator);

                try {
                    operatiosnGenerator.Generate();
                    
                    MessageBox.Show("Operations generated", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                _updateStructureStats();
            };
        }

        private void _updateStructureStats()
        {
            _txt_StatsDepth.Value = _structure.Depth.ToString();
            _txt_StatsAddressesCount.Value = _structure.AddressesCount.ToString();
            int i = 0;
            _lstbx_Addresses.ItemsSource = _structure.Addresses.Select(b => $"{i++.ToBinaryString(_structure.Depth, false)}: {b}");

            _frm_HeapStats.UpdateStats(_structure.HeapFile);
            _frm_HeapBlocks.RerenderAllBlocks(_structure.HeapFile);
        }

        #region UI Events

        private void _mnitem_FileClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the file
            // _structure.Clear();

            MessageBox.Show("Structure cleared!", Title);
            // _rerenderAllStats();
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Close();
        }

        private void _btn_HashToAddress_Click(object sender, RoutedEventArgs e)
        {
            var hash = int.Parse(_txtbx_Hash.Value);

            _txt_HashBinary.Value = hash.ToBinaryString();

            var addressIndex = _structure._getAddressIndex(new BitArray(BitConverter.GetBytes(hash)));
            _txt_BlockIndex.Value = addressIndex.ToString();
            _txt_BlockIndexBinary.Value = addressIndex.ToBinaryString(_structure.Depth, false);
        }

        private void _btn_IncreaseDepth_Click(object sender, RoutedEventArgs e)
        {
            try {
                _structure._increaseDepth();
            } catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _updateStructureStats();
        }

        
        private void _btn_DecreaseDepth_Click(object sender, RoutedEventArgs e)
        {
            try {
                _structure._decreaseDepth();
            } catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _updateStructureStats();
        }

        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {
            // Insert a new record
            var data = _frm_Insert.HeapData;

            try
            {
                _structure.Insert(data);

                MessageBox.Show($"Data inserted.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _updateStructureStats();
        }

        private void _btn_ManualInsertFormRefresh_Click(object sender, RoutedEventArgs e)
        {
            _frm_Insert.Id++;
            _frm_Insert.InitilizeDefaultValues();
        }

        private void _btn_Find_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var data = _structure.Find(
                    _frm_FindFilter.HeapData
                );

                if (data is null)
                {
                    MessageBox.Show("Data not found!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show($"Data found!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                _frm_FindResult.HeapData = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_Generate_Click(object sender, RoutedEventArgs e)
        {
            var dataGenerator = new HeapDataGenerator(int.Parse(_txtbx_GenerateSeed.Value));
            var dataCount = int.Parse(_txtbx_GenerateCount.Value);

            for (int i = 0; i < dataCount; i++)
            {
                _structure.Insert(dataGenerator.GenerateItem());
            }

            MessageBox.Show($"Generated {dataCount} records", Title);
            _updateStructureStats();
        }

        private void _btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = _frm_DeleteFilter.HeapData;

            try
            {
                _structure.Delete(
                    data
                );
               MessageBox.Show("Data deleted", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _updateStructureStats();
        }
    }
    #endregion
}