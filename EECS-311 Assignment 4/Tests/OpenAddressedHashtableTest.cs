using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{


    /// <summary>
    ///This is a test class for OpenAddressedHashtable and is intended
    ///to contain all OpenAddressedHashtable Unit Tests
    ///</summary>
    [TestClass]
    public class OpenAddressedHashtableTest
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

        /// <summary>
        /// Check type hierarchy
        ///</summary>
        [TestMethod]
        public void OpenAddressedHashtableParentTest()
        {
            Assert.IsTrue(typeof(OpenAddressedHashtable).BaseType == typeof(Dictionary), "OpenAddressedHashtable must be a subclass of Dictionary.");
        }

        /// <summary>
        ///A test for OpenAddressedHashtable Constructor
        ///</summary>
        [TestMethod]
        public void OpenAddressedHashtableConstructorTest()
        {
            new OpenAddressedHashtable(10);
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod]
        public void OpenHTStoreOverwriteTest()
        {
            OpenAddressedHashtable target = new OpenAddressedHashtable(100);
            target.Store("test", 1);
            Assert.AreEqual(target.Lookup("test"), 1, "dictionary does not store values correctly");
            target.Store("test", 2);
            Assert.AreEqual(target.Lookup("test"), 2, "Key should have its value reassigned");

        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod]
        [ExpectedException(typeof(DictionaryKeyNotFoundException))]
        public void OpenHTLookupThrowsDictionaryNotFoundExceptionTest()
        {
            OpenAddressedHashtable target = new OpenAddressedHashtable(100);
            target.Lookup("notfound");
            Assert.Fail("Target did not return excpetion when an unknown key was searched for");

        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod]
        [ExpectedException(typeof(HashtableFullException))]
        public void OpenHTOverflowThrowsHashtableFullExceptionTest()
        {
            OpenAddressedHashtable target = new OpenAddressedHashtable(2);
            target.Store("a", 1);
            target.Store("b", 1);
            target.Store("c", 1);
            Assert.Fail("OpenAddressedHashtable did not throw HashtableFullException when it was overfilled.");

        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod]
        public void OpenHTStoreAndLookupTest()
        {
            OpenAddressedHashtable target = new OpenAddressedHashtable(100);
            object[] testData = new object[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", 1, 2, 3, 4 };
            for (int i = 0; i < testData.Length; i++)
            {
                target.Store("x" + i.ToString(), testData[i]);
            }
            for (int i = 0; i < testData.Length; i++)
            {
                var testValue = target.Lookup("x" + i.ToString());
                Assert.AreEqual(testData[i], testValue, "the keys do not return the correct data: expected " + testData[i] + ", got " + testValue);
            }
        }

        /// <summary>
        /// Checks that Count properly tracks the number of items in the dictionary.
        /// </summary>
        [TestMethod]
        public void OpenHTCountTest()
        {
            OpenAddressedHashtable target = new OpenAddressedHashtable(100);
            Assert.AreEqual(0, target.Count, "Empty dictionary should have a count of zero.");
            target.Store("a", 1);
            Assert.AreEqual(1, target.Count, "Incorrect count - stored only one item, expected count of 1");
            target.Store("b", 2);
            Assert.AreEqual(2, target.Count, "Incorrect count - stored only two items, expected count of 2");
            target.Store("a", 2);
            Assert.AreEqual(2, target.Count, "Overwriting key that is already stored in the dictionary should not change the count.");
        }
    }
}
