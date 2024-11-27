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
using FRI.AUS2.SP2.GUI.Windows;
using FRI.AUS2.SP2.Libs.Models;

namespace FRI.AUS2.SP2.GUI.Controls
{
    /// <summary>
    /// Interaction logic for CustomerForm.xaml
    /// </summary>
    public partial class CustomerForm : UserControl
    {
        public Action<int>? OnIdChanged;
        public int Id
        {
            get { return int.Parse(_txtb_Id.Text); }
            set { _txtb_Id.Text = value.ToString(); }
        }

        public string Firstname
        {
            get { return _txtb_Firstname.Text; }
            set { _txtb_Firstname.Text = value; }
        }

        public string Lastname
        {
            get { return _txtb_Lastname.Text; }
            set { _txtb_Lastname.Text = value; }
        }

        public string ECV
        {
            get { return _txtb_Ecv.Text; }
            set { _txtb_Ecv.Text = value; }
        }

        private List<ServiceVisit> _items = new List<ServiceVisit>(Customer.VisitsMaxCount);
        public List<ServiceVisit> Items
        {
            get => _items;
            set
            {
                _items = value;
                _lview_Visits.Items.Clear();
                _items.ForEach(visit => _lview_Visits.Items.Add(visit));
            }
        }

        public bool ManageItems
        {
            get => _grbx_Visits.Visibility == Visibility.Visible;
            set => _grbx_Visits.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool ManageIds
        {
            get => _txtb_Id.IsEnabled;
            set 
            {
                _txtb_Id.IsEnabled = value;
                _txtb_Ecv.IsEnabled = value;
            }
        }

        public Customer Customer
        {
            get
            {
                return new Customer
                {
                    Id = Id,
                    Firstname = Firstname,
                    Lastname = Lastname,
                    ECV = ECV,
                    Visits = Items
                };
            }
            set
            {
                Id = value.Id;
                Firstname = value.Firstname;
                Lastname = value.Lastname;
                ECV = value.ECV;
                Items = value.Visits;
            }
        }

        public CustomerForm()
        {
            InitializeComponent();
        }

        private void _txtb_Id_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(_txtb_Id.Text, out int result))
            {
                OnIdChanged?.Invoke(result);
            }
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

        private void _grbx_Visits_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void _btn_AddVisit_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count >= Customer.VisitsMaxCount)
            {
                MessageBox.Show($"Max visits count is {Customer.VisitsMaxCount}");
                return;
            }

            var visitFormWindow = new ServiceVisitFormWindow();

            // todo doplnit setovanie nahodnymi hodnotami
            visitFormWindow.Form.ServiceVisit = new ServiceVisit()
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Descriptions = new string[] { "Description 1", "Description 2" },
                Price = 10.0
            };

            if (visitFormWindow.ShowDialog() == false)
            {
                MessageBox.Show("Visit was not saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _items.Add(visitFormWindow.Form.ServiceVisit);
            Items = _items;
        }

        private void _btn_RemoveVisit_Click(object sender, RoutedEventArgs e)
        {
            if (_lview_Visits.SelectedItem is null)
            {
                MessageBox.Show("No visit selected", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _items.Remove((ServiceVisit)_lview_Visits.SelectedItem);
            Items = _items;
        }

        private void _lview_Visits_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_lview_Visits.SelectedItem is null)
            {
                return;
            }

            var visitFormWindow = new ServiceVisitFormWindow();
            visitFormWindow.Form.ServiceVisit = (ServiceVisit)_lview_Visits.SelectedItem;

            if (visitFormWindow.ShowDialog() == false)
            {
                MessageBox.Show("Visit was not saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var index = _items.IndexOf((ServiceVisit)_lview_Visits.SelectedItem);
            _items[index] = visitFormWindow.Form.ServiceVisit;
            Items = _items;
        }
    }
}
