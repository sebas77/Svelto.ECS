using System.Threading;

//from unify wiki
namespace Svelto.DataStructures
{
    public class SingleLinkEntityView<T>
    {
        // Note; the Next member cannot be a property since
        // it participates in many CAS operations
        public SingleLinkEntityView<T> Next;
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
        private SingleLinkEntityView<T> head;

        public LockFreeLinkPool()
        {
            head = new SingleLinkEntityView<T>();
        }

        public void Push(SingleLinkEntityView<T> newEntityView)
        {
            newEntityView.Item = default(T);
            do
            {
                newEntityView.Next = head.Next;
            } while (!SyncMethods.CAS<SingleLinkEntityView<T>>(ref head.Next, newEntityView.Next, newEntityView));
            return;
        }

        public bool Pop(out SingleLinkEntityView<T> entityView)
        {
            do
            {
                entityView = head.Next;
                if (entityView == null)
                {
                    return false;
                }
            } while (!SyncMethods.CAS<SingleLinkEntityView<T>>(ref head.Next, entityView, entityView.Next));
            return true;
        }
    }

    public class LockFreeQueue<T>
    {

        SingleLinkEntityView<T> head;
        SingleLinkEntityView<T> tail;
        LockFreeLinkPool<T> trash;

        public LockFreeQueue()
        {
            head = new SingleLinkEntityView<T>();
            tail = head;
            trash = new LockFreeLinkPool<T>();
        }

        public void Enqueue(T item)
        {
            SingleLinkEntityView<T> oldTail = null;
            SingleLinkEntityView<T> oldTailNext;

            SingleLinkEntityView<T> newEntityView;
            if (!trash.Pop(out newEntityView))
            {
                newEntityView = new SingleLinkEntityView<T>();
            }
            else
            {
                newEntityView.Next = null;
            }
            newEntityView.Item = item;

            bool newEntityViewWasAdded = false;
            while (!newEntityViewWasAdded)
            {
                oldTail = tail;
                oldTailNext = oldTail.Next;

                if (tail == oldTail)
                {
                    if (oldTailNext == null)
                        newEntityViewWasAdded = SyncMethods.CAS<SingleLinkEntityView<T>>(ref tail.Next, null, newEntityView);
                    else
                        SyncMethods.CAS<SingleLinkEntityView<T>>(ref tail, oldTail, oldTailNext);
                }
            }
            SyncMethods.CAS<SingleLinkEntityView<T>>(ref tail, oldTail, newEntityView);
        }

        public bool Dequeue(out T item)
        {
            item = default(T);
            SingleLinkEntityView<T> oldHead = null;

            bool haveAdvancedHead = false;
            while (!haveAdvancedHead)
            {

                oldHead = head;
                SingleLinkEntityView<T> oldTail = tail;
                SingleLinkEntityView<T> oldHeadNext = oldHead.Next;

                if (oldHead == head)
                {
                    if (oldHead == oldTail)
                    {
                        if (oldHeadNext == null)
                        {
                            return false;
                        }
                        SyncMethods.CAS<SingleLinkEntityView<T>>(ref tail, oldTail, oldHeadNext);
                    }
                    else
                    {
                        item = oldHeadNext.Item;
                        haveAdvancedHead = SyncMethods.CAS<SingleLinkEntityView<T>>(ref head, oldHead, oldHeadNext);
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