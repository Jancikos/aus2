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
    /// Interaction logic for InputDialogWindow.xaml
    /// </summary>
    public partial class InputDialogWindow : Window
    {
        public string Label
        {
            get => _txt_title.Text;
            set 
            {
                Title = value;
                _txt_title.Text = value;
            }
        }
        public string Input { get => _txtb.Text; set => _txtb.Text = value; }

        public InputDialogWindow()
        {
            InitializeComponent();
        }

        private void _btn_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // Close the dialog and indicate success
        }

        private void _btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Close the dialog and indicate cancellation
        }
    }
}
