using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ServiceVisitForm.xaml
    /// </summary>
    public partial class ServiceVisitForm : UserControl
    {

        public DateOnly Date
        {
            get { return DateOnly.FromDateTime(_dtp_Date?.SelectedDate ?? DateTime.Now); }
            set { _dtp_Date.SelectedDate = value.ToDateTime(TimeOnly.MinValue); }
        }

        public double Price
        {
            get { return double.Parse(_txtb_Price.Text); }
            set { _txtb_Price.Text = value.ToString(); }
        }

        public string[] Descriptions
        {
            get
            {
                return _lview_Descs.Items.Cast<string>().ToArray();
            }
            set
            {
                _lview_Descs.Items.Clear();
                foreach (var desc in value)
                {
                    _lview_Descs.Items.Add(desc);
                }
            }
        }

        public ServiceVisit ServiceVisit
        {
            get
            {
                return new ServiceVisit
                {
                    Date = Date,
                    Price = Price,
                    Descriptions = Descriptions
                };
            }
            set
            {
                Date = value.Date;
                Price = value.Price;
                Descriptions = value.Descriptions;
            }
        }

        public ServiceVisitForm()
        {
            InitializeComponent();
        }

        private void _btn_AddDesc_Click(object sender, RoutedEventArgs e)
        {
            if (Descriptions.Count() >= ServiceVisit.DescriptionsMaxCount)
            {
                MessageBox.Show("Maximum number of descriptions reached", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var inputDialog = new InputDialogWindow
            {
                Label = "Description",
                Input = ""
            };

            if (inputDialog.ShowDialog() == false)
            {
                return;
            }

            var desc = inputDialog.Input;
            var descs = new List<string>(Descriptions);
            descs.Add(desc);
            Descriptions = descs.ToArray();
        }

        private void _btn_RemoveDesc_Click(object sender, RoutedEventArgs e)
        {
            if (_lview_Descs.SelectedIndex == -1)
            {
                MessageBox.Show("No description selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var descs = new List<string>(Descriptions);
            descs.RemoveAt(_lview_Descs.SelectedIndex);
            Descriptions = descs.ToArray();
        }

        private void _lview_Descs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_lview_Descs.SelectedIndex == -1)
            {
                return;
            }

            var inputDialog = new InputDialogWindow
            {
                Label = "Description",
                Input = Descriptions[_lview_Descs.SelectedIndex]
            };

            if (inputDialog.ShowDialog() == false)
            {
                return;
            }

            var desc = inputDialog.Input;
            var descs = new List<string>(Descriptions);
            descs[_lview_Descs.SelectedIndex] = desc;
            Descriptions = descs.ToArray();
        }
    }
}
