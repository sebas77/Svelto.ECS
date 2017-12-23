using System.Collections;
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    /// <summary>
    /// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
    /// See https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Getting%20Started for more information
    /// </summary>
    /// <typeparam name="T">The values in the queue.  Must implement the PriorityQueueEntityView interface</typeparam>
    public sealed class HeapPriorityQueue<T> : IPriorityQueue<T> 
		where T : PriorityQueueEntityView
    {
        private int _numEntityViews;
		private readonly FasterList<T> _entityViews;
        private long _numEntityViewsEverEnqueued;

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="maxEntityViews">The max entityViews ever allowed to be enqueued (going over this will cause an exception)</param>
        public HeapPriorityQueue()
        {
            _numEntityViews = 0;
			_entityViews = new FasterList<T>();
            _numEntityViewsEverEnqueued = 0;
        }

		public HeapPriorityQueue(int initialSize)
		{
			_numEntityViews = 0;
			_entityViews = new FasterList<T>(initialSize);
			_numEntityViewsEverEnqueued = 0;
		}
		
		/// <summary>
		/// Returns the number of entityViews in the queue.  O(1)
        /// </summary>
        public int Count
        {
            get
            {
                return _numEntityViews;
            }
        }

        /// <summary>
        /// Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once Count == MaxSize),
        /// attempting to enqueue another item will throw an exception.  O(1)
        /// </summary>
        public int MaxSize
        {
            get
            {
				return _entityViews.Count - 1;
            }
        }

        /// <summary>
        /// Removes every entityView from the queue.  O(n) (So, don't do this often!)
        /// </summary>
        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void Clear()
        {
            _entityViews.FastClear();

            _numEntityViews = 0;
        }

        /// <summary>
        /// Returns (in O(1)!) whether the given entityView is in the queue.  O(1)
        /// </summary>
        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public bool Contains(T entityView)
        {
            return (_entityViews[entityView.QueueIndex] == entityView);
        }

        /// <summary>
        /// Enqueue a entityView - .Priority must be set beforehand!  O(log n)
        /// </summary>
        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void Enqueue(T entityView, double priority)
        {
            entityView.Priority = priority;
            _numEntityViews++;
			if (_entityViews.Count < _numEntityViews)
				_entityViews.Resize(_numEntityViews + 1);

            _entityViews[_numEntityViews] = entityView;
            entityView.QueueIndex = _numEntityViews;
            entityView.InsertionIndex = _numEntityViewsEverEnqueued++;
            CascadeUp(_entityViews[_numEntityViews]);
        }

        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void Swap(T entityView1, T entityView2)
        {
            //Swap the entityViews
            _entityViews[entityView1.QueueIndex] = entityView2;
            _entityViews[entityView2.QueueIndex] = entityView1;

            //Swap their indicies
            int temp = entityView1.QueueIndex;
            entityView1.QueueIndex = entityView2.QueueIndex;
            entityView2.QueueIndex = temp;
        }

        //Performance appears to be slightly better when this is NOT inlined o_O
        private void CascadeUp(T entityView)
        {
            //aka Heapify-up
            int parent = entityView.QueueIndex / 2;
            while(parent >= 1)
            {
                T parentEntityView = _entityViews[parent];
                if(HasHigherPriority(parentEntityView, entityView))
                    break;

                //EntityView has lower priority value, so move it up the heap
                Swap(entityView, parentEntityView); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

                parent = entityView.QueueIndex / 2;
            }
        }

        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void CascadeDown(T entityView)
        {
            //aka Heapify-down
            T newParent;
            int finalQueueIndex = entityView.QueueIndex;
            while(true)
            {
                newParent = entityView;
                int childLeftIndex = 2 * finalQueueIndex;

                //Check if the left-child is higher-priority than the current entityView
                if(childLeftIndex > _numEntityViews)
                {
                    //This could be placed outside the loop, but then we'd have to check newParent != entityView twice
                    entityView.QueueIndex = finalQueueIndex;
                    _entityViews[finalQueueIndex] = entityView;
                    break;
                }

                T childLeft = _entityViews[childLeftIndex];
                if(HasHigherPriority(childLeft, newParent))
                {
                    newParent = childLeft;
                }

                //Check if the right-child is higher-priority than either the current entityView or the left child
                int childRightIndex = childLeftIndex + 1;
                if(childRightIndex <= _numEntityViews)
                {
                    T childRight = _entityViews[childRightIndex];
                    if(HasHigherPriority(childRight, newParent))
                    {
                        newParent = childRight;
                    }
                }

                //If either of the children has higher (smaller) priority, swap and continue cascading
                if(newParent != entityView)
                {
                    //Move new parent to its new index.  entityView will be moved once, at the end
                    //Doing it this way is one less assignment operation than calling Swap()
                    _entityViews[finalQueueIndex] = newParent;

                    int temp = newParent.QueueIndex;
                    newParent.QueueIndex = finalQueueIndex;
                    finalQueueIndex = temp;
                }
                else
                {
                    //See note above
                    entityView.QueueIndex = finalQueueIndex;
                    _entityViews[finalQueueIndex] = entityView;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherPriority(entityView, entityView) (ie. both arguments the same entityView) will return false
        /// </summary>
        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private bool HasHigherPriority(T higher, T lower)
        {
            return (higher.Priority < lower.Priority ||
                (higher.Priority == lower.Priority && higher.InsertionIndex < lower.InsertionIndex));
        }

        /// <summary>
        /// Removes the head of the queue (entityView with highest priority; ties are broken by order of insertion), and returns it.  O(log n)
        /// </summary>
        public T Dequeue()
        {
            T returnMe = _entityViews[1];
            Remove(returnMe);
            return returnMe;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).  O(1)
        /// </summary>
        public T First
        {
            get
            {
                return _entityViews[1];
            }
        }

        /// <summary>
        /// This method must be called on a entityView every time its priority changes while it is in the queue.  
        /// <b>Forgetting to call this method will result in a corrupted queue!</b>
        /// O(log n)
        /// </summary>
        #if NET_VERSION_4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void UpdatePriority(T entityView, double priority)
        {
            entityView.Priority = priority;
            OnEntityViewUpdated(entityView);
        }

        private void OnEntityViewUpdated(T entityView)
        {
            //Bubble the updated entityView up or down as appropriate
            int parentIndex = entityView.QueueIndex / 2;
            T parentEntityView = _entityViews[parentIndex];

            if(parentIndex > 0 && HasHigherPriority(entityView, parentEntityView))
            {
                CascadeUp(entityView);
            }
            else
            {
                //Note that CascadeDown will be called if parentEntityView == entityView (that is, entityView is the root)
                CascadeDown(entityView);
            }
        }

        /// <summary>
        /// Removes a entityView from the queue.  Note that the entityView does not need to be the head of the queue.  O(log n)
        /// </summary>
        public void Remove(T entityView)
        {
            if(_numEntityViews <= 1)
            {
                _entityViews[1] = null;
                _numEntityViews = 0;
                return;
            }

            //Make sure the entityView is the last entityView in the queue
            bool wasSwapped = false;
            T formerLastEntityView = _entityViews[_numEntityViews];
            if(entityView.QueueIndex != _numEntityViews)
            {
                //Swap the entityView with the last entityView
                Swap(entityView, formerLastEntityView);
                wasSwapped = true;
            }

            _numEntityViews--;
            _entityViews[entityView.QueueIndex] = null;

            if(wasSwapped)
            {
                //Now bubble formerLastEntityView (which is no longer the last entityView) up or down as appropriate
                OnEntityViewUpdated(formerLastEntityView);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(int i = 1; i <= _numEntityViews; i++)
                yield return _entityViews[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// <b>Should not be called in production code.</b>
        /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// </summary>
        public bool IsValidQueue()
        {
			for(int i = 1; i < _entityViews.Count; i++)
            {
                if(_entityViews[i] != null)
                {
                    int childLeftIndex = 2 * i;
					if(childLeftIndex < _entityViews.Count && _entityViews[childLeftIndex] != null && HasHigherPriority(_entityViews[childLeftIndex], _entityViews[i]))
                        return false;

                    int childRightIndex = childLeftIndex + 1;
					if(childRightIndex < _entityViews.Count && _entityViews[childRightIndex] != null && HasHigherPriority(_entityViews[childRightIndex], _entityViews[i]))
                        return false;
                }
            }
            return true;
        }
    }
}