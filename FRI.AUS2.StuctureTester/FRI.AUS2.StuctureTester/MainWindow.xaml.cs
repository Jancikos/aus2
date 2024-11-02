using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Trees;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;
using FRI.AUS2.StuctureTester.Utils;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static FRI.AUS2.Libs.Helpers.IntExtension;

namespace FRI.AUS2.StuctureTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KdTree<KdExampleData> _exampleStructure;
        private OperationsGenerator<KdExampleData>? _operationsGenerator;

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
            IsViewerActivated = false;
        }

        private void _initTreeNodes()
        {
            int i = 0;

            // // root
            // _exampleStructure.Insert(new KdExampleData() { A = 10, B = 10, Data = ++i });

            // // level 1
            // _exampleStructure.Insert(new KdExampleData() { A = 15, B = 10, Data = ++i });
            // _exampleStructure.Insert(new KdExampleData() { A = 5, B = 10, Data = ++i });

            // // level 2
            // _exampleStructure.Insert(new KdExampleData() { A = 15, B = 15, Data = ++i });
            // _exampleStructure.Insert(new KdExampleData() { A = 14, B = 5, Data = ++i });
            // _exampleStructure.Insert(new KdExampleData() { A = 10, B = 15, Data = ++i });
            // _exampleStructure.Insert(new KdExampleData() { A = 10, B = 5, Data = ++i });
        }

        private void _updateStatistics()
        {
            _txt_NodesCount.Text = _exampleStructure.NodesCount.ToString();
            _txt_TreeDepth.Text = _exampleStructure.Depth.ToString();
            _txt_Root.Text = _exampleStructure.RootNode?.Data.ToString() ?? "none";
        }

        #region Menu items
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
        #endregion

        #region Tree viewer

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
        private void _btn_ViewerRerender_Click(object sender, RoutedEventArgs e)
        {
            _viewerRerenderTree();
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

        #endregion

        #region Other UI events
        private void _btn_ManualInsert_Click(object sender, RoutedEventArgs e)
        {
            var newExampleData = _frm_Insert.KdDataModel;

            _exampleStructure.Insert(newExampleData);
            _updateStatistics();
            _viewerRerenderTree();
        }

        private void _btn_GenerateNodes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = _frm_Generate.Count;
                int seed = _frm_Generate.Seed;

                var random = new Random(seed);
                int i = 0;
                while (++i <= count)
                {
                    _exampleStructure.Insert(new KdExampleData()
                    {
                        X = random.Next(
                            _frm_Generate.X.min,
                            _frm_Generate.X.max
                        ),
                        Y = random.Next(
                            _frm_Generate.Y.min,
                            _frm_Generate.Y.max
                        ),
                        Data = KdExampleData.GetRandomData(random)
                    });
                }

                _updateStatistics();
                _viewerRerenderTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_ManualFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = _frm_Find.KdDataModel;

                var found = _exampleStructure.Find(data);

                _txt_findResult.Text = found.Count == 0
                    ? "Data not found!"
                    : $"Data: [{string.Join(", ", found.Select(d => d.Data))}]";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_ManualDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = _frm_Delete.KdDataModel;

                _exampleStructure.RemoveException(data);

                _updateStatistics();
                _viewerRerenderTree();

                _txt_deleteResult.Text = "Data deleted.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_ManualIteratorOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = _frm_InOrder.IsKdDataModelValid
                    ? _frm_InOrder.KdDataModel
                    : _exampleStructure.RootNode?.Data ?? throw new InvalidOperationException("No data in tree.");

                var it = _exampleStructure.GetIterator<KdTreeLevelOrderIterator<KdExampleData>>(data);
                if (it is null)
                {
                    _txt_InOrderResult.Text = "Data not found!";
                    return;
                }

                var sb = new StringBuilder();
                while (it.MoveNext())
                {
                    sb.AppendLine(it.Current.ToString());
                }
                _txt_InOrderResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _btn_testerRunTest_Click(object sender, RoutedEventArgs e)
        {
            _operationsGenerator = new OperationsGenerator<KdExampleData>(
                _exampleStructure,
                int.Parse(_txtb_testerOperationsCount.Text),
                int.Parse(_txtb_testerSeed.Text),
                (Random random, KdExampleData? filter) =>
                    filter is null
                     ? new KdExampleData()
                     {
                         X = random.Next(
                            _frm_Generate.X.min,
                            _frm_Generate.X.max
                        ),
                         Y = random.Next(
                            _frm_Generate.Y.min,
                            _frm_Generate.Y.max
                        ),
                         Data = KdExampleData.GetRandomData(random)
                     }
                     : new KdExampleData()
                     {
                         X = filter.X,
                         Y = filter.Y,
                         Data = KdExampleData.GetRandomData(random)
                     }
            );

            // init ratio of operations
            int.Parse(_txtb_operationsAdd.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.Insert));
            int.Parse(_txtb_operationsAddDuplicate.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.InsertDuplicate));
            int.Parse(_txtb_operationsDelete.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.Delete));
            int.Parse(_txtb_operationsDeleteSpecific.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.DeleteSpecific));
            int.Parse(_txtb_operationsFind.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.Find));
            int.Parse(_txtb_operationsFindSpecific.Text).Repeat(() => _operationsGenerator.AddOperation(OperationType.FindSpecific));

            // init log settings
            _operationsGenerator.LogsVerbosity = int.Parse(_txtb_operationsLogVerbosity.Text);
            _operationsGenerator.LogsStatsFrequency = int.Parse(_txtb_operationsLogStatsFreq.Text);

            _operationsGenerator.Generate();

            _updateStatistics();
            _viewerRerenderTree();

            MessageBox.Show("Test was runned.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _btn_testerLog_Click(object sender, RoutedEventArgs e)
        {
            if (_operationsGenerator is null)
            {
                MessageBox.Show("No test was runned yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var logUri = _operationsGenerator.LogsFileUri;
            // copy the path to clipboard
            Clipboard.SetText(logUri.AbsolutePath);

            MessageBox.Show($"Log file path was copied to clipboard: {logUri}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _txtb_Generate_Radnom_Number(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var random = new Random();
            var txtb = (TextBox)sender;

            if (txtb.Text == "")
            {
                txtb.Text = random.Next(0, 100).ToString();
                return;
            }

            txtb.Text = (int.Parse(txtb.Text) + 1).ToString();
        }

        private void _btn_ManualFindSpecific_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = _frm_FindSpecific.KdDataModel;

                var found = _exampleStructure.FindSpecific(data);

                _txt_FindSpecificResult.Text = found.Count == 0
                    ? "Data not found!"
                    : $"Data: [{string.Join(", ", found.Select(d => d.Data))}]";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region KdExampleData
        public class KdExampleData : IKdTreeData
        {
            public int X { get; set; }
            public int Y { get; set; }

            public string Data { get; set; } = "";

            public int Compare(int level, IKdTreeData other)
            {
                var otherModel = (KdExampleData)other;
                var dimension = level % GetDiminesionsCount();

                switch (dimension)
                {
                    case 0:
                        return X.CompareTo(otherModel.X);
                    case 1:
                        return Y.CompareTo(otherModel.Y);
                    default:
                        throw new InvalidOperationException("Invalid level.");
                }
            }
            public bool Equals(IKdTreeData other)
            {
                var otherModel = (KdExampleData)other;

                // return Data == otherModel.Data;
                // return X == otherModel.X && Y == otherModel.Y;
                return X == otherModel.X && Y == otherModel.Y && Data == otherModel.Data;
            }

            public bool PositionEquals(IKdTreeData other)
            {
                var otherModel = (KdExampleData)other;

                return X == otherModel.X && Y == otherModel.Y;
            }


            public int GetDiminesionsCount() => 2;

            public override string ToString()
            {
                return $"[{X}; {Y}]: {Data}";
            }

            /// <summary>
            /// generate random data string with length 10
            /// </summary>
            /// <param name="random"></param>
            /// <returns></returns>
            public static string GetRandomData(Random random)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var data = new char[10];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = chars[random.Next(chars.Length)];
                }
                return new string(data);
            }

            public static int GetRandomPosition(Random random)
            {
                return random.Next(0, 50);
            }
        }
        #endregion
    }
}