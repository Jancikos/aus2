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
    /// Interaction logic for HeapFileStats.xaml
    /// </summary>
    public partial class HeapFileStats : UserControl
    {
        public HeapFileStats()
        {
            InitializeComponent();
        }

        public void UpdateStats(HeapFile<HeapData> structure)
        {
            _txt_FileSize.Value = structure.FileSize.ToString();
            _txt_BlockSize.Value = structure.BlockSize.ToString();
            _txt_BlockFactor.Value = structure.ActiveBlock.BlockFactor.ToString();
            _txt_TDataSize.Value = structure.ActiveBlock.TDataSize.ToString();
            _txt_BlockMetaSize.Value = structure.ActiveBlock.MetedataSize.ToString();

            _txt_BlockDataSize.Value = structure.ActiveBlock.DataSize.ToString();
            _txt_BlockCount.Value = structure.BlocksCount.ToString();

            _txt_NextFreeBlock.Value = (structure.NextFreeBlock?.ToString() ?? "?") + $" [{structure.FreeBlocksCount}]";
            _txt_NextEmptyBlock.Value = (structure.NextEmptyBlock?.ToString() ?? "?") + $" [{structure.EmptyBlocksCount}]";

            _txt_ValidItemsCount.Value = structure.ValidItemsCount.ToString();
        }
    }
}
