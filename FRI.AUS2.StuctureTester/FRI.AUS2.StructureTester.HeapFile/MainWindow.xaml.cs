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

            _structure = new(1024, new(DefaultFilesFolder.LocalPath + "HeapData.bin"));
            UpdateStructureStatistics();

            if (!System.IO.Directory.Exists(DefaultFilesFolder.LocalPath))
            {
                System.IO.Directory.CreateDirectory(DefaultFilesFolder.LocalPath);
            }

            _frm_Insert.InitilizeDefaultValues();
        }

        public void UpdateStructureStatistics()
        {
            _txt_BlockSize.Value = _structure.BlockSize.ToString();
            _txt_BlockFactor.Value = _structure.ActiveBlock.BlockFactor.ToString();
            _txt_TDataSize.Value = _structure.ActiveBlock.TDataSize.ToString();
            _txt_BlockMetaSize.Value = _structure.ActiveBlock.MetedataSize.ToString();
            _txt_BlockDataSize.Value = _structure.ActiveBlock.DataSize.ToString();
        }

        #region UI Events

        private void _mnitem_FileClear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the file
            MessageBox.Show("Clearing the file", Title);
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
                int.Parse(_txtbx_FindBlockAddress.Value),
                new HeapData()
                {
                    Id = int.Parse(_txtbx_FindDataId.Value)
                }
            );

            if (data is null) {
                MessageBox.Show("Data not found", Title);
                data = new HeapData();
            }

            _frm_FindResult.HeapData = data;
        }
    }
        #endregion
}