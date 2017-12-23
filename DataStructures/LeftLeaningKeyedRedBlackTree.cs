// Uncomment this to enable the following debugging aids:
//   LeftLeaningRedBlackTree.HtmlFragment
//   LeftLeaningRedBlackTree.EntityView.HtmlFragment
//   LeftLeaningRedBlackTree.AssertInvariants
// #define DEBUGGING

using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Implements a left-leaning red-black tree.
/// </summary>
/// <remarks>
/// Based on the research paper "Left-leaning Red-Black Trees"
/// by Robert Sedgewick. More information available at:
/// http://www.cs.princeton.edu/~rs/talks/LLRB/RedBlack.pdf
/// http://www.cs.princeton.edu/~rs/talks/LLRB/08Penn.pdf
/// </remarks>
/// <typeparam name="TKey">Type of keys.</typeparam>
public class LeftLeaningKeyedRedBlackTree<TKey> where TKey: IComparable<TKey>
{
    /// <summary>
    /// Stores the root entityView of the tree.
    /// </summary>
    private EntityView _rootEntityView;

    /// <summary>
    /// Represents a entityView of the tree.
    /// </summary>
    /// <remarks>
    /// Using fields instead of properties drops execution time by about 40%.
    /// </remarks>
    [DebuggerDisplay("Key={Key}")]
    private class EntityView
    {
        /// <summary>
        /// Gets or sets the entityView's key.
        /// </summary>
        public TKey Key;

        /// <summary>
        /// Gets or sets the left entityView.
        /// </summary>
        public EntityView Left;

        /// <summary>
        /// Gets or sets the right entityView.
        /// </summary>
        public EntityView Right;

        /// <summary>
        /// Gets or sets the color of the entityView.
        /// </summary>
        public bool IsBlack;

#if DEBUGGING
        /// <summary>
        /// Gets an HTML fragment representing the entityView and its children.
        /// </summary>
        public string HtmlFragment
        {
            get
            {
                return
                    "<table border='1'>" +
                        "<tr>" +
                            "<td colspan='2' align='center' bgcolor='" + (IsBlack ? "gray" : "red") + "'>" + Key + ", " + Value + " [" + Siblings + "]</td>" +
                        "</tr>" +
                        "<tr>" +
                            "<td valign='top'>" + (null != Left ? Left.HtmlFragment : "[null]") + "</td>" +
                            "<td valign='top'>" + (null != Right ? Right.HtmlFragment : "[null]") + "</td>" +
                        "</tr>" +
                    "</table>";
            }
        }
#endif
    }

    /// <summary>
    /// Adds a key/value pair to the tree.
    /// </summary>
    /// <param name="key">Key to add.</param>
    public void Add(TKey key)
    {
        _rootEntityView = Add(_rootEntityView, key);
        _rootEntityView.IsBlack = true;
#if DEBUGGING
        AssertInvariants();
#endif
    }

    /// <summary>
    /// Removes a key/value pair from the tree.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns>True if key/value present and removed.</returns>
    public bool Remove(TKey key)
    {
        int initialCount = Count;
        if (null != _rootEntityView)
        {
            _rootEntityView = Remove(_rootEntityView, key);
            if (null != _rootEntityView)
            {
                _rootEntityView.IsBlack = true;
            }
        }
#if DEBUGGING
        AssertInvariants();
#endif
        return initialCount != Count;
    }

    /// <summary>
    /// Removes all entityViews in the tree.
    /// </summary>
    public void Clear()
    {
        _rootEntityView = null;
        Count = 0;
#if DEBUGGING
        AssertInvariants();
#endif
    }

    /// <summary>
    /// Gets a sorted list of keys in the tree.
    /// </summary>
    /// <returns>Sorted list of keys.</returns>
    public IEnumerable<TKey> GetKeys()
    {
        TKey lastKey = default(TKey);
        bool lastKeyValid = false;
        return Traverse(
            _rootEntityView,
            n => !lastKeyValid || !object.Equals(lastKey, n.Key),
            n =>
            {
                lastKey = n.Key;
                lastKeyValid = true;
                return lastKey;
            });
    }

    /// <summary>
    /// Gets the count of key/value pairs in the tree.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the minimum key in the tree.
    /// </summary>
    public TKey MinimumKey
    {
        get { return GetExtreme(_rootEntityView, n => n.Left, n => n.Key); }
    }

