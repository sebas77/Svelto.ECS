using System.Collections.Generic;
using System.Threading;

//from unify wiki
namespace Svelto.DataStructures
{
    public class SingleLinkNode<T>
    {
        // Note; the Next member cannot be a property since
        // it participates in many CAS operations
        public SingleLinkNode<T> Next;
        public T Item;
    }

    public static class SyncMethods
    {
        public static bool CAS<T>(ref T location, T comparand, T newValue) where T : class
        {
            return
                (object)comparand ==
                (object)Interlocked.CompareExchange<T>(ref location, newValue, comparand);
        }
    }

    public class LockFreeLinkPool<T>
    {
        private SingleLinkNode<T> head;

        public LockFreeLinkPool()
        {
            head = new SingleLinkNode<T>();
        }

        public void Push(SingleLinkNode<T> newNode)
        {
            newNode.Item = default(T);
            do
            {
                newNode.Next = head.Next;
            } while (!SyncMethods.CAS<SingleLinkNode<T>>(ref head.Next, newNode.Next, newNode));
            return;
        }

        public bool Pop(out SingleLinkNode<T> node)
        {
            do
            {
                node = head.Next;
                if (node == null)
                {
                    return false;
                }
            } while (!SyncMethods.CAS<SingleLinkNode<T>>(ref head.Next, node, node.Next));
            return true;
        }
    }

    public class LockFreeQueue<T>
    {

        SingleLinkNode<T> head;
        SingleLinkNode<T> tail;
        LockFreeLinkPool<T> trash;

        public LockFreeQueue()
        {
            head = new SingleLinkNode<T>();
            tail = head;
            trash = new LockFreeLinkPool<T>();
        }

        public void Enqueue(T item)
        {
            SingleLinkNode<T> oldTail = null;
            SingleLinkNode<T> oldTailNext;

            SingleLinkNode<T> newNode;
            if (!trash.Pop(out newNode))
            {
                newNode = new SingleLinkNode<T>();
            }
            else
            {
                newNode.Next = null;
            }
            newNode.Item = item;

            bool newNodeWasAdded = false;
            while (!newNodeWasAdded)
            {
                oldTail = tail;
                oldTailNext = oldTail.Next;

                if (tail == oldTail)
                {
                    if (oldTailNext == null)
                        newNodeWasAdded = SyncMethods.CAS<SingleLinkNode<T>>(ref tail.Next, null, newNode);
                    else
                        SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, oldTailNext);
                }
            }
            SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, newNode);
        }

        public bool Dequeue(out T item)
        {
            item = default(T);
            SingleLinkNode<T> oldHead = null;

            bool haveAdvancedHead = false;
            while (!haveAdvancedHead)
            {

                oldHead = head;
                SingleLinkNode<T> oldTail = tail;
                SingleLinkNode<T> oldHeadNext = oldHead.Next;

                if (oldHead == head)
                {
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null)
                        {
                            return false;
                        }
                        SyncMethods.CAS<SingleLinkNode<T>>(ref tail, oldTail, oldHeadNext);
                    }
                    else
                    {
                        item = oldHeadNext.Item;
                        haveAdvancedHead = SyncMethods.CAS<SingleLinkNode<T>>(ref head, oldHead, oldHeadNext);
                        if (haveAdvancedHead)
                        {
                            trash.Push(oldHead);
                        }
                    }
                }
            }
            return true;
        }
    }
}