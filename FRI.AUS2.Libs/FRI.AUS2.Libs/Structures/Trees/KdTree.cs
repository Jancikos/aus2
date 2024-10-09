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

        public KdTree()
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

            KdTreeNode<T>? parentNode = _findNode(data, out var lastVisitedNode);
            if (parentNode is not null)
            {
                // data already exists, duplicate => go to the deepest left child
                while (parentNode.LeftChild is not null)
                {
                    parentNode = parentNode.LeftChild;
                }
            }
            else
            {
                // data does not exist, insert new node to the last visited node
                parentNode = lastVisitedNode;
            }

            if (parentNode is null)
            {
                throw new InvalidOperationException("Invalid tree state. Parent for data not found.");
            }

            KdTreeNode<T> newNode = new KdTreeNode<T>(parentNode, data);
            int comparison = data.Compare(_getNodeLevel(parentNode), parentNode.Data);
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
        /// <param name="data"></param>
        /// <param name="lastVisitedNode">the node, which was visited last before returning value</param>
        /// <returns> the node where the data lives</returns>
        private KdTreeNode<T>? _findNode(T data, out KdTreeNode<T>? lastVisitedNode)
        {
            lastVisitedNode = null;
            var currentNode = _rootNode;

            while (currentNode != null)
            {
                lastVisitedNode = currentNode;
                int comparison = data.Compare(_getNodeLevel(currentNode), currentNode.Data);

                if (comparison == 0)
                {
                    // found
                    return currentNode;
                }

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

        private int _getNodeLevel(KdTreeNode<T> node)
        {
            int level = 0;
            var currentNode = node;

            while (currentNode.Parent != null)
            {
                level++;
                currentNode = currentNode.Parent;
            }

            return level;
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
        public T? Find(T data)
        {
            var node = _findNode(data, out _);

            return node?.Data ?? null;
        }

        public void Remove(T data)
        {
            throw new NotImplementedException();
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

        public KdTreeNode(KdTreeNode<T>? parent, T data)
        {
            Parent = parent;
            Data = data;
        }
    }
}