    /// <summary>
    /// Gets the maximum key in the tree.
    /// </summary>
    public TKey MaximumKey
    {
        get { return GetExtreme(_rootEntityView, n => n.Right, n => n.Key); }
    }

    /// <summary>
    /// Returns true if the specified entityView is red.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <returns>True if specified entityView is red.</returns>
    private static bool IsRed(EntityView entityView)
    {
        if (null == entityView)
        {
            // "Virtual" leaf entityViews are always black
            return false;
        }
        return !entityView.IsBlack;
    }

    /// <summary>
    /// Adds the specified key/value pair below the specified root entityView.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <param name="key">Key to add.</param>
    /// <param name="value">Value to add.</param>
    /// <returns>New root entityView.</returns>
    private EntityView Add(EntityView entityView, TKey key)
    {
        if (null == entityView)
        {
            // Insert new entityView
            Count++;
            return new EntityView { Key = key };
        }

        if (IsRed(entityView.Left) && IsRed(entityView.Right))
        {
            // Split entityView with two red children
            FlipColor(entityView);
        }

        // Find right place for new entityView
        int comparisonResult = KeyComparison(key, entityView.Key);
        if (comparisonResult < 0)
        {
            entityView.Left = Add(entityView.Left, key);
        }
        else if (0 < comparisonResult)
        {
            entityView.Right = Add(entityView.Right, key);
        }

        if (IsRed(entityView.Right))
        {
            // Rotate to prevent red entityView on right
            entityView = RotateLeft(entityView);
        }

        if (IsRed(entityView.Left) && IsRed(entityView.Left.Left))
        {
            // Rotate to prevent consecutive red entityViews
            entityView = RotateRight(entityView);
        }

        return entityView;
    }

    /// <summary>
    /// Removes the specified key/value pair from below the specified entityView.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <param name="key">Key to remove.</param>
    /// <returns>True if key/value present and removed.</returns>
    private EntityView Remove(EntityView entityView, TKey key)
    {
        int comparisonResult = KeyComparison(key, entityView.Key);
        if (comparisonResult < 0)
        {
            // * Continue search if left is present
            if (null != entityView.Left)
            {
                if (!IsRed(entityView.Left) && !IsRed(entityView.Left.Left))
                {
                    // Move a red entityView over
                    entityView = MoveRedLeft(entityView);
                }

                // Remove from left
                entityView.Left = Remove(entityView.Left, key);
            }
        }
        else
        {
            if (IsRed(entityView.Left))
            {
                // Flip a 3 entityView or unbalance a 4 entityView
                entityView = RotateRight(entityView);
            }
            if ((0 == KeyComparison(key, entityView.Key)) && (null == entityView.Right))
            {
                // Remove leaf entityView
                Debug.Assert(null == entityView.Left, "About to remove an extra entityView.");
                Count--;
                // Leaf entityView is gone
                return null;
            }
            // * Continue search if right is present
            if (null != entityView.Right)
            {
                if (!IsRed(entityView.Right) && !IsRed(entityView.Right.Left))
                {
                    // Move a red entityView over
                    entityView = MoveRedRight(entityView);
                }
                if (0 == KeyComparison(key, entityView.Key))
                {
                    // Remove leaf entityView
                    Count--;
                    // Find the smallest entityView on the right, swap, and remove it
                    EntityView m = GetExtreme(entityView.Right, n => n.Left, n => n);
                    entityView.Key = m.Key;
                    entityView.Right = DeleteMinimum(entityView.Right);
                }
                else
                {
                    // Remove from right
                    entityView.Right = Remove(entityView.Right, key);
                }
            }
        }

        // Maintain invariants
        return FixUp(entityView);
    }

    /// <summary>
    /// Flip the colors of the specified entityView and its direct children.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    private static void FlipColor(EntityView entityView)
    {
        entityView.IsBlack = !entityView.IsBlack;
        entityView.Left.IsBlack = !entityView.Left.IsBlack;
        entityView.Right.IsBlack = !entityView.Right.IsBlack;
    }

    /// <summary>
    /// Rotate the specified entityView "left".
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <returns>New root entityView.</returns>
    private static EntityView RotateLeft(EntityView entityView)
    {
        EntityView x = entityView.Right;
        entityView.Right = x.Left;
        x.Left = entityView;
        x.IsBlack = entityView.IsBlack;
        entityView.IsBlack = false;
        return x;
    }

