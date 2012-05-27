using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DictionaryTests
{
    
    
    /// <summary>
    ///This is a test class for ConstantTest and is intended
    ///to contain all ConstantTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConstantTest
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
        ///A test for Constant.Run
        ///</summary>
        [TestMethod()]
        public void RunConstantIntegerTest()
        {
            ListDictionary dict = new ListDictionary();
            //create syntaxTrees with only constants
            var expression1 = SyntaxTree.Parse("5");
            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);
            Assert.AreEqual(expression1Value, 5, "run method for constants is not returning the correct result: expected 5, got "+expression1Value.ToString());
        }

        /// <summary>
        ///A test for Constant.Run
        ///</summary>
        [TestMethod()]
        public void RunConstantStringTest()
        {
            ListDictionary dict = new ListDictionary();
            //create syntaxTrees with only constants
            var expression1 = SyntaxTree.Parse("\"\"foo\"\"");
            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);
            Assert.AreEqual(expression1Value, "foo", "run method for constants is not returning the correct result: expected \"foo\", got " + expression1Value.ToString());
        }

    }
}
