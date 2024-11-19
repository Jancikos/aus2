using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace FRI.AUS2.StructureTester.HeapFileTester.Controls
{
    /// <summary>
    /// Interaction logic for HeapFileBlocks.xaml
    /// </summary>
    public partial class HeapFileBlocks : UserControl
    {
        public Action<int, HeapData> OnItemDoubleClicked;

        public HeapFileBlocks()
        {
            InitializeComponent();
        }
        public void RerenderAllBlocks(HeapFile<HeapData> structure)
        {
            _treeView_Blocks.Items.Clear();

            int i = 0;
            foreach (var block in structure.GetAllDataBlocks())
            {
                var blockItem = new TreeViewItem()
                {
                    Header = $"{i + 1}. block [{block.ValidCount}]",
                    IsExpanded = true
                };

                var items = new List<TreeViewItem>() 
                {
                    new TreeViewItem() { Header = $"Address: {(i+1) * structure.BlockSize}", Tag = (i+1) * structure.BlockSize  },
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

            OnItemDoubleClicked?.Invoke((int)AddressItem.Tag, selectedData);
        }
    }
}
