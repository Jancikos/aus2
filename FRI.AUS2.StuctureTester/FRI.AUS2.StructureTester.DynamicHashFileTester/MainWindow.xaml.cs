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
using FRI.AUS2.StructureTester.DynamicHashFileTester.Models;

namespace FRI.AUS2.StructureTester.DynamicHashFileTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DynamicHashFile<DynamicHashFileData> _structure = new DynamicHashFile<DynamicHashFileData>();

        public MainWindow()
        {
            InitializeComponent();

            _btn_HashToAddress.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
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
            _txt_StructureDepth.Value = _structure.Depth.ToString();

            var addressIndex = _structure._getAddressIndex(hash);
            _txt_BlockIndex.Value = addressIndex.ToString();
            _txt_BlockIndexBinary.Value = addressIndex.ToBinaryString(_structure.Depth);
        }
    }
    #endregion
}