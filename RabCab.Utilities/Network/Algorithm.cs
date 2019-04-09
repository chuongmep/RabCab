// -----------------------------------------------------------------------------------
//     <copyright file="Algorithm.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;

namespace RabCab.Utilities.Network
{
    internal class Algorithm<T>
    {
        private Network<T> _network;
        private T _startNode;

        public Algorithm(Network<T> network)
        {
            SetNetwork(network);
        }

        private void SetNetwork(Network<T> nw)
        {
            _network = nw;
        }

        // Algorithm to recursively search for all paths between
        // chosen source - destination nodes
        private void DepthFirst(Network<T> network,
            List<T> visited,
            T end,
            int max_hops,
            int min_hops)
        {
            var back = visited[visited.Count - 1];

            var adjNodes = network.GetAdjNodeIDs(back);

            // Examine adjacent nodes
            foreach (var node in adjNodes)
            {
                var nodeString = node.Value;

                var startEqualTarget = ContainsNode(visited, nodeString) &&
                                       Compare(_startNode, end) &&
                                       Compare(nodeString, _startNode);

                if (ContainsNode(visited, nodeString) && !startEqualTarget) continue;

                if (Compare(nodeString, end))
                {
                    visited.Add(nodeString);

                    // Get hop count for this path
                    var size = visited.Count;
                    var hops = size - 1;

                    if ((max_hops < 1 || hops <= max_hops) && hops >= min_hops)
                    {
                        var path = new List<T>(visited);
                        network.AddPath(path);
                    }

                    visited.RemoveAt(hops);
                    break;
                }
            }

            // in breadth-first, recursion needs to come after visiting adjacent nodes
            foreach (var node in adjNodes)
            {
                var nodeString = node.Value;

                if (ContainsNode(visited, nodeString) || Compare(nodeString, end))
                    continue;

                visited.Add(nodeString);

                DepthFirst(network, visited, end, max_hops, min_hops);

                var n = visited.Count - 1;
                visited.RemoveAt(n);
            }
        }

        public void GetAllPaths(Network<T> network,
            T start,
            T target,
            int max_hops,
            int min_hops)
        {
            _startNode = start;

            var visited = new List<T>();
            visited.Add(start);

            DepthFirst(network, visited, target, max_hops, min_hops);
        }

        private bool ContainsNode(List<T> nodes, T node)
        {
            foreach (var n in nodes)
                if (Compare(n, node))
                    return true;

            return false;
        }

        public bool Compare(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}