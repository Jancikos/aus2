﻿using System;
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

        public Action? InsertAction { get; set; }
        public Action? GenerateAction { get; set; }

        public Func<IEnumerable<object>> GetTableAllItemsSource { protected get; set; } = () => new List<object>();

        public Func<GpsPoint, IEnumerable<object>>? GetTableFilteredItemsSource { protected get; set; } = null;

        public GeoItemsManagement()
        {
            InitializeComponent();

            this.DataContext = this;

            _initializeCmbxViewMode();
            _initializeTable();
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
            switch ((ItemsViewMode)_cmbx_ViewMode.SelectedValue)
            {
                case ItemsViewMode.All:
                    // weird but somehow offical way to do it
                    _tbl_GeoItems.ItemsSource = null;
                    _tbl_GeoItems.ItemsSource = GetTableAllItemsSource();
                    break;
                case ItemsViewMode.Filtered:
                    _filterData();
                    break;
            }
        }

        private void _filterData()
        {
            if (GetTableFilteredItemsSource is null)
            {
                MessageBox.Show("Filter action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpsFilter = new GpsPoint()
            {
                X = double.Parse(_txtb_FilterX.Text),
                Y = double.Parse(_txtb_FilterY.Text)
            };

            _tbl_GeoItems.ItemsSource = GetTableFilteredItemsSource(gpsFilter);
        }

        #endregion

        #region  UI Event Handlers
        private void _btn_Filter_Click(object sender, RoutedEventArgs e)
        {
            TableViewMode = ItemsViewMode.Filtered;
            _filterData();
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

        private void _btn_Generate_Click(object sender, RoutedEventArgs e)
        {
            if (GenerateAction is null)
            {
                MessageBox.Show("Generate action is not implemented!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GenerateAction();
        }
        #endregion
    }

    public enum ItemsViewMode
    {
        All,
        Filtered
    }
}
