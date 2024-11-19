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

            _structure = new(500, new(DefaultFilesFolder.LocalPath + "HeapData.bin"));
            _rerenderAllStats();

            _frm_Insert.InitilizeDefaultValues();

            _initOperationsGenerator();

            _structure.Clear();
            _rerenderAllStats();
        }

        private void _initOperationsGenerator()
        {
            _frm_OperationsGenerator.OpereationRatioInsertDuplicate = 0;
            _frm_OperationsGenerator.OpereationRatioFindSpecific = 0;
            _frm_OperationsGenerator.OpereationRatioDeleteSpecific = 0;

            _frm_OperationsGenerator.InitializeForm(new HeapFileOperationsGenerator(_structure));
            _frm_OperationsGenerator.RunTest += (sender, e) => {
                var operatiosnGenerator = new HeapFileOperationsGenerator(_structure);

                _frm_OperationsGenerator.InitializeGenerator(operatiosnGenerator);

                operatiosnGenerator.Generate();

                _rerenderAllStats();
            };
        }

        private void _rerenderAllStats()
        {
            UpdateStructureStatistics();
            RerenderAllBlocks();
        }

        public void UpdateStructureStatistics()
        {
            _txt_BlockSize.Value = _structure.BlockSize.ToString();
            _txt_BlockFactor.Value = _structure.ActiveBlock.BlockFactor.ToString();
            _txt_TDataSize.Value = _structure.ActiveBlock.TDataSize.ToString();
            _txt_BlockMetaSize.Value = _structure.ActiveBlock.MetedataSize.ToString();

            _txt_BlockDataSize.Value = _structure.ActiveBlock.DataSize.ToString();
            _txt_BlockCount.Value = _structure.BlocksCount.ToString();

            _txt_NextFreeBlock.Value = (_structure.NextFreeBlock?.ToString() ?? "?") + $" [{_structure.FreeBlocksCount}]";
            _txt_NextEmptyBlock.Value = (_structure.NextEmptyBlock?.ToString() ?? "?") + $" [{_structure.EmptyBlocksCount}]";
        }

        public void RerenderAllBlocks()
        {
            _treeView_Blocks.Items.Clear();

            int i = 0;
            foreach (var block in _structure.GetAllDataBlocks())
            {
                var blockItem = new TreeViewItem()
                {
                    Header = $"{i + 1}. block [{block.ValidCount}]",
                    IsExpanded = true
                };

                var items = new List<TreeViewItem>() 
                {
                    new TreeViewItem() { Header = $"Address: {(i+1) * _structure.BlockSize}", Tag = (i+1) * _structure.BlockSize  },
                    new TreeViewItem() { Header = $"Prev: {block.PreviousBlock?.ToString() ?? "?"}, Next: {block.NextBlock?.ToString() ?? "?"}" }
                };

                var dataItems = new TreeViewItem() { 
                    Header = "Data [" + block.ValidCount + "]",
                    Tag = "Data"
                };
                
                for (int j = 0; j < block.ValidCount; j++)
                {
                    var data = block.Items[j];

                    dataItems.Items.Add(new TreeViewItem() 
                    { 
                        Header = data.ToString(),
                        Tag = data
                    });
                }
                items.Add(dataItems);


                blockItem.ItemsSource = items;
                _treeView_Blocks.Items.Add(blockItem);

                ++i;
            }
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

            for (int i = 0; i < dataCount; i++)
            {
                _structure.Insert(dataGenerator.GenerateItem());
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

        private void _treeView_Blocks_OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var parentItem = (TreeViewItem)sender;

            TreeViewItem? AddressItem = null; 
            foreach (var item in parentItem.Items)
            {
                if (((TreeViewItem)item).Header.ToString().StartsWith("Address:"))
                {
                    AddressItem = (TreeViewItem)item;
                    break;
                }
            }

            TreeViewItem? dataItem = null; 
            foreach (var item in parentItem.Items)
            {
                if (((TreeViewItem)item).Tag == "Data")
                {
                    dataItem = (TreeViewItem)item;
                    break;
                }
            }
            if (dataItem is null)
            {
                e.Handled = false;
                return;
            }

            TreeViewItem? selectedItem = null;
            HeapData? selectedData = null;
            int i = 0;
            while (selectedData is null && i < dataItem.Items.Count)
            {
                selectedItem = (TreeViewItem)dataItem.Items[i++];
                if (selectedItem.IsSelected)
                {
                    selectedData = (HeapData)selectedItem.Tag;
                }
            }

            if (selectedData is null)
            {
                e.Handled = false;
                return;
            }

            _frm_FindFilter.Address = (int)AddressItem.Tag;
            _frm_FindFilter.HeapData = selectedData;

            _frm_DeleteFilter.Address = (int)AddressItem.Tag;
            _frm_DeleteFilter.HeapData = selectedData;
        }
    }
        #endregion
}