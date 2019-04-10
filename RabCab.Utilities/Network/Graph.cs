// -----------------------------------------------------------------------------------
//     <copyright file="Graph.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace RabCab.Network
{
    public class Graph<T> : IEnumerable<T>
    {
        public Graph() : this(null)
        {
        }

        public Graph(NodeList<T> nodeSet)
        {
            if (nodeSet == null)
                Nodes = new NodeList<T>();
            else
                Nodes = nodeSet;
        }

        public NodeList<T> Nodes { get; }
        public int Count => Nodes.Count;

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void AddNode(GraphNode<T> Node)
        {
            if (!Contains(Node.Value)) Nodes.Add(Node);
        }

        public void AddNode(T value)
        {
            if (!Contains(value)) Nodes.Add(new GraphNode<T>(value));
        }

        public void AddDirectedEdge(T from, T to, double cost)
        {
            var fromNode = (GraphNode<T>) Nodes.FindByValue(from);
            if (fromNode != null)
            {
                var toNode = (GraphNode<T>) Nodes.FindByValue(to);

                if (toNode != null)
                {
                    fromNode.Neighbors.Add(toNode);
                    fromNode.Costs.Add(cost);
                }
            }
        }

        public void AddUndirectedEdge(T from, T to, double cost)
        {
            AddDirectedEdge(from, to, cost);
            AddDirectedEdge(to, from, cost);
        }

        public bool Contains(T value)
        {
            return Nodes.FindByValue(value) != null;
        }

        public bool Remove(T value)
        {
            // first remove the Node from the nodeset
            var nodeToRemove = (GraphNode<T>) Nodes.FindByValue(value);
            if (nodeToRemove == null)
                // Node wasn't found
                return false;

            // otherwise, the Node was found
            Nodes.Remove(nodeToRemove);

            // enumerate through each Node in the nodeSet, removing edges to this Node
            foreach (GraphNode<T> gnode in Nodes)
            {
                var index = gnode.Neighbors.IndexOf(nodeToRemove);
                if (index != -1)
                {
                    // remove the reference to the Node and associated cost
                    gnode.Neighbors.RemoveAt(index);
                    gnode.Costs.RemoveAt(index);
                }
            }

            return true;
        }
    }
}