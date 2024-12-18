﻿using System;
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
using static FRI.AUS2.StuctureTester.MainWindow;

namespace FRI.AUS2.StuctureTester.Controls
{
    /// <summary>
    /// Interaction logic for KdDataForm.xaml
    /// </summary>
    public partial class KdDataForm : UserControl
    {
        public int X
        {
            get
            {
                return int.Parse(_txtb_X.Text);
            }
            set
            {
                _txtb_X.Text = value.ToString();
            }
        }

        public int Y 
        {
            get
            {
                return int.Parse(_txtb_Y.Text);
            }
            set
            {
                _txtb_Y.Text = value.ToString();
            }
        }

        public bool IsDataVisibile
        {
            set
            {
                var newVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                _txt_Data.Visibility = newVisibility;
                _txtb_Data.Visibility = newVisibility;
            }
            get
            {
                 return _txtb_Data.Visibility == Visibility.Visible;
            }
        }

        public string Data
        {
            get
            {
                return _txtb_Data.Text;
            }
            set
            {
                _txtb_Data.Text = value;
            }
        }

        public KdExampleData KdDataModel
        {
            get
            {
                return new KdExampleData()
                {
                    X = X,
                    Y = Y,
                    Data = IsDataVisibile ? Data : ""
                };
            }
        }
        public bool IsKdDataModelValid
        {
            get
            {
                try {
                    var data = KdDataModel;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public KdDataForm()
        {
            InitializeComponent();
        }

        #region UI Events
        private void _txtb_Generate_Radnom_Int(object sender, MouseButtonEventArgs e)
        {
            var random = new Random();
            var txtb = (TextBox)sender;

            txtb.Text = KdExampleData.GetRandomPosition(random).ToString();
        }

        private void _txtb_Generate_Radnom_String(object sender, MouseButtonEventArgs e)
        {
            var random = new Random();
            var txtb = (TextBox)sender;

            txtb.Text = KdExampleData.GetRandomData(random).ToString();
        }
    }
    #endregion
}
