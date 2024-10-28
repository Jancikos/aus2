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
using static FRI.AUS2.StuctureTester.MainWindow;

namespace FRI.AUS2.StuctureTester.Controls
{
    /// <summary>
    /// Interaction logic for KdDataForm.xaml
    /// </summary>
    public partial class KdDataForm : UserControl
    {
        public double A
        {
            get
            {
                return double.Parse(_txtb_A.Text);
            }
            set
            {
                _txtb_A.Text = value.ToString();
            }
        }

        public string B 
        {
            get
            {
                return _txtb_B.Text;
            }
            set
            {
                _txtb_B.Text = value;
            }
        }

        public int C
        {
            get
            {
                return int.Parse(_txtb_C.Text);
            }
            set
            {
                _txtb_C.Text = value.ToString();
            }
        }

        public double D
        {
            get
            {
                return double.Parse(_txtb_D.Text);
            }
            set
            {
                _txtb_D.Text = value.ToString();
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

        public int Data
        {
            get
            {
                return int.Parse(_txtb_Data.Text);
            }
            set
            {
                _txtb_Data.Text = value.ToString();
            }
        }

        public KdExampleData KdDataModel
        {
            get
            {
                return new KdExampleData()
                {
                    A = A,
                    B = B,
                    C = C,
                    D = D,
                    Data = IsDataVisibile ? Data : 0
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

            if (txtb.Text == "")
            {
                txtb.Text = random.Next(0, 100).ToString();
                return;
            }

            if (int.TryParse(txtb.Text, out int result))
            {
                txtb.Text = (result + 1).ToString();
                return;
            }
        }
    }
    #endregion
}
