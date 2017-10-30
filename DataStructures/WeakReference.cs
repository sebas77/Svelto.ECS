using System;
/// <span class="code-SummaryComment"><summary></span>
/// Represents a weak reference, which references an object while still allowing
/// that object to be reclaimed by garbage collection.
/// <span class="code-SummaryComment"></summary></span>
/// <span class="code-SummaryComment"><typeparam name="T">The type of the object that is referenced.</typeparam></span>

namespace Svelto.DataStructures
{
#if !NETFX_CORE
    public class WeakReference<T>
        : WeakReference where T : class
    {
        public bool IsValid { get { return Target != null && IsAlive == true; } }

        /// <span class="code-SummaryComment"><summary></span>
        /// Gets or sets the object (the target) referenced by the
        /// current WeakReference{T} object.
        /// <span class="code-SummaryComment"></summary></span>
        public new T Target
        {
            get
            {
                return (T)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }
        /// <span class="code-SummaryComment"><summary></span>
        /// Initializes a new instance of the WeakReference{T} class, referencing
        /// the specified object.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="target">The object to reference.</param></span>
        public WeakReference(T target)
        : base(target)
        { }

        /// <span class="code-SummaryComment"><summary></span>
        /// Initializes a new instance of the WeakReference{T} class, referencing
        /// the specified object and using the specified resurrection tracking.
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="target">An object to track.</param></span>
        /// <span class="code-SummaryComment"><param name="trackResurrection">Indicates when to stop tracking the object. </span>
        /// If true, the object is tracked
        /// after finalization; if false, the object is only tracked
        /// until finalization.<span class="code-SummaryComment"></param></span>
        public WeakReference(T target, bool trackResurrection)
            : base(target, trackResurrection)
        { }
    }
#else
    public class WeakReference<T> : System.WeakReference where T : class
    {
        public bool IsValid { get { return Target != null && IsAlive == true; } }

        public new T Target
        {
            get
            {
                return (T)base.Target;
            }
            set
            {
                base.Target = value;
            }
        }

        public WeakReference(T target)
        : base(target)
        { }

        public WeakReference(T target, bool trackResurrection)
            : base(target, trackResurrection)
        { }
    }
#endif
}
