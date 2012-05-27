using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HappyFunBlob
{
    /// <summary>
    /// A Node in a binary space partition tree
    /// </summary>
    public abstract class BSPTree
    {
        /// <summary>
        /// The X-, Y-, and Z-axis unit vectors
        /// </summary>
        static Vector3[] axes = new Vector3[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };

        /// <summary>
        /// Build an axis-aligned BSP tree from a list of objeccts
        /// </summary>
        public static BSPTree BuildTree(List<GameObject> objects)
        {
            throw new NotImplementedException("BSPTree.BuildTree() method not yet implemented.");
        }

        /// <summary>
        /// How desirable it is to split the list of objects at the specified point.  Larger scores are better.
        /// Tests how evenly the nodes are divided between front and back, i.e. how balanced the subtrees are.
        /// </summary>
        /// <param name="splitIndex">Where the split occurs</param>
        /// <param name="objects">The list of objects to split</param>
        /// <returns></returns>
        static int SplitScore(int splitIndex, List<GameObject> objects)
        {
            return Math.Min(splitIndex, objects.Count - splitIndex);
        }

        /// <summary>
        /// Finds a candidate plane for splitting two objects along a given axis.
        /// Note: the plane may not divide the objects cleanly, so you need to test the result using SplitsAt.
        /// </summary>
        static Plane SplittingPlane(GameObject o1, GameObject o2, Vector3 axis)
        {
            float cutPoint = (o1.BoundingBox.AxisMax(axis) + o2.BoundingBox.AxisMin(axis)) / 2;
            return new Plane(axis, -cutPoint);
        }

        /// <summary>
        /// Tests whether a given plane splits the objects in the list cleanly without intersecting any object.
        /// Should be called with the list of objects already sorted by along the candidate's axis.
        /// </summary>
        static bool SplitsAt(List<GameObject> objects, int splitIndex, Plane candidate)
        {
            int i;
            for (i = 0; i < splitIndex; i++)
                if (objects[i].BoundingBox.Intersects(candidate) != PlaneIntersectionType.Back)
                    return false;
            for (; i < objects.Count; i++)
                if (objects[i].BoundingBox.Intersects(candidate) != PlaneIntersectionType.Front)
                    return false;
            return true;
        }

        /// <summary>
        /// A procedure that can be called on a GameObject
        /// </summary>
        public delegate void TreeFunction(GameObject o);

        /// <summary>
        /// Calls function on every object in the tree that intersects the specified bounding box.
        /// </summary>
        public abstract void DoAllIntersectingObjects(BoundingBox bbox, TreeFunction function);

        /// <summary>
        /// Sorts the list of GameObjects by midpoint along the specified axis.
        /// </summary>
        public static void SortGameObjects(List<GameObject> gameObjects, Vector3 axis)
        {
            gameObjects.Sort(new Comparison<GameObject>(delegate(GameObject o1, GameObject o2)
            {
                float o1Min = o1.BoundingBox.AxisMidpoint(axis);
                float o2Min = o2.BoundingBox.AxisMidpoint(axis);
                if (o1Min < o2Min)
                    return -1;
                else if (o1Min == o2Min)
                    return 0;
                else
                    return 1;
            }));
        }
    }

    /// <summary>
    /// A leaf node of the BSP tree: contains a single game object and no splitting plane
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{GameObject}")]
    public class BSPTreeLeaf : BSPTree
    {
        /// <summary>
        /// The object this leaf is associated with
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// Makes a Leaf node for the given object
        /// </summary>
        /// <param name="o"></param>
        public BSPTreeLeaf(GameObject o)
        {
            GameObject = o;
        }

        /// <summary>
        /// Calls function on every object in the tree that intersects the specified bounding box.
        /// Since this is a leaf node, this amounts to testing GameObject for intersection with the
        /// bounding box and then calling function if they intersect.
        /// </summary>
        public override void DoAllIntersectingObjects(BoundingBox bbox, TreeFunction function)
        {
            throw new NotImplementedException("BSPTreeLeaf.DoAllIntersectingObjects() method not yet implemented.");
        }
    }

    /// <summary>
    /// Divides the objects in the BSP tree into two subtrees based on a splitting plane.
    /// All the nodes in the FrontSubTree are in front of the plane, all the nodes in the BackSubTree
    /// are behind it.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{SplitPlane}")]
    public class BSPTreeInteriorNode : BSPTree
    {
        /// <summary>
        /// The oriented plane dividing the objects in the BackSubTree from the FrontSubTree
        /// </summary>
        public Plane SplitPlane { get; set; }
        /// <summary>
        /// The set of objects behind the plane, organized as a BSP tree.
        /// </summary>
        public BSPTree BackSubTree { get; set; }

        /// <summary>
        /// The set of objects in front of the plane, organized as a BSP tree.
        /// </summary>
        public BSPTree FrontSubTree { get; set; }

        /// <summary>
        /// Creates a BSPTree given a splitting plane and front- and back- subtrees
        /// </summary>
        public BSPTreeInteriorNode(Plane splitPlane, BSPTree back, BSPTree front)
        {
            SplitPlane = splitPlane;
            BackSubTree = back;
            FrontSubTree = front;
        }

        /// <summary>
        /// Calls function on every object in the tree that intersects the specified bounding box.
        /// Since this is an interior node, this amounts to testing whether the bbox is in front of,
        /// behind, or intersecting the splitting plane, and walking the front subtree, the back subtree,
        /// or both.
        /// </summary>
        public override void DoAllIntersectingObjects(BoundingBox bbox, TreeFunction function)
        {
            throw new NotImplementedException("BSPTreeInteriorNode.DoAllIntersectingObjects() method not yet implemented.");
        }
    }

    /// <summary>
    /// This adds some useful methods to the BoundingBox type
    /// </summary>
    public static class BoundingBoxFunctions
    {
        /// <summary>
        /// Smallest coordinate of the bounding box along axis.
        /// Assumes the axis vector is positive-definie (i.e. X, Y, and Z are non-negative)
        /// </summary>
        public static float AxisMin(this BoundingBox bbox, Vector3 axis)
        {
            return Vector3.Dot(bbox.Min, axis);
        }

        /// <summary>
        /// Largest coordinate of the bounding box along axis.
        /// Assumes the axis vector is positive-definie (i.e. X, Y, and Z are non-negative)
        /// </summary>
        public static float AxisMax(this BoundingBox bbox, Vector3 axis)
        {
            return Vector3.Dot(bbox.Max, axis);
        }

        /// <summary>
        /// Midpoint of the bounding box along axis.
        /// </summary>
        public static float AxisMidpoint(this BoundingBox bbox, Vector3 axis)
        {
            return Vector3.Dot(bbox.Max + bbox.Min, axis) * 0.5f;
        }
    }
}
