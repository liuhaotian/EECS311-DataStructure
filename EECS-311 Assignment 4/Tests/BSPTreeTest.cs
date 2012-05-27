using HappyFunBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace DictionaryTests
{
    /// <summary>
    ///This is a test class for BSPTreeTest and is intended
    ///to contain all BSPTreeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BSPTreeTest
    {            
#if BSPTrees
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
        ///A test for SortGameObjects
        ///</summary>
        [TestMethod()]
        public void SortGameObjectsTest()
        {
            List<GameObject> gameObjects = new List<GameObject>(); // TODO: Initialize to an appropriate value
            gameObjects.Add(new Orb(null, new Vector3(0, 0, 0), null));
            gameObjects.Add(new Orb(null, new Vector3(100, 0, 0), null));
            Vector3 axis = Vector3.UnitX;
            BSPTree.SortGameObjects(gameObjects, axis);
            Assert.IsTrue(gameObjects[0].BoundingBox.AxisMin(axis) < gameObjects[1].BoundingBox.AxisMin(axis));
        }

        /// <summary>
        ///A test for SortGameObjects
        ///</summary>
        [TestMethod()]
        public void SortReversedGameObjectsTest()
        {
            List<GameObject> gameObjects = new List<GameObject>(); // TODO: Initialize to an appropriate value
            gameObjects.Add(new Orb(null, new Vector3(100, 0, 0), null));
            gameObjects.Add(new Orb(null, new Vector3(0, 0, 0), null));
            Vector3 axis = Vector3.UnitX;
            BSPTree.SortGameObjects(gameObjects, axis);
            Assert.IsTrue(gameObjects[0].BoundingBox.AxisMin(axis) < gameObjects[1].BoundingBox.AxisMin(axis));
        }

        internal virtual BSPTree CreateBSPTree()
        {
            // TODO: Instantiate an appropriate concrete class.
            BSPTree target = null;
            return target;
        }

        /// <summary>
        ///A test for DoAllIntersectingObjects
        ///</summary>
        [TestMethod()]
        public void DoAllIntersectingObjectsFrontTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            Orb orb1 = new Orb(null, Vector3.Zero, null);
            Orb orb2 = new Orb(null, axis * 100, null);
            objects.Add(orb1);
            objects.Add(orb2);
            BSPTree result = BSPTree.BuildTree(objects);

            int touchCount = 0;
            GameObject touched = null;

            result.DoAllIntersectingObjects(orb2.BoundingBox, delegate(GameObject o)
            {
                touchCount++;
                touched = o;
            });

            Assert.AreEqual<int>(touchCount, 1, "DoAllIntersectingObjects should have called procedure once, called it "+touchCount+" times.");
            Assert.AreEqual<GameObject>(touched, orb2, "Called procedure on back object when given the bounding box of the front object");
        }

        /// <summary>
        ///A test for DoAllIntersectingObjects
        ///</summary>
        [TestMethod()]
        public void DoAllIntersectingObjectsBackTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            Orb orb1 = new Orb(null, Vector3.Zero, null);
            Orb orb2 = new Orb(null, axis * 100, null);
            objects.Add(orb1);
            objects.Add(orb2);
            BSPTree result = BSPTree.BuildTree(objects);

            int touchCount = 0;
            GameObject touched = null;

            result.DoAllIntersectingObjects(orb1.BoundingBox, delegate(GameObject o)
            {
                touchCount++;
                touched = o;
            });

            Assert.AreEqual<int>(touchCount, 1, "DoAllIntersectingObjects should have called procedure once, called it " + touchCount + " times.");
            Assert.AreEqual<GameObject>(touched, orb1, "Called procedure on front object when given the bounding box of the back object");
        }

        /// <summary>
        ///A test for DoAllIntersectingObjects
        ///</summary>
        [TestMethod()]
        public void DoAllIntersectingObjectsFrontAndBackTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            Orb orb1 = new Orb(null, Vector3.Zero, null);
            Orb orb2 = new Orb(null, axis * 100, null);
            objects.Add(orb1);
            objects.Add(orb2);
            BSPTree result = BSPTree.BuildTree(objects);

            int touchCount = 0;
            GameObject touched = null;

            result.DoAllIntersectingObjects(new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100)), delegate(GameObject o)
            {
                touchCount++;
                touched = o;
            });

            Assert.AreEqual<int>(touchCount, 2, "DoAllIntersectingObjects should have called procedure twice, called it " + touchCount + " times.");
        }

        /// <summary>
        ///A test for DoAllIntersectingObjects
        ///</summary>
        [TestMethod()]
        public void DoAllIntersectingObjectsNullIntersectionTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            Orb orb1 = new Orb(null, Vector3.Zero, null);
            Orb orb2 = new Orb(null, axis * 100, null);
            objects.Add(orb1);
            objects.Add(orb2);
            BSPTree result = BSPTree.BuildTree(objects);

            int touchCount = 0;
            GameObject touched = null;

            result.DoAllIntersectingObjects(new BoundingBox(new Vector3(-100, -100, -100), new Vector3(-100, -100, -100)), delegate(GameObject o)
            {
                touchCount++;
                touched = o;
            });

            Assert.AreEqual<int>(touchCount, 0, "DoAllIntersectingObjects should not have called procedure, called it " + touchCount + " times.");
        }


        /// <summary>
        ///A test for BuildTree
        ///</summary>
        [TestMethod()]
        public void BuildLeafTest()
        {
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, new Vector3(0, 0, 0), null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeLeaf), "BuildTree should return a BSPTreeLeaf when called on one object");
            Assert.IsTrue(((BSPTreeLeaf)result).GameObject == objects[0], "GameObject field not set correctly on BSPTreeLeaf");
        }

        /// <summary>
        ///A simple test for BuildTree
        ///</summary>
        [TestMethod()]
        public void BuildCorrectFormatTree()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, axis * 100, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeInteriorNode), "BuildTree should return a BSPTreeInteriorNode when called on multiple objects");
            Assert.IsInstanceOfType(((BSPTreeInteriorNode)result).FrontSubTree, typeof(BSPTreeLeaf), "The front subtree should be a BSPTreeLeaf");
            Assert.IsInstanceOfType(((BSPTreeInteriorNode)result).BackSubTree, typeof(BSPTreeLeaf), "The back subtree should be a BSPTreeLeaf");
        }

        /// <summary>
        ///A simple test for BuildTree
        ///</summary>
        [TestMethod()]
        public void ArrangeGameObjectsCorrectlyTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, axis * 100, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeInteriorNode), "BuildTree should return a BSPTreeInteriorNode when called on multiple objects");
            BSPTreeLeaf backTree = (BSPTreeLeaf)((BSPTreeInteriorNode)result).BackSubTree;
            BSPTreeLeaf frontTree = (BSPTreeLeaf)((BSPTreeInteriorNode)result).FrontSubTree;
            Assert.AreEqual(backTree.GameObject.BoundingBox, objects[0].BoundingBox, "This box should be in the back subtree");
            Assert.AreEqual(frontTree.GameObject.BoundingBox, objects[1].BoundingBox, "This box should be in the front subtree");
        }

        /// <summary>
        ///A simple test for BuildTree
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(NoSplitAxisException))]
        public void NoSplitAvailableTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, Vector3.Zero, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.Fail("There should be now way to split on two equal objects");
        }

        /// <summary>
        ///A test for BuildTree
        ///</summary>
        [TestMethod()]
        public void BuildTreeSplitAlongXTest()
        {
            Vector3 axis = Vector3.UnitX;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, axis*100, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeInteriorNode), "BuildTree should return a BSPTreeInteriorNode when called on multiple objects");
            BSPTreeInteriorNode node = (BSPTreeInteriorNode)result;
            Assert.AreEqual<Vector3>(node.SplitPlane.Normal, axis, "BuildTree failed to split two objects along X axis");
            Assert.IsInstanceOfType(node.BackSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a back subtree that wasn't a leaf");
            Assert.IsInstanceOfType(node.FrontSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a Front subtree that wasn't a leaf");
            GameObject back = ((BSPTreeLeaf)node.BackSubTree).GameObject;
            GameObject front = ((BSPTreeLeaf)node.FrontSubTree).GameObject;
            Assert.IsTrue(back.BoundingBox.AxisMin(axis) < front.BoundingBox.AxisMin(axis), "Back subtree is in front of front subtree");
        }

        /// <summary>
        ///A test for BuildTree
        ///</summary>
        [TestMethod()]
        public void BuildTreeSplitAlongYTest()
        {
            Vector3 axis = Vector3.UnitY;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, axis * 100, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeInteriorNode), "BuildTree should return a BSPTreeInteriorNode when called on multiple objects");
            BSPTreeInteriorNode node = (BSPTreeInteriorNode)result;
            Assert.AreEqual<Vector3>(node.SplitPlane.Normal, axis, "BuildTree failed to split two objects along X axis");

            Assert.IsInstanceOfType(node.BackSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a back subtree that wasn't a leaf");
            Assert.IsInstanceOfType(node.FrontSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a Front subtree that wasn't a leaf");
            GameObject back = ((BSPTreeLeaf)node.BackSubTree).GameObject;
            GameObject front = ((BSPTreeLeaf)node.FrontSubTree).GameObject;
            Assert.IsTrue(back.BoundingBox.AxisMin(axis) < front.BoundingBox.AxisMin(axis), "Back subtree is in front of front subtree");
        }

        /// <summary>
        ///A test for BuildTree
        ///</summary>
        [TestMethod()]
        public void BuildTreeSplitAlongZTest()
        {
            Vector3 axis = Vector3.UnitZ;
            List<GameObject> objects = new List<GameObject>();
            objects.Add(new Orb(null, Vector3.Zero, null));
            objects.Add(new Orb(null, axis * 100, null));
            BSPTree result = BSPTree.BuildTree(objects);
            Assert.IsInstanceOfType(result, typeof(BSPTreeInteriorNode), "BuildTree should return a BSPTreeInteriorNode when called on multiple objects");
            BSPTreeInteriorNode node = (BSPTreeInteriorNode)result;
            Assert.AreEqual<Vector3>(node.SplitPlane.Normal, axis, "BuildTree failed to split two objects along X axis");

            Assert.IsInstanceOfType(node.BackSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a back subtree that wasn't a leaf");
            Assert.IsInstanceOfType(node.FrontSubTree, typeof(BSPTreeLeaf), "Splitting two objects resulted in a Front subtree that wasn't a leaf");
            GameObject back = ((BSPTreeLeaf)node.BackSubTree).GameObject;
            GameObject front = ((BSPTreeLeaf)node.FrontSubTree).GameObject;
            Assert.IsTrue(back.BoundingBox.AxisMin(axis) < front.BoundingBox.AxisMin(axis), "Back subtree is in front of front subtree");
        }

#endif
    }
}
