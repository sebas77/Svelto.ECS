using System.Collections.Generic;
using System.Threading;

namespace Svelto.DataStructures
{
    public class ThreadSafeQueue<T>
    {
        readonly Queue<T> m_Queue;

        readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim();

        public ThreadSafeQueue()
        {
            m_Queue = new Queue<T>();
        }

        public ThreadSafeQueue(int capacity)
        {
            m_Queue = new Queue<T>(capacity);
        }

        public ThreadSafeQueue(IEnumerable<T> collection)
        {
            m_Queue = new Queue<T>(collection);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Queue<T> localQ;

            LockQ.EnterReadLock();
            try
            {
                localQ = new Queue<T>(m_Queue);
            }

            finally
            {
                LockQ.ExitReadLock();
            }

            foreach (T item in localQ)
                yield return item;
        }

        public void Enqueue(T item)
        {
            LockQ.EnterWriteLock();
            try
            {
                m_Queue.Enqueue(item);
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public T Dequeue()
        {
            LockQ.EnterWriteLock();
            try
            {
                return m_Queue.Dequeue();
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public void EnqueueAll(IEnumerable<T> ItemsToQueue)
        {
            LockQ.EnterWriteLock();
            try
            {
                foreach (T item in ItemsToQueue)
                    m_Queue.Enqueue(item);
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public FasterList<T> DequeueAll()
        {
            LockQ.EnterWriteLock();
            try
            {
                FasterList<T> returnList = new FasterList<T>();

                while (m_Queue.Count > 0)
                    returnList.Add(m_Queue.Dequeue());

                return returnList;
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public void DequeueAllInto(FasterList<T> list)
        {
            LockQ.EnterWriteLock();
            try
            {
                while (m_Queue.Count > 0)
                    list.Add(m_Queue.Dequeue());
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public void DequeueInto(FasterList<T> list, int count)
        {
            LockQ.EnterWriteLock();
            try
            {
                int originalSize = m_Queue.Count;
                while (m_Queue.Count > 0 && originalSize - m_Queue.Count < count)
                    list.Add(m_Queue.Dequeue());
            }   

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public FasterList<U> DequeueAllAs<U>() where U:class
        {
            LockQ.EnterWriteLock();
            try
            {
                FasterList<U> returnList = new FasterList<U>();

                while (m_Queue.Count > 0)
                    returnList.Add(m_Queue.Dequeue() as U);

                return returnList;
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public T Peek()
        {
            LockQ.EnterWriteLock();
            try
            {
                T item = default(T);

                if (m_Queue.Count > 0)
                    item = m_Queue.Peek();

                return item;
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public void Clear()
        {
            LockQ.EnterWriteLock();
            try
            {
                m_Queue.Clear();
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public bool TryDequeue(out T item)
        {
            LockQ.EnterWriteLock();
            try
            {
                if (m_Queue.Count > 0)
                {
                    item = m_Queue.Dequeue();

                    return true;
                }
                else
                {
                    item = default(T);

                    return false;
                }
            }

            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                LockQ.EnterWriteLock();
                try
                {
                    return m_Queue.Count;
                }

                finally
                {
                    LockQ.ExitWriteLock();
                }
            }
        }
    }
}
