using System.Collections;
using FRI.AUS2.Libs.Structures.Trees.Interfaces;

namespace FRI.AUS2.Libs.Structures.Trees
{
    public class KdTree<T> where T : class, IKdTreeData
    {
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

        public KdTree()
        {
            _rootNode = null;
        }

        public void Clear()
        {
            _rootNode = null;
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">root node of hierarchy that will be visited </param>
        /// <param name="data"></param>
        /// <param name="lastVisitedNode">the node, which was visited last before returning value</param>
        /// <returns> the node where the data lives</returns>
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
        /// 
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

            var nodesToProcess = new Queue<KdTreeNode<T>>();
            nodesToProcess.Enqueue(rootNode);

            int count = 0;
            while (nodesToProcess.Count > 0)
            {
                var currentNode = nodesToProcess.Dequeue();

                if (currentNode is null)
                {
                    continue;
                }

                if (currentNode.LeftChild is not null)
                {
                    nodesToProcess.Enqueue(currentNode.LeftChild);
                }

                if (currentNode.RightChild is not null)
                {
                    nodesToProcess.Enqueue(currentNode.RightChild);
                }

                count++;
            }

            return count;
        }
        public IList<T> Find(T filter)
        {
            var data = new List<T>();

            if (_rootNode is null)
            {
                return data;
            }

            KdTreeNode<T>? node;
            var currentParent = _rootNode;
            do
            {
                node = _findConcretNode(currentParent, filter, out _);

                if (node is not null)
                {
                    data.Add(node.Data);
                }

                // search also in left subtree
                currentParent = node?.LeftChild;
            } while (currentParent is not null);

            return data;
        }

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
                    throw new InvalidOperationException("Node not found.");
                }
                return;
            }

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
                return;
            }

            // nodes stack - nodes will be replaced from the bottom to the top
            Stack<KdTreeNode<T>> nodesToBeReplaced = new Stack<KdTreeNode<T>>();
            nodesToBeReplaced.Push(node);

            bool isLastReplacedNodeLeaf = false;
            // replace nodes from the bottom to the top
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
        }

        /// <summary>
        /// finds replacement node for the given node
        /// 
        /// replacement node is the that can be used to replace the given node (when the given node wants to be removed)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private KdTreeNode<T>? _findReplacementNode(KdTreeNode<T>? node)
        {
            KdTreeNode<T>? replacementNode = null;

            // if node has right child
            // // fid node with minimum value (in the same dimension) in the right subtree
            if (node?.RightChild is not null)
            {
                // find node with minimum value in the right subtree
                var nodeDimension = node.Dimension;
                var minNode = node.RightChild;
                var minDValue = minNode.Data.GetDiminesionValue(nodeDimension);

                // search the whole tree where node.RightChild is root
                var it = GetInOrderIterator(node.RightChild);
                while (it.MoveNext())
                {
                    var currentNode = it.CurrentNode;
                    var currentDValue = currentNode?.Data?.GetDiminesionValue(nodeDimension);

                    if (currentDValue is not null && currentDValue < minDValue)
                    {
                        // FUTURE - porovnaj levely uzlov a vymemn, len ak je ten novy node leaf

                        minNode = it.CurrentNode;
                        minDValue = currentDValue.Value;
                    }
                }

                replacementNode = minNode;
            }


            // if node has no right child, but has left child
            // // find node with maximum value (in the same dimension) in the left subtree
            if (replacementNode is null && node?.LeftChild is not null)
            {
                // find node with maximum value in the left subtree
                var nodeDimension = node.Dimension;
                var maxNode = node.LeftChild;
                var maxDValue = maxNode.Data.GetDiminesionValue(nodeDimension);

                // search the whole tree where node.LeftChild is root
                var it = GetInOrderIterator(node.LeftChild);
                while (it.MoveNext())
                {
                    var currentNode = it.CurrentNode;
                    var currentDValue = currentNode?.Data?.GetDiminesionValue(nodeDimension);

                    if (currentDValue is not null && currentDValue > maxDValue)
                    {
                        // FUTURE - porovnaj levely uzlov a vymen, len ak je ten novy node leaf

                        maxNode = it.CurrentNode;
                        maxDValue = currentDValue.Value;
                    }
                }

                replacementNode = maxNode;
            }

            return replacementNode;
        }

        /// <summary>
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

        private void _setRootNode(KdTreeNode<T>? newRoot)
        {
            _rootNode = newRoot;
        }

        public KdTreeInOrderIterator<T>? GetInOrderIterator(T filter)
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

            return GetInOrderIterator(node);
        }


        public KdTreeInOrderIterator<T> GetInOrderIterator(KdTreeNode<T> rootNode)
        {
            return new KdTreeInOrderIterator<T>(rootNode);
        }

        /// <summary>
        /// throws exception if the filter is not found
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveException(T filter)
        {
            Remove(filter, true);
        }
    }

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

        /// <returns>depth of the subtree</returns>
        private int _getSubtreeDepth(KdTreeNode<T>? node)
        {
            if (node is null)
            {
                return 0;
            }

            // POZOR na rekurziu
            return 1 + System.Math.Max(_getSubtreeDepth(node.LeftChild), _getSubtreeDepth(node.RightChild));
        }
    }

    public class KdTreeInOrderIterator<T> : IEnumerator<T>
        where T : class, IKdTreeData
    {
        private Queue<KdTreeNode<T>> _nodesToProcess;

        private KdTreeNode<T>? _root;
        private KdTreeNode<T>? _current;
        public T Current => _current?.Data ?? throw new InvalidOperationException("Current node is not set.");
        internal KdTreeNode<T>? CurrentNode => _current;

        object IEnumerator.Current => Current;

        public KdTreeInOrderIterator(KdTreeNode<T> rootNode)
        {
            _nodesToProcess = new Queue<KdTreeNode<T>>();
            _root = rootNode;
            _processNode(_root);
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
            return true;
        }

        public void Reset()
        {
            _nodesToProcess.Clear();
            _processNode(_root);
        }

        private void _processNode(KdTreeNode<T>? node)
        {
            if (node is null)
            {
                return;
            }

            _processNode(node.LeftChild);
            _nodesToProcess.Enqueue(node);
            _processNode(node.RightChild);
        }
    }
}
