using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace DictionaryTests
{
    /// <summary>
    ///This is a test class for ListDictionaryTest and is intended
    ///to contain all ListDictionaryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ListDictionaryTest
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
        ///A test for ListDictionary Constructor
        ///</summary>
        [TestMethod()]
        public void ListDictionaryConstructorTest()
        {
            ListDictionary target = new ListDictionary();
           
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void StoreOverwriteTest()
        {
            ListDictionary target = new ListDictionary(); // TODO: Initialize to an appropriate value
            target.Store("test", 1);
            Assert.AreEqual(target.Lookup("test"), 1, "dictionary does not store values correctly");
            target.Store("test", 2);
            Assert.AreEqual(target.Lookup("test"), 2, "Key should have its value reassigned");
            
        }


        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(DictionaryKeyNotFoundException))]
        public void LookupThrowsDictionaryNotFoundExceptionTest()
        {
            ListDictionary target = new ListDictionary(); // TODO: Initialize to an appropriate value
            target.Lookup("notfound");
            Assert.Fail("Target did not return excpetion when an unknown key was searched for");
       
        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        public void StoreAndLookupTest()
        {
            ListDictionary target = new ListDictionary(); // TODO: Initialize to an appropriate value
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", 1, 2, 3, 4 };
            for (int i = 0; i < testData.Length; i++)
            {
                target.Store("x"+i.ToString(), testData[i]);
            }
            for (int i = 0; i < testData.Length; i++)
            {
                var testValue = target.Lookup("x"+i.ToString());
                Assert.AreEqual(testData[i], testValue, "the keys do not return the correct data: expected " + testData[i] + ", got " + testValue);
            }
        }

        [TestMethod()]
        public void OverwriteUsingAddTest()
        {
            ListDictionary target = new ListDictionary();
            target.Store("a", 1);
            target.Store("a", 2);
            Assert.AreEqual(target.Lookup("a"), 2, "Add should overwrite a value if the key is already in the dictionary");
        }

        /// <summary>
        /// Testing to see if proper checking is being used
        /// </summary>
        ///  
        [TestMethod()]
        public void CorrectTypeCheckingTest()
        {
            ListDictionary target = new ListDictionary();
            string var = "a";
            target.Store(var, 1);
            target.Store("a", 2);
            var result = target.Lookup("a");
            Assert.AreEqual(target.Lookup(var), result);
        }
    }
}
