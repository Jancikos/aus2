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
    /// Interaction logic for GenerateCustomersWindow.xaml
    /// </summary>
    public partial class GenerateCustomersWindow : Window
    {
        public int Count 
        {
            get
            {
                return int.Parse(_txtb_Count.Value);
            }
            set
            {
                _txtb_Count.Value = value.ToString();
            }
        }

        public int? Seed
        {
            get
            {
                try {
                    return int.Parse(_txtb_Seed.Value);
                } catch (FormatException) {
                    return null;
                }
            }
            set
            {
                _txtb_Seed.Value = value?.ToString() ?? "0";
            }
        }

        public GenerateCustomersWindow()
        {
            InitializeComponent();
        }

        private void _mnitem_Generate_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
