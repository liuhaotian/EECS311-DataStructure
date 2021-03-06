﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace PathPlanner
{
    /// <summary>
    /// Represents a graph to search
    /// </summary>
    public class UndirectedGraph
    {
        /// <summary>
        /// Creates a new graph with the specified nodes.
        /// </summary>
        public UndirectedGraph(Node[] nodes)
        {
            Nodes = nodes;
        }

        /// <summary>
        /// The nodes of the graph
        /// </summary>
        public readonly Node[] Nodes;

        /// <summary>
        /// Finds the shortest path between the specified nodes and returns it as a list of nodes.
        /// </summary>
        /// <param name="start">The starting node for the path</param>
        /// <param name="end">The ending node for the path</param>
        /// <returns>The path, represented as a list of nodes beginning with start and ending with end.</returns>
        public List<Node> FindPath(Node start, Node end)
        {
            //throw new NotImplementedException();
            List<Node> ret = new List<Node>(Nodes.Length);
            var heap = new BinaryHeap(Nodes.Length);
            
            double MaxCost = 0;
            foreach(Node tempnode in Nodes){
                foreach (UndirectedEdge tempedge in tempnode.Edges){
                    MaxCost += tempedge.Cost;
                }
            }
            MaxCost += 1;

            foreach (Node tempnode in Nodes)
            {
                tempnode.NodeCost = MaxCost;
                heap.Add(tempnode, MaxCost);
            }
            heap.DecreasePriority(start, 0);
            start.NodeCost = 0;
            start.Predecessor = null;

            while (heap.Count != 0)
            {
                Node tempnode = heap.ExtractMin();
                foreach (UndirectedEdge tempedge in tempnode.Edges)
                {
                    Node adjacent = tempedge.A;
                    if (adjacent == tempnode) adjacent = tempedge.B;

                    if (adjacent.NodeCost > tempedge.Cost + tempnode.NodeCost)
                    {
                        adjacent.NodeCost = tempedge.Cost + tempnode.NodeCost;
                        heap.DecreasePriority(adjacent, adjacent.NodeCost);
                        adjacent.Predecessor = tempnode;
                    }
                }
            }

            Node prevnode = end;
            ret.Add(prevnode);
            while (prevnode.Predecessor != null)
            {
                prevnode = prevnode.Predecessor;
                ret.Add(prevnode);
            }

            ret.Reverse();
            return ret;
        }

        #region Utility functions
        /// <summary>
        /// Reads a graph from a spreadsheet.
        /// </summary>
        public static UndirectedGraph FromSpreadsheet(string path)
        {
            object[][] data = Spreadsheet.ConvertAllNumbers(Spreadsheet.Read(path, ','));
            List<Node> nodes = new List<Node>();
            Dictionary<string, Node> nodeNames = new Dictionary<string, Node>();
            
            // Make all the nodes, one per row
            for (int rowNum = 1; rowNum<data.Length; rowNum++)
            {
                object[] row = data[rowNum];
                string name = (string)row[0];
                Node n = new Node(name, new PointF(Convert.ToSingle(row[1]), Convert.ToSingle(row[2])));
                nodes.Add(n);
                nodeNames[name] = n;
            }

            // Add all the edges
            for (int rowNum = 1; rowNum < data.Length; rowNum++)
            {
                object[] row = data[rowNum];
                string name = (string)row[0];
                Node n = nodeNames[name];
                for (int colNum = 3; colNum<row.Length; colNum++)
                {
                    string neighborName = (string)row[colNum];
                    if (neighborName != "")
                    {
                        Node neighbor = nodeNames[neighborName];
                        n.AddEdge(neighbor);
                    }
                }
            }

            // Finally, make the graph
            return new UndirectedGraph(nodes.ToArray());
        }

        /// <summary>
        /// Finds the node with a given name.
        /// </summary>
        /// <param name="nodeName">Name to search for</param>
        /// <returns>The node with that name, or null if there is no such node.</returns>
        public Node FindNode(String nodeName)
        {
            return Array.Find(Nodes, n => n.Name == nodeName);
        }
        #endregion
    }
}
