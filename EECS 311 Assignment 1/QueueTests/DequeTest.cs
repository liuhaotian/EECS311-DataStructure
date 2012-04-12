using Assignment_1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace QueueTests
{
    
    
    /// <summary>
    ///This is a test class for DequeTest and is intended
    ///to contain all DequeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DequeTest
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
            Deque target = new Deque();
            Assert.IsTrue(target.IsEmpty);
        }

        [TestMethod()]
        public void NonEmptyQueueReadsNotEmptyTest()
        {
            Deque target = new Deque();
            target.AddEnd(0);
            for (int i = 0; i < 1000000; i++)
            {
                target.AddEnd(i);
                Assert.IsFalse(target.IsEmpty);
            }
        }

        /// <summary>
        ///A test for Count
        ///</summary>
        [TestMethod()]
        public void CountTest()
        {
            Deque target = new Deque();
            for (int i = 0; i < 1000000; i++)
            {
                Assert.AreEqual<int>(target.Count, i);
                target.AddEnd(i);
            }
        }

        /// <summary>
        ///A test for AddEnd
        ///</summary>
        [TestMethod()]
        public void DataOrderPreservedTest()
        {
            Deque target = new Deque(); // TODO: Initialize to an appropriate value
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            for (int j = 0; j < 10; j++)
            {
                foreach (var x in testData)
                    target.AddEnd(x);
                foreach (var x in testData)
                    Assert.AreEqual<object>(x, target.RemoveFront());
                Assert.AreEqual<int>(0, target.Count);
            }
        }

        /// <summary>
        ///A test for RemoveFront
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(QueueEmptyException))]
        public void RemoveFrontOnEmptyQueueThrowsQueueEmptyExceptionTest()
        {
            Deque target = new Deque(); // TODO: Initialize to an appropriate value
            target.RemoveFront();
            Assert.Fail("RemoveFrontd from empty queue didn't throw QueueEmptyException");
        }

        [TestMethod()]
        public void NonEmptyQueueWrittenFromFrontReadsNotEmptyTest()
        {
            Deque target = new Deque();
            target.AddFront(0);
            for (int i = 0; i < 1000000; i++)
            {
                target.AddFront(i);
                Assert.IsFalse(target.IsEmpty);
            }
        }

        /// <summary>
        ///A test for AddFront
        ///</summary>
        [TestMethod()]
        public void DataOrderPreservedWrittenEndwardTest()
        {
            Deque target = new Deque(); // TODO: Initialize to an appropriate value
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
            for (int j = 0; j < 10; j++)
            {
                foreach (var x in testData)
                    target.AddFront(x);
                foreach (var x in testData)
                    Assert.AreEqual<object>(x, target.RemoveEnd());
                Assert.AreEqual<int>(0, target.Count);
            }
        }

        /// <summary>
        ///A test for RemoveEnd
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(QueueEmptyException))]
        public void RemoveEndOnEmptyQueueThrowsQueueEmptyExceptionTest()
        {
            Deque target = new Deque(); // TODO: Initialize to an appropriate value
            target.RemoveEnd();
            Assert.Fail("RemoveEndd from empty queue didn't throw QueueEmptyException");
        }


    }
}
