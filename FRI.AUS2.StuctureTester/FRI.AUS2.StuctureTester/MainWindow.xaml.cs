using FRI.AUS2.Libs;
using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
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

namespace FRI.AUS2.StuctureTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KdTree<KdExampleData> _exampleStructure;

        public MainWindow()
        {
            InitializeComponent();

            _exampleStructure = new KdTree<KdExampleData>();

            _exampleStructure.Insert(new KdExampleData() { X = 0, Y = 0, Data = 1 });

            _exampleStructure.Insert(new KdExampleData() { X = 0, Y = 1, Data = 2 });
            _exampleStructure.Insert(new KdExampleData() { X = 1, Y = 1, Data = 3 });
            _exampleStructure.Insert(new KdExampleData() { X = 2, Y = 3, Data = 4 });
            _exampleStructure.Insert(new KdExampleData() { X = 3, Y = 5, Data = 5 });
            _exampleStructure.Insert(new KdExampleData() { X = 5, Y = 8, Data = 6 });
            _exampleStructure.Insert(new KdExampleData() { X = 8, Y = 13, Data = 7 });
            _exampleStructure.Insert(new KdExampleData() { X = 13, Y = 21, Data = 8 });
        }

        private void _mnitem_Test_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"ExampleStructure node count: {_exampleStructure.NodesCount}", Title);
        }

        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    internal class KdExampleData : IKdTreeData
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Data { get; set; }

        public int Compare(int level, IKdTreeData other)
        {
            switch (level % 2)
            {
                case 0:
                    return X.CompareTo(((KdExampleData)other).X);
                case 1:
                    return Y.CompareTo(((KdExampleData)other).Y);
                default:
                    throw new InvalidOperationException("Invalid level.");
            }
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]: {Data}";
        }
    }
}