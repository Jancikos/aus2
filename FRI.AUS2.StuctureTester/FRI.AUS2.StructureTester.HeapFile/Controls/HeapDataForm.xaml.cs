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
using FRI.AUS2.StructureTester.HeapFileTester.Models;

namespace FRI.AUS2.StructureTester.HeapFileTester.Controls
{
    /// <summary>
    /// Interaction logic for HeapData.xaml
    /// </summary>
    public partial class HeapDataForm : UserControl
    {
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

        public HeapData HeapData
        {
            get
            {
                return new HeapData
                {
                    Id = Id,
                    Firstname = Firstname,
                    Lastname = Lastname,
                    Items = Items
                };
            }
            set
            {
                Id = value.Id;
                Firstname = value.Firstname;
                Lastname = value.Lastname;
                Items = value.Items;
            }
        }

        private List<NesteHeapDataItem> _items = new List<NesteHeapDataItem>(HeapData.ItemsMaxCount);
        public List<NesteHeapDataItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                _lview_Items.Items.Clear();
                _items.ForEach(item => _lview_Items.Items.Add($"{item.Date} - {item.Price}e ({item.Description})"));
            }
        }
        
        public HeapDataForm()
        {
            InitializeComponent();

            _initilizeDefaultValues();
        }

        private void _initilizeDefaultValues()
        {
            Id = 1;
            Firstname = "Janko";
            Lastname = "Polienko";
            Items = HeapData.GenerateRandomItems();
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

        private void _grbx_Items_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Items = HeapData.GenerateRandomItems();
        }
    }
}