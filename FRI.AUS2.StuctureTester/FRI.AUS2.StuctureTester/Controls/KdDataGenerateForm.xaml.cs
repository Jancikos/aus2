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

namespace FRI.AUS2.StuctureTester.Controls
{
    /// <summary>
    /// Interaction logic for KdDataGenerateForm.xaml
    /// </summary>
    public partial class KdDataGenerateForm : UserControl
    {
        public (int min, int max) X
        {
            get
            {
                return (
                    _frm_Min.X,
                    _frm_Max.X
                );
            }
        }

        public (int min, int max) Y
        {
            get
            {
                return (
                    _frm_Min.Y,
                    _frm_Max.Y
                );
            }
        }

        public int Seed
        {
            get
            {
                return int.Parse(_txtb_Seed.Text);
            }
        }

        public int Count
        {
            get
            {
                return int.Parse(_txtb_Count.Text);
            }
        }

        public KdDataGenerateForm()
        {
            InitializeComponent();

            _initializeValues();
        }

        private void _initializeValues()
        {
            _frm_Min.X = _frm_Min.Y = 0;
            _frm_Max.X = _frm_Max.Y = 50;

            _txtb_Count.Text = "20000";
            _txtb_Seed.Text = "0";
        }

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
}
