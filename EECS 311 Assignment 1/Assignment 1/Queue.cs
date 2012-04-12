using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment_1
{
    /// <summary>
    /// An abstract queue of objects
    /// </summary>
    public abstract class Queue
    {
        /// <summary>
        /// Add object to end of queue
        /// </summary>
        /// <param name="o">object to add</param>
        public abstract void Enqueue(object o);

        /// <summary>
        /// Remove object from beginning of queue.
        /// </summary>
        /// <returns>Object at beginning of queue</returns>
        public abstract object Dequeue();

        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// True if the queue is full and enqueuing of new elements is forbidden.
        /// </summary>
        public abstract bool IsFull { get; }

        /// <summary>
        /// True if the queue is empty and dequeuing is forbidden.
        /// </summary>
        public virtual bool IsEmpty { get { return Count == 0; } }
    }
}
