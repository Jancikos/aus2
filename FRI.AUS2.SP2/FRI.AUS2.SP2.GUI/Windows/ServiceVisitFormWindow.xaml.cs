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

namespace FRI.AUS2.SP2.GUI.Windows
{
    /// <summary>
    /// Interaction logic for ServiceVisitFormWindow.xaml
    /// </summary>
    public partial class ServiceVisitFormWindow : Window
    {
        public ServiceVisitFormWindow()
        {
            InitializeComponent();
        }

        private void _mnitem_Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
