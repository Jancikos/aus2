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

        private int _countNodes(KdTreeNode<T>? node)
        {
            if (node is null)
            {
                return 0;
            }

            // POZOR na rekurziu => nahradit iteratorom
            return 1 + _countNodes(node.LeftChild) + _countNodes(node.RightChild);
        }
        public T? Find(T filter)
        {
            if (_rootNode is null)
            {
                return null;
            }

            var node = _findConcretNode(_rootNode, filter, out _);

            return node?.Data ?? null;
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

            throw new NotImplementedException("Deletion of not leaf node is not implemented yet.");
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

        /// <value>level of the node in the tree</value>
        public int Level
        {
            get => _getLevel();
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
}
