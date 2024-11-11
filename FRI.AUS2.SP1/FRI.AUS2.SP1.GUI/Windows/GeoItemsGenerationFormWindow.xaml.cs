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

namespace FRI.AUS2.SP1.GUI.Windows
{
    /// <summary>
    /// Interaction logic for PropertyGenerationWindow.xaml
    /// </summary>
    public partial class GeoItemsGenerationFormWindow : Window
    {
        public int ParcelsCount 
        {
            get 
            {
                return int.Parse(_txtb_ParcelsCount.Text);
            }
        }

        public int PropertiesCount
        {
            get
            {
                return int.Parse(_txtb_PropertiesCount.Text);
            }
        }

        public double PropertiesOverlap
        {
            get
            {
                return double.Parse(_txtb_PropertiesOverlap.Text);
            }
        }

        public int Seed
        {
            get
            {
                return int.Parse(_txtb_Seed.Text);
            }
        }

        public int DoublePrecision
        {
            get
            {
                return int.Parse(_txtb_DoublePrecision.Text);
            }
        }

        public GeoItemsGenerationFormWindow()
        {
            InitializeComponent();
        }

        private void _mnitem_Generate_Click(object sender, RoutedEventArgs e)
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
