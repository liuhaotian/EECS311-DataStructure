using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment_1
{
    /// <summary>
    /// A double-ended queue
    /// Implement this as a doubly-linked list
    /// </summary>
    public class Deque
    {
        private class doubleLLC
        {
            public doubleLLC prev;
            public doubleLLC next;
            public object o;
        }
        private doubleLLC head;
        private doubleLLC tail = new doubleLLC();
        private int count = 0;
        public Deque() {
            head = tail;
        }

        /// <summary>
        /// Add object to end of queue
        /// </summary>
        /// <param name="o">object to add</param>
        public void AddFront(object o)
        {
            // Remove this line when you implement this method
            //throw new NotImplementedException();
            head.prev = new doubleLLC();
            head.o = o;
            head.prev.next = head;
            head = head.prev;
            count++;
        }

        /// <summary>
        /// Remove object from beginning of queue.
        /// </summary>
        /// <returns>Object at beginning of queue</returns>
        public object RemoveFront()
        {
            // Remove this line when you implement this mehtod
            //throw new NotImplementedException();
            object ret;
            if (count == 0)
                throw new QueueEmptyException();
            else
            {
                ret = head.o;
                if (head.next != null)
                {
                    head = head.next;
                    head.prev = null;
                }
                else
                    head.o = null;
                count--;
            }
            return ret;
        }

        /// <summary>
        /// Add object to end of queue
        /// </summary>
        /// <param name="o">object to add</param>
        public void AddEnd(object o)
        {
            // Remove this line when you implement this method
            //throw new NotImplementedException();
            tail.next = new doubleLLC();
            tail.o = o;
            tail.next.prev = tail;
            tail = tail.next;
            count++;
        }

        /// <summary>
        /// Remove object from beginning of queue.
        /// </summary>
        /// <returns>Object at beginning of queue</returns>
        public object RemoveEnd()
        {
            // Remove this line when you implement this mehtod
            //throw new NotImplementedException();
            object ret;
            if(tail.o != null)
                ret = tail.o;
            else
                throw new QueueEmptyException();
            if (tail.prev != null)
            {
                tail = tail.prev;
                tail.next = null;
            }
            else
                tail.o = null;
            count--;
            return ret;
        }

        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return count;
            }
        }

        /// <summary>
        /// True if the queue is empty and dequeuing is forbidden.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                // Remove this line when you fill this method in.
                //throw new NotImplementedException();
                return count == 0;
            }
        }
    }
}
