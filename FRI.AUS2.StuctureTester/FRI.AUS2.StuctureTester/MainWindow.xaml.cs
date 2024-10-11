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

        // viewer
        private bool _isViewerActivated;
        protected bool IsViewerActivated
        {
            get => _isViewerActivated;
            set
            {
                _isViewerActivated = value;

                if (_isViewerActivated)
                {
                    _viewerActivate();
                    return;
                }

                _viewerDeactivate();
            }
        }
        private bool _isViewerExpanded = true;
        private int _viewerExpandedLevel;

        public MainWindow()
        {
            InitializeComponent();

            _exampleStructure = new KdTree<KdExampleData>();
            _initTreeNodes();

            _updateStatistics();
            IsViewerActivated = true;
        }

        private void _initTreeNodes()
        {
            int i = 0;

            // root
            _exampleStructure.Insert(new KdExampleData() { X = 10, Y = 10, Data = ++i });

            // level 1
            _exampleStructure.Insert(new KdExampleData() { X = 15, Y = 10, Data = ++i });
            _exampleStructure.Insert(new KdExampleData() { X = 5, Y = 10, Data = ++i });

            // level 2
            _exampleStructure.Insert(new KdExampleData() { X = 15, Y = 15, Data = ++i });
            _exampleStructure.Insert(new KdExampleData() { X = 14, Y = 5, Data = ++i });
            _exampleStructure.Insert(new KdExampleData() { X = 10, Y = 15, Data = ++i });
            _exampleStructure.Insert(new KdExampleData() { X = 10, Y = 5, Data = ++i });
        }

        private void _mnitem_TreeClear_Click(object sender, RoutedEventArgs e)
        {
            // clear tree
            _exampleStructure.Clear();

            // rerender tree view and stats
            _viewerRerenderTree();
            _updateStatistics();
        }
        private void _mnitem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {
            var newExampleData = new KdExampleData()
            {
                X = int.Parse(_txtb_X.Text),
                Y = int.Parse(_txtb_Y.Text),
                Data = int.Parse(_txtb_Data.Text)
            };

            _exampleStructure.Insert(newExampleData);
            _updateStatistics();
            _viewerRerenderTree();
        }

        private void _updateStatistics()
        {
            _txt_NodesCount.Text = _exampleStructure.NodesCount.ToString();
            _txt_TreeDepth.Text = _exampleStructure.Depth.ToString();
        }

        private void _btn_ViewerRerender_Click(object sender, RoutedEventArgs e)
        {
            _viewerRerenderTree();
        }

        private void _viewerDeactivate()
        {
            _treeView_Tree.Items.Clear();

            _isViewerActivated = false;

            _chk_ViewerActive.IsChecked = false;
            _treeView_Tree.IsEnabled = false;

            // callapse all buttons
            _stk_ViewerControls.Children.OfType<Button>().ToList().ForEach(b => b.IsEnabled = false);
        }

        private void _viewerActivate()
        {
            _treeView_Tree.IsEnabled = true;
            _chk_ViewerActive.IsChecked = true;
            _isViewerActivated = true;

            _viewerRerenderTree();

            // activate all buttons
            _stk_ViewerControls.Children.OfType<Button>().ToList().ForEach(b => b.IsEnabled = true);
        }

        private void _viewerRerenderTree()
        {
            if (!IsViewerActivated) return;

            _treeView_Tree.Items.Clear();
            _isViewerExpanded = true;
            _viewerExpandedLevel = _exampleStructure.Depth - 1;

            if (_exampleStructure.RootNode is not null)
            {
                _treeView_Tree.Items.Add(_createTreeViewItem(_exampleStructure.RootNode));
            }
        }

        private TreeViewItem _createTreeViewItem(KdTreeNode<KdExampleData> node)
        {
            string prefix = node.IsLeftChild ? "L " : (node.IsRightChild ? "R " : "");

            var item = new TreeViewItem()
            {
                Header = $"{prefix}{node.Data}",
                IsExpanded = true,
                Foreground = node.Level % 2 == 0 ? Brushes.Red : Brushes.Blue // otrasna zlozitost
            };

            if (node.LeftChild is not null)
            {
                item.Items.Add(_createTreeViewItem(node.LeftChild));
            }

            if (node.RightChild is not null)
            {
                item.Items.Add(_createTreeViewItem(node.RightChild));
            }

            return item;
        }

        private void _btn_ViewerToggleCollapse_Click(object sender, RoutedEventArgs e)
        {
            _viewerToggleExpanded(_treeView_Tree);
        }

        private void _viewerToggleExpanded(TreeView treeView)
        {
            _isViewerExpanded = !_isViewerExpanded;
            _viewerExpandedLevel = _isViewerExpanded
                ? _exampleStructure.Depth - 1
                : 0;

            _viewerExpandToLevel(treeView, _viewerExpandedLevel);
        }

        private void _viewerExpandToLevel(TreeView treeView, int maxLevel)
        {
            int currentLevel = 0;

            var actaulLevelQueue = new Queue<TreeViewItem>();
            var nextLevelQueue = new Queue<TreeViewItem>();

            actaulLevelQueue.Enqueue((TreeViewItem)treeView.Items[0]);

            while (actaulLevelQueue.Count > 0)
            {
                var item = actaulLevelQueue.Dequeue();

                if (currentLevel < maxLevel)
                {
                    item.IsExpanded = true;
                }
                else
                {
                    item.IsExpanded = false;
                }

                item.Items.Cast<TreeViewItem>().ToList().ForEach(nextLevelQueue.Enqueue);

                if (actaulLevelQueue.Count == 0)
                {
                    actaulLevelQueue = nextLevelQueue;
                    nextLevelQueue = new Queue<TreeViewItem>();
                    currentLevel++;
                }
            }
        }

        private void _btn_ViewerExpandLevel_Click(object sender, RoutedEventArgs e)
        {
            _viewerExpandToLevel(_treeView_Tree, ++_viewerExpandedLevel);
        }

        private void _btn_ViewerCollapseLevel_Click(object sender, RoutedEventArgs e)
        {
            if (_viewerExpandedLevel > 0)
            {
                _viewerExpandedLevel--;
            }

            _viewerExpandToLevel(_treeView_Tree, _viewerExpandedLevel);
        }

        private void _chk_ViewerActive_Checked(object sender, RoutedEventArgs e)
        {
            if (_treeView_Tree is null) return;
            
            IsViewerActivated = _chk_ViewerActive?.IsChecked ?? false;
        }

        private void _btn_GenerateNodes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented.");
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
}