using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DictionaryTests
{
    
    
    /// <summary>
    ///This is a test class for VariableReferenceTest and is intended
    ///to contain all VariableReferenceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VariableReferenceTest
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
        ///A test for Run
        ///</summary>
        [TestMethod()]
        public void RunTest()
        {
            ListDictionary dict = new ListDictionary();

            //create syntaxTrees with a variable reference
            dict.Store("x", 4);
            var expression1 = SyntaxTree.Parse("x");
            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);
            //test to see that the variable has been retrieved correctly
            
            Assert.AreEqual(expression1Value, 4, "VariableReference test failed; expected 4, got "+expression1Value.ToString());

          
        }
    }
}
