using System;
using System.Runtime.Serialization;
/// <span class="code-SummaryComment"><summary></span>
/// Represents a weak reference, which references an object while still allowing
/// that object to be reclaimed by garbage collection.
/// <span class="code-SummaryComment"></summary></span>
/// <span class="code-SummaryComment"><typeparam name="T">The type of the object that is referenced.</typeparam></span>
[Serializable]
public class WeakReference<T>
    : WeakReference where T : class
{
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
    protected WeakReference(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
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
}
