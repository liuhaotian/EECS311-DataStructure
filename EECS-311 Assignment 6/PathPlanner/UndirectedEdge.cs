namespace PathPlanner
{
    /// <summary>
    /// Represents a weighted edge between two nodes, A and B.
    /// </summary>
    public class UndirectedEdge
    {
        /// <summary>
        /// Creates a new edge object.  Does not add it to the node list of either node.
        /// </summary>
        public UndirectedEdge(Node a, Node b, double cost)
        {
            A = a;
            B = b;
            Cost = cost;
        }

        /// <summary>
        /// One of the two nodes this edge links.
        /// </summary>
        public readonly Node A;

        /// <summary>
        /// The other node (besides A) this edge links.
        /// </summary>
        public readonly Node B;

        /// <summary>
        /// The cost (length) of the edge.
        /// </summary>
        public readonly double Cost;

        /// <summary>
        /// Returns whichever node this node links to the specified node.
        /// I.e. it returns the "other" node than this one.
        /// </summary>
        /// <param name="n">One of the two nodes in this edge</param>
        /// <returns>The other node that isn't n.</returns>
        public Node OtherNode(Node n)
        {
            return (n == A) ? B : A;
        }
    }
}
