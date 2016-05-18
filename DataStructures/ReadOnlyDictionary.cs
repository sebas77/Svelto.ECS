//note: ripped from openstacknetsdk

using System;
using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public struct ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionary{TKey, TValue}"/> class
        /// that is a wrapper around the specified dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to wrap.</param>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            _dictionary = dictionary;
        }

        public bool isInitialized { get { return _dictionary != null; } }

        /// <summary>
        /// Gets the element that has the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The element that has the specified key.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">If property is retrieved and <paramref name="key"/> is not found.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// Gets the element that has the specified key.
        /// </summary>
        /// <exception cref="NotSupportedException">If the property is set.</exception>
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// Gets the element that has the specified key.
        /// </summary>
        /// <exception cref="NotSupportedException">If the property is set.</exception>
        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is TKey))
                    return null;

                return this[(TKey)key];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the number of items in the dictionary.
        /// </summary>
        /// <value>
        /// The number of items in the dictionary.
        /// </value>
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        /// <summary>
        /// Gets a key collection that contains the keys of the dictionary.
        /// </summary>
        /// <value>
        /// A key collection that contains the keys of the dictionary.
        /// </value>
        public KeyCollection Keys
        {
            get
            {
                return new KeyCollection(_dictionary.Keys);
            }
        }

        /// <inheritdoc/>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        /// <inheritdoc/>
        ICollection IDictionary.Keys
        {
            get
            {
                return Keys;
            }
        }

        /// <summary>
        /// Gets a collection that contains the values in the dictionary.
        /// </summary>
        /// <value>
        /// A collection that contains the values in the object that implements <see cref="ReadOnlyDictionary{TKey, TValue}"/>.
        /// </value>
        public ValueCollection Values
        {
            get
            {
                return new ValueCollection(_dictionary.Values);
            }
        }

        /// <inheritdoc/>
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        /// <inheritdoc/>
        ICollection IDictionary.Values
        {
            get
            {
                return Values;
            }
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        bool IDictionary.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        bool IDictionary.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        object ICollection.SyncRoot
        {
            get
            {
                ICollection collection = this as ICollection;
                if (collection == null)
                    return collection.SyncRoot;

                throw new NotSupportedException("The current object does not support the SyncRoot property.");
            }
        }

        /// <summary>
        /// Determines whether the dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns><see langword="true"/> if the dictionary contains an element that has the specified key; otherwise, <see langword="false"/>.</returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        bool IDictionary.Contains(object key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (key is TKey)
                return ContainsKey((TKey)key);

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ReadOnlyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            IDictionary dictionary = _dictionary as IDictionary;
            if (dictionary != null)
                return dictionary.GetEnumerator();

            return new DictionaryEnumerator(_dictionary);
        }

        /// <summary>
        /// Retrieves the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value will be retrieved.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the object that implements <see cref="ReadOnlyDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

/// <summary>
        /// Represents a read-only collection of the keys of a <see cref="ReadOnlyDictionary{TKey, TValue}"/> object.
        /// </summary>
        public struct KeyCollection : ICollection<TKey>, ICollection
        {
            /// <summary>
            /// The wrapped collection of keys.
            /// </summary>
            private readonly ICollection<TKey> _keys;

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyCollection"/> class
            /// as a wrapper around the specified collection of keys.
            /// </summary>
            /// <param name="keys">The collection of keys to wrap.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="keys"/> is <see langword="null"/>.</exception>
            internal KeyCollection(ICollection<TKey> keys)
            {
                if (keys == null)
                    throw new ArgumentNullException("keys");

                _keys = keys;
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            /// <value>
            /// The number of elements in the collection.
            /// </value>
            public int Count
            {
                get
                {
                    return _keys.Count;
                }
            }

            /// <inheritdoc/>
            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            /// <inheritdoc/>
            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            /// <inheritdoc/>
            object ICollection.SyncRoot
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Copies the elements of the collection to an array, starting at a specific array index.
            /// </summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">If <paramref name="arrayIndex"/> is less than 0.</exception>
            /// <exception cref="ArgumentException">
            /// If <paramref name="array"/> is multidimensional.
            /// <para>-or-</para>
            /// <para>If the number of elements in the source collection is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</para>
            /// <para>-or-</para>
            /// <para>If the type <typeparamref name="TKey"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</para>
            /// </exception>
            public void CopyTo(TKey[] array, int arrayIndex)
            {
                _keys.CopyTo(array, arrayIndex);
            }

            /// <inheritdoc/>
            void ICollection.CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<TKey> GetEnumerator()
            {
                return _keys.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc/>
            bool ICollection<TKey>.Contains(TKey item)
            {
                return _keys.Contains(item);
            }

            /// <inheritdoc/>
            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Represents a read-only collection of the values of a <see cref="ReadOnlyDictionary{TKey, TValue}"/> object.
        /// </summary>
        public struct ValueCollection : ICollection<TValue>, ICollection
        {
            /// <summary>
            /// The wrapped collection of values.
            /// </summary>
            private readonly ICollection<TValue> _values;

            /// <summary>
            /// Initializes a new instance of the <see cref="ValueCollection"/> class
            /// as a wrapper around the specified collection of values.
            /// </summary>
            /// <param name="values">The collection of values to wrap.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null"/>.</exception>
            internal ValueCollection(ICollection<TValue> values)
            {
                if (values == null)
                    throw new ArgumentNullException("values");

                _values = values;
            }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            /// <value>
            /// The number of elements in the collection.
            /// </value>
            public int Count
            {
                get
                {
                    return _values.Count;
                }
            }

            /// <inheritdoc/>
            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            /// <inheritdoc/>
            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            /// <inheritdoc/>
            object ICollection.SyncRoot
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Copies the elements of the collection to an array, starting at a specific array index.
            /// </summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">If <paramref name="arrayIndex"/> is less than 0.</exception>
            /// <exception cref="ArgumentException">
            /// If <paramref name="array"/> is multidimensional.
            /// <para>-or-</para>
            /// <para>If the number of elements in the source collection is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</para>
            /// <para>-or-</para>
            /// <para>If the type <typeparamref name="TValue"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</para>
            /// </exception>
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                _values.CopyTo(array, arrayIndex);
            }

            /// <inheritdoc/>
            void ICollection.CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<TValue> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc/>
            bool ICollection<TValue>.Contains(TValue item)
            {
                return _values.Contains(item);
            }

            /// <inheritdoc/>
            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }
        }

        struct DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            public DictionaryEnumerator(IDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");

                _enumerator = dictionary.GetEnumerator();
            }

            /// <inheritdoc/>
            public DictionaryEntry Entry
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = _enumerator.Current;
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            /// <inheritdoc/>
            public object Key
            {
                get
                {
                    return _enumerator.Current.Key;
                }
            }

            /// <inheritdoc/>
            public object Value
            {
                get
                {
                    return _enumerator.Current.Value;
                }
            }

            /// <inheritdoc/>
            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
