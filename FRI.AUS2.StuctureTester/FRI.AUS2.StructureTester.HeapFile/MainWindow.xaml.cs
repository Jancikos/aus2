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
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.HeapFileTester.Models;
using FRI.AUS2.StructureTester.HeapFileTester.Utils;
using FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator;

namespace FRI.AUS2.StructureTester.HeapFileTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Uri DefaultFilesFolder = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\HeapFileTester\");

        private HeapFile<HeapData> _structure;


        public MainWindow()
        {
            InitializeComponent();

            if (!System.IO.Directory.Exists(DefaultFilesFolder.LocalPath))
            {
                System.IO.Directory.CreateDirectory(DefaultFilesFolder.LocalPath);
            }

            _frm_Insert.InitilizeDefaultValues();

            _frm_Blocks.OnItemDoubleClicked += (address, data) => {
                _frm_FindFilter.Address = address;
                _frm_FindFilter.HeapData = (HeapData)data;

                _frm_DeleteFilter.Address = address;
                _frm_DeleteFilter.HeapData = (HeapData)data;
            };

            _initOperationsGenerator();

            _structure = new(500, new(DefaultFilesFolder.LocalPath + "HeapData.bin"));
            _structure.Clear();
            _rerenderAllStats();
        }

        private void _initOperationsGenerator()
        {
            _frm_OperationsGenerator.OpereationRatioInsertDuplicate = 0;
            _frm_OperationsGenerator.OpereationRatioFind = 0;
            _frm_OperationsGenerator.OpereationRatioDelete = 0;

            _frm_OperationsGenerator.InitializeForm(new HeapFileOperationsGenerator(_structure));
            _frm_OperationsGenerator.RunTest += (sender, e) => {
                var operatiosnGenerator = new HeapFileOperationsGenerator(_structure);

                _frm_OperationsGenerator.InitializeGenerator(operatiosnGenerator);

                try {
                    operatiosnGenerator.Generate();
                    
                    MessageBox.Show("Operations generated", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                _rerenderAllStats();
            };
        }

        private void _rerenderAllStats()
        {
            _frm_Stats.UpdateStats(_structure);
            _frm_Blocks.RerenderAllBlocks(_structure);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _structure.Dispose();

            base.OnClosing(e);
        }

        #region UI Events

        private void _mnitem_FileClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the file
            _structure.Clear();
            
            MessageBox.Show("Structure cleared!", Title);
            _rerenderAllStats();
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Close();
        }

        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {
            // Insert a new record
            var data = _frm_Insert.HeapData;

            var address = _structure.Insert(data);

            MessageBox.Show($"Data inserted at address {address}", Title);
            _rerenderAllStats();
        }

        private void _btn_ManualByteSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "HeapDataManualExport.bin";
            string filePath = System.IO.Path.Combine(DefaultFilesFolder.LocalPath, fileName);

            // Save the file
            var data = _frm_Insert.HeapData;
            var bytes = data.ToBytes();

            // var bytes = _frm_Insert.HeapData.ToBytes();
            System.IO.File.WriteAllBytes(filePath, bytes);

            MessageBox.Show($"Data saved to {filePath}", Title);
        }

        private void _btn_ManualByteLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = DefaultFilesFolder.LocalPath,
                Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                MessageBox.Show("No file selected", Title);
                return;
            }
            
            string filePath = openFileDialog.FileName;

            // Load the file
            var bytes = System.IO.File.ReadAllBytes(filePath);
            var heapData = new HeapData();
            heapData.FromBytes(bytes);

            _frm_Insert.HeapData = heapData;

            MessageBox.Show($"Data loaded from {filePath}", Title);
        }

        private void _btn_Find_Click(object sender, RoutedEventArgs e)
        {
            var data = _structure.Find(
                _frm_FindFilter.Address,
                _frm_FindFilter.HeapData
            );

            if (data is null) {
                MessageBox.Show("Data not found", Title);
                data = new HeapData();
            }

            _frm_FindResult.HeapData = data;
        }

        private void _btn_Generate_Click(object sender, RoutedEventArgs e)
        {
            var dataGenerator = new HeapDataGenerator(int.Parse(_txtbx_GenerateSeed.Value));
            var dataCount = int.Parse(_txtbx_GenerateCount.Value);

            foreach (var data in dataGenerator.GenerateItems(dataCount))
            {
                _structure.Insert(data);
            }

            MessageBox.Show($"Generated {dataCount} records", Title);
            _rerenderAllStats();
        }

        private void _btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            var data = _frm_DeleteFilter.HeapData;

            string result = "Data deleted";
            try {
                _structure.Delete(
                    _frm_DeleteFilter.Address,
                    data
                );
            } catch (Exception ex) {
                result = ex.Message;
            }

            MessageBox.Show(result, Title);
            _txt_DeleteResult.Text = result;
            _rerenderAllStats();
        }
    }
        #endregion
}