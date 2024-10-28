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
        public (int min, int max) A
        {
            get
            {
                return (
                    (int) _frm_Min.A,
                    (int) _frm_Max.A
                );
            }
        }

        public (int min, int max) B
        {
            get
            {
                return (
                    int.Parse(_frm_Min.B),
                    int.Parse(_frm_Max.B)
                );
            }
        }

        public (int min, int max) C
        {
            get
            {
                return (
                    _frm_Min.C,
                    _frm_Max.C
                );
            }
        }

        public (int min, int max) D
        {
            get
            {
                return (
                    (int) _frm_Min.D,
                    (int) _frm_Max.D
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
            _frm_Min.A = 0;
            _frm_Max.A = 100;

            _frm_Min.B = "0";
            _frm_Max.B = "100";

            _frm_Min.C = 0;
            _frm_Max.C = 100;

            _frm_Min.D = 0;
            _frm_Max.D = 100;

            _txtb_Count.Text = "100";
            _txtb_Seed.Text = "0";
        }

        private void _txtb_Generate_Radnom_Int(object sender, MouseButtonEventArgs e)
        {
            // TODO - toto vytiahnut do nejakeho helpera
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
