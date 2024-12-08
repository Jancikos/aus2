using FRI.AUS2.SP2.GUI.Windows;
using FRI.AUS2.SP2.Libs;
using FRI.AUS2.SP2.Libs.Models;
using FRI.AUS2.StructureTester.Libs.Windows;
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

namespace FRI.AUS2.SP2.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Uri DefaultFilesFolder = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\AUS2\SP2\");
        SP2Backend _backend;

        private EhfDetailWindow? _ehfByIdsWindow = null;
        private EhfDetailWindow? _ehfByEcvWindow = null;
        private HfDetailWindow? _hfWindow = null;
     
        public MainWindow()
        {
            _backend = new SP2Backend(10000, DefaultFilesFolder);

            InitializeComponent();

            _frm_Insert.GenerateId = _backend.Generator.GenerateId;
            _frm_Insert.GenerateEcv = _backend.Generator.GenerateECV;
        }

        private void _renderCustomer(Customer customer)
        {
            _frm_Display.Customer = customer;
        }

        private void _rerenderStats()
        {
            _ehfByEcvWindow?.UpdateStats(_backend._dataByEcv);
            _ehfByIdsWindow?.UpdateStats(_backend._dataById);
            _hfWindow?.UpdateStats(_backend._allData);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            _backend.Dispose();
        }

        #region UI Event Handlers
        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();

            _ehfByEcvWindow?.Close();
            _ehfByIdsWindow?.Close();
            _hfWindow?.Close();
        }

        private void _mnitem_Clear_Click(object sender, RoutedEventArgs e)
        {
            _backend.Clear();
            _rerenderStats();

            MessageBox.Show("Dáta vyčistené!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _mnitem_Generate_Click(object sender, RoutedEventArgs e)
        {
            var form = new GenerateCustomersWindow();

            form.ShowDialog();

            if (form.DialogResult == false)
            {
               return;
            }

            try {
                _backend.GenerateCustomers(form.Count, form.Seed, form.Fast);

                MessageBox.Show("Data vygenerované!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }


            _rerenderStats();
        }

        private void _mnitem_TesterIds_Click(object sender, RoutedEventArgs e)
        {
            if (_ehfByIdsWindow is null)
            {
                _ehfByIdsWindow = new EhfDetailWindow();
                _ehfByIdsWindow.Title = "EHF detail - IDs";

                _ehfByIdsWindow.Closed += (s, e) => _ehfByIdsWindow = null;
                
                _ehfByIdsWindow.Show();
            }

            _ehfByIdsWindow.UpdateStats(_backend._dataById);

            _ehfByIdsWindow.Activate();
        }

        private void _mnitem_TesterECVs_Click(object sender, RoutedEventArgs e)
        {
            if (_ehfByEcvWindow is null)
            {
                _ehfByEcvWindow = new EhfDetailWindow();
                _ehfByEcvWindow.Title = "EHF detail - ECVs";

                _ehfByEcvWindow.Closed += (s, e) => _ehfByEcvWindow = null;

                _ehfByEcvWindow.Show();
            }

            _ehfByEcvWindow.UpdateStats(_backend._dataByEcv);

            _ehfByEcvWindow.Activate();
        }

        private void _mnitem_TesterCustomers_Click(object sender, RoutedEventArgs e)
        {
            if (_hfWindow is null)
            {
                _hfWindow = new HfDetailWindow();
                _hfWindow.Title = "HF detail - Customers";

                _hfWindow.Closed += (s, e) => _hfWindow = null;

                _hfWindow.Show();
            }

            _hfWindow.UpdateStats(_backend._allData);

            _hfWindow.Activate();
        }

        private void _mnitem_TesterAll_Click(object sender, RoutedEventArgs e)
        {
            _mnitem_TesterIds_Click(sender, e);
            _mnitem_TesterECVs_Click(sender, e);
            _mnitem_TesterCustomers_Click(sender, e);
        }
        private void _btn_ManualRandom_Click(object sender, RoutedEventArgs e)
        {
            _frm_Insert.Customer = _backend.Generator.GenerateCustomer();
        }
        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {
            var customer = _frm_Insert.Customer;

            if (!customer.IsValid())
            {
                MessageBox.Show("Vyplnte všetky polia!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                _backend.AddCustomer(customer);

                MessageBox.Show("Zákazník pridaný!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                _renderCustomer(customer);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _rerenderStats();
        }

        private void _btn_FindById_Click(object sender, RoutedEventArgs e)
        {
            try {
                var customer = _backend.GetCustomerById(int.Parse(_txtbx_FindId.Value));

                _renderCustomer(customer);

                MessageBox.Show("Nájdené!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void _btn_FindByEcv_Click(object sender, RoutedEventArgs e)
        {
            try {
                var customer = _backend.GetCustomerByEcv(_txtbx_FindEcv.Value);

                _renderCustomer(customer);

                MessageBox.Show("Nájdené!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_DeleteById_Click(object sender, RoutedEventArgs e)
        {
            try {
                _backend.DeleteCustomer(int.Parse(_txtbx_DeleteId.Value));

                MessageBox.Show("Zákazník zmazaný!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                _rerenderStats();
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_DeleteByEcv_Click(object sender, RoutedEventArgs e)
        {
            try {
                _backend.DeleteCustomer(_txtbx_DeleteEcv.Value);

                MessageBox.Show("Zákazník zmazaný!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                _rerenderStats();
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_UpdateById_Click(object sender, RoutedEventArgs e)
        {
            try {
                var customer = _frm_Display.Customer;

                if (!customer.IsValid())
                {
                    MessageBox.Show("Vyplnte všetky polia!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _backend.UpdateCustomerById(customer);

                MessageBox.Show("Zákazník upravený!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _rerenderStats();
        }

        private void _btn_UpdateByEcv_Click(object sender, RoutedEventArgs e)
        {
            try {
                var customer = _frm_Display.Customer;

                if (!customer.IsValid())
                {
                    MessageBox.Show("Vyplnte všetky polia!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _backend.UpdateCustomerByEcv(customer);

                MessageBox.Show("Zákazník upravený!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Chyba: {ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _rerenderStats();
        }
    }
    #endregion
}