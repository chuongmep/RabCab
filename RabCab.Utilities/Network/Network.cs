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

namespace RabCab.Network
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
        private readonly Graph<T> _graph = new Graph<T>();
        private readonly List<List<T>> _pathSetList = new List<List<T>>();

        public Network()
        {
        }

        public Network(List<Link<T>> links,
            bool biConnected = false)
        {
            foreach (var value in links)
            {
                var from = value.From;
                var to = value.To;
                var weight = value.Weight;

                _graph.AddNode(from);
                _graph.AddNode(to);

                if (biConnected)
                    _graph.AddUndirectedEdge(from, to, weight);
                else
                    _graph.AddDirectedEdge(from, to, weight);
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
            _pathSetList.Add(p);
        }

        public void ShowPaths()
        {
            foreach (var value in _pathSetList)
            {
                var path = value;

                foreach (var str in path) Debug.Write(str + " ");

                Debug.WriteLine("");
            }
        }

        public List<List<T>> GetPaths()
        {
            var pathList = new List<List<T>>();

            foreach (var value in _pathSetList) pathList.Add(value);

            return pathList;
        }

        // Get all nodes adjacent to n
        public NodeList<T> GetAdjNodeIDs(T label)
        {
            var nodeList = _graph.Nodes;
            var node = (GraphNode<T>) nodeList.FindByValue(label);
            var neighbors = node.Neighbors;

            return neighbors;
        }
    }
}