using System.Drawing;
using PathPlanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    
    
    /// <summary>
    ///This is a test class for BinaryHeapTest and is intended
    ///to contain all BinaryHeapTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BinaryHeapTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        /// A test for Add; make sure the count increases as you add.
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            var q = new BinaryHeap(100);
            q.TestHeapValidity(); 
            q.Add(new Node("foo", new PointF(0, 0)), 0);
            q.TestHeapValidity();
            Assert.AreEqual(1, q.Count, "Count did not increase after adding to heap.");
            q.Add(new Node("foo", new PointF(0, 0)), -1);
            q.TestHeapValidity(); 
            Assert.AreEqual(2, q.Count, "Count did not increase after adding to heap.");
            q.Add(new Node("foo", new PointF(0, 0)), 1);
            q.TestHeapValidity(); 
            Assert.AreEqual(3, q.Count, "Count did not increase after adding to heap.");
        }

        /// <summary>
        /// A test for ExtractMin.
        /// Add three objects with the lowest priority one in the middle, and see if you can extract them in the right order.
        ///</summary>
        [TestMethod()]
        public void ExtractTest1()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 1);
            q.TestHeapValidity();
            q.Add(b, 0);
            q.TestHeapValidity();
            q.Add(c, 10);
            q.TestHeapValidity();
            Assert.AreEqual(b,q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();            
        }

        /// <summary>
        /// A test for ExtractMin.
        /// Add three objects in order of priority, and see if you can extract them in the right order.
        ///</summary>
        [TestMethod()]
        public void ExtractTest2()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 0);
            q.TestHeapValidity();
            q.Add(b, 1);
            q.TestHeapValidity();
            q.Add(c, 10);
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(b, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
        }

        /// <summary>
        /// A test for ExtractMin.
        /// Add three objects wih the lowest priority one last, and see if you can extract them in the right order.
        ///</summary>
        [TestMethod()]
        public void ExtractTest3()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 1);
            q.TestHeapValidity();
            q.Add(b, 2);
            q.TestHeapValidity();
            q.Add(c, 0);
            q.TestHeapValidity();
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
            Assert.AreEqual(b, q.ExtractMin(), "ExtractMin did not return the queue item with the smallest priority.");
            q.TestHeapValidity();
        }

        /// <summary>
        /// Make a big heap with all the numbers from 0 to some number, but add them in semi-random order
        /// Then see if they come out in the right order.
        ///</summary>
        [TestMethod()]
        public void ExtractLotsTest()
        {
            int size = 997;  // It's important that this is a prime number
            var q = new BinaryHeap(size);
            int priority = 463;
            // Add objects with every possible priority from 0 to size, but in semi-random order
            // Depends on the result that (k1 + i k2) % p gives you all the numbers from 0 to p-1
            // for i=0 to p-1, for any k1,k2 and for any p that is prime.
            for (int i= 0;i<size;i++)
            {
                priority = (priority + 907) % size;    // 907 is another prime
                q.Add(new Node(priority.ToString(),
                               new PointF(0, 0))
                               { NodeCost = priority },
                      priority);
                q.TestHeapValidity();
            }
            for (int i = 0; i < size; i++)
            {
                Assert.AreEqual(i, q.ExtractMin().NodeCost, "Nodes extracted in wrong order.");
                q.TestHeapValidity();
            }
        }

        /// <summary>
        /// A test for DecreasePriority
        /// See if you can promote an object and have it come out first.
        ///</summary>
        [TestMethod()]
        public void DecreasePriorityPromotingItemTest()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 1);
            q.TestHeapValidity();
            q.Add(b, 0);
            q.TestHeapValidity();
            q.Add(c, 10);
            q.TestHeapValidity();
            q.DecreasePriority(c, -1);
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(b, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
        }

        /// <summary>
        /// A test for DecreasePriority
        /// See if changing the prority of the minimal item breaks something.
        ///</summary>
        [TestMethod()]
        public void DecreasePriorityOfMinTest()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 1);
            q.TestHeapValidity();
            q.Add(b, 0);
            q.TestHeapValidity();
            q.Add(c, 10);
            q.TestHeapValidity();
            q.DecreasePriority(b, -1);
            q.TestHeapValidity();
            Assert.AreEqual(b, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
        }

        /// <summary>
        /// A test for DecreasePriority
        /// Check if setting node to its current priority breaks something.
        ///</summary>
        [TestMethod()]
        public void DecreasePriorityNotChangingPriorityTest()
        {
            var a = new Node("a", new PointF(0, 0));
            var b = new Node("b", new PointF(0, 0));
            var c = new Node("c", new PointF(0, 0));
            var q = new BinaryHeap(10);
            q.Add(a, 0);
            q.TestHeapValidity();
            q.Add(b, 1);
            q.TestHeapValidity();
            q.Add(c, 2);
            q.TestHeapValidity();
            q.DecreasePriority(b, 1);
            q.TestHeapValidity();
            Assert.AreEqual(a, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(b, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
            Assert.AreEqual(c, q.ExtractMin(), "ExtractMin did not return the expected item after DecreasePriority was called.");
            q.TestHeapValidity();
        }

        /// <summary>
        /// Try decreasing the key on an item in a big heap
        ///</summary>
        [TestMethod()]
        public void DecreaseProrityBigHeapTest()
        {
            int size = 997;  // It's important that this is a prime number
            var q = new BinaryHeap(size);
            int priority = 463;
            Node x = null;
            // Add objects with every possible priority from 0 to size, but in semi-random order
            // Depends on the result that (k1 + i k2) % p gives you all the numbers from 0 to p-1
            // for i=0 to p-1, for any k1,k2 and for any p that is prime.
            for (int i = 0; i < size; i++)
            {
                priority = (priority + 907) % size;    // 907 is another prime
                var item = new Node(priority.ToString(), new PointF(0, 0)) {NodeCost = priority};
                if (priority == 350)
                    x = item;
                // Add the item, but if it's number 350, give it a big priority
                // and remember it so we can lower the priority again later.
                q.Add(item, priority==350?10000:priority);
                q.TestHeapValidity();
            }
            // Now set its priority to what it would have been
            q.DecreasePriority(x, 350);
            q.TestHeapValidity();
            // Now see if we get everything in order.
            for (int i = 0; i < size; i++)
            {
                Assert.AreEqual(i, q.ExtractMin().NodeCost, "Nodes extracted in wrong order.");
                q.TestHeapValidity();
            }
        }
    }
}
