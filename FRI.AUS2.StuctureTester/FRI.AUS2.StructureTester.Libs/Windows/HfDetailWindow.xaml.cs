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
using System.Windows.Shapes;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.StructureTester.Libs.Windows
{
    /// <summary>
    /// Interaction logic for HfDetailWindow.xaml
    /// </summary>
    public partial class HfDetailWindow : Window
    {
        public HfDetailWindow()
        {
            InitializeComponent();
        }
        public void UpdateStats<T>(HeapFile<T> structure) where T : class, IHeapFileData, new()
        {
            _frm_HfBlocks.RerenderAllBlocks(structure);
            _frm_HfStats.UpdateStats(structure);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
