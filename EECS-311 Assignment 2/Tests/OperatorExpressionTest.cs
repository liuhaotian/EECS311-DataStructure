using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DictionaryTests
{
    
    
    /// <summary>
    ///This is a test class for OperatorExpressionTest and is intended
    ///to contain all OperatorExpressionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class OperatorExpressionTest
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
        ///A test for Operator.Run
        ///</summary>
        [TestMethod()]
        public void RunSumTest()
        {
            ListDictionary dict = new ListDictionary();
            var expression1 = SyntaxTree.Parse("2 + 2");
            Assert.AreEqual(expression1.Run(dict), 4);
            
        }

        /// <summary>
        ///A test for Operator.Run
        ///</summary>
        [TestMethod()]
        public void RunProductTest()
        {
            ListDictionary dict = new ListDictionary();
            var expression1 = SyntaxTree.Parse("3 * 3");
            Assert.AreEqual(expression1.Run(dict), 9);

        }

        /// <summary>
        ///A test for Operator.Run
        ///</summary>
        [TestMethod()]
        public void RunNegationTest()
        {
            ListDictionary dict = new ListDictionary();
            dict.Store("a", 3);
            var expression1 = SyntaxTree.Parse("-a");
            Assert.AreEqual(expression1.Run(dict), -3);

        }

    }
}
