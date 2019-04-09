// -----------------------------------------------------------------------------------
//     <copyright file="Network.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;

namespace RabCab.Utilities.Network
{
    internal class Link<T>
    {
        public Link(T fromValue, T toValue, double weightValue = 1.0)
        {
            From = fromValue;
            To = toValue;
            Weight = weightValue;
        }

        public T From { get; set; }
        public T To { get; set; }
        public double Weight { get; set; }
    }

    internal class Network<T>
    {
        private readonly Graph<T> graph = new Graph<T>();
        private readonly List<List<T>> PathSetList = new List<List<T>>();

        public Network()
        {
        }

        public Network(List<Link<T>> links,
            bool bi_connected = false)
        {
            foreach (var value in links)
            {
                var from = value.From;
                var to = value.To;
                var weight = value.Weight;

                graph.AddNode(from);
                graph.AddNode(to);

                if (bi_connected)
                    graph.AddUndirectedEdge(from, to, weight);
                else
                    graph.AddDirectedEdge(from, to, weight);
            }
        }

        private Network(List<Link<int>> links,
            bool bi = false)
        {
        }

        ~Network()
        {
        }

        public void AddPath(List<T> p)
        {
            PathSetList.Add(p);
        }

        public void ShowPaths()
        {
            foreach (var value in PathSetList)
            {
                var path = value;

                foreach (var str in path) Debug.Write(str + " ");

                Debug.WriteLine("");
            }
        }

        public List<List<T>> GetPaths()
        {
            var pathList = new List<List<T>>();

            foreach (var value in PathSetList) pathList.Add(value);

            return pathList;
        }

        // Get all nodes adjacent to n
        public NodeList<T> GetAdjNodeIDs(T label)
        {
            var nodeList = graph.Nodes;
            var node = (GraphNode<T>) nodeList.FindByValue(label);
            var neighbors = node.Neighbors;

            return neighbors;
        }
    }
}