    /// <summary>
    /// Rotate the specified entityView "right".
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <returns>New root entityView.</returns>
    private static EntityView RotateRight(EntityView entityView)
    {
        EntityView x = entityView.Left;
        entityView.Left = x.Right;
        x.Right = entityView;
        x.IsBlack = entityView.IsBlack;
        entityView.IsBlack = false;
        return x;
    }

    /// <summary>
    /// Moves a red entityView from the right child to the left child.
    /// </summary>
    /// <param name="entityView">Parent entityView.</param>
    /// <returns>New root entityView.</returns>
    private static EntityView MoveRedLeft(EntityView entityView)
    {
        FlipColor(entityView);
        if (IsRed(entityView.Right.Left))
        {
            entityView.Right = RotateRight(entityView.Right);
            entityView = RotateLeft(entityView);
            FlipColor(entityView);

            // * Avoid creating right-leaning entityViews
            if (IsRed(entityView.Right.Right))
            {
                entityView.Right = RotateLeft(entityView.Right);
            }
        }
        return entityView;
    }

    /// <summary>
    /// Moves a red entityView from the left child to the right child.
    /// </summary>
    /// <param name="entityView">Parent entityView.</param>
    /// <returns>New root entityView.</returns>
    private static EntityView MoveRedRight(EntityView entityView)
    {
        FlipColor(entityView);
        if (IsRed(entityView.Left.Left))
        {
            entityView = RotateRight(entityView);
            FlipColor(entityView);
        }
        return entityView;
    }

    /// <summary>
    /// Deletes the minimum entityView under the specified entityView.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <returns>New root entityView.</returns>
    private EntityView DeleteMinimum(EntityView entityView)
    {
        if (null == entityView.Left)
        {
            // Nothing to do
            return null;
        }

        if (!IsRed(entityView.Left) && !IsRed(entityView.Left.Left))
        {
            // Move red entityView left
            entityView = MoveRedLeft(entityView);
        }

        // Recursively delete
        entityView.Left = DeleteMinimum(entityView.Left);

        // Maintain invariants
        return FixUp(entityView);
    }

    /// <summary>
    /// Maintains invariants by adjusting the specified entityViews children.
    /// </summary>
    /// <param name="entityView">Specified entityView.</param>
    /// <returns>New root entityView.</returns>
    private static EntityView FixUp(EntityView entityView)
    {
        if (IsRed(entityView.Right))
        {
            // Avoid right-leaning entityView
            entityView = RotateLeft(entityView);
        }

        if (IsRed(entityView.Left) && IsRed(entityView.Left.Left))
        {
            // Balance 4-entityView
            entityView = RotateRight(entityView);
        }

        if (IsRed(entityView.Left) && IsRed(entityView.Right))
        {
            // Push red up
            FlipColor(entityView);
        }

        // * Avoid leaving behind right-leaning entityViews
        if ((null != entityView.Left) && IsRed(entityView.Left.Right) && !IsRed(entityView.Left.Left))
        {
            entityView.Left = RotateLeft(entityView.Left);
            if (IsRed(entityView.Left))
            {
                // Balance 4-entityView
                entityView = RotateRight(entityView);
            }
        }

        return entityView;
    }

    /// <summary>
    /// Gets the (first) entityView corresponding to the specified key.
    /// </summary>
    /// <param name="key">Key to search for.</param>
    /// <returns>Corresponding entityView or null if none found.</returns>
    private EntityView GetEntityViewForKey(TKey key)
    {
        // Initialize
        EntityView entityView = _rootEntityView;
        while (null != entityView)
        {
            // Compare keys and go left/right
            int comparisonResult = key.CompareTo(entityView.Key);
            if (comparisonResult < 0)
            {
                entityView = entityView.Left;
            }
            else if (0 < comparisonResult)
            {
                entityView = entityView.Right;
            }
            else
            {
                // Match; return entityView
                return entityView;
            }
        }

        // No match found
        return null;
    }

    /// <summary>
    /// Gets an extreme (ex: minimum/maximum) value.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    /// <param name="entityView">EntityView to start from.</param>
    /// <param name="successor">Successor function.</param>
    /// <param name="selector">Selector function.</param>
    /// <returns>Extreme value.</returns>
    private static T GetExtreme<T>(EntityView entityView, Func<EntityView, EntityView> successor, Func<EntityView, T> selector)
    {
        // Initialize
        T extreme = default(T);
        EntityView current = entityView;
        while (null != current)
        {
            // Go to extreme
            extreme = selector(current);
            current = successor(current);
        }
        return extreme;
    }

