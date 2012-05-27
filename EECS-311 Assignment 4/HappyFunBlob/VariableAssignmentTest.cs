using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DictionaryTests
{
    
    
    /// <summary>
    ///This is a test class for VariableAssignmentTest and is intended
    ///to contain all VariableAssignmentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VariableAssignmentTest
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
        public void VariableAssignTest()
        {
            ListDictionary dict = new ListDictionary();
            //create syntaxTrees with a variable assignment
            var expression1 = SyntaxTree.Parse("x = 4");
            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);

            //test to see that the variable has been stored
            object result = dict.Lookup("x");
            Assert.AreEqual(result, 4, "Variable assignment failed; expected 4, got "+result.ToString());
        }

        /// <summary>
        ///A test for Reassignment
        ///</summary>
        [TestMethod()]
        public void VariableReassignmentTest()
        {
            ListDictionary dict = new ListDictionary();
            //create syntaxTrees with only constants
            var expression1 = SyntaxTree.Parse("x = 4");
            var expression2 = SyntaxTree.Parse("x = 5");

            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);
            object expression2Value = expression2.Run(dict);

            //test to see that the variable has been reassigned
            object result = dict.Lookup("x");
            Assert.AreEqual(result, 5, "VariableAssignment doesn't update existing variable; expected 5, got "+result.ToString());
        }

        /// <summary>
        ///A test for indirect assignment (x = 3; and y = x)
        ///</summary>
        [TestMethod()]
        public void IndirectAssignmentTest()
        {
            ListDictionary dict = new ListDictionary();
            //create syntaxTrees with only constants
            var expression1 = SyntaxTree.Parse("x = 4");
            var expression2 = SyntaxTree.Parse("y = x");

            //evaluate SyntaxTrees
            object expression1Value = expression1.Run(dict);
            object expression2Value = expression2.Run(dict);

            //test to see that the variable has been reassigned
            object result = dict.Lookup("y");
            Assert.AreEqual(result, 4, "Variable incorrectly updated by assignment statement; expected 4, got "+result.ToString());
        }
    }
}
