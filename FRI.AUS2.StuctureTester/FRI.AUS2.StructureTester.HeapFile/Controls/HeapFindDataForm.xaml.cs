using FRI.AUS2.StructureTester.HeapFileTester.Models;
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

namespace FRI.AUS2.StructureTester.HeapFileTester.Controls
{
    /// <summary>
    /// Interaction logic for HeapFindDataForm.xaml
    /// </summary>
    public partial class HeapFindDataForm : UserControl
    {
        public int Address
        {
            get { return int.Parse(_txtbx_BlockAddress.Value); }
            set { _txtbx_BlockAddress.Value = value.ToString(); }
        }
        public bool ShowAddress
        {
            get { return _txtbx_BlockAddress.Visibility == Visibility.Visible; }
            set { _txtbx_BlockAddress.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public int Id
        {
            get { return int.Parse(_txtbx_DataId.Value); }
            set { _txtbx_DataId.Value= value.ToString(); }
        }

        public HeapData HeapData
        {
            get
            {
                return new HeapData
                {
                    Id = Id
                };
            }
            set
            {
                Id = value.Id;
            }
        }

        public HeapFindDataForm()
        {
            InitializeComponent();
        }
    }
}
