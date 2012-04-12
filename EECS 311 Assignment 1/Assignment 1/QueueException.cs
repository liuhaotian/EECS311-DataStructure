using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assignment_1
{
    public class QueueException : Exception
    {
        public QueueException(string message)
            : base(message)
        { }
    }

    public class QueueEmptyException : QueueException
    {
        public QueueEmptyException()
            : base("Attempt to remove from an empty queue")
        { }
    }

    public class QueueFullException : QueueException
    {
        public QueueFullException()
            : base("Attempt to add to a full queue")
        { }
    }
}
