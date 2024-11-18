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

namespace FRI.AUS2.StructureTester.Libs.Controls
{
    /// <summary>
    /// Interaction logic for InlineText.xaml
    /// </summary>
    public partial class InlineText : UserControl
    {
        public string Title 
        {
            get
            {
                return _txt_Title.Text;
            }
            set
            {
                _txt_Title.Text = value;
            }
        }

        public string Value
        {
            get
            {
                return _txt_Value.Text;
            }
            set
            {
                _txt_Value.Text = value;
            }
        }

        public TextAlignment TextAlignment
        {
            get
            {
                return _txt_Value.TextAlignment;
            }
            set
            {
                _txt_Value.TextAlignment = value;
            }
        }

        public InlineText()
        {
            InitializeComponent();
        }
    }
}
