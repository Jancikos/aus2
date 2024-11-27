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

namespace FRI.AUS2.StructureTester.Libs.Controls.ExtendableHashFile
{
    /// <summary>
    /// Interaction logic for EhfDetail.xaml
    /// </summary>
    public partial class EhfDetail : UserControl
    {
        public EhfDetail()
        {
            InitializeComponent();
        }

        public void UpdateStats<T>(ExtendableHashFile<T> structure) where T : class, IExtendableHashFileData, new()
        {
            _frm_Addresses.UpdateStats(structure);

            _frm_HfBlocks.RerenderAllBlocks(structure.HeapFile);
            _frm_HfStats.UpdateStats(structure.HeapFile);
        }
    }
}
