namespace Svelto.DataStructures
{
    public class PriorityQueueEntityView
    {
        /// <summary>
        /// The Priority to insert this entityView at.  Must be set BEFORE adding a entityView to the queue
        /// </summary>
        public double Priority { get;
            set; 
        }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the order the entityView was inserted in
        /// </summary>
        public long InsertionIndex { get; set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; set; }
    }
}
