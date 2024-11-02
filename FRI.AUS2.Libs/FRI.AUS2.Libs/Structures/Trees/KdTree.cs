using System.Collections;
using System.Reflection.Metadata.Ecma335;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;

namespace FRI.AUS2.Libs.Structures.Trees
{
    public class KdTree<T> : IEnumerable<T> where T : class, IKdTreeData
    {
        #region Properties
        private KdTreeNode<T>? _rootNode;
        public KdTreeNode<T>? RootNode { get => _rootNode; }

        public int NodesCount
        {
            get => _countNodes(_rootNode);
        }

        public int Depth
        {
            get => _rootNode?.SubtreeDepth ?? 0;
        }
        #endregion

        public KdTree()
        {
            _rootNode = null;
        }

        #region Basic operations

        public void Clear()
        {
            _rootNode = null;
        }

        /// <summary>
        /// count all nodes in the tree
        /// </summary>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        private int _countNodes(KdTreeNode<T>? rootNode)
        {
            if (rootNode is null)
            {
                return 0;
            }

            int count = 0;
            var it =  GetIterator<KdTreeLevelOrderIterator<T>>();
            while (it.MoveNext())
            {
                count++;
            }

            return count;
        }

        private void _setRootNode(KdTreeNode<T>? newRoot)
        {
            _rootNode = newRoot;
        }

