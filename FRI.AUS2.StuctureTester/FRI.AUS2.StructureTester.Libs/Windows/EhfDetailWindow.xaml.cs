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
    /// Interaction logic for EhfDetailWindow.xaml
    /// </summary>
    public partial class EhfDetailWindow : Window
    {
        public EhfDetailWindow()
        {
            InitializeComponent();
        }

        public void UpdateStats<T>(ExtendableHashFile<T> structure) where T : class, IExtendableHashFileData, new()
        {
            _frm_Detail.UpdateStats(structure);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
