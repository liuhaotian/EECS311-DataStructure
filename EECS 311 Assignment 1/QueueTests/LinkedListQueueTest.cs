using Assignment_1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace QueueTests
{
    
    
    /// <summary>
    ///This is a test class for LinkedListQueueTest and is intended
    ///to contain all LinkedListQueueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinkedListQueueTest
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
            LinkedListQueue target = new LinkedListQueue();
            Assert.IsTrue(target.IsEmpty, "A newly created LinkedListQueue should have IsEmpty=true, but doesn't");
        }

        [TestMethod()]
        public void ConstructorCreatesNonFullQueueTest()
        {
            LinkedListQueue target = new LinkedListQueue();
            Assert.IsFalse(target.IsFull, "A newly created LinkedListQueue should have IsFull=false, but doesn't");
        }

        [TestMethod()]
        public void NonEmptyQueueReadsNotEmptyTest()
        {
            LinkedListQueue target = new LinkedListQueue();
            target.Enqueue(0);
            for (int i = 0; i < 1000000; i++)
            {
                target.Enqueue(i);
                Assert.IsFalse(target.IsEmpty, string.Format("LinkedListQueue reads as empty after adding {0} objects and never removing any", i + 1));
            }
        }

        /// <summary>
        ///A test for Count
        ///</summary>
        [TestMethod()]
        public void CountTest()
        {
            LinkedListQueue target = new LinkedListQueue();
            for (int i = 0; i < 1000000; i++)
            {
                Assert.AreEqual<int>(target.Count, i, string.Format("After adding {0} entries to LinkedListQueue, Count returns {1}", i, target.Count));
                target.Enqueue(i);
            }
        }

        /// <summary>
        ///A test for Enqueue
        ///</summary>
        [TestMethod()]
        public void DataOrderPreservedTest()
        {
            LinkedListQueue target = new LinkedListQueue(); // TODO: Initialize to an appropriate value
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            for (int j = 0; j < 10; j++)
            {
                foreach (var x in testData)
                    target.Enqueue(x);
                foreach (var x in testData)
                    Assert.AreEqual<object>(x, target.Dequeue(), "LinkedListQueue dequeueing elements in different order than they're enqueued in");
                Assert.AreEqual<int>(0, target.Count, "LinkedListQueue showing wrong count after enqueues and dequeues");
            }
        }

        /// <summary>
        ///A test for Dequeue
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(QueueEmptyException))]
        public void DequeueOnEmptyQueueThrowsQueueEmptyExceptionTest()
        {
            LinkedListQueue target = new LinkedListQueue(); // TODO: Initialize to an appropriate value
            target.Dequeue();
            Assert.Fail("Dequeued from empty queue didn't throw QueueEmptyException");
        }
    }
}
