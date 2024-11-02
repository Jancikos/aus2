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
using FRI.AUS2.SP1.Libs.Models;

namespace FRI.AUS2.SP1.GUI.Controls
{
    /// <summary>
    /// Interaction logic for GeoItemsManagement.xaml
    /// </summary>
    public partial class GeoItemsManagement : UserControl
    {
        public string Title { get; set; } = "Geo Items Management";

        public ItemsViewMode TableViewMode
        {
            get
            {
                return (ItemsViewMode)_cmbx_ViewMode.SelectedValue;
            }
            set
            {
                _cmbx_ViewMode.SelectedValue = (int)value;
            }
        }

        private ItemsFilterMode _filterMode;
        public ItemsFilterMode FilterMode
        {
            get
            {
                return _filterMode;
            }
            set
            {
                _filterMode = value;
                switch (value)
                {
                    case ItemsFilterMode.Point:
                        FilterByPointVisibility = Visibility.Visible;
                        FilterByRectangleVisibility = Visibility.Collapsed;
                        break;
                    case ItemsFilterMode.Rectangle:
                        FilterByPointVisibility = Visibility.Collapsed;
                        FilterByRectangleVisibility = Visibility.Visible;
                        break;
                }
            }

        }

        protected Visibility FilterByPointVisibility
        {
            get
            {
                return _grbx_Filter_ByPoint.Visibility;
            }
            set
            {
                _grbx_Filter_ByPoint.Visibility = value;
            }
        }
        protected Visibility FilterByRectangleVisibility
        {
            get
            {
                return _grbx_Filter_ByRect.Visibility;
            }
            set
            {
                _grbx_Filter_ByRect.Visibility = value;
            }
        }

        public Visibility ManageButtonsVisibility
        {
            get
            {
                return _stck_ManageButtons.Visibility;
            }
            set
            {
                _stck_ManageButtons.Visibility = value;
            }
        }

        public Action? InsertAction { get; set; }
        public Action<object>? EditAction { get; set; }
        public Action<object>? DeleteAction { get; set; }

        public Func<IEnumerable<object>> GetTableAllItemsSource { protected get; set; } = () => new List<object>();

        public Func<GpsPoint, IEnumerable<object>>? GetTableFilteredByPointItemsSource { protected get; set; } = null;
        public Func<GpsPoint, GpsPoint, IEnumerable<object>>? GetTableFilteredByRectangleItemsSource { protected get; set; } = null;

        public GeoItemsManagement()
        {
            InitializeComponent();

            this.DataContext = this;

            _initializeCmbxViewMode();
            _initializeTable();

            FilterMode = ItemsFilterMode.Point;
        }

        private void _initializeCmbxViewMode()
        {
            _cmbx_ViewMode.SelectedValuePath = "Key";
            _cmbx_ViewMode.DisplayMemberPath = "Value";

            _cmbx_ViewMode.Items.Add(new KeyValuePair<int, string>((int)ItemsViewMode.All, "Všetky"));
            _cmbx_ViewMode.Items.Add(new KeyValuePair<int, string>((int)ItemsViewMode.Filtered, "Filtrované"));

            _cmbx_ViewMode.SelectedIndex = 0;
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

        public void RerenderTable()
        {
            _tbl_GeoItems.ItemsSource = null;

            switch ((ItemsViewMode)_cmbx_ViewMode.SelectedValue)
            {
                case ItemsViewMode.All:
                    // weird but somehow offical way to do it
                    _tbl_GeoItems.ItemsSource = GetTableAllItemsSource();
                    break;
                case ItemsViewMode.Filtered:
                    _filterData();
                    break;
            }
        }

        private void _filterData()
        {
            switch (FilterMode)
            {
                case ItemsFilterMode.Point:
                    _filterByPoint();
                    break;
                case ItemsFilterMode.Rectangle:
                    _filterByRectangle();
                    break;
            }
        }

        private void _filterByPoint()
        {
            if (GetTableFilteredByPointItemsSource is null)
            {
                MessageBox.Show("Filter by point action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpsFilter = new GpsPoint()
            {
                X = double.Parse(_txtb_FilterX.Text),
                Y = double.Parse(_txtb_FilterY.Text)
            };

            _tbl_GeoItems.ItemsSource = GetTableFilteredByPointItemsSource(gpsFilter);
        }

        private void _filterByRectangle()
        {
            if (GetTableFilteredByRectangleItemsSource is null)
            {
                MessageBox.Show("Filter by rectangle action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpsA = new GpsPoint()
            {
                X = double.Parse(_txtb_Filter_ByRectA_X.Text),
                Y = double.Parse(_txtb_Filter_ByRectA_Y.Text)
            };

            var gpsB = new GpsPoint()
            {
                X = double.Parse(_txtb_Filter_ByRectB_X.Text),
                Y = double.Parse(_txtb_Filter_ByRectB_Y.Text)
            };

            _tbl_GeoItems.ItemsSource = GetTableFilteredByRectangleItemsSource(gpsA, gpsB);
        }

        #endregion

        #region  UI Event Handlers
        private void _btn_Filter_Click(object sender, RoutedEventArgs e)
        {
            if (TableViewMode != ItemsViewMode.Filtered)
            {
                TableViewMode = ItemsViewMode.Filtered;
                return;
            }

            RerenderTable();
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
            RerenderTable();
        }

        private void _cmbx_ViewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RerenderTable();
        }

        private void _btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteAction is null)
            {
                MessageBox.Show("Delete action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // get selected item
            var selectedItem = _tbl_GeoItems.SelectedItem;
            if (selectedItem is null)
            {
                MessageBox.Show("No item selected!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeleteAction(selectedItem);
        }
        private void _btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (EditAction is null)
            {
                MessageBox.Show("Edit action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // get selected item
            var selectedItem = _tbl_GeoItems.SelectedItem;
            if (selectedItem is null)
            {
                MessageBox.Show("No item selected!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EditAction(selectedItem);
        }
    }
    #endregion

    public enum ItemsViewMode
    {
        All,
        Filtered
    }
    public enum ItemsFilterMode
    {
        Point,
        Rectangle
    }
}
