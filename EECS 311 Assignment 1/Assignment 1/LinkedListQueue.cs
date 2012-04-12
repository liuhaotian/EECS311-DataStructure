using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment_1
{
    /// <summary>
    /// A queue internally implemented as a linked list of objects
    /// </summary>
    public class LinkedListQueue : Queue
    {
        private class doubleLLC {
            public doubleLLC prev;
            public doubleLLC next;
            public object o;
        }
        private doubleLLC head;
        private doubleLLC tail = new doubleLLC();
        private int count = 0;
        public LinkedListQueue()
        {
            head = tail;
        }

        /// <summary>
        /// Add object to end of queue
        /// </summary>
        /// <param name="o">object to add</param>
        public override void Enqueue(object o)
        {
            // Remove this line when you implement this method
            //throw new NotImplementedException();
            tail.o = o;
            tail.next = new doubleLLC();
            tail.next.prev = tail;
            tail = tail.next;
            count++;
        }

        /// <summary>
        /// Remove object from beginning of queue.
        /// </summary>
        /// <returns>Object at beginning of queue</returns>
        public override object Dequeue()
        {
            // Remove this line when you implement this mehtod
            //throw new NotImplementedException();
            object ret;
            if (head.o != null)
                ret = head.o;
            else
                throw new QueueEmptyException();
            if (head.next != null)
            {
                head = head.next;
                head.prev = null;
            }
            else
                head.o = null;
            count--;
            return ret;
        }

        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public override int Count
        {
            get
            {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return count;
            }
        }

        /// <summary>
        /// True if the queue is full and enqueuing of new elements is forbidden.
        /// Note: LinkedListQueues can be grown to arbitrary length, and so can
        /// never fill.
        /// </summary>
        public override bool IsFull
        {
            get
            {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return false;
            }
        }
    }
}
