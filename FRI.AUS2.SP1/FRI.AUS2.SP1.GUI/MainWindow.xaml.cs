using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FRI.AUS2.SP1.GUI.Windows;
using FRI.AUS2.SP1.Libs;
using FRI.AUS2.SP1.Libs.Models;

namespace FRI.AUS2.SP1.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SP1Backend _backend;

        public MainWindow()
        {
            InitializeComponent();

            _backend = new SP1Backend();

            _initializePropertiesManagment();
        }

        private void _initializePropertiesManagment()
        {
            _mng_Properties.InsertAction = () =>
            {
                var form = new PropertyFormWindow();

                form.ShowDialog();

                if (form.DialogResult == false)
                {
                    return;
                }

                _backend.AddProperty(
                    form.StreetNumber,
                    form.Description,
                    form.PosA,
                    form.PosB
                );

                MessageBox.Show("Property added!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            };
        }

        #region UI Event Handlers
        private void _mnitem_Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Test", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
    #endregion
}