        /// <summary>
        /// this method is used to compare all dimensions of the two IKdTreeData objects 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool CompareAllDimensions(IKdTreeData a, IKdTreeData b)
        {
            if (a.GetDiminesionsCount() != b.GetDiminesionsCount())
            {
                return false;
            }

            for (int i = 0; i < a.GetDiminesionsCount(); i++)
            {
                if (a.Compare(i, b) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Insert

        /// <summary>
        /// O(log n)
        /// </summary>
        /// <param name="data"></param>
        public void Insert(T data)
        {
            if (_rootNode is null)
            {
                _rootNode = new KdTreeNode<T>(null, data);
                return;
            }

            KdTreeNode<T>? parentNode = _findNode(_rootNode, data, out var lastVisitedNode);

            if (parentNode is not null && parentNode.LeftChild is not null)
            {
                // data already exists, duplicate => go to the left child and find parent from that tree
                parentNode = parentNode?.LeftChild;
                while (parentNode is not null)
                {
                    parentNode = _findNode(parentNode, data, out lastVisitedNode);
                    parentNode = parentNode?.LeftChild;
                }
            }

            // data does not exist, insert new node to the last visited node
            parentNode = lastVisitedNode;

            if (parentNode is null)
            {
                throw new InvalidOperationException("Invalid tree state. Parent for data not found.");
            }

            KdTreeNode<T> newNode = new KdTreeNode<T>(parentNode, data);
            int comparison = data.Compare(parentNode.Level, parentNode.Data);
            if (comparison <= 0)
            {
                parentNode.LeftChild = newNode;
            }
            else
            {
                parentNode.RightChild = newNode;
            }
        }

        #endregion

        #region Find

        /// <summary>
        /// O(log n)
        /// 
        /// returns data from all nodes thats position (Compare in all dimensions) is the same as the filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IList<T> Find(T filter)
        {
            return _findData(filter);
        }

        /// <summary>
        /// O(log n)
        /// 
        /// returns data from all nodes thats position (Compare in all dimensions) and Equals is the same as the filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IList<T> FindSpecific(T filter)
        {
            return _findData(filter, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="onlyEquals"></param>
        /// <returns></returns>
        private IList<T> _findData(T filter, bool onlyEquals = false)
        {
            var data = new List<T>();

            if (_rootNode is null)
            {
                return data;
            }

            _processNodes(_rootNode, filter, node => data.Add(node.Data), onlyEquals);
            
            return data;
        }

        private void _processNodes(KdTreeNode<T> root, T filter, Action<KdTreeNode<T>> action, bool onlyEquals = false)
        {
            var currentParent = root;
            do
            {
                var node = _findConcretNode(currentParent, filter, out _);

                if (node is not null)
                {
                    if (!onlyEquals || node.Data.Equals(filter))
                    {
                        action(node);
                    }
                }

                // search also in left subtree
                currentParent = node?.LeftChild;
            } while (currentParent is not null);
        }

        /// <summary>
        /// O(log n)
        /// 
        /// returns nodes with the same position as the filter within the tree from the root parameter
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filter"></param>
        /// <param name="onlyEquals">whether to return only nodes that also fits the Equals method</param>
        /// <returns></returns>
        private IList<KdTreeNode<T>> _findNodes(KdTreeNode<T> root, T filter, bool onlyEquals = false)
        {
            var nodes = new List<KdTreeNode<T>>();

            _processNodes(root, filter, nodes.Add, onlyEquals);

            return nodes;
        }

        /// <summary>
        /// O(log n) 
        /// </summary>
        /// <param name="root">root node of hierarchy that will be visited </param>
        /// <param name="data"></param>
        /// <param name="lastVisitedNode">the node, which was visited last before returning value</param>
        /// <returns> the node with the same position (on some dimension) as the data</returns>
        private KdTreeNode<T>? _findNode(KdTreeNode<T> root, T data, out KdTreeNode<T>? lastVisitedNode)
        {
            lastVisitedNode = null;
            var currentNode = root;
            int level = root.Level;

            while (currentNode != null)
            {
                lastVisitedNode = currentNode;
                int comparison = data.Compare(level, currentNode.Data);

                if (comparison == 0)
                {
                    // found
                    return currentNode;
                }

                // not found, go to next level
                level++;

                if (comparison < 0)
                {
                    // go search in left subtree
                    currentNode = currentNode.LeftChild;
                    continue;
                }

                // go search in right subtree
                currentNode = currentNode.RightChild;
            }

            // not found
            return null;
        }

        /// <summary>
        /// O(log n)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filter"></param>
        /// <param name="lastVisitedNode"></param>
        /// <returns> the node where all dimensions of data are same as in the node</returns>
        private KdTreeNode<T>? _findConcretNode(KdTreeNode<T> root, T filter, out KdTreeNode<T>? lastVisitedNode)
        {
            var node = _findNode(root, filter, out lastVisitedNode);

            if (node is null)
            {
                // not found
                return null;
            }

            // check if keys are same in all dimensions
            bool allDimesionsMatch = false;
            while (node is not null && !allDimesionsMatch)
            {
                allDimesionsMatch = true;
                for (int i = 0; i < filter.GetDiminesionsCount(); i++)
                {
                    if (filter.Compare(i, node.Data) != 0)
                    {
                        allDimesionsMatch = false;

                        // not same, try to find in left subtree
                        if (node.LeftChild is null)
                        {
                            return null; // left tree doesnt exists, cant be found
                        }

                        node = _findNode(node.LeftChild, filter, out _);
                        break;
                    }
                }
            }

            return node;
        }

        #endregion

        #region Remove

        /// <summary>
        /// removes all nodes with the given filter (by Compare in all dimensions and also Equals)
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="throwIfNotFound">whether to throw exception if the filter is not found</param>
        public void RemoveSpecific(T filter, bool throwIfNotFound = false)
        {
            if (_rootNode is null)
            {
                if (throwIfNotFound)
                {
                    throw new InvalidOperationException("Tree is empty.");
                }
                return;
            }

            var nodes = _findNodes(_rootNode, filter, true);
            foreach (var node in nodes)
            {
                try
                {
                    _removeNode(node);
                }
                catch (Exception)
                {
                    if (throwIfNotFound)
                    {
                        throw;
                    }
                }
            }

        }

        /// <summary>
        /// deletes the node with the first occurence of the given filter (by Compare in all dimensions)
        /// /// </summary>
        /// <param name="filter"></param>
        /// <param name="throwIfNotFound">whether to throw exception if the filter is not found</param>
        public void Remove(T filter, bool throwIfNotFound = false)
        {
            if (_rootNode is null)
            {
                if (throwIfNotFound)
                {
                    throw new InvalidOperationException("Tree is empty.");
                }
                return;
            }

            var node = _findConcretNode(_rootNode, filter, out _);
            if (node is null)
            {
                if (throwIfNotFound)
                {
                    throw new InvalidOperationException("Concrete node not found.");
                }
                return;
            }

            try
            {
                _removeNode(node);
            }
            catch (Exception)
            {
                if (throwIfNotFound)
                {
                    throw;
                }

                return;
            }
        }

        /// <summary>
        /// O (log n)
        /// </summary>
        /// <param name="nodeToRemove"></param>
        private void _removeNode(KdTreeNode<T> nodeToRemove)
        {
            var nodesToRemove = new Queue<KdTreeNode<T>>();
            nodesToRemove.Enqueue(nodeToRemove);
            var firstIteration = true;

            while (nodesToRemove.Count > 0)
            {
                var node = nodesToRemove.Dequeue();

                do {
                    if (node.IsLeaf)
                    {
                        if (node == _rootNode)
                        {
                            // only one node in the tree
                            _setRootNode(null);
                            return;
                        }

                        // remove leaf node
                        if (node.Parent is not null)
                        {
                            if (node.IsLeftChild)
                            {
                                node.Parent.LeftChild = null;
                            }
                            if (node.IsRightChild)
                            {
                                node.Parent.RightChild = null;
                            }
                            node.Parent = null;
                        }

                        continue; // namiesto GOTO end... 
                    }

                    // nodes stack - nodes will be replaced from the bottom to the top
                    Stack<KdTreeNode<T>> nodesToBeReplaced = new Stack<KdTreeNode<T>>();
                    nodesToBeReplaced.Push(node);

                    bool isLastReplacedNodeLeaf = false;
                    do
                    {
                        KdTreeNode<T>? replacementNode = _findReplacementNode(nodesToBeReplaced.Peek());

                        if (replacementNode is null)
                        {
                            // no replacement node found
                            throw new InvalidOperationException("No replacement node found.");
                        }

                        nodesToBeReplaced.Push(replacementNode);

                        if (replacementNode.IsLeaf)
                        {
                            isLastReplacedNodeLeaf = true;
                        }
                    } while (!isLastReplacedNodeLeaf);

                    // replace nodes
                    KdTreeNode<T> acutalLeaf = nodesToBeReplaced.Pop();
                    do
                    {
                        KdTreeNode<T> toBeReplaced = nodesToBeReplaced.Pop();
                        _replaceNode(toBeReplaced, acutalLeaf);
                        acutalLeaf = toBeReplaced;
                    } while (nodesToBeReplaced.Count > 0);
                } while (false);

                // insert nodes that were removed during the remove process
                // if it is not the first iteration, insert the node back to the tree (duplicate dimension node from the right subtree)
                if (!firstIteration)
                {
                    Insert(node.Data);
                }

                firstIteration = false;
            }

            /// <summary>
            ///   
            /// finds replacement node for the given node
            /// 
            /// replacement node is the that can be used to replace the given node (when the given node wants to be removed)
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            KdTreeNode<T>? _findReplacementNode(KdTreeNode<T>? node)
            {
                KdTreeNode<T>? replacementNode = null;

                // if node has right child
                // // find node with minimum value (in the same dimension) in the right subtree
                if (node?.RightChild is not null)
                {
                    // find node with minimum value in the right subtree
                    var nodeLevel = node.Level;
                    var minNode = node.RightChild;
                    var nodesWithSameValueAsMinNode = new List<KdTreeNode<T>>();

                    // search the whole tree where node.RightChild is root
                    var it = new KdTreeOnlyNodesWithSmallerDimensionIterator<T>(node.RightChild, node.Dimension);
                    if (it is not null)
                    {
                        while (it.MoveNext())
                        {
                            var currentNode = it.CurrentNode;

                            if (minNode is null)
                            {
                                throw new InvalidOperationException("Min node cannot be null.");
                            }

                            if (currentNode is not null)
                            {
                                var currentComparison = currentNode.Data.Compare(nodeLevel, minNode.Data);
                                if (currentComparison == 0 && currentNode != node.RightChild)
                                {
                                    nodesWithSameValueAsMinNode.Add(currentNode);
                                    continue;
                                }

                                if (currentComparison < 0)
                                {
                                    minNode = it.CurrentNode;
                                    nodesWithSameValueAsMinNode.Clear();
                                }
                            }
                        }
                    }

                    if (minNode is not null)
                    {
                        // if there are nodes with the same dimension value as minNode, remove them first
                        if (nodesWithSameValueAsMinNode.Count > 0)
                        {
                            foreach (var nodeWithSameValue in nodesWithSameValueAsMinNode)
                            {
                                // kontrola, ci uz nie je v zasobniku
                                if (!nodesToRemove.Contains(nodeWithSameValue)) {
                                    nodesToRemove.Enqueue(nodeWithSameValue);
                                }
                            }
                        }
                    }

                    replacementNode = minNode;
                }


                // if node has no right child, but has left child
                // // find node with maximum value (in the same dimension) in the left subtree
                if (replacementNode is null && node?.LeftChild is not null)
                {
                    // find node with maximum value in the left subtree
                    var nodeLevel = node.Level;
                    var maxNode = node.LeftChild;

                    // search the whole tree where node.LeftChild is root
                    var it = new KdTreeOnlyNodesWithBiggerDimensionIterator<T>(node.LeftChild, node.Dimension);
                    while (it.MoveNext())
                    {
                        if (maxNode is null)
                        {
                            throw new InvalidOperationException("Max node cannot be null.");
                        }

                        var currentNode = it.CurrentNode;
                        if (currentNode is not null)
                        {
                            var currentComparison = currentNode.Data.Compare(nodeLevel, maxNode.Data);

                            if (currentComparison > 0)
                            {
                                maxNode = it.CurrentNode;
                            }
                        }
                    }

                    replacementNode = maxNode;
                }

                return replacementNode;
            }
        }

        /// <summary>
        /// O(1)
        /// 
        /// replaces toBeReplaced node with leaf node
        /// 
        /// leaf node will be moved to the place of toBeReplaced node (in the hierarchy)
        /// toBeReplaced node will be removed from the hierarchy (but not deleted)
        /// </summary>
        /// <param name="toBeReplaced"></param>
        /// <param name="leaf"></param>
        private void _replaceNode(KdTreeNode<T> toBeReplaced, KdTreeNode<T> leaf)
        {
            if (!leaf.IsLeaf)
            {
                throw new InvalidOperationException("Leaf node expected.");
            }

            if (leaf.Parent is not null)
            {
                var leafParent = leaf.Parent;
                // remove leaf from its parent
                if (leaf.IsLeftChild)
                {
                    leafParent.LeftChild = null;
                }
                if (leaf.IsRightChild)
                {
                    leafParent.RightChild = null;
                }
            }


            // add leaf to the place of toBeReplaced
            leaf.Parent = toBeReplaced.Parent;
            if (toBeReplaced.Parent is not null)
            {
                if (toBeReplaced.IsLeftChild)
                {
                    toBeReplaced.Parent.LeftChild = leaf;
                }
                if (toBeReplaced.IsRightChild)
                {
                    toBeReplaced.Parent.RightChild = leaf;
                }
                toBeReplaced.Parent = null;
            }
            leaf.LeftChild = toBeReplaced.LeftChild;
            if (toBeReplaced.LeftChild is not null)
            {
                toBeReplaced.LeftChild.Parent = leaf;
                toBeReplaced.LeftChild = null;
            }
            leaf.RightChild = toBeReplaced.RightChild;
            if (toBeReplaced.RightChild is not null)
            {
                toBeReplaced.RightChild.Parent = leaf;
                toBeReplaced.RightChild = null;
            }

            // if toBeReplaced is root node, set new root node
            if (toBeReplaced == _rootNode)
            {
                _setRootNode(leaf);
            }
        }

        /// <summary>
        /// throws exception if the filter is not found
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveException(T filter)
        {
            Remove(filter, true);
        }
        #endregion

        #region Iterators

        // IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return GetIterator<KdTreeLevelOrderIterator<T>>();
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TKdTreeIterator? GetIterator<TKdTreeIterator>(T filter) where TKdTreeIterator : KdTreeIterator<T>
        {
            if (_rootNode is null)
            {
                return null;
            }

            var node = _findConcretNode(_rootNode, filter, out _);

            if (node is null)
            {
                return null;
            }

            return GetIterator<TKdTreeIterator>(node);
        }

        public TKdTreeIterator GetIterator<TKdTreeIterator>(KdTreeNode<T>? node) where TKdTreeIterator : KdTreeIterator<T>
        {
            // must be created by reflection, because of the generic type 
            // !!! be aware of possible run time exception, if the iterator does not have a constructor with the node parameter
            return (TKdTreeIterator)Activator.CreateInstance(typeof(TKdTreeIterator), node)!;
        }

        public TKdTreeIterator GetIterator<TKdTreeIterator>() where TKdTreeIterator : KdTreeIterator<T>
        {
            return GetIterator<TKdTreeIterator>(RootNode);
        }
        #endregion
    }

    #region KdTreeNode
    public class KdTreeNode<T> where T : class, IKdTreeData
    {
        /// <summary>
        /// null if this is the root node
        /// </summary>
        /// <value></value>
        public KdTreeNode<T>? Parent { get; set; }

        public KdTreeNode<T>? LeftChild { get; set; }

        public KdTreeNode<T>? RightChild { get; set; }

        public T Data { get; set; }

        /// <value>degree (number of direct sons) of the node</value>
        public int Degree
        {
            get
            {
                int degree = 0;

                if (LeftChild is not null)
                {
                    degree++;
                }
                if (RightChild is not null)
                {
                    degree++;
                }

                return degree;
            }
        }

        /// <value>level of the node in the tree</value>
        public int Level
        {
            get => _getLevel();
        }

        /// <value>dimension of the node in the tree</value>
        public int Dimension
        {
            get => Level % Data.GetDiminesionsCount();
        }

        /// <value>depth of the subtree where this note is root</value>
        public int SubtreeDepth
        {
            get => _getSubtreeDepth(this);
        }

        /// <value>indicates whether the node is the leaf node</value>
        public bool IsLeaf
        {
            get => LeftChild is null && RightChild is null;
        }

        /// <value>indicates whether the node is the right child of its parent</value>
        public bool IsRightChild
        {
            get
            {
                return Parent?.RightChild == this;
            }
        }

        /// <value>indicates whether the node is the left child of its parent</value>
        public bool IsLeftChild
        {
            get
            {
                return Parent?.LeftChild == this;
            }
        }


        public KdTreeNode(KdTreeNode<T>? parent, T data)
        {
            Parent = parent;
            Data = data;
        }

        /// <returns>level of the node in the tree</returns>
        private int _getLevel()
        {
            int level = 0;
            var currentNode = this;

            while (currentNode.Parent != null)
            {
                level++;
                currentNode = currentNode.Parent;
            }

            return level;
        }

        /// <summary>
        /// get the depth of the subtree where the node is the root
        /// 
        /// POZOR zlozitost je O(log n) + 2 * O(log n) = O(log n)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int _getSubtreeDepth(KdTreeNode<T>? node)
        {
            if (node is null || node.IsLeaf)
            {
                return 0;
            }

            var it = new KdTreeLevelOrderIterator<T>(node);
            var lastVisitedNode = node;
            while (it.MoveNext())
            {
                if (it.CurrentNode is not null)
                {
                    lastVisitedNode = it.CurrentNode;
                }
            }

            return lastVisitedNode.Level - node.Level;
        }
    }
    #endregion

    #region Iterator classes
    public abstract class KdTreeIterator<T> : IEnumerator<T>
        where T : class, IKdTreeData
    {
        protected Queue<KdTreeNode<T>> _nodesToProcess;

        protected KdTreeNode<T>? _root;
        protected KdTreeNode<T>? _current;
        public T Current => _current?.Data ?? throw new InvalidOperationException("Current node is not set.");
        internal KdTreeNode<T>? CurrentNode => _current;

        object IEnumerator.Current => Current;

        public KdTreeIterator(KdTreeNode<T>? rootNode)
        {
            _root = rootNode;
            _nodesToProcess = new Queue<KdTreeNode<T>>();

            Reset();
        }

        public void Dispose()
        {
            _nodesToProcess.Clear();
        }

        public bool MoveNext()
        {
            if (_nodesToProcess.Count == 0)
            {
                return false;
            }

            _current = _nodesToProcess.Dequeue();
            _processNode(_current);
            return true;
        }

        public void Reset()
        {
            _nodesToProcess.Clear();

            _current = null;
            if (_root is not null)
            {
                _nodesToProcess.Enqueue(_root);
            }
        }

        protected abstract void _processNode(KdTreeNode<T>? node);
    }

    public class KdTreeLevelOrderIterator<T> : KdTreeIterator<T>
        where T : class, IKdTreeData
    {
        public KdTreeLevelOrderIterator(KdTreeNode<T> rootNode) : base(rootNode)
        {
        }

        protected override void _processNode(KdTreeNode<T>? node)
        {
            if (node is null)
            {
                return;
            }

            if (node.LeftChild is not null)
            {
                _nodesToProcess.Enqueue(node.LeftChild);
            }

            if (node.RightChild is not null)
            {
                _nodesToProcess.Enqueue(node.RightChild);
            }
        }
    }

    public class KdTreeOnlyNodesWithBiggerDimensionIterator<T> : KdTreeOnlyNodesWithThatDimensionIterator<T>
        where T : class, IKdTreeData
    {
        public KdTreeOnlyNodesWithBiggerDimensionIterator(KdTreeNode<T> rootNode, int dimension) : base(rootNode, dimension)
        {
        }

        protected override void _enqueueChildrenWithinDimension(KdTreeNode<T> node)
        {
            if (node.RightChild is not null)
            {
                _nodesToProcess.Enqueue(node.RightChild);
            }
        }
    }

    public class KdTreeOnlyNodesWithSmallerDimensionIterator<T> : KdTreeOnlyNodesWithThatDimensionIterator<T>
        where T : class, IKdTreeData
    {
        public KdTreeOnlyNodesWithSmallerDimensionIterator(KdTreeNode<T> rootNode, int dimension) : base(rootNode, dimension)
        {
        }

        protected override void _enqueueChildrenWithinDimension(KdTreeNode<T> node)
        {
            if (node.LeftChild is not null)
            {
                _nodesToProcess.Enqueue(node.LeftChild);
            }
        }
    }

    public abstract class KdTreeOnlyNodesWithThatDimensionIterator<T> : KdTreeIterator<T>
        where T : class, IKdTreeData
    {
        private int _dimension;

        public KdTreeOnlyNodesWithThatDimensionIterator(KdTreeNode<T> rootNode, int dimension) : base(rootNode)
        {
            _dimension = dimension;
        }

        protected override void _processNode(KdTreeNode<T>? node)
        {
            if (node is null)
            {
                return;
            }

            // TODO - pozor na tuto operaciu - stale pocita level s O(log n) zlozitostou
            if (node.Dimension == _dimension)
            {
                _enqueueChildrenWithinDimension(node);
                return;
            } 
            
            if (node.LeftChild is not null)
            {
                _nodesToProcess.Enqueue(node.LeftChild);
            }
            if (node.RightChild is not null)
            {
                _nodesToProcess.Enqueue(node.RightChild);
            }
        }

        protected abstract void _enqueueChildrenWithinDimension(KdTreeNode<T> node);
    }

    #endregion
}