    /// <summary>
    /// Traverses a subset of the sequence of entityViews in order and selects the specified entityViews.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="entityView">Starting entityView.</param>
    /// <param name="condition">Condition method.</param>
    /// <param name="selector">Selector method.</param>
    /// <returns>Sequence of selected entityViews.</returns>
    private IEnumerable<T> Traverse<T>(EntityView entityView, Func<EntityView, bool> condition, Func<EntityView, T> selector)
    {
        // Create a stack to avoid recursion
        Stack<EntityView> stack = new Stack<EntityView>();
        EntityView current = entityView;
        while (null != current)
        {
            if (null != current.Left)
            {
                // Save current state and go left
                stack.Push(current);
                current = current.Left;
            }
            else
            {
                do
                {
                    // Select current entityView if relevant
                    if (condition(current))
                    {
                       yield return selector(current);
                    }
                    // Go right - or up if nothing to the right
                    current = current.Right;
                }
                while ((null == current) &&
                       (0 < stack.Count) &&
                       (null != (current = stack.Pop())));
            }
        }
    }

    /// <summary>
    /// Compares the specified keys (primary) and values (secondary).
    /// </summary>
    /// <param name="leftKey">The left key.</param>
    /// <param name="rightKey">The right key.</param>
    /// <returns>CompareTo-style results: -1 if left is less, 0 if equal, and 1 if greater than right.</returns>
    private int KeyComparison(TKey leftKey, TKey rightKey)
    {
        return leftKey.CompareTo(rightKey);
    }

#if DEBUGGING
    /// <summary>
    /// Asserts that tree invariants are not violated.
    /// </summary>
    private void AssertInvariants()
    {
        // Root is black
        Debug.Assert((null == _rootEntityView) || _rootEntityView.IsBlack, "Root is not black");
        // Every path contains the same number of black entityViews
        Dictionary<EntityView, EntityView> parents = new Dictionary<LeftLeaningRedBlackTree<TKey, TValue>.EntityView, LeftLeaningRedBlackTree<TKey, TValue>.EntityView>();
        foreach (EntityView entityView in Traverse(_rootEntityView, n => true, n => n))
        {
            if (null != entityView.Left)
            {
                parents[entityView.Left] = entityView;
            }
            if (null != entityView.Right)
            {
                parents[entityView.Right] = entityView;
            }
        }
        if (null != _rootEntityView)
        {
            parents[_rootEntityView] = null;
        }
        int treeCount = -1;
        foreach (EntityView entityView in Traverse(_rootEntityView, n => (null == n.Left) || (null == n.Right), n => n))
        {
            int pathCount = 0;
            EntityView current = entityView;
            while (null != current)
            {
                if (current.IsBlack)
                {
                    pathCount++;
                }
                current = parents[current];
            }
            Debug.Assert((-1 == treeCount) || (pathCount == treeCount), "Not all paths have the same number of black entityViews.");
            treeCount = pathCount;
        }
        // Verify entityView properties...
        foreach (EntityView entityView in Traverse(_rootEntityView, n => true, n => n))
        {
            // Left entityView is less
            if (null != entityView.Left)
            {
                Debug.Assert(0 > KeyAndValueComparison(entityView.Left.Key, entityView.Left.Value, entityView.Key, entityView.Value), "Left entityView is greater than its parent.");
            }
            // Right entityView is greater
            if (null != entityView.Right)
            {
                Debug.Assert(0 < KeyAndValueComparison(entityView.Right.Key, entityView.Right.Value, entityView.Key, entityView.Value), "Right entityView is less than its parent.");
            }
            // Both children of a red entityView are black
            Debug.Assert(!IsRed(entityView) || (!IsRed(entityView.Left) && !IsRed(entityView.Right)), "Red entityView has a red child.");
            // Always left-leaning
            Debug.Assert(!IsRed(entityView.Right) || IsRed(entityView.Left), "EntityView is not left-leaning.");
            // No consecutive reds (subset of previous rule)
            //Debug.Assert(!(IsRed(entityView) && IsRed(entityView.Left)));
        }
    }

    /// <summary>
    /// Gets an HTML fragment representing the tree.
    /// </summary>
    public string HtmlDocument
    {
        get
        {
            return
                "<html>" +
                    "<body>" +
                        (null != _rootEntityView ? _rootEntityView.HtmlFragment : "[null]") +
                    "</body>" +
                "</html>";
        }
    }
#endif
}
