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

namespace FRI.AUS2.SP1.GUI.Controls
{
    /// <summary>
    /// Interaction logic for GeoItemsManagement.xaml
    /// </summary>
    public partial class GeoItemsManagement : UserControl
    {
        public string Title { get; set; } = "Geo Items Management";

        public GeoItemsManagement()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        #region  UI Event Handlers
        private void _btn_Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void _btn_Insert_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    #endregion
}
