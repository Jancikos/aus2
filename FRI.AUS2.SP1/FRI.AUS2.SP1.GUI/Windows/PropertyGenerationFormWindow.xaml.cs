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
    public partial class PropertyGenerationFormWindow : Window
    {
        public int Count 
        {
            get 
            {
                return int.Parse(_txtb_Count.Text);
            }
        }

        public int Seed
        {
            get
            {
                return int.Parse(_txtb_Seed.Text);
            }
        }

        public string DescriptionPrefix
        {
            get
            {
                return _txtb_DescriptionPrefix.Text;
            }
        }

        public (int min, int max) StreetNumber
        {
            get
            {
                return (
                    int.Parse(_txtb_StreetNumberMin.Text), 
                    int.Parse(_txtb_StreetNumberMax.Text)
                );
            }
        }

        public (int min, int max) PosA_X
        {
            get
            {
                return (
                    int.Parse(_txtb_PosA_XMin.Text),
                    int.Parse(_txtb_PosA_XMax.Text)
                );
            }
        }

        public (int min, int max) PosA_Y
        {
            get
            {
                return (
                    int.Parse(_txtb_PosA_YMin.Text),
                    int.Parse(_txtb_PosA_YMax.Text)
                );
            }
        }

        public (int min, int max) PosB_X
        {
            get
            {
                return (
                    int.Parse(_txtb_PosB_XMin.Text),
                    int.Parse(_txtb_PosB_XMax.Text)
                );
            }
        }

        public (int min, int max) PosB_Y
        {
            get
            {
                return (
                    int.Parse(_txtb_PosB_YMin.Text),
                    int.Parse(_txtb_PosB_YMax.Text)
                );
            }
        }



        
        public PropertyGenerationFormWindow()
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
