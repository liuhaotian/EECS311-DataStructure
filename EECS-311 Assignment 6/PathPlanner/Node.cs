using System;
using System.Collections.Generic;
using System.Drawing;

namespace PathPlanner
{
    /// <summary>
    /// Represents a graph node
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Create a new graph node with no edges.
        /// </summary>
        public Node(string name, PointF position)
        {
            Name = name;
            Position = position;
        }

        #region General node information
        /// <summary>
        /// The name of the node, for debugging and user interface purposes.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Screen position of the node.
        /// </summary>
        public PointF Position { get; set; }

        /// <summary>
        /// Edges of the node
        /// </summary>
        public readonly List<UndirectedEdge> Edges = new List<UndirectedEdge>();

        #endregion

        #region Node information specific to Dijkstra's algorithm.
        /// <summary>
        /// Position of this node in whatever priority queue it's being held in.
        /// This is provided to allow the BinaryHeap code to quickly find the node in the
        /// heap in its arrays when doing DecreaseKey.
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Estimated cost of getting to the node from the start point.
        /// </summary>
        public double NodeCost { get; set; }

        public Node Predecessor { get; set; }
        #endregion

        /// <summary>
        /// Creates an edge between this node and another node.
        /// Its cost is set to the distance between their screen positions.
        /// </summary>
        public void AddEdge(Node neighbor)
        {
            double cost = Math.Sqrt(Square(Position.X - neighbor.Position.X) + Square(Position.Y - neighbor.Position.Y));
            UndirectedEdge e = new UndirectedEdge(this, neighbor, cost);
            Edges.Add(e);
            neighbor.Edges.Add(e);
        }
        double Square(double x) { return x * x; }
    }
}
