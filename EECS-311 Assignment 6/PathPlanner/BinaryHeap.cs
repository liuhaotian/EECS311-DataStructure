using System;

namespace PathPlanner
{
    /// <summary>
    /// A min-type priority queue of Nodes
    /// </summary>
    public class BinaryHeap
    {
        #region Instance variables

        readonly Node[] data;
        readonly double[] priorities;
        int count;
        #endregion

        /// <summary>
        /// Creates a new, empty priority queue with the specified capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of nodes that will be stored in the queue.</param>
        public BinaryHeap(int capacity) {
            data = new Node[capacity];
            priorities = new double[capacity];
            count = 0;
        }

        /// <summary>
        /// Adds an item to the queue.  Is position is determined by its priority relative to the other items in the queue.
        /// This is called HeapInsert in the lecture slides.
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="priority">Priority value to attach to this item.  Note: this is a min heap, so lower priority values come out first.</param>
        public void Add(Node item, double priority) {
            if (count == data.Length)
                throw new Exception("Heap capacity exceeded");

            // Add the item to the heap in the end position of the array (i.e. as a leaf of the tree)
            int position = count++;
            data[position] = item;
            item.QueuePosition = position;
            priorities[position] = priority;
            // Move it upward into position, if necessary
            MoveUp(position);
        }

        /// <summary>
        /// Extracts the item in the queue with the minimal priority value.
        /// </summary>
        /// <returns></returns>
        public Node ExtractMin()
        {
            //throw new NotImplementedException();
            Node ret = data[0];
            if (1 == count)
            {
                count--;
                return ret;
            }
            else
            {
                Swap(0, count - 1);
                count--;
                MoveDown(0);
                return ret;
            }
        }

        /// <summary>
        /// Reduces the priority of a node already in the queue.
        /// Called DecreaseKey in the lecture slides
        /// </summary>
        public void DecreasePriority(Node n, double priority)
        {
            //throw new NotImplementedException();
            if(priorities[n.QueuePosition] < priority){
                priorities[n.QueuePosition] = priority;
                MoveDown(n.QueuePosition);
            }
            else{
                priorities[n.QueuePosition] = priority;
                MoveUp(n.QueuePosition);
            }
        }

        /// <summary>
        /// Moves the node at the specified position upward, it it violates the Heap Property.
        /// This is the while loop from the HeapInsert procedure in the slides.
        /// </summary>
        /// <param name="position"></param>
        void MoveUp(int position)
        {
            //throw new NotImplementedException();
            if (position != 0)
            {
                int parent = Parent(position);
                if (priorities[parent] > priorities[position])
                {
                    Swap(parent, position);
                    MoveUp(parent);
                }
            }
        }

        /// <summary>
        /// Moves the node at the specified position down, if it violates the Heap Property
        /// Called "Heapify" in the lecture notes.
        /// </summary>
        /// <param name="position"></param>
        void MoveDown(int position) {
            //throw new NotImplementedException();
            int left = LeftChild(position);
            int right = RightChild(position);
            if (left < count && priorities[position] > priorities[left])
            {
                if (right >= count || priorities[right] > priorities[left])
                {
                    Swap(position, left);
                    MoveDown(left);
                }
                else
                {
                    Swap(position, right);
                    MoveDown(right);
                }
            }
            else if (right < count && priorities[position] > priorities[right] && priorities[left] > priorities[right])
            {
                if (left >= count || priorities[right] < priorities[left])
                {
                    Swap(position, right);
                    MoveDown(right);
                }
                else
                {
                    Swap(position, left);
                    MoveDown(left);
                }
            }
        }

        /// <summary>
        /// Number of items waiting in queue
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        #region Utilities
        /// <summary>
        /// Swaps the nodes at the respective positions in the heap
        /// Updates the nodes' QueuePosition properties accordingly.
        /// </summary>
        void Swap(int position1, int position2)
        {
            Node temp = data[position1];
            data[position1] = data[position2];
            data[position2] = temp;
            data[position1].QueuePosition = position1;
            data[position2].QueuePosition = position2;

            double temp2 = priorities[position1];
            priorities[position1] = priorities[position2];
            priorities[position2] = temp2;
        }

        /// <summary>
        /// Gives the position of a node's parent, the node's position in the queue.
        /// </summary>
        static int Parent(int position)
        {
            return (position - 1) / 2;
        }

        /// <summary>
        /// Returns the position of a node's left child, given the node's position.
        /// </summary>
        static int LeftChild(int position)
        {
            return 2 * position + 1;
        }

        /// <summary>
        /// Returns the position of a node's right child, given the node's position.
        /// </summary>
        static int RightChild(int position)
        {
            return 2 * position + 2;
        }

        /// <summary>
        /// Checks all entries in the heap to see if they satisfy the heap property.
        /// </summary>
        public void TestHeapValidity()
        {
            for (int i=1; i<count; i++)
                if (priorities[Parent(i)]>priorities[i])
                    throw new Exception("Heap violates the Heap Property at position "+i);
        }
        #endregion
    }
}
