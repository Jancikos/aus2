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

namespace FRI.AUS2.SP1.GUI.Controls
{
    /// <summary>
    /// Interaction logic for GeoItemsManagement.xaml
    /// </summary>
    public partial class GeoItemsManagement : UserControl
    {
        public string Title { get; set; } = "Geo Items Management";

        public Action? InsertAction { get; set; }

        public GeoItemsManagement()
        {
            InitializeComponent();

            this.DataContext = this;

            _initializeTable();
        }

        #region Table

        private void _initializeTable()
        {
            _tbl_GeoItems.AutoGenerateColumns = false;
            _tbl_GeoItems.IsReadOnly = true;
            _tbl_GeoItems.CanUserSortColumns = false;
        }

        public void AddTableColumn(string header, string bindingPath)
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(bindingPath)
            };

            _tbl_GeoItems.Columns.Add(column);
        }

        public void SetTableItemsSource<T>(IEnumerable<T> itemsSource)
        {
            _tbl_GeoItems.ItemsSource = itemsSource;
        }
        #endregion

        #region  UI Event Handlers
        private void _btn_Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void _btn_Insert_Click(object sender, RoutedEventArgs e)
        {
            if (InsertAction is null)
            {
                MessageBox.Show("Insert action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InsertAction();
        }

        private void _btn_Rerender_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(_tbl_GeoItems.ItemsSource).Refresh();
        }
    }
    #endregion
}
