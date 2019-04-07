using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RabCab.Utilities.Network
{
    public class Node<T>
    {
        // Private member-variables

        public Node()
        {
        }

        public Node(T data) : this(data, null)
        {
        }

        public Node(T data, NodeList<T> neighbors)
        {
            Value = data;
            Neighbors = neighbors;
        }

        public T Value { get; set; }
        protected NodeList<T> Neighbors { get; set; }
    }

    public class NodeList<T> : Collection<Node<T>>
    {
        public NodeList()
        {
        }

        public NodeList(int initialSize)
        {
            // Add the specified number of items
            for (var i = 0; i < initialSize; i++)
                Items.Add(default(Node<T>));
        }

        public Node<T> FindByValue(T value)
        {
            // search the list for the value
            foreach (var node in Items)
                if (node.Value.Equals(value))
                    return node;

            // if we reached here, we didn't find a matching node
            return null;
        }
    }

    public class GraphNode<T> : Node<T>
    {
        private List<double> _costs;

        public GraphNode()
        {
        }

        public GraphNode(T value) : base(value)
        {
        }

        public GraphNode(T value, NodeList<T> neighbors) : base(value, neighbors)
        {
        }

        public new NodeList<T> Neighbors
        {
            get
            {
                if (base.Neighbors == null)
                    base.Neighbors = new NodeList<T>();

                return base.Neighbors;
            }
        }

        public List<double> Costs
        {
            get
            {
                if (_costs == null)
                    _costs = new List<double>();

                return _costs;
            }
        }
    }
}