using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment_1
{
    /// <summary>
    /// A queue internally implemented as an array
    /// </summary>
    public class ArrayQueue : Queue
    {
        private int head = 0;
        private int tail = 0;
        private object[] arrayQueue = new object[100];
        
        /// <summary>
        /// Add object to end of queue
        /// </summary>
        /// <param name="o">object to add</param>
        public override void Enqueue(object o)
        {
            // Remove this line when you implement this method
            //throw new NotImplementedException();
            if ((head - 1 - tail + arrayQueue.Length) % arrayQueue.Length == 0)
                throw new QueueFullException();
            else
            {
                arrayQueue[tail] = o;
                tail = (tail + 1) % arrayQueue.Length;
            }
        }

        /// <summary>
        /// Remove object from beginning of queue.
        /// </summary>
        /// <returns>Object at beginning of queue</returns>
        public override object Dequeue()
        {
            // Remove this line when you implement this mehtod
            //throw new NotImplementedException();
            if (head == tail)
                throw new QueueEmptyException();
            object ret = arrayQueue[head];
            head = (head + 1) % arrayQueue.Length;
            return ret;
        }

        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public override int Count
        {
            get { 
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return (tail - head + arrayQueue.Length) % arrayQueue.Length;
            }
        }

        /// <summary>
        /// True if the queue is full and enqueuing of new elements is forbidden.
        /// </summary>
        public override bool IsFull
        {
            get {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return (head - 1 - tail + arrayQueue.Length) % arrayQueue.Length == 0;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return head == tail;
            }
        }
    }
}
