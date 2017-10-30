
using System.Collections.Generic;
using System.Threading;

namespace Svelto.DataStructures
{
    /// <summary>
    ///   original code: http://devplanet.com/blogs/brianr/archive/2008/09/29/thread-safe-dictionary-update.aspx
    ///   simplified (not an IDictionary) and apdated (uses FasterList)
    /// </summary>
    /// <typeparam name = "TKey"></typeparam>
    /// <typeparam name = "TValue"></typeparam>

    public class ThreadSafeDictionary<TKey, TValue>
    {
        public ThreadSafeDictionary(int v)
        {
            dict = new Dictionary<TKey, TValue>(v);
        }

        public ThreadSafeDictionary()
        {
            dict = new Dictionary<TKey, TValue>();
        }

        // setup the lock;
        public virtual int Count
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return dict.Count;
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return dict.IsReadOnly;
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }
        }

        public virtual FasterList<TKey> Keys
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return new FasterList<TKey>(dict.Keys);
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }
        }

        public virtual FasterList<TValue> Values
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return new FasterList<TValue>(dict.Values);
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                LockQ.EnterReadLock();
                try
                {
                    return dict[key];
                }
                finally
                {
                    LockQ.ExitReadLock();
                }
            }

            set
            {
                LockQ.EnterWriteLock();
                try
                {
                    dict[key] = value;
                }
                finally
                {
                    LockQ.ExitWriteLock();
                }
            }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Add(item);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public virtual void Clear()
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Clear();
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            LockQ.EnterReadLock();
            try
            {
                return dict.Contains(item);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            LockQ.EnterReadLock();
            try
            {
                dict.CopyTo(array, arrayIndex);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            LockQ.EnterWriteLock();
            try
            {
                return dict.Remove(item);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public virtual void Add(TKey key, TValue value)
        {
            LockQ.EnterWriteLock();
            try
            {
                dict.Add(key, value);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            LockQ.EnterReadLock();
            try
            {
                return dict.ContainsKey(key);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        public virtual bool Remove(TKey key)
        {
            LockQ.EnterWriteLock();
            try
            {
                return dict.Remove(key);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            LockQ.EnterReadLock();
            try
            {
                return dict.TryGetValue(key, out value);
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        /// <summary>
        ///   Merge does a blind remove, and then add.  Basically a blind Upsert.
        /// </summary>
        /// <param name = "key">Key to lookup</param>
        /// <param name = "newValue">New Value</param>
        public void MergeSafe(TKey key, TValue newValue)
        {
            LockQ.EnterWriteLock();
            try
            {
                // take a writelock immediately since we will always be writing
                if (dict.ContainsKey(key))
                    dict.Remove(key);

                dict.Add(key, newValue);
            }
            finally
            {
                LockQ.ExitWriteLock();
            }
        }

        /// <summary>
        ///   This is a blind remove. Prevents the need to check for existence first.
        /// </summary>
        /// <param name = "key">Key to remove</param>
        public void RemoveSafe(TKey key)
        {
            LockQ.EnterReadLock();
            try
            {
                if (dict.ContainsKey(key))
                    LockQ.EnterWriteLock();
                try
                {
                    dict.Remove(key);
                }
                finally
                {
                    LockQ.ExitWriteLock();
                }
            }
            finally
            {
                LockQ.ExitReadLock();
            }
        }

        // This is the internal dictionary that we are wrapping
        readonly IDictionary<TKey, TValue> dict;

        readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim();
    }
}
