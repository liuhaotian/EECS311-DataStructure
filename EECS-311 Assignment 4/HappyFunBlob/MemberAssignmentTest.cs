using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DictionaryTests
{
    
    
    /// <summary>
    ///This is a test class for MemberAssignmentTest and is intended
    ///to contain all MemberAssignmentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MemberAssignmentTest
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
        ///A test for MemberReference.Run()
        ///</summary>
        [TestMethod()]
        public void MemberRefTest()
        {
            ListDictionary dict = new ListDictionary();
            dict.Store("x", "hello");
            var expression = SyntaxTree.Parse("x.length");
            object expressionValue = expression.Run(dict);
            Assert.AreEqual(expressionValue, "hello".Length, "The memberField of an object is not called properly (MemberReference is failing)");
        }

        class TestThingy
        {
            public int a = 0;
        }

        /// <summary>
        ///A test for MemberReference.Run()
        ///</summary>
        [TestMethod()]
        public void MemberAssignTest()
        {
            ListDictionary dict = new ListDictionary();
            TestThingy x = new TestThingy();
            dict.Store("x", x);
            var expression = SyntaxTree.Parse("x.a = 7");
            object expressionValue = expression.Run(dict);
            Assert.AreEqual(x.a, 7, "MemberAssignment.Run does not properly update field; expected 7, got "+x.a.ToString());
            Assert.AreEqual(expressionValue, 7, "MemberAssignment.Run should return the new value of the field; expected 7, got "+expressionValue);
        }


        /// <summary>
        ///A test which tests MethodCall.Run
        ///</summary>
        [TestMethod()]
        public void MemberMethodTest1Arg()
        {
            ListDictionary dict = new ListDictionary();
            dict.Store("x", "hello");
            var expression = SyntaxTree.Parse("x.Substring(2)");
            object expressionValue = expression.Run(dict);
            Assert.AreEqual(expressionValue, "llo", "Method call with 1 argument returned the wrong value");
        }

        /// <summary>
        ///A test which tests MethodCall.Run
        ///</summary>
        [TestMethod()]
        public void MemberMethodTest0Arg()
        {
            ListDictionary dict = new ListDictionary();
            dict.Store("x", 1);
            var expression = SyntaxTree.Parse("x.ToString()");
            object expressionValue = expression.Run(dict);
            Assert.AreEqual(expressionValue, "1", "Method call with no arguments returned the wrong value");
        }

        /// <summary>
        ///A test which tests MethodCall.Run
        ///</summary>
        [TestMethod()]
        public void MemberMethodTest2Args()
        {
            ListDictionary dict = new ListDictionary();
            dict.Store("x", "hello");
            var expression = SyntaxTree.Parse("x.Substring(2,1)");
            object expressionValue = expression.Run(dict);
            Assert.AreEqual(expressionValue, "l", "Method call with two arguments returned the wrong value");
        }

    }
}
