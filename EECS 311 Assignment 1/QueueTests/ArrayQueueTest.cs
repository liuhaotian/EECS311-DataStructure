using Assignment_1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace QueueTests
{
    
    
    /// <summary>
    ///This is a test class for ArrayQueueTest and is intended
    ///to contain all ArrayQueueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArrayQueueTest
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

        [TestMethod()]
        public void ConstructorCreatesEmptyQueueTest()
        {
            ArrayQueue target = new ArrayQueue();
            Assert.IsTrue(target.IsEmpty, "A newly created ArrayQueue should have IsEmpty=true, but doesn't");
        }

        [TestMethod()]
        public void ConstructorCreatesNonFullQueueTest()
        {
            ArrayQueue target = new ArrayQueue();
            Assert.IsFalse(target.IsFull, "A newly created ArrayQueue should have IsFull=false, but doesn't");
        }

        [TestMethod()]
        public void QueueEventuallyFillsText()
        {
            ArrayQueue target = new ArrayQueue();
            int i;
            for (i = 0; i < 1000000 && !target.IsFull; i++)
                target.Enqueue(i);
            Assert.IsTrue(target.IsFull, "Added 1000000 elements to ArrayQueue and it never registered IsFull=true");
        }

        [TestMethod()]
        public void NonEmptyQueueReadsNotEmptyTest()
        {
            ArrayQueue target = new ArrayQueue();
            target.Enqueue(0);
            for (int i = 0; i < 1000000 && !target.IsFull; i++)
            {
                target.Enqueue(i);
                Assert.IsFalse(target.IsEmpty, string.Format("ArrayQueue reads as empty after adding {0} objects and never removing any", i+1));
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(QueueFullException), "Adding to full queue should throw QueueFullException")]
        public void EnqueueToFullQueueThrowsQueueFullExceptionTest()
        {
            ArrayQueue target = new ArrayQueue();
            Assert.IsFalse(target.IsFull);
            int i;
            for (i = 0; i < 1000000 && !target.IsFull; i++)
                target.Enqueue(i);
            Assert.IsTrue(target.IsFull, "Added 1000000 elements to ArrayQueue and it still doesn't return IsFull=true");
            target.Enqueue(0);
            Assert.Fail("Enqueue to full queue didn't throw QueueEmptyException");
        }

        /// <summary>
        ///A test for Count
        ///</summary>
        [TestMethod()]
        public void CountTest()
        {
            ArrayQueue target = new ArrayQueue();
            for (int i = 0; i < 1000000 && !target.IsFull; i++)
            {
                Assert.AreEqual<int>(target.Count, i, string.Format("After adding {0} entries to ArrayQueue, Count returns {1}", i, target.Count));
                target.Enqueue(i);
            }
        }

        /// <summary>
        ///A test for Enqueue
        ///</summary>
        [TestMethod()]
        public void DataOrderPreservedTest()
        {
            ArrayQueue target = new ArrayQueue(); // TODO: Initialize to an appropriate value
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            for (int j = 0; j < 10; j++)
            {
                foreach (var x in testData)
                    target.Enqueue(x);
                foreach (var x in testData)
                    Assert.AreEqual<object>(x, target.Dequeue(), "ArrayQueue dequeueing elements in different order than they're enqueued in");
                Assert.AreEqual<int>(0, target.Count, "ArrayQueue showing wrong count after enqueues and dequeues");
            }
        }

        /// <summary>
        ///A test for Dequeue
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(QueueEmptyException))]
        public void DequeueOnEmptyQueueThrowsQueueEmptyExceptionTest()
        {
            ArrayQueue target = new ArrayQueue(); // TODO: Initialize to an appropriate value
            target.Dequeue();
            Assert.Fail("Dequeued from empty queue didn't throw QueueEmptyException");
        }
    }
}
