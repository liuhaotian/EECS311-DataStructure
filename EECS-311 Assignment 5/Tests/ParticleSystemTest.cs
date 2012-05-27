using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DictionaryTests
{
    /// <summary>
    ///This is a test class for ParticleSystemTest and is intended
    ///to contain all ParticleSystemTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ParticleSystemTest
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
        /// Test of insertion sort implementation for depth sorting.
        ///</summary>
        [TestMethod()]
        public void InsertionDepthSortTest()
        {
            ParticleSystem s = new ParticleSystem(null, 3000, 300);
            s.UpdateDepths(Vector3.UnitZ, Vector3.Zero);
            for (int i = 0; i < 2; i++)
            {
                s.InsertionDepthSort();
                Assert.IsTrue(s.IsDepthSorted());
            }
            s.UpdateDepths(Vector3.UnitX, Vector3.Zero);
            for (int i = 0; i < 2; i++)
            {
                s.InsertionDepthSort();
                Assert.IsTrue(s.IsDepthSorted());
            }
        }

        /// <summary>
        /// Test of quicksort implementation for depth sorting.
        ///</summary>
        [TestMethod()]
        public void QuicksortDepthSortTest()
        {
            ParticleSystem s = new ParticleSystem(null, 3000, 300);
            s.UpdateDepths(Vector3.UnitZ, Vector3.Zero);
            for (int i = 0; i < 2; i++)
            {
                s.QuicksortDepthSort();
                Assert.IsTrue(s.IsDepthSorted());
            }
            s.UpdateDepths(Vector3.UnitX, Vector3.Zero);
            for (int i = 0; i < 2; i++)
            {
                s.QuicksortDepthSort();
                Assert.IsTrue(s.IsDepthSorted());
            }
        }
    }
}
