using FRI.AUS2.SP1.Libs.Models;
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
    /// Interaction logic for PropertyFormWindow.xaml
    /// </summary>
    public partial class GeoItemFormWindow : Window
    {
        public int Number
        {
            get
            {
                return int.Parse(_txtb_Number.Text);
            }
            set
            {
                _txtb_Number.Text = value.ToString();
            }
        }

        public string Description
        {
            get
            {
                return _txtb_Description.Text;
            }
            set
            {
                _txtb_Description.Text = value;
            }
        }

        public GpsPoint PosA
        {
            get
            {
                return new GpsPoint()
                {
                    X = double.Parse(_txtb_PosA_X.Text),
                    Y = double.Parse(_txtb_PosA_Y.Text)
                };
            }
            set
            {
                _txtb_PosA_X.Text = value.X.ToString();
                _txtb_PosA_Y.Text = value.Y.ToString();
            }
        }

        public GpsPoint PosB
        {
            get
            {
                return new GpsPoint()
                {
                    X = double.Parse(_txtb_PosB_X.Text),
                    Y = double.Parse(_txtb_PosB_Y.Text)
                };
            }
            set
            {
                _txtb_PosB_X.Text = value.X.ToString();
                _txtb_PosB_Y.Text = value.Y.ToString();
            }
        }

        public GeoItemFormWindow()
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
