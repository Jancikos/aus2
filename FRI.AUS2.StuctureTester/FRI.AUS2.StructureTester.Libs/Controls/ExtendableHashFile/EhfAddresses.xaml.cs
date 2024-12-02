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
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;

namespace FRI.AUS2.StructureTester.Libs.Controls.ExtendableHashFile
{
    /// <summary>
    /// Interaction logic for EhfAddresses.xaml
    /// </summary>
    public partial class EhfAddresses : UserControl
    {
        public EhfAddresses()
        {
            InitializeComponent();
        }

        public void UpdateStats<T>(ExtendableHashFile<T> structure) where T : class, IExtendableHashFileData, new()
        {
            _txt_StatsDepth.Value = structure.Depth.ToString();
            _txt_StatsAddressesCount.Value = structure.AddressesCount.ToString();

            _lstbx_Addresses.ItemsSource = structure.Depth > 7
                ? new List<string> { "Too many addresses to display" }
                : structure.Addresses.Select((b, i) => $"{i}. {i.ToBinaryString(structure.Depth, false)}: {b}");

        }
    }
}
